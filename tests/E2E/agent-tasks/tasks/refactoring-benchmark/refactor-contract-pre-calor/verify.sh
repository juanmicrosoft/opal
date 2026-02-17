#!/usr/bin/env bash
# Verification script for refactor-contract-pre-calor
# Checks that:
# 1. Sqrt function has precondition (>= x 0)
# 2. Function ID f001 preserved

set -euo pipefail

WORKSPACE="$1"
CALR_FILE="$WORKSPACE/Contract.calr"

# Check file exists
if [[ ! -f "$CALR_FILE" ]]; then
    echo "FAIL: Contract.calr not found"
    exit 1
fi

# Check precondition exists - flexible pattern to handle whitespace variations
# Accepts: §Q (>= x 0), §Q(>= x 0), §Q ( >= x 0 ), etc.
# Also accepts equivalent: (> x -1), (not (< x 0))
if ! grep -A10 '§F{f001:Sqrt' "$CALR_FILE" | grep -qE '§Q[[:space:]]*\([[:space:]]*(>=|>)[[:space:]]*x[[:space:]]*(0|-1)[[:space:]]*\)|§Q.*x.*[>]=?.*0'; then
    echo "FAIL: Precondition for non-negative x not found in Sqrt function"
    exit 1
fi

# Check function ID preserved
if ! grep -q '§F{f001:Sqrt' "$CALR_FILE"; then
    echo "FAIL: Function ID f001 not preserved"
    exit 1
fi

echo "PASS: Precondition (>= x 0) added to Sqrt, ID preserved"
exit 0
