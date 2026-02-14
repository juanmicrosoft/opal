#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Contract.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Contract.calr not found"; exit 1; }

# Check postcondition exists for Abs - flexible pattern for whitespace
# Accepts: §S (>= result 0), §S(>= result 0), etc.
if ! grep -A10 '§F{f002:Abs' "$CALR_FILE" | grep -qE '§S[[:space:]]*\([[:space:]]*(>=|>)[[:space:]]*result[[:space:]]*(0|-1)|§S.*result.*[>]=?.*0'; then
    echo "FAIL: Postcondition for non-negative result not found in Abs"
    exit 1
fi

if ! grep -q '§F{f002:Abs' "$CALR_FILE"; then
    echo "FAIL: Function ID f002 not preserved"
    exit 1
fi

echo "PASS: Postcondition added, ID preserved"
exit 0
