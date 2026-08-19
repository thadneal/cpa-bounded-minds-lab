#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"
dotnet build Cpa.BoundedMindsLab.sln -c Release
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- --self-test
