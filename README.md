# Visual Music

A Windows app that visualizes note-based music files.  
Supports midi, tracker and sid files. Can produce accompanying audio automatically or you can supply your own mixed-down audio file.  
Can export mp4 video files with support for 360-degree VR videos.  
[Example video](https://www.youtube.com/watch?v=I5NhREgxhDw)

## Building the source

### Prerequisites

Install [Visual Studio](https://visualstudio.microsoft.com/) with the ".NET desktop development" and "Desktop development with C++" workloads.  
Install [MonoGame for Visual Studio](http://community.monogame.net/t/monogame-3-7-1-release/11173).  
Install [Git LFS](https://git-lfs.github.com/).  
Install Vcpkg and Fluidsynth:  
```
git clone https://github.com/microsoft/vcpkg.git
cd vcpkg
bootstrap-vcpkg.bat
vcpkg integrate install
vcpkg install fluidsynth:x64-windows
```

### Installation

```
git clone https://github.com/yousernaym/vm.git
cd vm
bootstrap.bat
```
Build VisualMusic.sln.  
For playback of general midi songs, get a [soundfont file](https://musescore.org/en/node/109371), rename it to soundfont.sf2 and put it in the app folder.  
For optimal module playback, import XmPlay from File -> Third-party integration.. -> Import XmPlay.

## Usage

### Import a song from the web
1. Press F2, F3 or F4 to go to one of the song browsers.
2. Click on a song download link.
3. When returned to the playback screen, press CTRL-Space to start playback.

### Camera control
Keyboard keys: wasdqexc  
Hold Caps Lock and move mouse and/or click mouse buttons.

### Key frames (WIP)
Press CTRL-K to insert a key frame at the current playback position.  
Changes to supported properties will be stored in the currently highlighted key frame. Only Viewport widht and Camera is supported atm.

### Etc.
The rest should be self explanatory or possible to figure out with some experimentation.

## Author

[Michael Hällström](mailto:mickehapps@outlook.com)

## Acknowledgments

Third-party components:
* [MonoGame](http://www.monogame.net/)
* [Fluidsynth](http://www.fluidsynth.org/)
* [libmikmod](http://mikmod.sourceforge.net/)
* [libsidplayfp](https://sourceforge.net/projects/sidplay-residfp/)
* [Media Foundation](https://docs.microsoft.com/en-us/windows/win32/medfound/microsoft-media-foundation-sdk)
* [Spatial Media Metadata Injector](https://github.com/google/spatial-media)
* [CefSharp](https://github.com/cefsharp/CefSharp)
* [XNA for WinForms](https://github.com/SimonDarksideJ/XNAGameStudio/wiki/WinForms-Series-1-Graphics-Device)

Inspiration: [smalin](https://www.youtube.com/user/smalin)