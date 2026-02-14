#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
MATH_FILE="$WORKSPACE/src/Math.cs"
UTILS_FILE="$WORKSPACE/src/Utils.cs"

[[ -f "$UTILS_FILE" ]] || { echo "FAIL: Utils.cs not found"; exit 1; }
[[ -f "$MATH_FILE" ]] || { echo "FAIL: Math.cs not found"; exit 1; }

# Check Abs in Utils
if ! grep -qE 'public.*int\s+Abs' "$UTILS_FILE"; then
    echo "FAIL: Abs not found in Utils.cs"
    exit 1
fi

# Check Distance still in Math
if ! grep -qE 'public.*int\s+Distance' "$MATH_FILE"; then
    echo "FAIL: Distance not found in Math.cs"
    exit 1
fi

# Check Distance calls Abs (via UtilsModule or directly)
if ! grep -A10 'public.*int Distance' "$MATH_FILE" | grep -qE 'Abs\(|\.Abs\('; then
    echo "FAIL: Distance should call Abs"
    exit 1
fi

echo "PASS: Abs moved, Distance updated to call it"
exit 0
