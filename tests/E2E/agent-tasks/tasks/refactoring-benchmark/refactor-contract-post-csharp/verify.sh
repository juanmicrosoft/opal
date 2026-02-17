#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Contract.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Contract.cs not found"; exit 1; }

# Check for postcondition comment
if ! grep -B5 'public int Abs' "$CS_FILE" | grep -qiE 'postcondition.*result.*>=.*0|result >= 0'; then
    echo "FAIL: Postcondition comment not found for Abs"
    exit 1
fi

echo "PASS: Postcondition comment added"
exit 0
