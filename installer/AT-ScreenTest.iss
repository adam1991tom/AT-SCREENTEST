; Inno Setup script for AT ScreenTest
; Build with: ISCC.exe installer\AT-ScreenTest.iss
; Requires the app to already be published to ..\publish (see README.md)

#define MyAppName "AT ScreenTest"
#define MyAppVersion "1.1.1"
#define MyAppPublisher "Adam Tomlinson"
#define MyAppExeName "ScreenTest.exe"

[Setup]
AppId={{BAAF5B8E-7503-417B-8495-D0C98371B7A1}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\AT ScreenTest
DefaultGroupName=AT ScreenTest
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=AT-ScreenTest-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\AT ScreenTest"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall AT ScreenTest"; Filename: "{uninstallexe}"
Name: "{autodesktop}\AT ScreenTest"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch AT ScreenTest"; Flags: nowait postinstall skipifsilent
