#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Extract.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Extract.calr not found"; exit 1; }

# Check Square function exists with ID f008
if ! grep -q '§F{f008:Square' "$CALR_FILE"; then
    echo "FAIL: Square function with ID f008 not found"
    exit 1
fi

# Check Square has postcondition - flexible pattern
if ! grep -A10 '§F{f008:Square' "$CALR_FILE" | grep -qE '§S[[:space:]]*\(|§S.*result'; then
    echo "FAIL: Square missing postcondition (>= result 0)"
    exit 1
fi

# Check ComputeNested calls Square
if ! grep -A20 '§F{f004:ComputeNested' "$CALR_FILE" | grep -q '§C{Square}'; then
    echo "FAIL: ComputeNested should call Square"
    exit 1
fi

# Check original ID preserved
if ! grep -q '§F{f004:ComputeNested' "$CALR_FILE"; then
    echo "FAIL: Function ID f004 not preserved"
    exit 1
fi

echo "PASS: Square extracted with contract, ComputeNested calls it, IDs preserved"
exit 0
