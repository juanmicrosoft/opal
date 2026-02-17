#!/usr/bin/env bash
# Verification script for refactor-extract-simple-csharp
# Checks that:
# 1. SumOfSquares method exists
# 2. Distance method calls SumOfSquares

set -euo pipefail

WORKSPACE="$1"
CS_FILE="$WORKSPACE/Extract.cs"

# Check file exists
if [[ ! -f "$CS_FILE" ]]; then
    echo "FAIL: Extract.cs not found"
    exit 1
fi

# Check SumOfSquares method exists
if ! grep -qE '(private|public)\s+int\s+SumOfSquares\s*\(' "$CS_FILE"; then
    echo "FAIL: SumOfSquares method not found"
    exit 1
fi

# Check SumOfSquares has two int parameters
if ! grep -E 'SumOfSquares\s*\(\s*int\s+\w+\s*,\s*int\s+\w+\s*\)' "$CS_FILE" > /dev/null; then
    echo "FAIL: SumOfSquares does not have two int parameters"
    exit 1
fi

# Check Distance method calls SumOfSquares
if ! grep -A10 'public int Distance' "$CS_FILE" | grep -q 'SumOfSquares('; then
    echo "FAIL: Distance method does not call SumOfSquares"
    exit 1
fi

echo "PASS: SumOfSquares extracted correctly, Distance calls it"
exit 0
