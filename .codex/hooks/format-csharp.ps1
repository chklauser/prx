$ErrorActionPreference = "Stop"

$root = git rev-parse --show-toplevel
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet tool restore --tool-manifest (Join-Path $root "dotnet-tools.json") | Out-Null
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet csharpier format $root
exit $LASTEXITCODE
