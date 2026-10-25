; ============================================================
;  Dictionary Tool - Inno Setup script (TUY CHON)
;  Tao 1 file setup.exe chuyen nghiep (co Add/Remove Programs + uninstall).
;
;  Cach dung:
;    1. Chay  installer\build-release.ps1  de co thu muc
;       installer\release\Dictionary Tool\
;    2. Cai Inno Setup (https://jrsoftware.org/isinfo.php)
;    3. Mo file .iss nay bang Inno Setup roi bam Build (Ctrl+F9),
;       hoac:  "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\DictionaryTool.iss
;  Ket qua:  installer\Output\DictionaryTool-Setup.exe
; ============================================================

#define AppName     "Dictionary Tool"
#define AppVersion  "1.0.0"
#define AppExe      "Dictionary Tool.exe"
#define Publisher   "ThanhCode93"
#define SrcDir      "release\Dictionary Tool"

[Setup]
AppId={{B1A2C3D4-3333-4A2B-9C3D-4E5F6A7B8C9D}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#Publisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#AppExe}
OutputDir=Output
OutputBaseFilename=DictionaryTool-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
; Cai cho rieng user hien tai -> khong can quyen Admin
PrivilegesRequired=lowest

[Languages]
Name: "vi"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Tao shortcut ngoai Desktop"; GroupDescription: "Shortcut:"

[Files]
Source: "{#SrcDir}\{#AppExe}";        DestDir: "{app}"; Flags: ignoreversion
Source: "{#SrcDir}\*.config";         DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "{#SrcDir}\momo.jpg";         DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "{#SrcDir}\Data\*";           DestDir: "{app}\Data"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#SrcDir}\x86\*";            DestDir: "{app}\x86";  Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "{#SrcDir}\x64\*";            DestDir: "{app}\x64";  Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist

[Icons]
Name: "{group}\{#AppName}";        Filename: "{app}\{#AppExe}"
Name: "{group}\Go cai dat {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}";  Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Chay {#AppName} ngay bay gio"; Flags: nowait postinstall skipifsilent
