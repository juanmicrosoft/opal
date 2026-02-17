#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Signature.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Signature.calr not found"; exit 1; }

# Check Greet has two parameters now
param_count=$(grep -A10 '§F{f001:Greet' "$CALR_FILE" | grep -c '§I{' || echo 0)
if [[ "$param_count" -lt 2 ]]; then
    echo "FAIL: Greet should have 2 parameters (found $param_count)"
    exit 1
fi

# Check greeting parameter exists
if ! grep -A10 '§F{f001:Greet' "$CALR_FILE" | grep -q '§I{str:greeting}'; then
    echo "FAIL: 'greeting' parameter not found"
    exit 1
fi

# Check ID preserved
if ! grep -q '§F{f001:Greet' "$CALR_FILE"; then
    echo "FAIL: Function ID f001 not preserved"
    exit 1
fi

echo "PASS: Parameter added, ID preserved"
exit 0
