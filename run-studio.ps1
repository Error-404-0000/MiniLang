$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $repoRoot

Get-Process MiniLang.Studio -ErrorAction SilentlyContinue | Stop-Process -Force
dotnet build .\apps\MiniLang.Studio\MiniLang.Studio.csproj -p:Platform=x64

$exePath = Join-Path $repoRoot "apps\MiniLang.Studio\bin\x64\Debug\net9.0-windows10.0.19041.0\win-x64\MiniLang.Studio.exe"
Start-Process -FilePath $exePath | Out-Null
Write-Host "MiniLang Studio launched from $exePath"
