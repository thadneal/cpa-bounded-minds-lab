$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    Get-Content docs/FROZEN_PROTOCOL_SHA256.txt | ForEach-Object {
        if ($_ -match '^([0-9a-f]{64})  (.+)$') {
            $expected = $matches[1]
            $path = $matches[2]
            $actual = (Get-FileHash -Algorithm SHA256 $path).Hash.ToLowerInvariant()
            if ($actual -ne $expected) {
                throw "Frozen protocol source changed: $path"
            }
        }
    }
    dotnet build Cpa.BoundedMindsLab.sln -c Release
    dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- --self-test
}
finally {
    Pop-Location
}
