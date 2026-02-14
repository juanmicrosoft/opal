#!/usr/bin/env bash
# End-to-end integration test for refactoring benchmark
#
# This script validates that:
# 1. Task definitions are valid JSON
# 2. Fixtures exist and are valid
# 3. Verification scripts are executable
# 4. Compilation works for both Calor and C# fixtures
# 5. (Optional) Run a small subset of tasks with the agent
#
# Usage: ./test-refactoring-benchmark.sh [--full]
#        --full: Also runs actual agent tasks (slower)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/lib/helpers.sh"

# Parse arguments
FULL_TEST=false
while [[ $# -gt 0 ]]; do
    case "$1" in
        --full)
            FULL_TEST=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Track test results
TESTS_PASSED=0
TESTS_FAILED=0

pass() {
    ((TESTS_PASSED++))
    log_pass "$1"
}

fail() {
    ((TESTS_FAILED++))
    log_fail "$1"
}

# ============================================================================
# TEST 1: Verify task definitions
# ============================================================================
echo ""
echo "============================================"
echo "Test 1: Task Definition Validation"
echo "============================================"

TASK_COUNT=0
TASK_ERRORS=0

for task_dir in "$TASKS_DIR"/refactoring-benchmark/*/; do
    [[ -d "$task_dir" ]] || continue
    task_file="$task_dir/task.json"
    task_name=$(basename "$task_dir")

    if [[ ! -f "$task_file" ]]; then
        fail "$task_name: missing task.json"
        ((TASK_ERRORS++))
        continue
    fi

    # Validate JSON syntax
    if ! jq empty "$task_file" 2>/dev/null; then
        fail "$task_name: invalid JSON syntax"
        ((TASK_ERRORS++))
        continue
    fi

    # Validate required fields
    id=$(jq -r '.id // empty' "$task_file")
    prompt=$(jq -r '.prompt // empty' "$task_file")
    fixture=$(jq -r '.fixture // empty' "$task_file")

    if [[ -z "$id" ]]; then
        fail "$task_name: missing 'id' field"
        ((TASK_ERRORS++))
        continue
    fi

    if [[ -z "$prompt" ]]; then
        fail "$task_name: missing 'prompt' field"
        ((TASK_ERRORS++))
        continue
    fi

    if [[ -z "$fixture" ]]; then
        fail "$task_name: missing 'fixture' field"
        ((TASK_ERRORS++))
        continue
    fi

    ((TASK_COUNT++))
done

if [[ $TASK_ERRORS -eq 0 ]]; then
    pass "All $TASK_COUNT task definitions are valid"
else
    fail "$TASK_ERRORS task definition errors found"
fi

# ============================================================================
# TEST 2: Verify fixtures exist and have required files
# ============================================================================
echo ""
echo "============================================"
echo "Test 2: Fixture Validation"
echo "============================================"

FIXTURE_COUNT=0
FIXTURE_ERRORS=0

# Check all refactoring fixtures (only those ending with -calor or -csharp)
for fixture_dir in "$FIXTURES_DIR"/refactor-*-{calor,csharp}/; do
    [[ -d "$fixture_dir" ]] || continue
    fixture_name=$(basename "$fixture_dir")
    ((FIXTURE_COUNT++))

    # Check for CLAUDE.md
    if [[ ! -f "$fixture_dir/CLAUDE.md" ]]; then
        fail "$fixture_name: missing CLAUDE.md"
        ((FIXTURE_ERRORS++))
    fi

    # Check for source files
    has_calr=$(find "$fixture_dir" -name "*.calr" -type f 2>/dev/null | head -1)
    has_cs=$(find "$fixture_dir" -name "*.cs" -type f 2>/dev/null | head -1)

    if [[ -z "$has_calr" && -z "$has_cs" ]]; then
        fail "$fixture_name: no source files (.calr or .cs)"
        ((FIXTURE_ERRORS++))
    fi

    # Check for .csproj in C# fixtures
    if [[ -n "$has_cs" && -z "$has_calr" ]]; then
        has_csproj=$(find "$fixture_dir" -name "*.csproj" -type f 2>/dev/null | head -1)
        if [[ -z "$has_csproj" ]]; then
            fail "$fixture_name: C# project missing .csproj"
            ((FIXTURE_ERRORS++))
        fi
    fi
done

if [[ $FIXTURE_ERRORS -eq 0 ]]; then
    pass "All $FIXTURE_COUNT fixtures are valid"
else
    fail "$FIXTURE_ERRORS fixture errors found"
fi

# ============================================================================
# TEST 3: Verify verification scripts
# ============================================================================
echo ""
echo "============================================"
echo "Test 3: Verification Script Validation"
echo "============================================"

SCRIPT_COUNT=0
SCRIPT_ERRORS=0

for task_dir in "$TASKS_DIR"/refactoring-benchmark/*/; do
    [[ -d "$task_dir" ]] || continue
    verify_script="$task_dir/verify.sh"
    task_name=$(basename "$task_dir")

    if [[ ! -f "$verify_script" ]]; then
        fail "$task_name: missing verify.sh"
        ((SCRIPT_ERRORS++))
        continue
    fi

    # Check if executable
    if [[ ! -x "$verify_script" ]]; then
        fail "$task_name: verify.sh not executable"
        ((SCRIPT_ERRORS++))
        continue
    fi

    # Basic syntax check with bash -n
    if ! bash -n "$verify_script" 2>/dev/null; then
        fail "$task_name: verify.sh has syntax errors"
        ((SCRIPT_ERRORS++))
        continue
    fi

    ((SCRIPT_COUNT++))
done

if [[ $SCRIPT_ERRORS -eq 0 ]]; then
    pass "All $SCRIPT_COUNT verification scripts are valid"
else
    fail "$SCRIPT_ERRORS verification script errors found"
fi

# ============================================================================
# TEST 4: Calor compilation test
# ============================================================================
echo ""
echo "============================================"
echo "Test 4: Calor Compilation"
echo "============================================"

CALOR_FIXTURES=0
CALOR_ERRORS=0

# Build compiler first
if [[ -f "$COMPILER" ]] || dotnet build "$REPO_ROOT/src/Calor.Compiler/Calor.Compiler.csproj" -c Debug --nologo -v q 2>/dev/null; then
    for fixture_dir in "$FIXTURES_DIR"/refactor-*-calor/; do
        [[ -d "$fixture_dir" ]] || continue
        fixture_name=$(basename "$fixture_dir")
        ((CALOR_FIXTURES++))

        # Try to compile each .calr file
        while IFS= read -r -d '' calr_file; do
            cs_file="${calr_file%.calr}.g.cs"
            if ! "$COMPILER" --input "$calr_file" --output "$cs_file" 2>/dev/null; then
                fail "$fixture_name: compilation failed for $(basename "$calr_file")"
                ((CALOR_ERRORS++))
            fi
        done < <(find "$fixture_dir" -name "*.calr" -type f -print0 2>/dev/null)
    done

    if [[ $CALOR_ERRORS -eq 0 ]]; then
        pass "All $CALOR_FIXTURES Calor fixtures compile successfully"
    else
        fail "$CALOR_ERRORS Calor compilation errors found"
    fi
else
    log_warn "Could not build Calor compiler - skipping compilation test"
fi

# ============================================================================
# TEST 5: C# compilation test
# ============================================================================
echo ""
echo "============================================"
echo "Test 5: C# Compilation"
echo "============================================"

CSHARP_FIXTURES=0
CSHARP_ERRORS=0

for fixture_dir in "$FIXTURES_DIR"/refactor-*-csharp/; do
    [[ -d "$fixture_dir" ]] || continue
    fixture_name=$(basename "$fixture_dir")
    ((CSHARP_FIXTURES++))

    # Find .csproj and build
    csproj_file=$(find "$fixture_dir" -name "*.csproj" -type f 2>/dev/null | head -1)
    if [[ -n "$csproj_file" ]]; then
        if ! dotnet build "$csproj_file" --nologo -v q 2>/dev/null; then
            fail "$fixture_name: C# compilation failed"
            ((CSHARP_ERRORS++))
        fi
    fi
done

if [[ $CSHARP_ERRORS -eq 0 ]]; then
    pass "All $CSHARP_FIXTURES C# fixtures compile successfully"
else
    fail "$CSHARP_ERRORS C# compilation errors found"
fi

# ============================================================================
# TEST 6: C# contract comment detection
# ============================================================================
echo ""
echo "============================================"
echo "Test 6: C# Contract Comment Detection"
echo "============================================"

TOTAL_CONTRACTS=0
for fixture_dir in "$FIXTURES_DIR"/refactor-*-csharp/; do
    [[ -d "$fixture_dir" ]] || continue
    fixture_name=$(basename "$fixture_dir")

    contracts=$(count_csharp_contracts_in_dir "$fixture_dir")
    TOTAL_CONTRACTS=$((TOTAL_CONTRACTS + contracts))
    log_debug "$fixture_name: $contracts contract comments"
done

if [[ $TOTAL_CONTRACTS -gt 0 ]]; then
    pass "Detected $TOTAL_CONTRACTS C# contract comments across all fixtures"
else
    log_warn "No C# contract comments detected - consider adding some for testing"
fi

# ============================================================================
# TEST 7: Task categories coverage
# ============================================================================
echo ""
echo "============================================"
echo "Test 7: Task Category Coverage"
echo "============================================"

CATEGORIES=""
for task_dir in "$TASKS_DIR"/refactoring-benchmark/*/; do
    [[ -d "$task_dir" ]] || continue
    task_file="$task_dir/task.json"
    [[ -f "$task_file" ]] || continue

    # Extract category from task ID (e.g., refactor-extract-simple-calor -> extract)
    id=$(jq -r '.id // empty' "$task_file" 2>/dev/null)
    if [[ -n "$id" ]]; then
        category=$(echo "$id" | sed 's/refactor-\([^-]*\)-.*/\1/')
        if ! echo "$CATEGORIES" | grep -q "$category"; then
            CATEGORIES="$CATEGORIES $category"
        fi
    fi
done

# Expected categories
EXPECTED="extract rename inline contract sig"
MISSING=""
for cat in $EXPECTED; do
    if ! echo "$CATEGORIES" | grep -q "$cat"; then
        MISSING="$MISSING $cat"
    fi
done

if [[ -z "$MISSING" ]]; then
    pass "All expected categories present: $EXPECTED"
else
    fail "Missing categories:$MISSING"
fi

# Count tasks per language (based on fixture name in task.json)
CALOR_TASKS=$(find "$TASKS_DIR"/refactoring-benchmark -name "task.json" -exec grep -l 'calor' {} \; 2>/dev/null | wc -l | tr -d ' ')
CSHARP_TASKS=$(find "$TASKS_DIR"/refactoring-benchmark -name "task.json" -exec grep -l 'csharp' {} \; 2>/dev/null | wc -l | tr -d ' ')

log_info "Task distribution: Calor=$CALOR_TASKS, C#=$CSHARP_TASKS"

if [[ $CALOR_TASKS -gt 0 && $CSHARP_TASKS -gt 0 ]]; then
    pass "Both Calor and C# tasks present"
else
    fail "Missing Calor or C# tasks"
fi

# ============================================================================
# TEST 8: Full agent test (optional)
# ============================================================================
if [[ "$FULL_TEST" == "true" ]]; then
    echo ""
    echo "============================================"
    echo "Test 8: Agent Execution (Full Test)"
    echo "============================================"

    check_claude_cli

    # Run a small subset - one Calor and one C# task
    AGENT_TASKS="refactor-extract-simple-calor refactor-extract-simple-csharp"

    for task_id in $AGENT_TASKS; do
        task_dir="$TASKS_DIR/refactoring-benchmark/$task_id"
        if [[ -d "$task_dir" ]]; then
            log_info "Running: $task_id"
            if run_task_with_voting "$task_dir" "true"; then
                pass "$task_id: agent completed successfully"
            else
                fail "$task_id: agent failed"
            fi
        fi
    done
else
    echo ""
    echo "============================================"
    echo "Test 8: Agent Execution (Skipped)"
    echo "============================================"
    log_info "Use --full to run agent tests"
fi

# ============================================================================
# SUMMARY
# ============================================================================
echo ""
echo "============================================"
echo "Test Summary"
echo "============================================"

TOTAL=$((TESTS_PASSED + TESTS_FAILED))
echo -e "${BOLD}Total:${NC} $TOTAL tests"
echo -e "${GREEN}Passed:${NC} $TESTS_PASSED"
echo -e "${RED}Failed:${NC} $TESTS_FAILED"

if [[ $TESTS_FAILED -eq 0 ]]; then
    echo ""
    echo -e "${GREEN}${BOLD}All tests passed!${NC}"
    exit 0
else
    echo ""
    echo -e "${RED}${BOLD}Some tests failed.${NC}"
    exit 1
fi
