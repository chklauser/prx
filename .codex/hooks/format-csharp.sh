#!/usr/bin/env bash

set -euo pipefail

root="$(git rev-parse --show-toplevel)"
dotnet tool restore --tool-manifest "$root/dotnet-tools.json" >/dev/null
dotnet csharpier format "$root"
