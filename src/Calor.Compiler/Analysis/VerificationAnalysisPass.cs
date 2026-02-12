using Calor.Compiler.Analysis.BugPatterns;
using Calor.Compiler.Analysis.Dataflow;
using Calor.Compiler.Analysis.Dataflow.Analyses;
using Calor.Compiler.Analysis.Security;
using Calor.Compiler.Ast;
using Calor.Compiler.Binding;
using Calor.Compiler.Diagnostics;
using Calor.Compiler.Verification.Z3.KInduction;

namespace Calor.Compiler.Analysis;

/// <summary>
/// Options for verification analyses.
/// </summary>
public sealed class VerificationAnalysisOptions
{
    /// <summary>
    /// Enable dataflow analyses (uninitialized variables, dead code).
    /// </summary>
    public bool EnableDataflow { get; init; } = true;

    /// <summary>
    /// Enable bug pattern detection (div by zero, null deref, etc.).
    /// </summary>
    public bool EnableBugPatterns { get; init; } = true;

    /// <summary>
    /// Enable security taint analysis.
    /// </summary>
    public bool EnableTaintAnalysis { get; init; } = true;

    /// <summary>
    /// Enable loop invariant synthesis with k-induction.
    /// </summary>
    public bool EnableKInduction { get; init; } = false; // Off by default - expensive

    /// <summary>
    /// Use Z3 SMT solver for precise analysis (slower but more accurate).
    /// </summary>
    public bool UseZ3Verification { get; init; } = true;

    /// <summary>
    /// Z3 solver timeout in milliseconds.
    /// </summary>
    public uint Z3TimeoutMs { get; init; } = 5000;

    /// <summary>
    /// Bug pattern detection options.
    /// </summary>
    public BugPatternOptions? BugPatternOptions { get; init; }

    /// <summary>
    /// Taint analysis options.
    /// </summary>
    public TaintAnalysisOptions? TaintOptions { get; init; }

    /// <summary>
    /// K-induction options.
    /// </summary>
    public KInductionOptions? KInductionOptions { get; init; }

    public static VerificationAnalysisOptions Default => new();

    public static VerificationAnalysisOptions Fast => new()
    {
        UseZ3Verification = false,
        EnableKInduction = false
    };

    public static VerificationAnalysisOptions Thorough => new()
    {
        EnableDataflow = true,
        EnableBugPatterns = true,
        EnableTaintAnalysis = true,
        EnableKInduction = true,
        UseZ3Verification = true,
        Z3TimeoutMs = 10000
    };
}

/// <summary>
/// Results of verification analyses.
/// </summary>
public sealed class VerificationAnalysisResult
{
    /// <summary>
    /// Number of functions analyzed.
    /// </summary>
    public int FunctionsAnalyzed { get; init; }

    /// <summary>
    /// Number of dataflow issues found.
    /// </summary>
    public int DataflowIssues { get; init; }

    /// <summary>
    /// Number of bug patterns found.
    /// </summary>
    public int BugPatternsFound { get; init; }

    /// <summary>
    /// Number of taint vulnerabilities found.
    /// </summary>
    public int TaintVulnerabilities { get; init; }

    /// <summary>
    /// Number of loop invariants synthesized.
    /// </summary>
    public int LoopInvariantsSynthesized { get; init; }

    /// <summary>
    /// Analysis duration.
    /// </summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Comprehensive verification analysis pass that combines dataflow,
/// bug patterns, taint tracking, and loop analysis.
/// </summary>
public sealed class VerificationAnalysisPass
{
    private readonly DiagnosticBag _diagnostics;
    private readonly VerificationAnalysisOptions _options;

    public VerificationAnalysisPass(DiagnosticBag diagnostics, VerificationAnalysisOptions? options = null)
    {
        _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        _options = options ?? VerificationAnalysisOptions.Default;
    }

    /// <summary>
    /// Runs verification analyses on an AST module by first binding it.
    /// </summary>
    public VerificationAnalysisResult Analyze(ModuleNode module)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Bind the module to get bound nodes
        var bindingDiagnostics = new DiagnosticBag();
        var binder = new Binder(bindingDiagnostics);
        var boundModule = binder.Bind(module);

        // Run analyses on the bound module
        var result = AnalyzeBound(boundModule);

        sw.Stop();
        return new VerificationAnalysisResult
        {
            FunctionsAnalyzed = result.FunctionsAnalyzed,
            DataflowIssues = result.DataflowIssues,
            BugPatternsFound = result.BugPatternsFound,
            TaintVulnerabilities = result.TaintVulnerabilities,
            LoopInvariantsSynthesized = result.LoopInvariantsSynthesized,
            Duration = sw.Elapsed
        };
    }

    /// <summary>
    /// Runs verification analyses on an already-bound module.
    /// </summary>
    public VerificationAnalysisResult AnalyzeBound(BoundModule module)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var dataflowIssues = 0;
        var bugPatternsFound = 0;
        var taintVulnerabilities = 0;
        var loopInvariants = 0;

        foreach (var function in module.Functions)
        {
            // Dataflow analysis
            if (_options.EnableDataflow)
            {
                dataflowIssues += RunDataflowAnalysis(function);
            }

            // Bug pattern detection
            if (_options.EnableBugPatterns)
            {
                var bugOptions = _options.BugPatternOptions ?? new BugPatternOptions
                {
                    UseZ3Verification = _options.UseZ3Verification,
                    Z3TimeoutMs = _options.Z3TimeoutMs
                };
                var bugRunner = new BugPatternRunner(_diagnostics, bugOptions);
                var beforeCount = _diagnostics.Count;
                bugRunner.CheckFunction(function);
                bugPatternsFound += _diagnostics.Count - beforeCount;
            }

            // Taint analysis
            if (_options.EnableTaintAnalysis)
            {
                var taintOptions = _options.TaintOptions ?? TaintAnalysisOptions.Default;
                var taintAnalysis = new TaintAnalysis(function, taintOptions);
                taintVulnerabilities += taintAnalysis.Vulnerabilities.Count;
                taintAnalysis.ReportDiagnostics(_diagnostics);
            }

            // K-induction for loops
            if (_options.EnableKInduction)
            {
                var kOptions = _options.KInductionOptions ?? new KInductionOptions
                {
                    TimeoutMs = _options.Z3TimeoutMs
                };
                var loopRunner = new LoopAnalysisRunner(_diagnostics, kOptions);
                var beforeCount = _diagnostics.Count;
                loopRunner.AnalyzeFunction(function);
                // Count synthesized invariants (info diagnostics)
                loopInvariants = _diagnostics.Skip(beforeCount)
                    .Count(d => d.Code == DiagnosticCode.LoopInvariantSynthesized);
            }
        }

        sw.Stop();
        return new VerificationAnalysisResult
        {
            FunctionsAnalyzed = module.Functions.Count,
            DataflowIssues = dataflowIssues,
            BugPatternsFound = bugPatternsFound,
            TaintVulnerabilities = taintVulnerabilities,
            LoopInvariantsSynthesized = loopInvariants,
            Duration = sw.Elapsed
        };
    }

    private int RunDataflowAnalysis(BoundFunction function)
    {
        var issueCount = 0;

        try
        {
            // Build CFG
            var cfg = ControlFlowGraph.Build(function);

            // Get parameter names for initialization analysis
            var paramNames = function.Symbol.Parameters.Select(p => p.Name);

            // Uninitialized variable analysis
            var uninitAnalysis = new UninitializedVariablesAnalysis(cfg, paramNames);
            uninitAnalysis.ReportDiagnostics(_diagnostics);
            issueCount += uninitAnalysis.UninitializedUses.Count;

            // Live variable analysis for dead store detection
            var liveAnalysis = new LiveVariablesAnalysis(cfg);
            foreach (var (block, stmt, variable) in liveAnalysis.FindDeadAssignments())
            {
                // Skip loop variables and parameters
                if (function.Symbol.Parameters.Any(p => p.Name == variable))
                    continue;

                _diagnostics.ReportWarning(
                    stmt.Span,
                    DiagnosticCode.DeadStore,
                    $"Assignment to '{variable}' is never read (dead store)");
                issueCount++;
            }
        }
        catch
        {
            // Ignore analysis failures - continue with other analyses
        }

        return issueCount;
    }
}
