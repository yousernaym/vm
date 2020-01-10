# Visual Music

A Windows app that visualizes note-based music files.
Supports midi, tracker and sid files. Can produce accompanying audio automatically or you can supply your own mixed down audio file.
Can export mp4 video files with support for 360-degree video.

## Installing

Installing vcpkg and fluidsynth:
```
git clone https://github.com/microsoft/vcpkg.git
cd vcpkg
bootstrap-vcpkg.bat
vcpkg integrate install
vcpkg install fluidsynth
```

Installing Visual Music:
```
git clone https://github.com/yousernaym/vm.git
cd vm
bootstrap.bat
```
Then open the solution with Visual Studio and build.
For playback of general midi songs, get a [soundfont file](https://musescore.org/en/node/109371), rename it to soundfont.sf2 and put it in the app folder (eg., bin/debug).

## Author

[Michael Hällström](mailto:mickehapps@outlook.com)

## Acknowledgments

Third-party dependencies:
*[MonoGame](http://www.monogame.net/)
*[Fluidsynth](http://www.fluidsynth.org/)
*[libmikmod](http://mikmod.sourceforge.net/)
*[libsidplayfp](https://sourceforge.net/projects/sidplay-residfp/) (from my command-line tool [Remuxer](https://github.com/yousernaym/Remuxer)
*[Media Foundation](https://docs.microsoft.com/en-us/windows/win32/medfound/microsoft-media-foundation-sdk)
*[Spatial Media Metadata Injector](https://github.com/google/spatial-media)
*[CefSharp](https://github.com/cefsharp/CefSharp)
*[README-Template.md]([https://gist.github.com/PurpleBooth/109311bb0361f32d87a2#file-readme-template-md)])
*[XNA WinForms](https://github.com/SimonDarksideJ/XNAGameStudio/wiki/WinForms-Series-1-Graphics-Device)

Inspiration: [smalin](https://www.youtube.com/user/smalin)
