#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Rename.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Rename.cs not found"; exit 1; }

# Check parameter renamed from 'val' to 'value'
if grep -qE 'Calculate\s*\(\s*int\s+val\s*\)' "$CS_FILE"; then
    echo "FAIL: Parameter still named 'val'"
    exit 1
fi

if ! grep -qE 'Calculate\s*\(\s*int\s+value\s*\)' "$CS_FILE"; then
    echo "FAIL: Parameter not renamed to 'value'"
    exit 1
fi

# Check body uses 'value'
if grep -A5 'public int Calculate' "$CS_FILE" | grep -qE 'val\s*\*|return\s+val'; then
    echo "FAIL: Method body still uses 'val'"
    exit 1
fi

echo "PASS: Parameter renamed from 'val' to 'value'"
exit 0
