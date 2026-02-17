#!/usr/bin/env bash
# Verification script for refactor-extract-simple-calor
# Checks that:
# 1. SumOfSquares function exists with ID f005
# 2. Distance function calls SumOfSquares
# 3. Original IDs (f001) are preserved

set -euo pipefail

WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Extract.calr"

# Check file exists
if [[ ! -f "$CALR_FILE" ]]; then
    echo "FAIL: Extract.calr not found"
    exit 1
fi

# Check SumOfSquares function exists with correct ID
if ! grep -q '§F{f005:SumOfSquares' "$CALR_FILE"; then
    echo "FAIL: SumOfSquares function with ID f005 not found"
    exit 1
fi

# Check SumOfSquares has correct parameters
if ! grep -A2 '§F{f005:SumOfSquares' "$CALR_FILE" | grep -q '§I{i32:'; then
    echo "FAIL: SumOfSquares missing i32 parameters"
    exit 1
fi

# Check Distance function calls SumOfSquares
if ! grep -A10 '§F{f001:Distance' "$CALR_FILE" | grep -q '§C{SumOfSquares}'; then
    echo "FAIL: Distance function does not call SumOfSquares"
    exit 1
fi

# Check original Distance function ID preserved
if ! grep -q '§F{f001:Distance' "$CALR_FILE"; then
    echo "FAIL: Original Distance function ID f001 not preserved"
    exit 1
fi

echo "PASS: SumOfSquares extracted correctly, Distance calls it, IDs preserved"
exit 0
