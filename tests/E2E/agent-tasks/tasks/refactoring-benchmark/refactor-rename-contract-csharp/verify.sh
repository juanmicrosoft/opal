#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Rename.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Rename.cs not found"; exit 1; }

# Check new parameter name in signature
if ! grep -qE 'ValidatedOp\s*\(\s*int\s+value' "$CS_FILE"; then
    echo "FAIL: Parameter not renamed to 'value'"
    exit 1
fi

# Check nameof updated
if grep 'nameof(num)' "$CS_FILE" > /dev/null; then
    echo "FAIL: nameof(num) should be nameof(value)"
    exit 1
fi

# Check body uses 'value'
if grep -A10 'public int ValidatedOp' "$CS_FILE" | grep -qE 'num\s*[<>=]|return\s+num'; then
    echo "FAIL: Method body still uses 'num'"
    exit 1
fi

echo "PASS: Parameter renamed consistently"
exit 0
