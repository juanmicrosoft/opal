#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
MATH_FILE="$WORKSPACE/src/Math.calr"
UTILS_FILE="$WORKSPACE/src/Utils.calr"

[[ -f "$UTILS_FILE" ]] || { echo "FAIL: Utils.calr not found"; exit 1; }
[[ -f "$MATH_FILE" ]] || { echo "FAIL: Math.calr not found"; exit 1; }

# Check Abs moved to Utils
if ! grep -q '§F{f001:Abs' "$UTILS_FILE"; then
    echo "FAIL: Abs with ID f001 not found in Utils.calr"
    exit 1
fi

# Check Distance still exists with ID preserved
if ! grep -q '§F{f003:Distance' "$MATH_FILE"; then
    echo "FAIL: Distance with ID f003 not found in Math.calr"
    exit 1
fi

# Check Distance calls Abs (may need qualified name or just Abs)
if ! grep -A15 '§F{f003:Distance' "$MATH_FILE" | grep -qE '§C\{.*Abs'; then
    echo "FAIL: Distance should call Abs"
    exit 1
fi

echo "PASS: Abs moved, Distance updated to call it, IDs preserved"
exit 0
