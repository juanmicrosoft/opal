#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
MATH_FILE="$WORKSPACE/src/Math.calr"
UTILS_FILE="$WORKSPACE/src/Utils.calr"

[[ -f "$UTILS_FILE" ]] || { echo "FAIL: Utils.calr not found"; exit 1; }

# Check SafeDivide moved to Utils with ID preserved
if ! grep -q '§F{f002:SafeDivide' "$UTILS_FILE"; then
    echo "FAIL: SafeDivide with ID f002 not found in Utils.calr"
    exit 1
fi

# Check precondition preserved - flexible pattern for divisor != 0
# Accepts: (!= b 0), (not (== b 0)), b != 0 variations
if ! grep -A10 '§F{f002:SafeDivide' "$UTILS_FILE" | grep -qE '§Q.*(!=[[:space:]]*b|b[[:space:]]*!=|not.*==.*b.*0|b.*0)'; then
    echo "FAIL: SafeDivide precondition not preserved"
    exit 1
fi

# Check postcondition preserved
if ! grep -A10 '§F{f002:SafeDivide' "$UTILS_FILE" | grep -q '§S'; then
    echo "FAIL: SafeDivide postcondition not preserved"
    exit 1
fi

echo "PASS: SafeDivide moved with all contracts preserved"
exit 0
