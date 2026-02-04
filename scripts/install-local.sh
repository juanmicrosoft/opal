#!/usr/bin/env bash
set -euo pipefail

# Local Install Script for calor
# Run from repo root: ./scripts/install-local.sh

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

cd "$REPO_ROOT"

echo "Building calor..."
dotnet build src/Calor.Compiler/Calor.Compiler.csproj -c Release

echo "Packing..."
dotnet pack src/Calor.Compiler/Calor.Compiler.csproj -c Release -o ./nupkg

echo "Installing globally..."
dotnet tool install -g --add-source ./nupkg calor 2>/dev/null \
  || dotnet tool update -g --add-source ./nupkg calor

echo "Verifying..."
calor --help

echo "Done! calor installed globally."
