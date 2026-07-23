# Visual Music

A Windows app that visualizes note-based music files, inspired by Stephen Malinovski's [MAM videos](https://www.youtube.com/user/smalin).  
Supports midi, tracker and sid files. Can produce accompanying audio automatically or you can supply your own mixed-down audio file.  
Can export mkv video files with support for 360-degree videos.  
[Example video](https://www.youtube.com/watch?v=I5NhREgxhDw)

## Installation

Run a [Windows installer](https://github.com/yousernaym/vm/releases)

## Building the source

* Install [Visual Studio 2026](https://visualstudio.microsoft.com/) with the following components:
  * Workloads
    * .NET desktop development
    * Desktop development with C++
* Install Vcpkg and wire it into MSBuild:
    ```
    git clone https://github.com/microsoft/vcpkg.git
    cd vcpkg
    bootstrap-vcpkg.bat
    vcpkg integrate install
    ```
    Fluidsynth and Ffmpeg/x264 are **not** installed by hand. The Media and MidMix
    C++ projects use vcpkg [manifest mode](https://learn.microsoft.com/vcpkg/concepts/manifest-mode):
    each declares its dependencies in a `vcpkg.json` and pins the package versions via
    `builtin-baseline` (currently vcpkg release `2026.06.01` = `f3e10653cc27d62a37a3763cd84b38bca07c6075`), so the
    first solution build of `VisualMusic.sln` restores them automatically into a local
    `vcpkg_installed/` folder. (`vcpkg integrate install` is still required — it's what
    locates the vcpkg root and runs the manifest restore during the build.)

    > **Heads-up:** the **first** solution build compiles Ffmpeg/x264 and Fluidsynth from
    > source — expect **~30–40 minutes** and an internet connection. The result is
    > cached (in the per-project `vcpkg_installed/` and vcpkg's binary cache), so every
    > later build reuses it and is fast; the dependencies are not rebuilt unless you
    > change a `vcpkg.json`, wipe `vcpkg_installed/`, or upgrade your MSVC toolset.
* Clone repo with submodules:
```
    git clone --recurse-submodules https://github.com/yousernaym/vm.git
``` 
* Build vm\VisualMusic.sln
* (Optional) To get audio from midi files, place a [soundfont file](https://musescore.org/en/node/109371) named `soundfont.sf2` in the exe folder

## Basic usage

### Import a song from the web
1. Press F2, F3 or F4 to go to one of the song browsers
2. Click on a song download link (not a playback link)
3. When returned to the visualization screen, press CTRL-Space or your keyboard's play button to start playback

### Camera control
Click in the song area to give it focus.
Move with WASD + RF.
Click scroll wheel to lock mouse movement for camera rotation. Click again to release lock.


### Key frames
Right-click on a control to add a key frame for that control at the current playback position.

## Sub projects

* [Remuxer](https://github.com/yousernaym/remuxer) (command-line tool) - Converts tracker and sid files to midi/wav. Based on libRemuxer which is based on the third-party libraries libsidplayfp and libopenmpt. libRemuxer should ideally have been linked directly to Visual Music but was not, because of a conflict between the Monogame and libsidplayfp licenses.
* [Midilib](https://github.com/yousernaym/midilib) (C# library) - Midi parser
* [MidMix](https://github.com/yousernaym/midmix) (C++ library) - Audio mixdown for midi files, based on [Fluidsynth](http://www.fluidsynth.org/)
* [Media](https://github.com/yousernaym/media) (C++ library)
  * Video export, based on [Ffmpeg](https://ffmpeg.org/doxygen/trunk/index.html)
  * Audio playback, based on [Media Foundation](https://docs.microsoft.com/en-us/windows/win32/medfound/microsoft-media-foundation-sdk)

## Third-party projects
* [MonoGame](https://github.com/yousernaym/monogame) (C# library) - Graphics (with a few forked changes necessary for Visual Music to function properly)
* [CefSharp](https://github.com/cefsharp/CefSharp) (C# library) - Web browser
