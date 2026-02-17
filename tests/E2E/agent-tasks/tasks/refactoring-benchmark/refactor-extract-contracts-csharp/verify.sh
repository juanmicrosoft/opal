#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Extract.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Extract.cs not found"; exit 1; }

# Check ValidateIndex method exists
if ! grep -qE '(private|public)\s+bool\s+ValidateIndex' "$CS_FILE"; then
    echo "FAIL: ValidateIndex method not found"
    exit 1
fi

# Check ValidateIndex has contract comments
if ! grep -B5 'bool ValidateIndex' "$CS_FILE" | grep -qiE 'precondition|postcondition'; then
    echo "FAIL: ValidateIndex missing contract comments"
    exit 1
fi

echo "PASS: ValidateIndex extracted with contract comments"
exit 0
