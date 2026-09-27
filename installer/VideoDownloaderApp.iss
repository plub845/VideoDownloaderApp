; ============================================================================
; VideoDownloaderApp - Inno Setup 7 Installer Script
; Version: 1.1.0
; Theme: Warm Cream / Soft Yellow (สีเนื้อนวล)
; Requires: Inno Setup 7 at C:\Program Files\Inno Setup 7
; ============================================================================

#define AppName "Video Downloader Pro"
#define AppVersion "1.1.0"
#define AppPublisher "plub845"
#define AppURL "https://github.com/plub845/VideoDownloaderApp"
#define AppExeName "VideoDownloaderApp.exe"
#define AppIconName "VDapp.icon.ico"

[Setup]
; --- App Identity ---
AppId={{B7E2F3A4-5D6C-7E8F-9A0B-1C2D3E4F5A6B}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}/issues
AppUpdatesURL={#AppURL}/releases

; --- Install Location ---
DefaultDirName={code:GetDefaultDirName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
DisableProgramGroupPage=yes

; --- Output ---
OutputDir=..\publish\installer
OutputBaseFilename=VideoDownloaderApp_Setup_v{#AppVersion}
SetupIconFile=..\VideoDownloaderApp\VDapp.icon.ico

; --- Appearance ---
WizardStyle=modern
WizardSizePercent=110
WizardSmallImageFile=wizard_small.bmp
WizardImageFile=wizard_image.bmp

; --- Compression ---
Compression=lzma2/ultra64
SolidCompression=yes

; --- Permissions ---
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; --- Architecture ---
ArchitecturesInstallIn64BitMode=x64compatible

; --- Uninstall ---
UninstallDisplayIcon={app}\{#AppIconName}
UninstallDisplayName={#AppName}
UninstallFilesDir={app}\uninstall

; --- Version Info ---
VersionInfoVersion={#AppVersion}.0
VersionInfoCompany={#AppPublisher}
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppVersion}
VersionInfoDescription=Video Downloader Pro - โปรแกรมดาวน์โหลดวิดีโอและเสียง

; --- Misc ---
ShowLanguageDialog=auto
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
; --- Customize wizard messages for warm, friendly tone ---
WelcomeLabel1=Welcome to {#AppName} Setup
WelcomeLabel2=This will install {#AppName} v{#AppVersion} on your computer.%n%nIt is recommended that you close all other applications before continuing.
FinishedHeadingLabel=Installation Complete!
FinishedLabel={#AppName} has been successfully installed on your computer.%nClick Finish to exit Setup.

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"
Name: "quicklaunchicon"; Description: "Create a Quick Launch shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
; --- Main App Files (from dotnet publish output) ---
Source: "..\publish\windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; --- App Icon ---
Source: "..\VideoDownloaderApp\VDapp.icon.ico"; DestDir: "{app}"; Flags: ignoreversion

; --- Engine Files (yt-dlp + ffmpeg) -> install to AppData ---
Source: "..\VideoDownloaderApp\main\yt-dlp.exe"; DestDir: "{localappdata}\VideoDownloaderApp"; Flags: ignoreversion
Source: "..\VideoDownloaderApp\main\ffmpeg.exe"; DestDir: "{localappdata}\VideoDownloaderApp"; Flags: ignoreversion

[Icons]
; --- Start Menu ---
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppIconName}"; Comment: "เปิด Video Downloader Pro"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"; IconFilename: "{app}\{#AppIconName}"

; --- Desktop ---
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppIconName}"; Tasks: desktopicon

; --- Quick Launch ---
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent unchecked

[UninstallDelete]
; --- Clean up engine files on uninstall ---
Type: files; Name: "{localappdata}\VideoDownloaderApp\yt-dlp.exe"
Type: files; Name: "{localappdata}\VideoDownloaderApp\ffmpeg.exe"
Type: dirifempty; Name: "{localappdata}\VideoDownloaderApp"

; ============================================================================
; Pascal Script - Color Theme + .NET 8 Check
; ============================================================================
[Code]

// ---- External function for downloading .NET Runtime ----
function URLDownloadToFile(pCaller: Integer; szURL, szFileName: String;
  dwReserved: Integer; lpfnCB: Integer): Integer;
  external 'URLDownloadToFileW@urlmon.dll stdcall';

// ---- Default install directory logic ----
function GetDefaultDirName(Param: String): String;
begin
  if DirExists('D:\Programs') or DirExists('D:\') then
    Result := 'D:\Programs\VideoDownloaderApp'
  else
    Result := ExpandConstant('{autopf}\VideoDownloaderApp');
end;

// ---- Check if .NET 8 Desktop Runtime is installed ----
function IsDotNet8DesktopInstalled: Boolean;
var
  FindRec: TFindRec;
  DotNetPath: String;
begin
  Result := False;
  DotNetPath := ExpandConstant('{commonpf}\dotnet\shared\Microsoft.WindowsDesktop.App');
  if DirExists(DotNetPath) then
  begin
    if FindFirst(DotNetPath + '\8.*', FindRec) then
    begin
      repeat
        if FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY <> 0 then
        begin
          Result := True;
          Break;
        end;
      until not FindNext(FindRec);
      FindClose(FindRec);
    end;
  end;
  
  // Also check 32-bit path as fallback
  if not Result then
  begin
    DotNetPath := ExpandConstant('{commonpf32}\dotnet\shared\Microsoft.WindowsDesktop.App');
    if DirExists(DotNetPath) then
    begin
      if FindFirst(DotNetPath + '\8.*', FindRec) then
      begin
        repeat
          if FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY <> 0 then
          begin
            Result := True;
            Break;
          end;
        until not FindNext(FindRec);
        FindClose(FindRec);
      end;
    end;
  end;
end;

// ---- Download and install .NET 8 Desktop Runtime ----
procedure DownloadAndInstallDotNet8;
var
  DownloadUrl: String;
  TempFile: String;
  ResultCode: Integer;
  ErrorCode: Integer;
begin
  DownloadUrl := 'https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe';
  TempFile := ExpandConstant('{tmp}\dotnet8-desktop-runtime.exe');

  WizardForm.StatusLabel.Caption := 'Downloading .NET 8 Desktop Runtime... Please wait.';
  WizardForm.StatusLabel.Update;

  ErrorCode := URLDownloadToFile(0, DownloadUrl, TempFile, 0, 0);

  if ErrorCode = 0 then
  begin
    WizardForm.StatusLabel.Caption := 'Installing .NET 8 Desktop Runtime...';
    WizardForm.StatusLabel.Update;

    if Exec(TempFile, '/install /quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
    begin
      if ResultCode = 0 then
        Log('.NET 8 Desktop Runtime installed successfully.')
      else if ResultCode = 1641 then
        Log('.NET 8 Desktop Runtime installed. Restart required.')
      else if ResultCode = 3010 then
        Log('.NET 8 Desktop Runtime installed. Soft restart required.')
      else
        Log('.NET 8 Desktop Runtime installer returned code: ' + IntToStr(ResultCode));
    end
    else
    begin
      MsgBox('Could not run .NET 8 installer.' + #13#10 +
             'Please install .NET 8 Desktop Runtime manually from:' + #13#10 +
             'https://dotnet.microsoft.com/download/dotnet/8.0',
             mbError, MB_OK);
    end;
  end
  else
  begin
    MsgBox('Could not download .NET 8 Desktop Runtime.' + #13#10 +
           'Error code: ' + IntToStr(ErrorCode) + #13#10#13#10 +
           'Please download and install it manually from:' + #13#10 +
           'https://dotnet.microsoft.com/download/dotnet/8.0',
           mbError, MB_OK);
  end;
end;

// ---- PrepareToInstall: Check .NET 8 before installing ----
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  NeedsRestart := False;

  if not IsDotNet8DesktopInstalled then
  begin
    Log('.NET 8 Desktop Runtime not found. Starting download...');
    DownloadAndInstallDotNet8;

    // Re-check after installation attempt
    if not IsDotNet8DesktopInstalled then
    begin
      Result := '.NET 8 Desktop Runtime could not be installed automatically.' + #13#10 +
                'Please install it manually from https://dotnet.microsoft.com/download/dotnet/8.0' + #13#10 +
                'then run this setup again.';
    end;
  end
  else
  begin
    Log('.NET 8 Desktop Runtime is already installed.');
  end;
end;

// ============================================================================
// Warm Cream Theme Customization (สีเนื้อนวล)
// ============================================================================
// Color format: TColor = $00BBGGRR
// Warm Cream  #FFF8F0 = RGB(255,248,240) = $F0F8FF
// Light Gold  #F8E8D0 = RGB(248,232,208) = $D0E8F8
// Soft Gold   #F0DDB8 = RGB(240,221,184) = $B8DDF0
// Dark Brown  #5A4838 = RGB(90,72,56)    = $38485A
// Medium Brown#7A6A58 = RGB(122,106,88)  = $586A7A
// ============================================================================

procedure InitializeWizard;
var
  WarmCream: TColor;
  LightGold: TColor;
  SoftGold: TColor;
  DarkBrown: TColor;
  MediumBrown: TColor;
begin
  // Define warm cream color palette
  WarmCream := $F0F8FF;     // #FFF8F0 - main background
  LightGold := $D0E8F8;     // #F8E8D0 - header panel
  SoftGold := $B8DDF0;      // #F0DDB8 - accents
  DarkBrown := $38485A;     // #5A4838 - primary text
  MediumBrown := $586A7A;   // #7A6A58 - secondary text

  // Main form background
  WizardForm.Color := WarmCream;

  // Header panel (MainPanel)
  WizardForm.MainPanel.Color := LightGold;
  WizardForm.PageNameLabel.Font.Color := DarkBrown;
  WizardForm.PageNameLabel.Font.Size := 11;
  WizardForm.PageNameLabel.Font.Style := [fsBold];
  WizardForm.PageDescriptionLabel.Font.Color := MediumBrown;
  WizardForm.PageDescriptionLabel.Font.Size := 9;

  // Inner content area
  WizardForm.InnerPage.Color := WarmCream;

  // Welcome page text styling
  WizardForm.WelcomeLabel1.Font.Color := DarkBrown;
  WizardForm.WelcomeLabel1.Font.Size := 14;
  WizardForm.WelcomeLabel2.Font.Color := MediumBrown;
  WizardForm.WelcomeLabel2.Font.Size := 10;

  // Finished page text styling
  WizardForm.FinishedHeadingLabel.Font.Color := DarkBrown;
  WizardForm.FinishedHeadingLabel.Font.Size := 14;
  WizardForm.FinishedLabel.Font.Color := MediumBrown;
  WizardForm.FinishedLabel.Font.Size := 10;

  // Directory page
  WizardForm.DirEdit.Color := $E8F0F8;  // Slightly darker cream for input
  WizardForm.DirEdit.Font.Color := DarkBrown;

  // Task list styling
  WizardForm.TasksList.Color := WarmCream;
  WizardForm.TasksList.Font.Color := DarkBrown;

  // Component list
  WizardForm.ComponentsList.Color := WarmCream;
  WizardForm.ComponentsList.Font.Color := DarkBrown;

  // Ready memo
  WizardForm.ReadyMemo.Color := $E8F0F8;
  WizardForm.ReadyMemo.Font.Color := DarkBrown;
  WizardForm.ReadyMemo.Font.Size := 9;

  // Status labels
  WizardForm.StatusLabel.Font.Color := DarkBrown;
  WizardForm.FilenameLabel.Font.Color := MediumBrown;

  // Bevel line under header - make it blend with theme
  WizardForm.Bevel1.Visible := False;

  Log('Warm Cream theme applied successfully.');
end;

// ---- Show friendly message after setup completes ----
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    Log('Installation completed. App installed to: ' + ExpandConstant('{app}'));
    Log('Engine files installed to: ' + ExpandConstant('{localappdata}\VideoDownloaderApp'));
  end;
end;
