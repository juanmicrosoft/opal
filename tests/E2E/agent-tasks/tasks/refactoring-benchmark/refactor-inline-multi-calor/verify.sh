#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Inline.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Inline.calr not found"; exit 1; }

# Check SumOfSquares no longer calls Square
if grep -A15 '§F{f006:SumOfSquares' "$CALR_FILE" | grep -q '§C{Square}'; then
    echo "FAIL: SumOfSquares should have Square inlined at both sites"
    exit 1
fi

# Check SumOfSquares has inlined multiplications - flexible pattern
# Accepts: (* x x), (*  x  x), or any multiplication of x/y with itself
if ! grep -A15 '§F{f006:SumOfSquares' "$CALR_FILE" | grep -qE '\(\*[[:space:]]*[xy][[:space:]]+[xy][[:space:]]*\)'; then
    echo "FAIL: SumOfSquares should have inlined (* x x) and (* y y)"
    exit 1
fi

# Check postcondition preserved - flexible pattern
if ! grep -A15 '§F{f006:SumOfSquares' "$CALR_FILE" | grep -qE '§S[[:space:]]*\(|§S.*result'; then
    echo "FAIL: SumOfSquares should retain postcondition"
    exit 1
fi

echo "PASS: Square inlined at both call sites, postcondition preserved"
exit 0
