$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $repoRoot

dotnet build .\apps\MiniLang.Studio\MiniLang.Studio.csproj -p:Platform=x64
