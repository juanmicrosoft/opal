#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Inline.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Inline.calr not found"; exit 1; }

# Check Calculate no longer calls Double (inlined)
if grep -A15 '§F{f002:Calculate' "$CALR_FILE" | grep -q '§C{Double}'; then
    echo "FAIL: Calculate should have Double inlined (no more §C{Double})"
    exit 1
fi

# Check Calculate has the inlined multiplication
if ! grep -A15 '§F{f002:Calculate' "$CALR_FILE" | grep -qE '\(\*.*2\)|\(\* [ab] 2\)'; then
    echo "FAIL: Calculate should have inlined (* x 2) expressions"
    exit 1
fi

# Check ID preserved
if ! grep -q '§F{f002:Calculate' "$CALR_FILE"; then
    echo "FAIL: Function ID f002 not preserved"
    exit 1
fi

echo "PASS: Double inlined into Calculate, ID preserved"
exit 0
