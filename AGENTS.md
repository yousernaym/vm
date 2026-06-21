# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## What this repository is

**Visual Music** (`VM.exe`) is a Windows desktop app that renders note-based music (MIDI, tracker
modules, SID) as real-time 3D visualizations and exports them to MKV video (including 360°). The app
code lives in [VisualMusic/](VisualMusic/); everything under [Dependencies/](Dependencies/) is a git
submodule (a separate repo) that this solution builds and the app consumes.

**Start here:** the detailed app-level guide is [VisualMusic/AGENTS.md](VisualMusic/AGENTS.md) — read it
for the WPF app architecture, MVVM layout, rendering pipeline, undo/redo, and UI navigation.

## Solution layout

Everything builds from one aggregate solution at the repo root: `VisualMusic.sln`. It contains the app,
the in-house submodules, the forked MonoGame, and the vendored C++ audio libraries, wired together with
project dependencies so a single build produces a runnable app.

| Project | Language | How VisualMusic uses it | Guide |
|---|---|---|---|
| VisualMusic | C# (net10.0-windows, WPF) | the app itself; assembly `VM.exe` | [VisualMusic/AGENTS.md](VisualMusic/AGENTS.md) |
| midiLib | C# (net48) | `ProjectReference` — MIDI parsing (`Midi.Song`) | [Dependencies/midiLib/AGENTS.md](Dependencies/midiLib/AGENTS.md) |
| MonoGame (WindowsDX) | C# (fork) | `ProjectReference` — 3D graphics framework | [Dependencies/MonoGame/AGENTS.md](Dependencies/MonoGame/AGENTS.md) |
| Media | C++ → `media.dll` | P/Invoke ([VisualMusic/Media.cs](VisualMusic/Media.cs)) — FFmpeg video export + Media Foundation audio playback | [Dependencies/Media/AGENTS.md](Dependencies/Media/AGENTS.md) |
| MidMix | C++ → `MidMix.dll` | P/Invoke ([VisualMusic/MidMix.cs](VisualMusic/MidMix.cs)) — Fluidsynth MIDI→WAV mixdown | [Dependencies/MidMix/AGENTS.md](Dependencies/MidMix/AGENTS.md) |
| Remuxer | C# exe + C++ `libRemuxer.dll` | launched as a child process (`remuxer/remuxer.exe`, see [VisualMusic/Project.cs](VisualMusic/Project.cs)) — converts MOD/SID → MIDI+WAV | [Dependencies/Remuxer/AGENTS.md](Dependencies/Remuxer/AGENTS.md) |

Data-flow in one line: MOD/SID are converted to MIDI by **Remuxer**, MIDI is parsed by **midiLib** into a
`Midi.Song`, **MonoGame** renders it, **MidMix** synthesizes MIDI audio, and **Media** plays audio back
and exports video.

## Build

Full prerequisites and the exact vcpkg steps are in [README.md](README.md). In short:

- Visual Studio 2026 with the **.NET desktop** and **Desktop development with C++** workloads (plus the
  Spectre-mitigated MSVC v143 x64/x86 libs).
- vcpkg (pinned commit) with `fluidsynth:x64-windows` and `ffmpeg[x264]:x64-windows`.
- Clone with `--recurse-submodules` — the `Dependencies/*` repos must be present or the solution won't load.
- Build `VisualMusic.sln` (Debug/Release). Use the **x64** solution platform: the native DLLs
  are 64-bit, so the app must run 64-bit. The first build auto-runs `dotnet tool restore` (the MonoGame
  content-builder tool, pinned in [VisualMusic/.config/dotnet-tools.json](VisualMusic/.config/dotnet-tools.json)).

### Building from the command line (agents: read this)

This is a **mixed C#/C++ solution**, so you must build it with **MSBuild**, not `dotnet build`
(`dotnet build` cannot build the C++ `.vcxproj` projects). `msbuild` is usually **not on `PATH`** in a
plain shell — do not give up if `msbuild` "isn't found". Locate it with `vswhere` (always installed with
VS, at a fixed path) and call it by full path. From the repo root (`d:\dev\vm`), in PowerShell:

```powershell
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -prerelease -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"
& $msbuild VisualMusic.sln /p:Configuration=Debug /p:Platform=x64 /m /nologo
```

Notes:
- The platform **must** be `x64` (not `Any CPU` / `Win32`); the native DLLs are 64-bit.
- Use `/p:Configuration=Release` for a release build. `/m` enables parallel builds; `/t:Rebuild` forces a
  clean rebuild.
- On this dev machine MSBuild also happens to be on `PATH` (a VS Developer shell), so a bare
  `msbuild VisualMusic.sln /p:Platform=x64` may work too — but the `vswhere` form above is the reliable one
  for agents and fresh shells.
- First build only: if you hit a missing-tool error, run `dotnet tool restore` in [VisualMusic/](VisualMusic/) first.

How the pieces reach the app output (this part trips people up — it's spread across several project files):

1. The C++ projects (Media, MidMix, libRemuxer + its vendored libs) build into the repo-root `x64\<Config>\`
   (their `OutDir` is `$(SolutionDir)$(Platform)\$(Configuration)\`).
2. VisualMusic's post-build (in `VisualMusic.csproj`) `xcopy`s `x64\<Config>\*.dll` into the app output, and
   copies the entire Remuxer build into `<app output>\remuxer\`.
3. MonoGame `.fx`/content is copied via the `Content\` items in `VisualMusic.csproj`.

Optional at runtime: place a `soundfont.sf2` next to `VM.exe` for MIDI audio synthesis.

## Working across submodules

`Dependencies/midiLib`, `Media`, `MidMix`, `Remuxer`, and `MonoGame` are independent git repos (see
[.gitmodules](.gitmodules)); `Remuxer` itself nests `libRemuxer`. Edits inside a submodule are commits in
*that* repo — this repo only tracks the submodule commit pointer. **MonoGame** is a large upstream fork and
most of `Remuxer/libRemuxer` (openmpt, sidplayfp) is vendored third-party code: treat both as
upstream and change only what Visual Music requires.

## Testing

There are no automated test projects for the app; verification is manual (import a MIDI/MOD/SID file, check
the visualization and audio sync, exercise export and undo/redo). See [VisualMusic/AGENTS.md](VisualMusic/AGENTS.md).
