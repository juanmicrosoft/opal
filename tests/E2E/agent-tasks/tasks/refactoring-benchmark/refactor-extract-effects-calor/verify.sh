#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Extract.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Extract.calr not found"; exit 1; }

# Check ComputeSum function exists with ID f007
if ! grep -q '§F{f007:ComputeSum' "$CALR_FILE"; then
    echo "FAIL: ComputeSum function with ID f007 not found"
    exit 1
fi

# Check ComputeSum is pure (no §E declaration)
if grep -A10 '§F{f007:ComputeSum' "$CALR_FILE" | grep -q '§E{'; then
    echo "FAIL: ComputeSum should be pure (no effects)"
    exit 1
fi

# Check LogAndCompute still has effect
if ! grep -A10 '§F{f003:LogAndCompute' "$CALR_FILE" | grep -q '§E{cw}'; then
    echo "FAIL: LogAndCompute should retain §E{cw} effect"
    exit 1
fi

# Check LogAndCompute calls ComputeSum
if ! grep -A15 '§F{f003:LogAndCompute' "$CALR_FILE" | grep -q '§C{ComputeSum}'; then
    echo "FAIL: LogAndCompute should call ComputeSum"
    exit 1
fi

echo "PASS: Pure ComputeSum extracted, LogAndCompute retains effect and calls it"
exit 0
