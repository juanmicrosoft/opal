#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Inline.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Inline.cs not found"; exit 1; }

# Check Calculate no longer calls Double
if grep -A10 'public int Calculate' "$CS_FILE" | grep -q 'Double('; then
    echo "FAIL: Calculate should have Double inlined"
    exit 1
fi

# Check Calculate has inlined expression
if ! grep -A10 'public int Calculate' "$CS_FILE" | grep -qE '\*\s*2|2\s*\*'; then
    echo "FAIL: Calculate should have inlined multiplication"
    exit 1
fi

echo "PASS: Double inlined into Calculate"
exit 0
