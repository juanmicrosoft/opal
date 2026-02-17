#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Signature.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Signature.cs not found"; exit 1; }

# Check TryParse returns int? (nullable)
if ! grep -qE 'public\s+int\?\s+TryParse' "$CS_FILE"; then
    echo "FAIL: TryParse should return int?"
    exit 1
fi

echo "PASS: Return type changed to int?"
exit 0
