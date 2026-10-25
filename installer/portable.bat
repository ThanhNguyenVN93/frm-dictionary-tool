@echo off
rem ============================================================
rem  Dictionary Tool - portable.bat
rem  "Giai nen" thanh 1 thu muc portable chay lien, KHONG dong
rem  cham he thong (khong shortcut he thong, khong registry).
rem  Cau hinh luu ngay canh exe nen mang di may khac van giu nguyen.
rem ============================================================
setlocal
chcp 65001 >nul

set "APPNAME=Dictionary Tool"
set "SRC=%~dp0"
set "DEST=%SRC%%APPNAME% - Portable"

echo.
echo   Tao ban portable "%APPNAME%"
echo   Den: "%DEST%"
echo.

if not exist "%SRC%%APPNAME%.exe" (
    echo [LOI] Khong thay "%APPNAME%.exe" canh file bat nay.
    pause & exit /b 1
)

if not exist "%DEST%" mkdir "%DEST%"

echo   ^> Copy chuong trinh + du lieu...
copy /Y "%SRC%%APPNAME%.exe" "%DEST%\" >nul
if exist "%SRC%%APPNAME%.exe.config" copy /Y "%SRC%%APPNAME%.exe.config" "%DEST%\" >nul
if exist "%SRC%momo.jpg" copy /Y "%SRC%momo.jpg" "%DEST%\" >nul
if exist "%SRC%Data" robocopy "%SRC%Data" "%DEST%\Data" /E /NFL /NDL /NJH /NJS /NP >nul
if exist "%SRC%x86"  robocopy "%SRC%x86"  "%DEST%\x86"  /E /NFL /NDL /NJH /NJS /NP >nul
if exist "%SRC%x64"  robocopy "%SRC%x64"  "%DEST%\x64"  /E /NFL /NDL /NJH /NJS /NP >nul

echo   ^> Tao shortcut ngay trong thu muc portable...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$ws=New-Object -ComObject WScript.Shell;" ^
  "$s=$ws.CreateShortcut((Join-Path '%DEST%' '%APPNAME%.lnk'));" ^
  "$s.TargetPath=(Join-Path '%DEST%' '%APPNAME%.exe');" ^
  "$s.WorkingDirectory='%DEST%';" ^
  "$s.IconLocation=(Join-Path '%DEST%' '%APPNAME%.exe')+',0';" ^
  "$s.Save()"

echo.
echo   Xong! Mo thu muc "%APPNAME% - Portable" va chay "%APPNAME%.exe".
echo   Co the copy nguyen thu muc nay vao USB de mang di.
echo.
pause
endlocal
