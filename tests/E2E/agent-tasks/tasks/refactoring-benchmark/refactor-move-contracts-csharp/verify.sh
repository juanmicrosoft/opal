#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
UTILS_FILE="$WORKSPACE/src/Utils.cs"

[[ -f "$UTILS_FILE" ]] || { echo "FAIL: Utils.cs not found"; exit 1; }

# Check SafeDivide moved to Utils
if ! grep -qE 'public\s+int\s+SafeDivide' "$UTILS_FILE"; then
    echo "FAIL: SafeDivide not found in Utils.cs"
    exit 1
fi

# Check contract comments preserved
if ! grep -B5 'public int SafeDivide' "$UTILS_FILE" | grep -qiE 'precondition|postcondition'; then
    # Also accept inline validation
    if ! grep -A10 'public int SafeDivide' "$UTILS_FILE" | grep -qE 'DivideByZeroException|== 0'; then
        echo "FAIL: SafeDivide contract/validation not preserved"
        exit 1
    fi
fi

echo "PASS: SafeDivide moved with contracts"
exit 0
