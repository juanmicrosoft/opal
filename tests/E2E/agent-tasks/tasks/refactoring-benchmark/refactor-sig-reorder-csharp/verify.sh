#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Signature.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Signature.cs not found"; exit 1; }

# Check CreateRange parameter order: step first
if ! grep -qE 'CreateRange\s*\(\s*int\s+step' "$CS_FILE"; then
    echo "FAIL: First parameter should be 'step'"
    exit 1
fi

# Check CountSteps calls CreateRange with new order
if ! grep -A5 'public int CountSteps' "$CS_FILE" | grep -qE 'CreateRange\s*\(\s*1'; then
    echo "FAIL: CountSteps should call CreateRange(1, ...)"
    exit 1
fi

echo "PASS: Parameters reordered, caller updated"
exit 0
