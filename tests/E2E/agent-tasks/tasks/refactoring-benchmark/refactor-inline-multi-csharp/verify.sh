#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Inline.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Inline.cs not found"; exit 1; }

# Check SumOfSquares no longer calls Square
if grep -A10 'public int SumOfSquares' "$CS_FILE" | grep -q 'Square('; then
    echo "FAIL: SumOfSquares should have Square inlined"
    exit 1
fi

# Check SumOfSquares has inlined squaring
if ! grep -A10 'public int SumOfSquares' "$CS_FILE" | grep -qE '[xy]\s*\*\s*[xy]'; then
    echo "FAIL: SumOfSquares should have inlined x*x and y*y"
    exit 1
fi

echo "PASS: Square inlined at both call sites"
exit 0
