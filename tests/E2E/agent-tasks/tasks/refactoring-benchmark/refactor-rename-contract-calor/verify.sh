#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Rename.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Rename.calr not found"; exit 1; }

# Check old name 'num' not present in ValidatedOp
if grep -A15 '§F{f002:ValidatedOp' "$CALR_FILE" | grep -qE '§I\{i32:num\}|num[^a-zA-Z]'; then
    echo "FAIL: Parameter still named 'num' (should be 'value')"
    exit 1
fi

# Check new name 'value' is present
if ! grep -A5 '§F{f002:ValidatedOp' "$CALR_FILE" | grep -q '§I{i32:value}'; then
    echo "FAIL: Parameter not renamed to 'value'"
    exit 1
fi

# Check contracts updated to use 'value' - flexible pattern
if ! grep -A15 '§F{f002:ValidatedOp' "$CALR_FILE" | grep -qE '§Q[[:space:]]*\(.*value|§Q.*value'; then
    echo "FAIL: Preconditions not updated to use 'value'"
    exit 1
fi

# Check ID preserved
if ! grep -q '§F{f002:ValidatedOp' "$CALR_FILE"; then
    echo "FAIL: Function ID f002 not preserved"
    exit 1
fi

echo "PASS: Parameter renamed, contracts updated, ID preserved"
exit 0
