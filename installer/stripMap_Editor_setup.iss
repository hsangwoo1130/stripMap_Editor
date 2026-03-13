#define AppName "StripMap Editor"
#define AppVersion "1.3.0"
#define AppExeName "stripMap_Editor.exe"
#define SourceDir "..\bin\x86\Release"
#define OutputDir "."

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#AppVersion}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
OutputDir={#OutputDir}
OutputBaseFilename=StripMapEditor_v{#AppVersion}_Setup
Compression=lzma2
SolidCompression=yes
DisableDirPage=no
DisableProgramGroupPage=yes
; 바탕화면 단축아이콘 비활성화
DisableStartupPrompt=no
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=6.1sp1

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[Files]
; 실행 파일
Source: "{#SourceDir}\{#AppExeName}";          DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\{#AppExeName}.config";   DestDir: "{app}"; Flags: ignoreversion

; 설정 파일 (이미 존재하면 덮어쓰지 않음 - 운영사 설정 보존)
Source: "{#SourceDir}\config.ini";             DestDir: "{app}"; Flags: onlyifdoesntexist

; 의존성 DLL
Source: "{#SourceDir}\Konscious.Security.Cryptography.Argon2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\Konscious.Security.Cryptography.Blake2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\Serilog.dll";                                 DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\Serilog.Sinks.File.dll";                      DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\System.Buffers.dll";                          DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\System.Configuration.ConfigurationManager.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\System.Data.SqlClient.dll";                   DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\System.Diagnostics.DiagnosticSource.dll";     DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\System.Memory.dll";                           DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\System.Numerics.Vectors.dll";                 DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\System.Runtime.CompilerServices.Unsafe.dll";  DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\System.Threading.Channels.dll";               DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\System.Threading.Tasks.Extensions.dll";       DestDir: "{app}"; Flags: ignoreversion

; TIBCO Rendezvous
Source: "{#SourceDir}\TIBCO.Rendezvous.dll";       DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\TIBCO.Rendezvous.netmodule"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; 시작 메뉴 항목만 생성 (바탕화면 단축아이콘 없음)
Name: "{group}\{#AppName}";        Filename: "{app}\{#AppExeName}"
Name: "{group}\설정 파일 열기";    Filename: "{app}\config.ini"
Name: "{group}\{#AppName} 제거";   Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "설치 완료 후 {#AppName} 실행"; Flags: nowait postinstall skipifsilent
