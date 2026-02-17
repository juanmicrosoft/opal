#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
MATH_FILE="$WORKSPACE/src/Math.calr"
UTILS_FILE="$WORKSPACE/src/Utils.calr"

[[ -f "$UTILS_FILE" ]] || { echo "FAIL: Utils.calr not found"; exit 1; }

# Check Abs function moved to Utils with ID preserved
if ! grep -q '§F{f001:Abs' "$UTILS_FILE"; then
    echo "FAIL: Abs function with ID f001 not found in Utils.calr"
    exit 1
fi

# Check Abs has postcondition preserved - flexible pattern
if ! grep -A10 '§F{f001:Abs' "$UTILS_FILE" | grep -qE '§S[[:space:]]*\(|§S.*result'; then
    echo "FAIL: Abs postcondition not preserved"
    exit 1
fi

# Check Abs removed from Math (or Math doesn't exist)
if [[ -f "$MATH_FILE" ]] && grep -q '§F{f001:Abs' "$MATH_FILE"; then
    echo "FAIL: Abs should be removed from Math.calr"
    exit 1
fi

echo "PASS: Abs moved to Utils with ID and postcondition preserved"
exit 0
