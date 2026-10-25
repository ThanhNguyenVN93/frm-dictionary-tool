@echo off
rem ============================================================
rem  Dictionary Tool - installer.bat
rem  Cai dat vao %LOCALAPPDATA%\Dictionary Tool (khong can quyen Admin),
rem  tao shortcut Desktop + Start Menu, va sinh uninstall.bat.
rem ============================================================
setlocal EnableDelayedExpansion
chcp 65001 >nul

set "APPNAME=Dictionary Tool"
set "SRC=%~dp0"
set "DEST=%LOCALAPPDATA%\%APPNAME%"
set "EXE=%DEST%\%APPNAME%.exe"

echo.
echo   Cai dat "%APPNAME%"
echo   Tu : "%SRC%"
echo   Den: "%DEST%"
echo.

if not exist "%SRC%%APPNAME%.exe" (
    echo [LOI] Khong thay "%APPNAME%.exe" canh file bat nay.
    echo       Hay chay installer.bat tu trong thu muc phat hanh.
    pause & exit /b 1
)

if not exist "%DEST%" mkdir "%DEST%"

echo   ^> Copy chuong trinh...
copy /Y "%SRC%%APPNAME%.exe" "%DEST%\" >nul
if exist "%SRC%%APPNAME%.exe.config" copy /Y "%SRC%%APPNAME%.exe.config" "%DEST%\" >nul
if exist "%SRC%momo.jpg" copy /Y "%SRC%momo.jpg" "%DEST%\" >nul

echo   ^> Copy du lieu tu dien + SQLite native...
if exist "%SRC%Data" robocopy "%SRC%Data" "%DEST%\Data" /E /NFL /NDL /NJH /NJS /NP >nul
if exist "%SRC%x86"  robocopy "%SRC%x86"  "%DEST%\x86"  /E /NFL /NDL /NJH /NJS /NP >nul
if exist "%SRC%x64"  robocopy "%SRC%x64"  "%DEST%\x64"  /E /NFL /NDL /NJH /NJS /NP >nul

echo   ^> Tao shortcut (Desktop + Start Menu)...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$ws=New-Object -ComObject WScript.Shell;" ^
  "foreach($dir in @([Environment]::GetFolderPath('Desktop'),[Environment]::GetFolderPath('Programs'))){" ^
  "  $lnk=Join-Path $dir '%APPNAME%.lnk';" ^
  "  $s=$ws.CreateShortcut($lnk);" ^
  "  $s.TargetPath='%EXE%';" ^
  "  $s.WorkingDirectory='%DEST%';" ^
  "  $s.IconLocation='%EXE%,0';" ^
  "  $s.Description='Tra tu dien Anh - Viet - Trung';" ^
  "  $s.Save() }"

echo   ^> Tao uninstall.bat...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$dest='%DEST%';" ^
  "$lines=@('@echo off','chcp 65001 >nul','echo Go cai dat %APPNAME% ...'," ^
  "  'del \"%%USERPROFILE%%\Desktop\%APPNAME%.lnk\" 2>nul'," ^
  "  'del \"%%APPDATA%%\Microsoft\Windows\Start Menu\Programs\%APPNAME%.lnk\" 2>nul'," ^
  "  'cd /d \"%%TEMP%%\"'," ^
  "  'rmdir /S /Q \"'+$dest+'\"'," ^
  "  'echo Da go cai dat.','pause');" ^
  "Set-Content -Path (Join-Path $dest 'uninstall.bat') -Value $lines -Encoding Default"

echo.
echo   Hoan tat! Mo "%APPNAME%" tu Desktop hoac Start Menu.
echo   De go cai dat: chay "%DEST%\uninstall.bat".
echo.
pause
endlocal
