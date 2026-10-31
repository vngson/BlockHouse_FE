param(
    [string]$WebSource = (Join-Path (Split-Path $PSScriptRoot -Parent) 'BLH_website'),
    [string]$BackendSource = (Join-Path (Split-Path $PSScriptRoot -Parent) 'Blockhouse_BE'),
    [string]$BackendOutput = '',
    [string]$IsccPath = '',
    [string]$RuntimeInstaller = '',
    [string]$NodeExe = 'node',
    [string]$NpmCli = '',
    [string]$DotnetExe = 'dotnet',
    [switch]$SkipInstaller
)
$ErrorActionPreference = 'Stop'
function Check-Exit([string]$Step) { if ($LASTEXITCODE -ne 0) { throw "$Step failed ($LASTEXITCODE)" } }
$WebSource = (Resolve-Path -LiteralPath $WebSource).Path
$BackendSource = (Resolve-Path -LiteralPath $BackendSource).Path
if (!$BackendOutput) { $BackendOutput = Join-Path $BackendSource 'installer\artifacts\backend' }
$BackendOutput = [IO.Path]::GetFullPath($BackendOutput)
$backendExecutable = Join-Path $BackendOutput 'BE.exe'
$python = Join-Path $BackendSource 'venv\Scripts\python.exe'
if (!(Test-Path -LiteralPath $python)) { throw 'Create the backend venv and install requirements + pyinstaller first.' }
Push-Location $WebSource
try {
    if ($NpmCli) { & $NodeExe $NpmCli run build } else { & npm.cmd run build }
    Check-Exit 'Frontend build'
    if ($NpmCli) { & $NodeExe $NpmCli test } else { & npm.cmd test }
    Check-Exit 'Frontend tests'
} finally { Pop-Location }
$webDestination = Join-Path $PSScriptRoot 'web'
# Mirror only the generated web folder: stale asset hashes must not survive a rebuild.
if (Test-Path -LiteralPath $webDestination) {
    $resolvedWeb = (Resolve-Path -LiteralPath $webDestination).Path
    if ((Get-Item -LiteralPath $resolvedWeb).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Generated web folder must not be a link.' }
    if ($resolvedWeb -ne (Join-Path $PSScriptRoot 'web')) { throw 'Unexpected generated web path.' }
    Remove-Item -LiteralPath $resolvedWeb -Recurse -Force
}
New-Item -ItemType Directory -Path $webDestination | Out-Null
Copy-Item -Path (Join-Path $WebSource 'dist\*') -Destination $webDestination -Recurse
# WebView2 local mapping has no hosting headers, so ship an equivalent meta CSP.
$index = Join-Path $webDestination 'index.html'
$html = [IO.File]::ReadAllText($index)
$policy = "default-src 'self'; script-src 'self'; style-src 'self'; style-src-attr 'unsafe-inline'; font-src 'self'; img-src 'self' data:; connect-src http://127.0.0.1:5050; object-src 'none'; base-uri 'self'; form-action 'self'"
$html = $html.Replace('<head>', '<head><meta http-equiv="Content-Security-Policy" content="' + $policy + '">')
[IO.File]::WriteAllText($index, $html)
$backendWeb = Join-Path $BackendSource 'web'
if (Test-Path -LiteralPath $backendWeb) {
    $resolvedBackendWeb = (Resolve-Path -LiteralPath $backendWeb).Path
    if ($resolvedBackendWeb -ne (Join-Path $BackendSource 'web') -or ((Get-Item -LiteralPath $resolvedBackendWeb).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Unexpected backend web folder.' }
    Remove-Item -LiteralPath $resolvedBackendWeb -Recurse -Force
}
Copy-Item -LiteralPath $webDestination -Destination $backendWeb -Recurse
Push-Location $BackendSource
try {
    & $python -B -m unittest discover -s tests -p 'test_*.py'
    Check-Exit 'Backend tests'
    & $python -m PyInstaller --noconfirm --log-level WARN --distpath $BackendOutput BE.spec
    Check-Exit 'Backend package'
    Copy-Item -LiteralPath $backendWeb -Destination $BackendOutput -Recurse -Force
} finally { Pop-Location }
# Keep the conventional VS Code/terminal executable in sync with release artifacts.
$legacyDirectory = Join-Path $BackendSource 'dist'
$legacyExecutable = Join-Path $legacyDirectory 'BE.exe'
try {
    New-Item -ItemType Directory -Force -Path $legacyDirectory | Out-Null
    if ($backendExecutable -ne $legacyExecutable -and (!(Test-Path -LiteralPath $legacyExecutable) -or (Get-FileHash -LiteralPath $legacyExecutable).Hash -ne (Get-FileHash -LiteralPath $backendExecutable).Hash)) {
        Copy-Item -LiteralPath $backendExecutable -Destination $legacyExecutable -Force -ErrorAction Stop
    }
    Copy-Item -LiteralPath $backendWeb -Destination $legacyDirectory -Recurse -Force -ErrorAction Stop
    Write-Host "Terminal backend refreshed: $legacyExecutable"
} catch {
    Write-Warning "Cannot refresh $legacyExecutable. Stop the running dist BE.exe before rebuilding; the new backend is at $backendExecutable. $($_.Exception.Message)"
}
$payload = Join-Path $PSScriptRoot 'publish\win-x64'
# Remove only previously generated payload after checking its resolved location.
if (Test-Path -LiteralPath $payload) {
    $resolvedPayload = (Resolve-Path -LiteralPath $payload).Path
    if ((Get-Item -LiteralPath $resolvedPayload).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Publish folder must not be a link.' }
    if ($resolvedPayload -ne (Join-Path $PSScriptRoot 'publish\win-x64')) { throw 'Unexpected publish path.' }
    Remove-Item -LiteralPath $resolvedPayload -Recurse -Force
}
& $DotnetExe publish (Join-Path $PSScriptRoot 'BlockHouse.csproj') -c Release -p:BlockHouseRelease=true "-p:BackendExecutable=$backendExecutable" -r win-x64 --self-contained true -o $payload --nologo -v minimal
Check-Exit 'WPF publish'
Copy-Item -LiteralPath $backendExecutable -Destination $payload
if (Get-ChildItem -LiteralPath $payload -Recurse -File | Where-Object { $_.Extension -in '.db','.sqlite3','.bak' }) { throw 'Database found in payload. Refusing to package.' }
if ($SkipInstaller) { Write-Host "Desktop payload: $payload"; return }
if (!$IsccPath) {
    $compiler = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($compiler) { $IsccPath = $compiler.Source }
    else { $IsccPath = Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe' }
}
if (!(Test-Path -LiteralPath $IsccPath)) { throw 'Pass -IsccPath with the Inno Setup 6.7+ compiler path.' }
if (!$RuntimeInstaller) { $RuntimeInstaller = Join-Path $BackendSource 'installer\vendor\MicrosoftEdgeWebView2RuntimeInstallerX64.exe' }
if (!(Test-Path -LiteralPath $RuntimeInstaller)) { throw 'Pass -RuntimeInstaller with the official Microsoft offline WebView2 x64 installer.' }
$signature = Get-AuthenticodeSignature -LiteralPath $RuntimeInstaller
if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation') { throw 'WebView2 installer signature is not valid Microsoft signature.' }
& $IsccPath /Q "/DPayloadDir=$payload" "/DRuntimeInstaller=$RuntimeInstaller" (Join-Path $BackendSource 'installer\BlockHouse.iss')
Check-Exit 'Installer compile'
Write-Host "Setup: $(Join-Path $BackendSource 'installer\output\BlockHouse-Setup-2.0.16-x64.exe')"
