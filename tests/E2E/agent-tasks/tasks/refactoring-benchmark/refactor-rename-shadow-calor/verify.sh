#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Rename.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Rename.calr not found"; exit 1; }

# Check parameter renamed from 'x' to 'input'
if grep -A5 '§F{f003:OuterCalc' "$CALR_FILE" | grep -q '§I{i32:x}'; then
    echo "FAIL: Parameter still named 'x' (should be 'input')"
    exit 1
fi

if ! grep -A5 '§F{f003:OuterCalc' "$CALR_FILE" | grep -q '§I{i32:input}'; then
    echo "FAIL: Parameter not renamed to 'input'"
    exit 1
fi

# Check variable 'inner' still exists (not accidentally renamed)
if ! grep -A10 '§F{f003:OuterCalc' "$CALR_FILE" | grep -q 'inner'; then
    echo "FAIL: Variable 'inner' should not be changed"
    exit 1
fi

# Check ID preserved
if ! grep -q '§F{f003:OuterCalc' "$CALR_FILE"; then
    echo "FAIL: Function ID f003 not preserved"
    exit 1
fi

echo "PASS: Parameter renamed, local variable preserved, ID preserved"
exit 0
