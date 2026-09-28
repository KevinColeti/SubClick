#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\artifacts\publish"
#endif

[Setup]
AppId={{CBDA8964-A820-4A6A-8D62-27BC7B29C45B}
AppName=SubClick
AppVersion={#AppVersion}
AppPublisher=SubClick contributors
AppPublisherURL=https://github.com/KevinColeti/SubClick
AppSupportURL=https://github.com/KevinColeti/SubClick/issues
AppUpdatesURL=https://github.com/KevinColeti/SubClick/releases
DefaultDirName={localappdata}\Programs\SubClick
DefaultGroupName=SubClick
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.22000
OutputDir=..\artifacts\installer
OutputBaseFilename=SubClick-{#AppVersion}-win-x64-setup
SetupIconFile=..\assets\SubClick.ico
UninstallDisplayIcon={app}\SubClick.exe
LicenseFile=..\LICENSE
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ChangesAssociations=yes
CloseApplications=yes
RestartApplications=no
SetupMutex=SubClick.Setup
AppMutex=SubClick.Running

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[CustomMessages]
english.ContextCommand=Find subtitles — SubClick
brazilianportuguese.ContextCommand=Buscar legendas — SubClick
english.ContextTask=Add SubClick to the video context menu
brazilianportuguese.ContextTask=Adicionar SubClick ao menu de contexto de vídeos
english.Launch=Open SubClick
brazilianportuguese.Launch=Abrir SubClick

[Tasks]
Name: "contextmenu"; Description: "{cm:ContextTask}"; Flags: checkedonce

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb"

[Icons]
Name: "{userprograms}\SubClick"; Filename: "{app}\SubClick.exe"; WorkingDir: "{app}"

[Registry]
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mkv\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mkv\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mkv\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mkv\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mp4\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mp4\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mp4\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mp4\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.avi\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.avi\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.avi\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.avi\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mov\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mov\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mov\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mov\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.wmv\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.wmv\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.wmv\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.wmv\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m4v\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m4v\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m4v\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m4v\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.webm\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.webm\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.webm\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.webm\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mpg\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mpg\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mpg\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mpg\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mpeg\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mpeg\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mpeg\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mpeg\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.ts\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.ts\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.ts\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.ts\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m2ts\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m2ts\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m2ts\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m2ts\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mts\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mts\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mts\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mts\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.vob\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.vob\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.vob\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.vob\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.ogv\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.ogv\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.ogv\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.ogv\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.flv\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.flv\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.flv\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.flv\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.divx\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.divx\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.divx\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.divx\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.3gp\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.3gp\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.3gp\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.3gp\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.3g2\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.3g2\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.3g2\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.3g2\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.f4v\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.f4v\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.f4v\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.f4v\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.asf\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.asf\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.asf\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.asf\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m2v\shell\SubClick"; ValueType: string; ValueName: "MUIVerb"; ValueData: "{cm:ContextCommand}"; Tasks: contextmenu; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m2v\shell\SubClick"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\SubClick.exe"",0"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m2v\shell\SubClick"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.m2v\shell\SubClick\command"; ValueType: string; ValueData: """{app}\SubClick.exe"" ""%1"""; Tasks: contextmenu

[Run]
Filename: "{app}\SubClick.exe"; Description: "{cm:Launch}"; Flags: nowait postinstall skipifsilent

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssPostInstall) and not WizardIsTaskSelected('contextmenu') then
  begin
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.mkv\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.mp4\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.avi\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.mov\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.wmv\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.m4v\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.webm\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.mpg\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.mpeg\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.ts\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.m2ts\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.mts\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.vob\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.ogv\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.flv\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.divx\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.3gp\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.3g2\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.f4v\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.asf\shell\SubClick');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\SystemFileAssociations\.m2v\shell\SubClick');
  end;
end;
