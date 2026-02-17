#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Signature.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Signature.calr not found"; exit 1; }

# Check CreateRange parameter order: step, start, end
# First param should be step
first_param=$(grep -A3 '§F{f003:CreateRange' "$CALR_FILE" | grep '§I{' | head -1)
if ! echo "$first_param" | grep -q 'step'; then
    echo "FAIL: First parameter should be 'step'"
    exit 1
fi

# Check CountSteps calls CreateRange with correct order (step=1 first)
if ! grep -A15 '§F{f004:CountSteps' "$CALR_FILE" | grep -A5 '§C{CreateRange}' | head -3 | grep -q '§A 1'; then
    echo "FAIL: CountSteps should call CreateRange with step=1 as first arg"
    exit 1
fi

# Check IDs preserved
if ! grep -q '§F{f003:CreateRange' "$CALR_FILE"; then
    echo "FAIL: Function ID f003 not preserved"
    exit 1
fi

if ! grep -q '§F{f004:CountSteps' "$CALR_FILE"; then
    echo "FAIL: Function ID f004 not preserved"
    exit 1
fi

echo "PASS: Parameters reordered, caller updated, IDs preserved"
exit 0
