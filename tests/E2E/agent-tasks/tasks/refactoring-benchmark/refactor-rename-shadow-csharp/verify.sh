#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Rename.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Rename.cs not found"; exit 1; }

# Check parameter renamed
if grep -qE 'OuterCalc\s*\(\s*int\s+x\s*\)' "$CS_FILE"; then
    echo "FAIL: Parameter still named 'x'"
    exit 1
fi

if ! grep -qE 'OuterCalc\s*\(\s*int\s+input\s*\)' "$CS_FILE"; then
    echo "FAIL: Parameter not renamed to 'input'"
    exit 1
fi

# Check 'inner' variable preserved
if ! grep -A5 'public int OuterCalc' "$CS_FILE" | grep -q 'inner'; then
    echo "FAIL: Variable 'inner' should not be changed"
    exit 1
fi

echo "PASS: Parameter renamed, local variable preserved"
exit 0
