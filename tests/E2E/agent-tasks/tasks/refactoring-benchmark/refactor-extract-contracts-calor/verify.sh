#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Extract.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Extract.calr not found"; exit 1; }

# Check ValidateIndex function exists with ID f006
if ! grep -q '§F{f006:ValidateIndex' "$CALR_FILE"; then
    echo "FAIL: ValidateIndex function with ID f006 not found"
    exit 1
fi

# Check ValidateIndex has contracts
if ! grep -A15 '§F{f006:ValidateIndex' "$CALR_FILE" | grep -q '§Q\|§S'; then
    echo "FAIL: ValidateIndex missing contracts"
    exit 1
fi

# Check original ProcessArray still has contracts
if ! grep -A15 '§F{f002:ProcessArray' "$CALR_FILE" | grep -q '§Q\|§S'; then
    echo "FAIL: ProcessArray lost its contracts"
    exit 1
fi

echo "PASS: ValidateIndex extracted with contracts, ProcessArray contracts preserved"
exit 0
