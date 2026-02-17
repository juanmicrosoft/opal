#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Contract.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Contract.cs not found"; exit 1; }

# Check for precondition comment or guard clause
if ! grep -B5 'public int Sqrt' "$CS_FILE" | grep -qiE 'precondition.*x.*>=.*0|x >= 0'; then
    if ! grep -A10 'public int Sqrt' "$CS_FILE" | grep -qE 'x\s*<\s*0|ArgumentOutOfRangeException'; then
        echo "FAIL: Precondition not found for Sqrt"
        exit 1
    fi
fi

echo "PASS: Precondition added"
exit 0
