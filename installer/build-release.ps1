<#
    build-release.ps1 — Dựng bản phát hành "Dictionary Tool" thành 1 thư mục gọn
    để phát cho người dùng (kèm 2 file .bat: installer.bat / portable.bat).

    Cách chạy (PowerShell, từ thư mục gốc repo hoặc bất kỳ đâu):
        pwsh installer\build-release.ps1
        # hoặc Windows PowerShell:
        powershell -ExecutionPolicy Bypass -File installer\build-release.ps1

    Kết quả: installer\release\Dictionary Tool\  (copy nguyên thư mục này đi phát).
#>
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repo    = Split-Path -Parent $PSScriptRoot          # thư mục gốc repo
$proj    = Join-Path $repo "DictionaryTool.UI\DictionaryTool.UI.csproj"
$binDir  = Join-Path $repo "DictionaryTool.UI\bin\$Configuration"
$stage   = Join-Path $PSScriptRoot "release\Dictionary Tool"
$exeName = "Dictionary Tool.exe"

function Find-MSBuild {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $p = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe |
             Select-Object -First 1
        if ($p) { return $p }
    }
    $fallback = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($fallback) { return $fallback.Source }
    throw "Không tìm thấy MSBuild. Hãy cài Visual Studio hoặc Build Tools."
}

$msbuild = Find-MSBuild
Write-Host "MSBuild: $msbuild"

# Costura (Fody) chỉ dệt DLL khi target restore & build là 2 lần gọi riêng biệt:
# restore trước, rồi Rebuild sau, nếu không exe sẽ không gộp được DLL.
Write-Host "==> Restore..."
& $msbuild $proj /t:Restore /p:Configuration=$Configuration /v:m /nologo
Write-Host "==> Rebuild ($Configuration)..."
& $msbuild $proj /t:Rebuild /p:Configuration=$Configuration /v:m /nologo

$exe = Join-Path $binDir $exeName
if (-not (Test-Path $exe)) { throw "Không thấy $exe sau khi build." }

# Dọn & dựng lại thư mục staging
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stage | Out-Null

Write-Host "==> Copy file phát hành..."
# exe + config
Copy-Item $exe $stage -Force
$cfg = Join-Path $binDir "$exeName.config"
if (Test-Path $cfg) { Copy-Item $cfg $stage -Force }

# Dữ liệu từ điển offline + native SQLite (bắt buộc đi kèm exe)
foreach ($sub in @("Data", "x86", "x64")) {
    $src = Join-Path $binDir $sub
    if (Test-Path $src) {
        Copy-Item $src (Join-Path $stage $sub) -Recurse -Force
    }
}
# ảnh donate (nếu có)
$momo = Join-Path $binDir "momo.jpg"
if (Test-Path $momo) { Copy-Item $momo $stage -Force }

# 2 file .bat + README đi kèm
Copy-Item (Join-Path $PSScriptRoot "installer.bat")        $stage -Force
Copy-Item (Join-Path $PSScriptRoot "portable.bat")         $stage -Force
Copy-Item (Join-Path $PSScriptRoot "README-install.txt")   $stage -Force

# Không phát hành .pdb / .xml / DLL managed rời (Costura đã nhúng hết vào exe)
Get-ChildItem $stage -Recurse -Include *.pdb, *.xml | Remove-Item -Force -ErrorAction SilentlyContinue

$size = [math]::Round(((Get-ChildItem $stage -Recurse | Measure-Object Length -Sum).Sum / 1MB), 1)
Write-Host ""
Write-Host "Xong! Thư mục phát hành: $stage  (~$size MB)" -ForegroundColor Green
Write-Host "Copy nguyên thư mục này đi phát. Người dùng chạy installer.bat (cài) hoặc portable.bat (bản chạy liền)."
