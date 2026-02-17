#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Inline.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Inline.calr not found"; exit 1; }

# Check NormalizeScore no longer calls Clamp
if grep -A15 '§F{f004:NormalizeScore' "$CALR_FILE" | grep -q '§C{Clamp}'; then
    echo "FAIL: NormalizeScore should have Clamp inlined"
    exit 1
fi

# Check NormalizeScore still has postconditions - flexible pattern
# Accepts: §S (>= result 0), §S(result >= 0), etc.
if ! grep -A15 '§F{f004:NormalizeScore' "$CALR_FILE" | grep -qE '§S[[:space:]]*\(|§S.*result'; then
    echo "FAIL: NormalizeScore should retain postconditions"
    exit 1
fi

# Check ID preserved
if ! grep -q '§F{f004:NormalizeScore' "$CALR_FILE"; then
    echo "FAIL: Function ID f004 not preserved"
    exit 1
fi

echo "PASS: Clamp inlined, postconditions preserved, ID preserved"
exit 0
