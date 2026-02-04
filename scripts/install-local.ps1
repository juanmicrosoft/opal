# Local Install Script for calor (Windows)
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

Write-Host "Building calor..."
dotnet build src/Calor.Compiler/Calor.Compiler.csproj -c Release

Write-Host "Packing..."
dotnet pack src/Calor.Compiler/Calor.Compiler.csproj -c Release -o ./nupkg

Write-Host "Installing globally..."
try {
    dotnet tool install -g --add-source ./nupkg calor
} catch {
    dotnet tool update -g --add-source ./nupkg calor
}

Write-Host "Verifying..."
calor --help

Write-Host "Done! calor installed globally."
