#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Inline.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Inline.cs not found"; exit 1; }

# Check NormalizeScore no longer calls Clamp
if grep -A15 'public int NormalizeScore' "$CS_FILE" | grep -q 'Clamp('; then
    echo "FAIL: NormalizeScore should have Clamp inlined"
    exit 1
fi

# Check NormalizeScore has clamping logic
if ! grep -A15 'public int NormalizeScore' "$CS_FILE" | grep -qE '< 0|> 100|<= 0|>= 100'; then
    echo "FAIL: NormalizeScore should have inlined clamping logic"
    exit 1
fi

echo "PASS: Clamp inlined into NormalizeScore"
exit 0
