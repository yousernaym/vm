# Visual Music

A Windows app that visualizes note-based music files, inspired by Stephen Malinovski's [MAM videos](https://www.youtube.com/user/smalin).  
Supports midi, tracker and sid files. Can produce accompanying audio automatically or you can supply your own mixed-down audio file.  
Can export mkv video files with support for 360-degree videos.  
[Example video](https://www.youtube.com/watch?v=I5NhREgxhDw)

## Installation

Run a [Windows installer](https://github.com/yousernaym/vm/releases)

## Building the source

* Install [Visual Studio](https://visualstudio.microsoft.com/) with the following workloads:
  * .NET desktop development
  * Desktop development with C++  
  * MSVC *version_numbers* C++ x64/x86 Spectre-mitigated libs (*version_number*)  
* Install Vcpkg, Fluidsynth and Ffmpeg:
    ```
    git clone https://github.com/microsoft/vcpkg.git
    cd vcpkg
    bootstrap-vcpkg.bat
    vcpkg integrate install
    vcpkg install fluidsynth:x64-windows
    vcpkg install ffmpeg[x264]:x64-windows
    ```
* Download this repo including submodules:
```
    git clone --recurse-submodules https://github.com/yousernaym/vm.git
``` 
* Open VisualMusic.sln in Visual Studio and build (if compilation of Content.mgcb fails, restart Visual Studio)
* (Optional) To get General Midi audio, place a [soundfont file](https://musescore.org/en/node/109371) named `soundfont.sf2` in the exe folder

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

#### [Remuxer](https://github.com/yousernaym/remuxer)  
* Mod import, based on [libmikmod](http://mikmod.sourceforge.net/) and [libopenmpt](https://lib.openmpt.org/libopenmpt/)
* Sid import, based on [libsidplayfp](https://sourceforge.net/projects/sidplay-residfp/)
#### [Midilib](https://github.com/yousernaym/midilib)
* Midi import
#### [MidMix](https://github.com/yousernaym/midmix)
* General Midi audio, based on [Fluidsynth](http://www.fluidsynth.org/)
#### [Media](https://github.com/yousernaym/media)
* Video export, based on [Ffmpeg]()
* Audio playback, based on [Media Foundation](https://docs.microsoft.com/en-us/windows/win32/medfound/microsoft-media-foundation-sdk)
#### [MonoGame fork](https://github.com/yousernaym/monogame)
* Graphics
* The fork contains a ConstantBuffer fix and a Curve optimization, both necessary for Visual Music to function properly

## Third-party projects
* [CefSharp](https://github.com/cefsharp/CefSharp) - Web browser (Nuget packages)
* [XNA for WinForms](https://github.com/SimonDarksideJ/XNAGameStudio/wiki/WinForms-Series-1-Graphics-Device) - Integration of MonoGame with Winforms (C# code)

## Author

[Michael H�llstr�m](mailto:mickehapps@outlook.com)
