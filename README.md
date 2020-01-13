# Visual Music

A Windows app that visualizes note-based music files [smalin](https://www.youtube.com/user/smalin) style.
Supports midi, tracker and sid files. Can produce accompanying audio automatically or you can supply your own mixed down audio file.
Can export mp4 video files with support for 360-degree videos.

## Installation

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

Install [Visual Studio](https://visualstudio.microsoft.com/) (include the ".NET desktop development" and "Desktop development with C++" workloads).
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

### Installing Visual Music
```
git clone https://github.com/yousernaym/vm.git
cd vm
bootstrap.bat
```
Then open VisualMusic.sln in Visual Studio, wait for the required Nuget packages to get installed and build the solution.
For playback of general midi songs, get a [soundfont file](https://musescore.org/en/node/109371), rename it to soundfont.sf2 and put it in the app folder (eg., bin/debug).
For optimal module playback, use XmPlay (File -> Third-party integration.. -> Import XmPlay).

## Basic Usage

### Import a song from the web
1. Press F2, F3 or F4 to go to one of the song browsers.
2. Click on a song download link.
3. When returned to the playback screen, press CTRL-Space to start playback.

### Camera control (needs improvement)
Keyboard keys: wasdqexc
Hold Caps Lock and move mouse and/or click mouse buttons.

### Key frames (WIP)
To insert a key frame at the current playback position, press CTRL-K.
Changes to supported properties will be stored in the currently highlighted key frame.
Only Viewport widht and Camera is supported atm.

## Deployment

Follow these steps to create an installation file:
1. Make a release build.
2. Use [InnoSetup](https://www.jrsoftware.org/isinfo.php) to compile the .iss script in the InnoSetup subfolder.

## Author

[Michael H�llstr�m](mailto:mickehapps@outlook.com)

## Acknowledgments

Third-party code:
* [MonoGame](http://www.monogame.net/)
* [Fluidsynth](http://www.fluidsynth.org/)
* [libmikmod](http://mikmod.sourceforge.net/)
* [libsidplayfp](https://sourceforge.net/projects/sidplay-residfp/)
* [Media Foundation](https://docs.microsoft.com/en-us/windows/win32/medfound/microsoft-media-foundation-sdk)
* [Spatial Media Metadata Injector](https://github.com/google/spatial-media)
* [CefSharp](https://github.com/cefsharp/CefSharp)
* [README-Template.md](https://gist.github.com/PurpleBooth/109311bb0361f32d87a2#file-readme-template-md)
* [XNA for WinForms](https://github.com/SimonDarksideJ/XNAGameStudio/wiki/WinForms-Series-1-Graphics-Device)
