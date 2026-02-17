#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
MATH_FILE="$WORKSPACE/src/Math.cs"
UTILS_FILE="$WORKSPACE/src/Utils.cs"

[[ -f "$UTILS_FILE" ]] || { echo "FAIL: Utils.cs not found"; exit 1; }

# Check Abs method moved to Utils
if ! grep -qE 'public\s+int\s+Abs' "$UTILS_FILE"; then
    echo "FAIL: Abs method not found in Utils.cs"
    exit 1
fi

# Check Abs removed from Math
if [[ -f "$MATH_FILE" ]] && grep -qE 'public\s+int\s+Abs\s*\(' "$MATH_FILE"; then
    echo "FAIL: Abs should be removed from Math.cs"
    exit 1
fi

echo "PASS: Abs moved to Utils"
exit 0
