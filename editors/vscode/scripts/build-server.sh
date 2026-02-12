#!/usr/bin/env bash
# Build calor-lsp for a specific platform
# Usage: ./build-server.sh <vscode-target>
# Targets: win32-x64, win32-arm64, darwin-x64, darwin-arm64, linux-x64, linux-arm64

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
VSCODE_DIR="$(dirname "$SCRIPT_DIR")"
REPO_ROOT="$(dirname "$(dirname "$VSCODE_DIR")")"
LSP_PROJECT="$REPO_ROOT/src/Calor.LanguageServer/Calor.LanguageServer.csproj"
SERVER_DIR="$VSCODE_DIR/server"

# Convert VS Code target to .NET RID
get_dotnet_rid() {
    case "$1" in
        "win32-x64")    echo "win-x64" ;;
        "win32-arm64")  echo "win-arm64" ;;
        "darwin-x64")   echo "osx-x64" ;;
        "darwin-arm64") echo "osx-arm64" ;;
        "linux-x64")    echo "linux-x64" ;;
        "linux-arm64")  echo "linux-arm64" ;;
        *)              echo "" ;;
    esac
}

build_for_target() {
    local VSCODE_TARGET=$1
    local DOTNET_RID
    DOTNET_RID=$(get_dotnet_rid "$VSCODE_TARGET")

    if [ -z "$DOTNET_RID" ]; then
        echo "Unknown target: $VSCODE_TARGET"
        echo "Valid targets: win32-x64, win32-arm64, darwin-x64, darwin-arm64, linux-x64, linux-arm64"
        exit 1
    fi

    echo "Building for $VSCODE_TARGET (dotnet RID: $DOTNET_RID)..."

    # Clean previous build
    rm -rf "$SERVER_DIR"
    mkdir -p "$SERVER_DIR"

    dotnet publish "$LSP_PROJECT" \
        -c Release \
        -r "$DOTNET_RID" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:DebugType=none \
        -o "$SERVER_DIR"

    # Remove unnecessary files from server directory
    rm -f "$SERVER_DIR"/*.pdb "$SERVER_DIR"/*.json

    echo "Built server for $VSCODE_TARGET"
}

# If a specific target is provided, build only that one
if [ -n "$1" ]; then
    build_for_target "$1"
else
    echo "Usage: $0 <vscode-target>"
    echo "Targets: win32-x64, win32-arm64, darwin-x64, darwin-arm64, linux-x64, linux-arm64"
    exit 1
fi
