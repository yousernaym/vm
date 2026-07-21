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
  `builtin-baseline`), and the first solution build installs them into a local `vcpkg_installed/` per submodule.
- Clone with `--recurse-submodules` — the `Dependencies/*` repos must be present or the solution won't load.
- Build `VisualMusic.sln` (Debug/Release). Use the **Any CPU** solution platform (the only one);
  C++ projects still build as x64 under the hood. The first build auto-runs `dotnet tool restore` (the MonoGame
  content-builder tool, pinned in [VisualMusic/.config/dotnet-tools.json](VisualMusic/.config/dotnet-tools.json)).

### Building from the command line (agents: read this)

This is a **mixed C#/C++ solution**, so you must build it with **MSBuild**, not `dotnet build`
(`dotnet build` cannot build the C++ `.vcxproj` projects). `msbuild` is usually **not on `PATH`** in a
plain shell — do not give up if `msbuild` "isn't found". Locate it with `vswhere` (always installed with
VS, at a fixed path) and call it by full path. From the repo root (`d:\dev\vm`), in PowerShell:

```powershell
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -prerelease -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"
& $msbuild d:\dev\vm\VisualMusic.sln /p:Configuration=Debug /p:Platform="Any CPU" /m /nologo
```

Notes:
- **Pass the full path to the `.sln`.** The agent PowerShell shell does **not** reliably start in the repo
  root, so a bare `VisualMusic.sln` fails with `MSB1009: Project file does not exist`. Use the absolute
  path `d:\dev\vm\VisualMusic.sln` (the Bash tool, by contrast, does persist its cwd at the repo root).
- The solution platform is **`Any CPU`** (the only supported solution platform). That still builds the C++
  projects as **x64** (via sln project mappings); native DLLs land in repo-root `x64\<Config>\`.
- Use `/p:Configuration=Release` for a release build. `/m` enables parallel builds; `/t:Rebuild` forces a
  clean rebuild.
- On this dev machine MSBuild also happens to be on `PATH` (a VS Developer shell), so a bare
  `msbuild VisualMusic.sln /p:Platform="Any CPU"` may work too — but the `vswhere` form above is the reliable one
  for agents and fresh shells.
- First build only: if you hit a missing-tool error, run `dotnet tool restore` in [VisualMusic/](VisualMusic/) first.
- **Reading the output:** a successful build is ~126 KB and noisy with *pre-existing, benign* warnings
  (MVVMTK0034 "should not be directly referenced", SYSLIB0003, CS0618, CS0168) plus a transient
  `VisualMusic_<hash>_wpftmp.csproj` (WPF's markup-compile pass — not an error). To find real failures
  grep for `": error "` and exclude `aka.ms` / `/errors/` (those substrings appear in warning help-links
  and cause false positives). Success indicators: `VM.dll ->` is emitted and the post-build native/remuxer
  copy target runs without errors.

### Where the app output lands (sln vs csproj — easy to get wrong)

- `VisualMusic.sln` with `/p:Configuration=<Config> /p:Platform="Any CPU"`: the app is **Any CPU**, so the
  runnable output is always
  `VisualMusic\bin\<Config>\net10.0-windows10.0.26100.0\` — **no `x64\` segment**. (The C# assembly is
  platform-agnostic; the native DLLs copied in are still x64 from `x64\<Config>\`.)
- Building `VisualMusic.csproj` directly: **don't** for a runnable app. `CopyNativeOutputs` soft-skips
  when `x64\<Config>\media.dll` / `MidMix.dll` or a packaged Remuxer bin are missing (so
  `VisualMusic.Tests` can build the managed assembly alone without `AdditionalProperties`). Always
  build the `.sln` for a complete `VM.exe`.

Stale copies of `VM.exe`/`VM.dll` can linger in old folders (e.g. `bin\x64\Debug\` from a former project-x64
config). To confirm where a build landed, read the
`VM.dll -> <path>` line in the build log, or find the newest binary:
`Get-ChildItem d:\dev\vm\VisualMusic\bin -Recurse -Filter VM.exe | Sort-Object LastWriteTime -Descending`

How the pieces reach the app output (this part trips people up — it's spread across several project files):

1. The C++ projects (Media, MidMix, libRemuxer + its vendored libs) build into the repo-root `x64\<Config>\`
   (project platform is x64 even when the solution platform is Any CPU).
2. VisualMusic's `CopyNativeOutputs` target copies `x64\<Config>\*.dll` into the app output, and
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
`Media/test-files/`, `MidMix/test-files/`). `VisualMusic.Tests` copies each into `test-files/<owner>/`. MonoGame is
not covered. Full commands are in [AGENTS.md](AGENTS.md) (unit with `Category!=Integration` / GoogleTest /
Integration after `VisualMusic.sln` Debug|Any CPU).
