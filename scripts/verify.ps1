$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet build Cpa.BoundedMindsLab.sln -c Release
    dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- --self-test
}
finally {
    Pop-Location
}
