#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Contract.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Contract.cs not found"; exit 1; }

# Check for effect documentation comment
if ! grep -B5 'public void PrintValue' "$CS_FILE" | grep -qiE 'effect.*console|writes.*console|cw'; then
    echo "FAIL: Effect documentation not found for PrintValue"
    exit 1
fi

echo "PASS: Effect documentation added"
exit 0
