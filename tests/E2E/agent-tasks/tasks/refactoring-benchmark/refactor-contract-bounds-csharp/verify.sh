#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Contract.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Contract.cs not found"; exit 1; }

# Check for bounds validation
if ! grep -B5 'public int GetElement' "$CS_FILE" | grep -qiE 'precondition.*index.*>=.*0|index >= 0'; then
    if ! grep -A10 'public int GetElement' "$CS_FILE" | grep -qE 'index\s*<\s*0|ArgumentOutOfRangeException'; then
        echo "FAIL: Index bounds validation not found"
        exit 1
    fi
fi

echo "PASS: Array bounds validation added"
exit 0
