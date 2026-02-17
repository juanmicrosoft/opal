#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Signature.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Signature.cs not found"; exit 1; }

# Check Greet has greeting parameter
if ! grep -qE 'Greet\s*\([^)]*string\s+greeting' "$CS_FILE"; then
    echo "FAIL: 'greeting' parameter not found"
    exit 1
fi

# Check Greet has two string parameters
if ! grep -qE 'Greet\s*\(\s*string\s+\w+\s*,\s*string\s+\w+\s*\)' "$CS_FILE"; then
    echo "FAIL: Greet should have 2 string parameters"
    exit 1
fi

echo "PASS: Parameter added"
exit 0
