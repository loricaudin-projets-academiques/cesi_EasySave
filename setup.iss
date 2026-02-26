#define MyAppName "EasySave"
#define MyAppPublisher "CESI"
#define MyAppURL "https://github.com/loricaudin-projets-academiques/cesi_EasySave"
#define MyAppExeName "EasySave.GUI.exe"

#ifndef MyAppVersion
  #define MyAppVersion "3.0.0"
#endif

[Setup]
AppId={{B8F3A1D2-7E4C-4F5A-9D6B-1C2E3F4A5B6C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppPublisher}\{#MyAppName}
DisableDirPage=yes
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=publish
OutputBaseFilename=EasySave-Setup-{#MyAppVersion}
SetupIconFile=EasySave.GUI\icon.ico
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
ChangesEnvironment=yes
PrivilegesRequired=admin

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\EasySave.GUI\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "publish\EasySave.CLI\*"; DestDir: "{app}\CLI"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "publish\CryptoSoft\*"; DestDir: "{app}\CryptoSoft"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "publish\EasyLog.Server\*"; DestDir: "{app}\EasyLog.Server"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\{#MyAppPublisher}"
Type: filesandordirs; Name: "{app}"

[Registry]
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"; ValueType: expandsz; ValueName: "Path"; ValueData: "{olddata};{app}\CLI"; Check: NeedsAddPath(ExpandConstant('{app}\CLI'))

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{cmd}"; Parameters: "/C taskkill /im {#MyAppExeName} /f /t"; Flags: runhidden

[Code]
function NeedsAddPath(Param: string): Boolean;
var
  OrigPath: string;
begin
  if not RegQueryStringValue(HKLM, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', OrigPath) then
  begin
    Result := True;
    exit;
  end;
  Result := Pos(';' + Uppercase(Param) + ';', ';' + Uppercase(OrigPath) + ';') = 0;
end;

procedure DeinitializeUninstall();
begin
  DelTree(ExpandConstant('{userappdata}\{#MyAppPublisher}'), True, True, True);
  DelTree(ExpandConstant('{commonappdata}\{#MyAppPublisher}'), True, True, True);
end;
