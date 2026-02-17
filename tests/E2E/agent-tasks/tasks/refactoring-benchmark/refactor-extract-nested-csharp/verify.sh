#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Extract.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Extract.cs not found"; exit 1; }

# Check Square method exists
if ! grep -qE '(private|public)\s+int\s+Square' "$CS_FILE"; then
    echo "FAIL: Square method not found"
    exit 1
fi

# Check ComputeNested calls Square
if ! grep -A10 'public int ComputeNested' "$CS_FILE" | grep -q 'Square('; then
    echo "FAIL: ComputeNested should call Square"
    exit 1
fi

echo "PASS: Square extracted, ComputeNested calls it"
exit 0
