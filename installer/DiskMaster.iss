; Compiler via scripts/Build-Release.ps1 -WithInstaller
#ifndef AppVersion
  #error AppVersion doit être fourni.
#endif
#ifndef PublishDir
  #error PublishDir doit être fourni.
#endif
#ifndef OutputDir
  #error OutputDir doit être fourni.
#endif

[Setup]
AppId={{70DE9D7B-87C6-4CBD-9DC6-F09F76917E5C}
AppName=DiskMaster
AppVersion={#AppVersion}
AppPublisher=Gamehooper Studio
DefaultDirName={localappdata}\Programs\DiskMaster
DefaultGroupName=DiskMaster
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
OutputDir={#OutputDir}
OutputBaseFilename=DiskMaster-{#AppVersion}-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\DiskMaster.exe
LicenseFile=..\LICENSE
CloseApplications=yes
RestartApplications=no
SetupLogging=yes
#ifdef SigningEnabled
SignTool=diskmasterSign
SignedUninstaller=yes
#endif

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\DiskMaster"; Filename: "{app}\DiskMaster.exe"
Name: "{group}\Guide d’utilisation"; Filename: "{app}\docs\USER-GUIDE.md"

[Run]
Filename: "{app}\DiskMaster.exe"; Description: "Ouvrir DiskMaster"; Flags: nowait postinstall skipifsilent unchecked

; Les données utilisateur restent dans LocalAppData\DiskMaster.
; Aucun UninstallDelete récursif : la désinstallation n’efface pas les diagnostics.
