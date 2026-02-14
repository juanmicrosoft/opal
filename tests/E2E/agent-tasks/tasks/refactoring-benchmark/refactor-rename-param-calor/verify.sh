#!/usr/bin/env bash
# Verification script for refactor-rename-param-calor
# Checks that:
# 1. Parameter is renamed from 'val' to 'value'
# 2. All references updated (parameter, postcondition, return)
# 3. Function ID f001 preserved

set -euo pipefail

WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Rename.calr"

# Check file exists
if [[ ! -f "$CALR_FILE" ]]; then
    echo "FAIL: Rename.calr not found"
    exit 1
fi

# Check original parameter name 'val' is NOT present in Calculate function
if grep -A5 '§F{f001:Calculate' "$CALR_FILE" | grep -q '§I{i32:val}'; then
    echo "FAIL: Parameter still named 'val' (should be 'value')"
    exit 1
fi

# Check new parameter name 'value' IS present
if ! grep -A5 '§F{f001:Calculate' "$CALR_FILE" | grep -q '§I{i32:value}'; then
    echo "FAIL: Parameter not renamed to 'value'"
    exit 1
fi

# Check postcondition uses 'value' not 'val'
if grep -A10 '§F{f001:Calculate' "$CALR_FILE" | grep '§S' | grep -q 'val'; then
    echo "FAIL: Postcondition still references 'val'"
    exit 1
fi

# Check return expression uses 'value' not 'val'
if grep -A10 '§F{f001:Calculate' "$CALR_FILE" | grep '§R' | grep -q 'val[^u]'; then
    echo "FAIL: Return expression still references 'val'"
    exit 1
fi

# Check function ID preserved
if ! grep -q '§F{f001:Calculate' "$CALR_FILE"; then
    echo "FAIL: Function ID f001 not preserved"
    exit 1
fi

echo "PASS: Parameter renamed from 'val' to 'value' consistently, ID preserved"
exit 0
