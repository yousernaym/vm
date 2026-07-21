# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repository is

**Visual Music** (`VM.exe`) is a Windows desktop app that renders note-based music (MIDI, tracker
modules, SID) as real-time 3D visualizations and exports them to MKV video (including 360°). The app
code lives in [VisualMusic/](VisualMusic/); everything under [Dependencies/](Dependencies/) is a git
submodule (a separate repo) that this solution builds and the app consumes.

**Start here:** the detailed app-level guide is [VisualMusic/CLAUDE.md](VisualMusic/CLAUDE.md) — read it
for the WPF app architecture, MVVM layout, rendering pipeline, undo/redo, and UI navigation.

## Solution layout

Everything builds from one aggregate solution at the repo root: `VisualMusic.sln`. It contains the app,
the in-house submodules, the forked MonoGame, and the vendored C++ audio libraries, wired together with
project dependencies so a single build produces a runnable app.

| Project | Language | How VisualMusic uses it | Guide |
|---|---|---|---|
| VisualMusic | C# (net10.0-windows, WPF) | the app itself; assembly `VM.exe` | [VisualMusic/CLAUDE.md](VisualMusic/CLAUDE.md) |
| midiLib | C# (net10.0) | `ProjectReference` — MIDI parsing (`Midi.Song`) | [Dependencies/midiLib/CLAUDE.md](Dependencies/midiLib/CLAUDE.md) |
| MonoGame (WindowsDX) | C# (fork) | `ProjectReference` — 3D graphics framework | [Dependencies/MonoGame/CLAUDE.md](Dependencies/MonoGame/CLAUDE.md) |
| Media | C++ → `media.dll` | P/Invoke ([VisualMusic/Media.cs](VisualMusic/Media.cs)) — FFmpeg video export + Media Foundation audio playback | [Dependencies/Media/CLAUDE.md](Dependencies/Media/CLAUDE.md) |
| MidMix | C++ → `MidMix.dll` | P/Invoke ([VisualMusic/MidMix.cs](VisualMusic/MidMix.cs)) — Fluidsynth MIDI→WAV mixdown | [Dependencies/MidMix/CLAUDE.md](Dependencies/MidMix/CLAUDE.md) |
| Remuxer | C# exe + C++ `libRemuxer.dll` | launched as a child process (`remuxer/remuxer.exe`, see [VisualMusic/Project.cs](VisualMusic/Project.cs)) — converts MOD/SID → MIDI+WAV | [Dependencies/Remuxer/CLAUDE.md](Dependencies/Remuxer/CLAUDE.md) |

Data-flow in one line: MOD/SID are converted to MIDI by **Remuxer**, MIDI is parsed by **midiLib** into a
`Midi.Song`, **MonoGame** renders it, **MidMix** synthesizes MIDI audio, and **Media** plays audio back
and exports video.

## Build

Full prerequisites and the exact vcpkg steps are in [README.md](README.md). In short:

- Visual Studio 2026 with the **.NET desktop** and **Desktop development with C++** workloads (plus the
  Spectre-mitigated MSVC v143 x64/x86 libs).
- vcpkg with `vcpkg integrate install`. Fluidsynth and `ffmpeg[x264]` are restored automatically via
  vcpkg **manifest mode**: Media and MidMix each ship a `vcpkg.json` (pinning versions with a
  `builtin-baseline`), and the first x64 build installs them into a local `vcpkg_installed/` per submodule.
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
& $msbuild d:\dev\vm\VisualMusic.sln /p:Configuration=Debug /p:Platform=x64 /m /nologo
```

Notes:
- **Pass the full path to the `.sln`.** The agent PowerShell shell does **not** reliably start in the repo
  root, so a bare `VisualMusic.sln` fails with `MSB1009: Project file does not exist`. Use the absolute
  path `d:\dev\vm\VisualMusic.sln` (the Bash tool, by contrast, does persist its cwd at the repo root).
- The platform **must** be `x64` (not `Any CPU` / `Win32`); the native DLLs are 64-bit.
- Use `/p:Configuration=Release` for a release build. `/m` enables parallel builds; `/t:Rebuild` forces a
  clean rebuild.
- On this dev machine MSBuild also happens to be on `PATH` (a VS Developer shell), so a bare
  `msbuild VisualMusic.sln /p:Platform=x64` may work too — but the `vswhere` form above is the reliable one
  for agents and fresh shells.
- First build only: if you hit a missing-tool error, run `dotnet tool restore` in [VisualMusic/](VisualMusic/) first.
- **Reading the output:** a successful build is ~126 KB and noisy with *pre-existing, benign* warnings
  (MVVMTK0034 "should not be directly referenced", SYSLIB0003, CS0618, CS0168) plus a transient
  `VisualMusic_<hash>_wpftmp.csproj` (WPF's markup-compile pass — not an error). To find real failures
  grep for `": error "` and exclude `aka.ms` / `/errors/` (those substrings appear in warning help-links
  and cause false positives). Success indicators: `VM.dll ->` is emitted and the post-build `File(s) copied`
  lines run.

### Where the app output lands (sln vs csproj — easy to get wrong)

The `.sln` maps the VisualMusic *project* to different project platforms per configuration, so the app's
output folder depends on how you build:

- `VisualMusic.sln` with `/p:Configuration=Debug /p:Platform=x64` (the normal build): the sln maps the
  app project to **Any CPU**, so the runnable output is
  `VisualMusic\bin\Debug\net10.0-windows10.0.26100.0\` — **no `x64\` segment**. (The C# assembly is
  platform-agnostic; the native DLLs xcopied in are still x64.)
- `VisualMusic.sln` with `/p:Configuration=Release /p:Platform=x64`: the sln maps Release|x64 to **x64**,
  so the output is `VisualMusic\bin\x64\Release\net10.0-windows10.0.26100.0\` (asymmetric with Debug).
- Building `VisualMusic.csproj` directly: **don't.** `$(SolutionDir)` is undefined outside the solution,
  so after compiling, the post-build xcopy of native DLLs + remuxer fails with `MSB3073`, and the output
  goes to yet another folder (`bin\x64\Debug\...`) that lacks the native DLLs. Always build the `.sln`.

Stale copies of `VM.exe`/`VM.dll` can linger in the folders you are *not* building to (e.g.
`bin\x64\Debug\` from an old direct-csproj build). To confirm where a build landed, read the
`VM.dll -> <path>` line in the build log, or find the newest binary:
`Get-ChildItem d:\dev\vm\VisualMusic\bin -Recurse -Filter VM.exe | Sort-Object LastWriteTime -Descending`

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

Essential automated tests live in each first-party repo (xUnit for C#, GoogleTest for libRemuxer Song/FileFormat).
Fixtures live in the deepest owning submodule (`midiLib/test-files/`, `Remuxer/libRemuxer/test-files/`,
`Media/test-files/`, `MidMix/test-files/`); test projects copy them into output `test-files/`. MonoGame is
not covered. Full commands are in [AGENTS.md](AGENTS.md) (unit with `Category!=Integration` / GoogleTest /
Integration after `VisualMusic.sln` Debug|x64).
