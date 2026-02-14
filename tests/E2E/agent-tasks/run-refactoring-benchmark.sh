#!/usr/bin/env bash
# ============================================================================
# Calor vs C# Refactoring Benchmark
# ============================================================================
#
# Runs refactoring tasks on both Calor and C# codebases, comparing success
# rates and measuring Calor's advantages in:
# - Contract propagation
# - Effect tracking
# - Stable ID preservation
#
# Usage:
#   ./run-refactoring-benchmark.sh              # Full benchmark (40 tasks)
#   ./run-refactoring-benchmark.sh --quick      # Single run per task (faster)
#   ./run-refactoring-benchmark.sh --category extract  # Specific category
#   ./run-refactoring-benchmark.sh --verbose    # Debug output
#
# ============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Source helper library
source "$SCRIPT_DIR/lib/helpers.sh"

# ============================================================================
# CONFIGURATION
# ============================================================================

# Note: SCRIPT_DIR is overwritten by helpers.sh, so we save it here
BENCHMARK_SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_DIR="$BENCHMARK_SCRIPT_DIR/benchmark-results"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
CALOR_RESULTS="$RESULTS_DIR/calor-$TIMESTAMP.json"
CSHARP_RESULTS="$RESULTS_DIR/csharp-$TIMESTAMP.json"
REPORT_FILE="$RESULTS_DIR/comparison-$TIMESTAMP.md"

# ============================================================================
# RESULT TRACKING
# Note: Using simple variables instead of associative arrays for bash 3.x compatibility
# ============================================================================

TOTAL_CALOR_PASS=0
TOTAL_CALOR_FAIL=0
TOTAL_CSHARP_PASS=0
TOTAL_CSHARP_FAIL=0

# ============================================================================
# BENCHMARK FUNCTIONS
# ============================================================================

# Initialize results directory
init_results() {
    mkdir -p "$RESULTS_DIR"
    log_info "Results will be saved to: $RESULTS_DIR"
}

# Run benchmark for a specific language
# Usage: run_language_benchmark "calor|csharp" "category" "single_run"
run_language_benchmark() {
    local lang="$1"
    local category="${2:-}"
    local single_run="${3:-false}"

    log_info "Running $lang refactoring benchmark..."

    # Build filter pattern: matches task IDs like "refactor-extract-simple-calor"
    local filter_pattern="$lang"
    if [[ -n "$category" ]]; then
        # Filter by sub-category within refactoring-benchmark
        # e.g., "sig" matches "refactor-sig-*-calor"
        filter_pattern="$category.*$lang"
    fi

    local args=("--category" "refactoring-benchmark" "--filter" "$filter_pattern")

    if [[ "$single_run" == "true" ]]; then
        args+=("--single-run")
    fi

    # Run the tests and capture output
    # Note: Use AGENT_TASKS_DIR since SCRIPT_DIR is overwritten by helpers.sh
    local output
    output=$("$AGENT_TASKS_DIR/run-agent-tests.sh" "${args[@]}" 2>&1) || true

    echo "$output"
}

# Parse results from test output
# Usage: parse_results "output" "calor|csharp"
parse_results() {
    local output="$1"
    local lang="$2"

    # Extract pass/fail counts from output
    local passed failed
    passed=$(echo "$output" | grep -oE 'Passed:[[:space:]]+[0-9]+' | grep -oE '[0-9]+' | tail -1) || passed=0
    failed=$(echo "$output" | grep -oE 'Failed:[[:space:]]+[0-9]+' | grep -oE '[0-9]+' | tail -1) || failed=0

    if [[ "$lang" == "calor" ]]; then
        TOTAL_CALOR_PASS=$passed
        TOTAL_CALOR_FAIL=$failed
    else
        TOTAL_CSHARP_PASS=$passed
        TOTAL_CSHARP_FAIL=$failed
    fi

    log_info "$lang results: $passed passed, $failed failed"
}

# Calculate pass rate
calculate_pass_rate() {
    local passed="$1"
    local failed="$2"

    local total=$((passed + failed))
    if [[ $total -eq 0 ]]; then
        echo "0.00"
        return
    fi

    # Calculate percentage with 2 decimal places using awk
    awk "BEGIN {printf \"%.2f\", ($passed / $total) * 100}"
}

# Calculate advantage ratio
calculate_advantage() {
    local calor_rate="$1"
    local csharp_rate="$2"

    # Check if csharp_rate is zero using awk (portable, no bc dependency)
    if awk "BEGIN {exit !($csharp_rate == 0)}"; then
        echo "N/A"
        return
    fi

    awk "BEGIN {printf \"%.2f\", $calor_rate / $csharp_rate}"
}

# Compare two floating point numbers
# Usage: float_gt "3.14" "2.71" && echo "greater"
float_gt() {
    awk "BEGIN {exit !($1 > $2)}"
}

float_lt() {
    awk "BEGIN {exit !($1 < $2)}"
}

float_eq() {
    awk "BEGIN {exit !($1 == $2)}"
}

# Generate comparison report
generate_report() {
    local calor_rate csharp_rate advantage

    calor_rate=$(calculate_pass_rate "$TOTAL_CALOR_PASS" "$TOTAL_CALOR_FAIL")
    csharp_rate=$(calculate_pass_rate "$TOTAL_CSHARP_PASS" "$TOTAL_CSHARP_FAIL")
    advantage=$(calculate_advantage "$calor_rate" "$csharp_rate")

    cat > "$REPORT_FILE" << EOF
# Calor vs C# Refactoring Benchmark Results

**Generated:** $(date)
**Test Run ID:** $TIMESTAMP

## Summary

| Metric | Calor | C# |
|--------|-------|-----|
| Tasks Passed | $TOTAL_CALOR_PASS | $TOTAL_CSHARP_PASS |
| Tasks Failed | $TOTAL_CALOR_FAIL | $TOTAL_CSHARP_FAIL |
| **Pass Rate** | **${calor_rate}%** | **${csharp_rate}%** |
| Advantage Ratio | ${advantage}x | 1.00x |

## Analysis

### Key Differentiators

1. **Contract Propagation**: Calor's first-class contracts (§Q, §S) are automatically
   preserved during refactoring operations, while C# relies on comment-based contracts
   that must be manually updated.

2. **Effect Tracking**: Calor's effect system (§E{cw}, §E{fs}, etc.) explicitly tracks
   side effects, making it clear when extracted code has or doesn't have effects.
   C# has no equivalent system.

3. **Stable IDs**: Calor's unique function IDs (f001, f002, etc.) survive refactoring
   operations, enabling reliable cross-references. C# uses names which can break
   references during rename operations.

### Pass Rate Comparison

$(if float_gt "$calor_rate" "$csharp_rate"; then
    echo "✅ **Calor outperformed C#** with a ${advantage}x advantage in pass rate."
    echo ""
    echo "This suggests that Calor's design features (contracts, effects, stable IDs)"
    echo "make it easier for AI agents to perform correct refactoring operations."
elif float_lt "$calor_rate" "$csharp_rate"; then
    echo "⚠️ **C# outperformed Calor** in this benchmark run."
    echo ""
    echo "This may indicate areas where Calor's syntax or tooling needs improvement,"
    echo "or could be due to the specific tasks selected."
else
    echo "📊 **Pass rates were equal** between Calor and C#."
fi)

## Task Categories Tested

- **Extract Method**: Extracting code into new functions with contract preservation
- **Rename Symbol**: Renaming parameters while updating all references
- **Inline Function**: Inlining function calls while preserving contracts
- **Move Method**: Moving functions between modules with dependency updates
- **Add Contract**: Adding preconditions, postconditions, and effect declarations
- **Change Signature**: Modifying function signatures and updating call sites

## Detailed Results

### Calor Tasks

| Status | Count |
|--------|-------|
| Passed | $TOTAL_CALOR_PASS |
| Failed | $TOTAL_CALOR_FAIL |
| Total | $((TOTAL_CALOR_PASS + TOTAL_CALOR_FAIL)) |

### C# Tasks

| Status | Count |
|--------|-------|
| Passed | $TOTAL_CSHARP_PASS |
| Failed | $TOTAL_CSHARP_FAIL |
| Total | $((TOTAL_CSHARP_PASS + TOTAL_CSHARP_FAIL)) |

## Methodology

- Each task runs 3 times with majority voting (2/3 required to pass)
- Both Calor and C# tasks use equivalent prompts adjusted for language syntax
- Verification includes:
  - Compilation check (both languages)
  - Z3 contract verification (Calor only, when contracts are involved)

## Raw Results

- Calor results: \`$CALOR_RESULTS\`
- C# results: \`$CSHARP_RESULTS\`

---

*Generated by Calor Refactoring Benchmark Suite*
EOF

    log_info "Report generated: $REPORT_FILE"
}

# Print summary to console
print_benchmark_summary() {
    local calor_rate csharp_rate

    calor_rate=$(calculate_pass_rate "$TOTAL_CALOR_PASS" "$TOTAL_CALOR_FAIL")
    csharp_rate=$(calculate_pass_rate "$TOTAL_CSHARP_PASS" "$TOTAL_CSHARP_FAIL")

    echo ""
    echo "============================================"
    echo "Refactoring Benchmark Summary"
    echo "============================================"
    echo ""
    echo "           Calor        C#"
    echo "           -----        --"
    echo -e "Passed:    ${GREEN}$TOTAL_CALOR_PASS${NC}            ${GREEN}$TOTAL_CSHARP_PASS${NC}"
    echo -e "Failed:    ${RED}$TOTAL_CALOR_FAIL${NC}            ${RED}$TOTAL_CSHARP_FAIL${NC}"
    echo "--------------------------------------------"
    echo -e "Pass Rate: ${BOLD}${calor_rate}%${NC}       ${BOLD}${csharp_rate}%${NC}"
    echo ""

    if float_gt "$calor_rate" "$csharp_rate"; then
        echo -e "${GREEN}✓ Calor demonstrated better refactoring success${NC}"
    elif float_lt "$calor_rate" "$csharp_rate"; then
        echo -e "${YELLOW}⚠ C# had higher pass rate in this run${NC}"
    else
        echo -e "${BLUE}≡ Pass rates were equal${NC}"
    fi

    echo ""
    echo "Full report: $REPORT_FILE"
    echo "============================================"
}

# ============================================================================
# USAGE
# ============================================================================

print_benchmark_usage() {
    cat << EOF
Calor vs C# Refactoring Benchmark

Usage: $0 [OPTIONS]

Options:
  --quick           Run each task once (no majority voting) - faster
  --category <cat>  Run specific category: extract, rename, inline, move, contract, signature
  --verbose         Enable verbose debug output
  --help            Show this help message

Examples:
  $0                     # Full benchmark (40 tasks, 3 runs each)
  $0 --quick             # Quick run (40 tasks, 1 run each)
  $0 --category extract  # Only extract-method tasks
  $0 --verbose           # With debug output

Output:
  Results are saved to: $RESULTS_DIR/
  - comparison-<timestamp>.md  - Comparison report
  - calor-<timestamp>.json     - Raw Calor results
  - csharp-<timestamp>.json    - Raw C# results
EOF
}

# ============================================================================
# MAIN
# ============================================================================

main() {
    local quick=false
    local category=""
    VERBOSE=false

    # Parse arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            --quick)
                quick=true
                shift
                ;;
            --category)
                category="$2"
                shift 2
                ;;
            --verbose|-v)
                VERBOSE=true
                export VERBOSE
                shift
                ;;
            --help|-h)
                print_benchmark_usage
                exit 0
                ;;
            *)
                echo "Unknown option: $1"
                print_benchmark_usage
                exit 1
                ;;
        esac
    done

    echo ""
    echo "============================================"
    echo "Calor vs C# Refactoring Benchmark"
    echo "============================================"
    echo ""

    # Initialize
    check_jq
    init_results

    local single_run="false"
    if $quick; then
        single_run="true"
        log_info "Running in quick mode (single run per task)"
    else
        log_info "Running in full mode (3 runs per task with majority voting)"
    fi
    echo ""

    # Run Calor benchmark
    echo "============================================"
    echo "Phase 1: Calor Refactoring Tasks"
    echo "============================================"
    local calor_output
    calor_output=$(run_language_benchmark "calor" "$category" "$single_run")
    echo "$calor_output" > "$CALOR_RESULTS"
    parse_results "$calor_output" "calor"
    echo ""

    # Run C# benchmark
    echo "============================================"
    echo "Phase 2: C# Refactoring Tasks"
    echo "============================================"
    local csharp_output
    csharp_output=$(run_language_benchmark "csharp" "$category" "$single_run")
    echo "$csharp_output" > "$CSHARP_RESULTS"
    parse_results "$csharp_output" "csharp"
    echo ""

    # Generate comparison report
    generate_report

    # Print summary
    print_benchmark_summary
}

main "$@"
