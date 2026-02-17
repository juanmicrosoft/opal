#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Contract.calr"

[[ -f "$CALR_FILE" ]] || { echo "FAIL: Contract.calr not found"; exit 1; }

# Check preconditions exist for GetElement - flexible patterns
# Accepts various whitespace and equivalent forms
if ! grep -A15 '§F{f003:GetElement' "$CALR_FILE" | grep -qE '§Q[[:space:]]*\([[:space:]]*(>=|>)[[:space:]]*index|§Q.*index.*[>]=?.*0'; then
    echo "FAIL: Precondition for index >= 0 not found"
    exit 1
fi

if ! grep -A15 '§F{f003:GetElement' "$CALR_FILE" | grep -qE '§Q[[:space:]]*\([[:space:]]*<[[:space:]]*index[[:space:]]*length|§Q.*index.*<.*length'; then
    echo "FAIL: Precondition for index < length not found"
    exit 1
fi

if ! grep -q '§F{f003:GetElement' "$CALR_FILE"; then
    echo "FAIL: Function ID f003 not preserved"
    exit 1
fi

echo "PASS: Array bounds contracts added, ID preserved"
exit 0
