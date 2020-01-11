; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Visual Music"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Mikesoft"
#define MyAppExeName "VM.exe"
#define MyAppDataDir "{userappdata}\" + MyAppName
#define SongFileType "VisualMusicSong"
#define UserFiles "{userdocs}\" + MyAppName

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{3F797ABA-DFBA-4CB6-8F1F-DFBA6986E064}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=.
OutputBaseFilename={#MyAppName} setup
Compression=lzma
SolidCompression=yes
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
ArchitecturesInstallIn64BitMode = x64
ArchitecturesAllowed = x64
ChangesAssociations=True
UninstallDisplayName={#MyAppName}
ShowTasksTreeLines=False

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
;Name: "associateSidFiles"; Description: "SID files"
;Name: "associateSidfiles\Sid"; Description: ".sid"; Types: full

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; Flags: unchecked
Name: "docsFolder"; Description: "Create subfolder in user's Documents folder"
;Name: "associateVms"; Description: "Associate with .vms files";
;Name: "associateMod"; Description: "Associate with module files";
;Name: "associateMod\mod"; Description: ".mod";
;Name: "associateMod\xm"; Description: ".xm";

[Files]
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "..\VisualMusic\bin\anycpu\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

Source: "..\VisualMusic\bin\anycpu\Release\Media.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\Microsoft.WindowsAPICodePack.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\Microsoft.WindowsAPICodePack.Shell.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\midilib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\MonoGame.Framework.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\SharpDX.Direct3D11.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\SharpDX.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\SharpDX.DXGI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\Content\*"; DestDir: "{app}\Content"; Flags: ignoreversion createallsubdirs recursesubdirs
Source: "..\VisualMusic\bin\anycpu\Release\Remuxer\*.dll"; DestDir: "{app}\Remuxer"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\Remuxer\Remuxer.exe"; DestDir: "{app}\Remuxer"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\Remuxer\roms\*"; DestDir: "{app}\Remuxer\Roms"; Flags: ignoreversion
Source: "Files\Tparty\*"; DestDir: "{#MyAppDataDir}\tparty"; Flags: ignoreversion createallsubdirs recursesubdirs
Source: "..\VisualMusic\minjector.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Files\vc2017 dlls\*"; DestDir: "{app}"; Flags: ignoreversion createallsubdirs recursesubdirs

;Cefsharp
Source: "..\VisualMusic\bin\anycpu\Release\x64\cef.pak"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\CefSharp.BrowserSubprocess.Core.dll"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\CefSharp.BrowserSubprocess.exe"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\CefSharp.Core.dll"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\CefSharp.dll"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\CefSharp.WinForms.dll"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\cef_100_percent.pak"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\cef_200_percent.pak"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\cef_extensions.pak"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\chrome_elf.dll"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\d3dcompiler_47.dll"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\icudtl.dat"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\libcef.dll"; DestDir: "{app}\cefsharp\x64"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\natives_blob.bin"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\snapshot_blob.bin"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\locales\*"; DestDir: "{app}\cefsharp\x64\locales\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\swiftshader\libEGL.dll"; DestDir: "{app}\cefsharp\x64\swiftshader\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\swiftshader\libGLESv2.dll"; DestDir: "{app}\cefsharp\x64\swiftshader\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\v8_context_snapshot.bin"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "..\VisualMusic\bin\anycpu\Release\x64\chrome_elf.dll"; DestDir: "{app}\cefsharp\x64\"; Flags: ignoreversion
Source: "Files\cefsharp\LICENSE.txt"; DestDir: "{app}\cefsharp"; Flags: ignoreversion

;Midmix + fluidsynth
Source: "..\VisualMusic\bin\AnyCPU\Release\MidMix.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\AnyCPU\Release\libfluidsynth-1.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\AnyCPU\Release\glib-2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\AnyCPU\Release\libcharset.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\AnyCPU\Release\libcharset.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\AnyCPU\Release\pcre.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\AnyCPU\Release\libiconv.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\VisualMusic\bin\AnyCPU\Release\libintl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Files\soundfont.sf2"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Dirs]
Name: "{#MyAppDataDir}"; Flags: uninsalwaysuninstall
Name: "{#UserFiles}\Props"; Flags: uninsalwaysuninstall; Tasks: docsFolder
Name: "{#UserFiles}\Projects"; Flags: uninsalwaysuninstall; Tasks: docsFolder
Name: "{#UserFiles}\Videos"; Flags: uninsalwaysuninstall; Tasks: docsFolder

[Registry]
Root: HKCR; SubKey: ".vms"; ValueType: string; ValueName: ""; ValueData: "VisualMusicSong"
Root: HKCR; SubKey: "{#SongFileType}"; ValueType: string; ValueName: ""; ValueData: "Visual Music song"; Flags: uninsdeletekey
Root: HKCR; Subkey: "{#SongFileType}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKCR; SubKey: "{#SongFileType}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""
;Root: HKCU;  Subkey: "Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.vms\UserChoice"; ValueData: "{#SongFileType}";  ValueType: string;  ValueName: "Progid"; Tasks: associateVms
