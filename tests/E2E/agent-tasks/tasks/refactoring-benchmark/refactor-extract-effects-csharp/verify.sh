#!/usr/bin/env bash
set -euo pipefail
WORKSPACE="$1"
CS_FILE="$WORKSPACE/Extract.cs"

[[ -f "$CS_FILE" ]] || { echo "FAIL: Extract.cs not found"; exit 1; }

# Check ComputeSum method exists
if ! grep -qE '(private|public)\s+int\s+ComputeSum' "$CS_FILE"; then
    echo "FAIL: ComputeSum method not found"
    exit 1
fi

# Check LogAndCompute calls ComputeSum
if ! grep -A10 'public int LogAndCompute' "$CS_FILE" | grep -q 'ComputeSum('; then
    echo "FAIL: LogAndCompute should call ComputeSum"
    exit 1
fi

echo "PASS: ComputeSum extracted, LogAndCompute calls it"
exit 0
