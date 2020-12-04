# Visual Music

A Windows app that visualizes note-based music files.  
Supports midi, tracker and sid files. Can produce accompanying audio automatically or you can supply your own mixed-down audio file.  
Can export mp4 video files with support for 360-degree videos.  
[Example video](https://www.youtube.com/watch?v=I5NhREgxhDw)

## Prerequisites

* Install [Visual Studio](https://visualstudio.microsoft.com/) with the following workloads:
	* .NET desktop development
	* Desktop development with C++  
* Install [MonoGame for Visual Studio](http://community.monogame.net/t/monogame-3-7-1-release/11173)  
* Install [Python 3](https://www.python.org/), and make sure it is added to PATH (needed for 360-degree videos)
* Install Vcpkg and Fluidsynth (needed for General Midi songs):
```
git clone https://github.com/microsoft/vcpkg.git
cd vcpkg
bootstrap-vcpkg.bat
vcpkg integrate install
vcpkg install fluidsynth:x64-windows
```
* Run `bootstrap.bat` in the repo folder. After this is done, Python is not needed anymore.
* (Optional) To get General Midi audio, place a [soundfont file](https://musescore.org/en/node/109371) named `soundfont.sf2` in the app folder (eg., bin\Debug).

## Usage

### Import a song from the web
1. Press F2, F3 or F4 to go to one of the song browsers.
2. Click on a song download link (**not** a playback link).
3. When returned to the playback screen, press CTRL-Space to start playback.

### Camera control
Keyboard keys: wasdqexc  
Hold Caps Lock and move mouse and/or click mouse buttons.

### Key frames (WIP)
Press CTRL-K to insert a key frame at the current playback position.  
Changes to supported properties will be stored in the currently highlighted key frame. Only Viewport widht and Camera is supported atm.

### XMPlay
For optimal module playback, import XmPlay: `File -> Third-party integration.. -> Import XmPlay`.

The rest should be self explanatory or possible to figure out with some experimentation.

## Sub projects

* [Remuxer](https://github.com/yousernaym/remuxer) - Mod/sid import (command-line tool)
* [Midilib](https://github.com/yousernaym/midilib) - Midi import (C# class library)
* [MidMix](https://github.com/yousernaym/midmix) - General Midi audio mixdown (C++ dll)
* [Media](https://github.com/yousernaym/media) - Video export, audio playback (C++ dll)

## Author

[Michael Hällström](mailto:mickehapps@outlook.com)

## Acknowledgments

Third-party components:
* [MonoGame](http://www.monogame.net/) - Graphics
* [Fluidsynth](http://www.fluidsynth.org/) - General Midi audio
* [libmikmod](http://mikmod.sourceforge.net/) - Module import
* [libsidplayfp](https://sourceforge.net/projects/sidplay-residfp/) - SID import
* [Media Foundation](https://docs.microsoft.com/en-us/windows/win32/medfound/microsoft-media-foundation-sdk) - Video export
* [Spatial Media Metadata Injector](https://github.com/google/spatial-media) - 360-degree video export
* [CefSharp](https://github.com/cefsharp/CefSharp) - Web browser
* [XNA for WinForms](https://github.com/SimonDarksideJ/XNAGameStudio/wiki/WinForms-Series-1-Graphics-Device) - Integration of MonoGame with Winforms

Inspiration: [smalin](https://www.youtube.com/user/smalin)
