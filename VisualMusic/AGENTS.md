# AGENTS.md

This file provides guidance for AI coding agents when working with code in this repository.

## Project Overview

**Visual Music** is a WPF Windows desktop application that visualizes note-based music files (MIDI, tracker modules, SID files) in real-time 3D.

Key characteristics:
- Produces video exports (MKV format, including 360-degree video support)
- Uses MonoGame for 3D graphics rendering
- Supports auto-generated audio from MIDI via Fluidsynth or user-supplied audio
- Multi-platform music source support via libRemuxer (OpenMPT, libsidplayfp)

## Build & Development

### Prerequisites
- **Visual Studio 2026** with:
  - .NET desktop development workload
  - Desktop development with C++ workload
  - MSVC v143 - VS 2022 C++ x64/x86 Spectre-mitigated libs
- **vcpkg** with `vcpkg integrate install` (no specific checkout needed — versions are pinned per-project)
- **Fluidsynth** and **FFmpeg** are restored automatically via vcpkg **manifest mode**: the Media and
  MidMix submodules each ship a `vcpkg.json` (with `builtin-baseline`
  `f3e10653cc27d62a37a3763cd84b38bca07c6075`, vcpkg release `2026.06.01`), so the first x64 build installs them into a local
  `vcpkg_installed/` — no manual `vcpkg install` step.
- **.NET 10.0** (project targets `net10.0-windows10.0.26100.0`)

### Building

Build `VisualMusic.sln` (in the repo root) via Visual Studio 2026 or:

```bash
msbuild VisualMusic.sln /p:Configuration=Release /p:Platform="Any CPU"
```

**Output location** (details in the root [AGENTS.md](../AGENTS.md) build section):
- `VisualMusic/bin/<Config>/net10.0-windows10.0.26100.0/` — no `x64/` segment (app is Any CPU; natives copied from repo-root `x64/<Config>/`)
- Assembly name: `VM.exe`
- Post-build `CopyNativeOutputs` packages native DLLs and Remuxer into the output folder; it
  soft-skips when natives are missing (managed-only / unit-test builds). Always build the `.sln`
  for a runnable app.

### Running

Open and run in Visual Studio 2026, or launch `VM.exe` directly from the bin folder.

**Optional:** Place a `soundfont.sf2` in the executable folder for MIDI audio synthesis (required for hearing MIDI playback).

### Project Structure

**Solution organization:**
- `VisualMusic/` — Main WPF application (primary project)
- `Dependencies/` — Submodules and third-party libraries
  - `midilib/` (C# MIDI parser)
  - `Media/` (C++ video export via FFmpeg, audio playback via Media Foundation)
  - `MidMix/` (C++ MIDI audio synthesis via Fluidsynth)
  - `Remuxer/` (C++ tool + C# wrapper for module/SID conversion via libRemuxer)
  - `MonoGame/` (forked graphics library for rendering)
  - `MonoGame/` dependencies: libsidplayfp, libopenmpt (audio format libraries)
- `InnoSetup/` — Windows installer script

## Architecture

### High-Level Design

The application follows an **MVVM-based WPF architecture** with:
- **MainWindow.xaml** — WPF host window using MahApps.Metro styling
- **MainViewModel** — Command routing, project lifecycle, screen switching (via MVVM Toolkit)
- **MonoGameHost** — HwndHost child window for MonoGame rendering at ~120 fps
- **SongRenderer** — MonoGame renderer implementing `ISongDrawHost` interface
- **Project** — Core data model holding notes, tracks, playback state, camera, keyframes
- **TrackView** — Per-track visualization properties and note rendering

### Key Patterns

**ISongDrawHost Interface** (`ISongDrawHost.cs`)
- Abstracts the drawing surface used by the WPF MonoGame renderer
- Implemented by `SongRenderer`
- Provides graphics resources, viewport info, timing, and invalidation callbacks
- Used by `Project.drawSong()` for frame rendering

**IImportService Interface** (`Controls/IImportService.cs`)
- Implemented by `MainViewModel`
- Called by web browser controls (CefSharp) when user downloads a song file
- Handles MIDI, MOD, and SID file imports with user-selectable options

**WPF Rendering Host**
- `MainWindow` hosts the app shell and browser/song views
- `MonoGameHost` bridges WPF and Win32 for MonoGame integration (custom HwndHost with P/Invoke)
- `SongRenderer` owns rendering, input routing, and `ISongDrawHost` services

### Rendering Pipeline

1. **MonoGameHost** (HwndHost) creates a Win32 child window hosting MonoGame's GraphicsDevice
2. **DispatcherTimer** in MonoGameHost drives **SongRenderer.Update()** and **SongRenderer.Draw()** at ~120 fps
3. **SongRenderer** manages:
   - Graphics resources (SpriteBatch, shaders, content)
   - Input (mouse/keyboard via WndProc)
   - Camera and viewport
4. **Project.drawSong()** queries `SongRenderer` (via `ISongDrawHost`) for graphics state and updates playback position
5. **TrackView** and **NoteStyle** classes render individual notes as geometric meshes

### Project & Playback State

**Project class** (`Project.cs`):
- Contains MIDI data (`Midi.Song notes`)
- Holds track properties (`List<TrackView> trackViews`)
- Manages playback: position (`NormSongPos`), tempo mapping, frame timing
- Serializable (XML DataContract) for save/load
- References static `ISongDrawHost` for rendering and invalidation

**Playback Model:**
- `NormSongPos` — normalized [0, 1] song position
- `SongPosT`, `SongPosB`, `SongPosS` — position in ticks, beats, seconds
- `SongLengthT`, `SongLengthS` — total duration
- Tempo events from MIDI are mapped to real-world timing

### Commands & Import Flow

**MainViewModel relays commands** (via MVVM Toolkit `[RelayCommand]`):
- **File:** ImportMidi, ImportMod, ImportSid, OpenProject, SaveProject, ExportVideo
- **Edit:** Undo, Redo, Camera load/save, Track properties, Lyrics & keyframes
- **Playback:** Play/pause, seek, nudge, jump
- **View:** Switch to Song/ModBrowser/SidBrowser/MidiBrowser screens

**Import Pipeline:**
1. User clicks download link in web browser → `SongWebBrowserWpf` calls `IImportService.ImportSong()`
2. `MainViewModel.ImportSong()` opens an import dialog (`ImportSongWindow`)
3. For SID files with multiple sub-songs, shows `SubSongWindow` picker
4. `Remuxer` (C++ wrapper) converts file to MIDI + WAV
5. New `Project` created, assigned to renderer, undo stack updated

### Undo/Redo

**UndoItems class** (`UndoItems.cs`):
- Stack-based undo/redo for full Project snapshots
- Descriptions available for menu text ("Undo Import MIDI", etc.)
- Called whenever a significant action modifies `Project`

### File Extensions & Serialization

**Project save format:**
- Extension: defined in `Project.DefaultFileExt`
- Uses `DataContractSerializer` with known types from `ProjectSerializer.KnownTypes`
- Saves to temp file first, then copied atomically to avoid corruption on crash

## WPF Application Status

- Main UI is WPF-based (MetroWindow, MahApps styling)
- All 5 property tabs (Style, Material, Light, Spatial, Audio) are WPF UserControls
- Rendering is hosted by `MonoGameHost` and `SongRenderer`
- Menu commands and keyboard bindings are mapped through WPF/MVVM
- Web browsers (MOD, SID, MIDI) use CefSharp WPF controls
- TbSliderWpf, TwoDHueSatWpf, HueSatButtonWpf custom controls are active
- TrackPropsViewModel exposes all per-tab properties with write-back callbacks

## Important Dependencies & P/Invoke

- **CefSharp** — Chromium embedded browser for web-based song browsers
- **MahApps.Metro** — WPF theming and controls
- **MonoGame** — Forked, custom changes for Visual Music
- **NAudio** — Audio playback helpers
- **Community Toolkit MVVM** — ObservableObject, RelayCommand source generators
- **Media.dll** (C++ native) — FFmpeg (video export), Media Foundation (audio)
- **MidMix.dll** (C++ native) — Fluidsynth MIDI synthesis
- **Remuxer/** binaries — Module/SID to MIDI conversion (external CLI tool rather than a linked library due to a license conflict between MonoGame and libsidplayfp)

**Win32 P/Invoke in MonoGameHost:**
- Window creation and sizing (CreateWindowEx, SetWindowPos, DestroyWindow)
- Message routing (WndProc overrides for mouse/keyboard)
- Coordinates MonoGame GraphicsDevice with WPF's HwndHost

## Known Issues & TODOs

From `todo.txt`:
- Viewport width change for lines (not yet implemented)
- Keyframe interpolation limited to viewport width and camera
- Hardware tessellation for lines (MonoGame limitation)
- Individual world matrices per track
- Parallax/normal mapping not yet supported
- Minimum width constraint for ribbon notes

## Basic UI Navigation

- **F2/F3/F4** — Switch to MOD/SID/MIDI browser screens
- **CTRL-Space** or media play key — Start/stop playback
- **Camera move:** `W A S D R F`
- **Camera rotate:** `Shift + W A S D Q E`
- **Keyframes** — Right-click a keyframeable property control to add/remove a property keyframe; select rows in the keyframe list and press Delete to remove them

## Testing

`VisualMusic.Tests` (xUnit) covers import formats, undo stack, keyframe interpolation, download helpers,
HVSC length lookup, remuxer stdout regexes, Project tempo math with a fake `ISongDrawHost`, and
Media/MidMix P/Invoke Integration smokes (next to [Media.cs](Media.cs) / [MidMix.cs](MidMix.cs)).

**Unit** (no native build required):

```powershell
dotnet test D:\dev\vm\VisualMusic\VisualMusic.Tests\VisualMusic.Tests.csproj --filter "Category!=Integration" --nologo
```

**Integration** (after `VisualMusic.sln` Debug|Any CPU so the test project copies `media.dll` / `MidMix.dll` + FFmpeg/Fluidsynth):

```powershell
dotnet test D:\dev\vm\VisualMusic\VisualMusic.Tests\VisualMusic.Tests.csproj --filter "Category=Integration" --nologo
```

Fixtures are copied from submodule `test-files/` trees into output `test-files/<owner>/` (see root [AGENTS.md](../AGENTS.md)).

Manual checks still useful: import MIDI/MOD/SID, playback sync, video export, undo/redo in the UI.
