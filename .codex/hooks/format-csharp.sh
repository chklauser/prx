#!/usr/bin/env bash

set -euo pipefail

root="$(jj root)"
dotnet tool restore --tool-manifest "$root/dotnet-tools.json" >/dev/null
dotnet csharpier format "$root"
