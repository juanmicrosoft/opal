#!/usr/bin/env bash
# Verification script for refactor-contract-effect-calor
# Checks that:
# 1. PrintValue function has effect declaration §E{cw}
# 2. Function ID f004 preserved

set -euo pipefail

WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Contract.calr"

# Check file exists
if [[ ! -f "$CALR_FILE" ]]; then
    echo "FAIL: Contract.calr not found"
    exit 1
fi

# Check effect declaration exists
if ! grep -A10 '§F{f004:PrintValue' "$CALR_FILE" | grep -q '§E{cw}'; then
    echo "FAIL: Effect declaration §E{cw} not found in PrintValue function"
    exit 1
fi

# Check function ID preserved
if ! grep -q '§F{f004:PrintValue' "$CALR_FILE"; then
    echo "FAIL: Function ID f004 not preserved"
    exit 1
fi

echo "PASS: Effect declaration §E{cw} added to PrintValue, ID preserved"
exit 0
