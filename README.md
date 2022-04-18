 Visual Music

A Windows app that visualizes note-based music files, inspired by Stephen Malinovski's [MAM videos](https://www.youtube.com/user/smalin).  
Supports midi, tracker and sid files. Can produce accompanying audio automatically or you can supply your own mixed-down audio file.  
Can export mkv video files with support for 360-degree videos.  
[Example video](https://www.youtube.com/watch?v=I5NhREgxhDw)

## Installation

Run a [Windows installer](https://github.com/yousernaym/vm/releases)

## Building the source

* Install [Visual Studio 2022](https://visualstudio.microsoft.com/) with the following components:
  * Workloads
    * .NET desktop development
    * Desktop development with C++
  * Individual components
    * MSVC v143 - VS 2022 C++ x64/x86 Spectre-mitigated libs (Latest)
* Install Vcpkg, Fluidsynth and Ffmpeg:
    ```
    git clone https://github.com/microsoft/vcpkg.git
    cd vcpkg
    git checkout aebb363eaa0b658beb19cbefdd5aa2f9cbc14f1e
    bootstrap-vcpkg.bat
    vcpkg integrate install
    vcpkg install fluidsynth:x64-windows
    vcpkg install ffmpeg[x264]:x64-windows
    ```
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
Move: wasdrf  
Rotate: Shift + wasdqe

### Key frames (WIP)
Press CTRL-K to insert a key frame at the current playback position.  
Changes to supported properties will be stored in the currently highlighted key frame. Currently only Viewport widht and Camera is supported.

## Sub projects

* [Remuxer](https://github.com/yousernaym/remuxer) (command-line tool) - Converts tracker and sid files to midi/wav. Based on libRemuxer which is based on the third-party libraries libsidplayfp, libmikmod and libopenmpt. libRemuxer should ideally have been linked directly to Visual Music but was not, because of a conflict between the Monogame and libsidplayfp licenses.
* [Midilib](https://github.com/yousernaym/midilib) (C# library) - Midi parser
* [MidMix](https://github.com/yousernaym/midmix) (C++ library) - Audio mixdown for midi files, based on [Fluidsynth](http://www.fluidsynth.org/)
* [Media](https://github.com/yousernaym/media) (C++ library)
  * Video export, based on [Ffmpeg](https://ffmpeg.org/doxygen/trunk/index.html)
  * Audio playback, based on [Media Foundation](https://docs.microsoft.com/en-us/windows/win32/medfound/microsoft-media-foundation-sdk)

## Third-party projects
* [MonoGame](https://github.com/yousernaym/monogame) (C# library) - Graphics (with a few forked changes necessary for Visual Music to function properly)
* [CefSharp](https://github.com/cefsharp/CefSharp) (C# library) - Web browser
* [XNA for WinForms](https://github.com/SimonDarksideJ/XNAGameStudio/wiki/WinForms-Series-1-Graphics-Device) (C# code) - Integration of MonoGame with Winforms
