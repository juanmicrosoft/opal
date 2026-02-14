#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Signature.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Signature.calr not found"; exit 1; }

# Check TryParse returns Option<i32>
if ! grep -A5 '§F{f002:TryParse' "$CALR_FILE" | grep -qE '§O\{Option<i32>\}'; then
    echo "FAIL: TryParse should return Option<i32>"
    exit 1
fi

# Check ID preserved
if ! grep -q '§F{f002:TryParse' "$CALR_FILE"; then
    echo "FAIL: Function ID f002 not preserved"
    exit 1
fi

echo "PASS: Return type changed to Option<i32>, ID preserved"
exit 0
