# Visual Music

An app for visualizing note-based music files.
Supports midi, tracker and sid files. Can produce accompanying audio automatically (requires a soundfont file for midi songs) or you can supply your own mixed down audio file.
Can export mp4 video files with support for 360-degree videos.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

Installing vcpkg and fluidsynth:

```
git clone https://github.com/microsoft/vcpkg.git
cd vcpkg
bootstrap-vcpkg.bat
vcpkg integrate install
vcpkg install fluidsynth
```

### Installing

```
git clone https://github.com/yousernaym/vm.git
cd vm
bootstrap.bat
```
Then open the solution with Visual Studio 2019 or later and build.

## Author

Michael Hällström - [yousernaym](https://github.com/yousernaym)

## Acknowledgments

* Third-party dependencies:
[MonoGame](http://www.monogame.net/)
[Fluidsynth](http://www.fluidsynth.org/)
[libmikmod](http://mikmod.sourceforge.net/)
[libsidplayfp](https://sourceforge.net/projects/sidplay-residfp/) (from my command-line tool [Remuxer](https://github.com/yousernaym/Remuxer)
[Media Foundation](https://docs.microsoft.com/en-us/windows/win32/medfound/microsoft-media-foundation-sdk)
[Spatial Media Metadata Injector](https://github.com/google/spatial-media)
[CefSharp](https://github.com/cefsharp/CefSharp)
[README-Template.md]([https://gist.github.com/PurpleBooth/109311bb0361f32d87a2#file-readme-template-md)])
[WinForms XNA](https://github.com/SimonDarksideJ/XNAGameStudio/wiki/WinForms-Series-1-Graphics-Device)


Inspired by [Stephen Malinowski](https://www.youtube.com/user/smalin)
