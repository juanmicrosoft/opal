namespace Calor.Evaluation.LlmTasks;

/// <summary>
/// Result of running a single LLM task for one language.
/// </summary>
public record LlmTaskLanguageResult
{
    /// <summary>
    /// The language (Calor or CSharp).
    /// </summary>
    public required string Language { get; init; }

    /// <summary>
    /// The generated source code.
    /// </summary>
    public required string GeneratedCode { get; init; }

    /// <summary>
    /// Whether the code compiled successfully.
    /// </summary>
    public bool CompilationSuccess { get; init; }

    /// <summary>
    /// Compilation errors, if any.
    /// </summary>
    public List<string> CompilationErrors { get; init; } = new();

    /// <summary>
    /// Results for each test case.
    /// </summary>
    public List<TestCaseResult> TestResults { get; init; } = new();

    /// <summary>
    /// Contract verification results (Calor only).
    /// </summary>
    public ContractVerificationResults? ContractResults { get; init; }

    /// <summary>
    /// LLM generation metadata.
    /// </summary>
    public LlmGenerationMetadata? GenerationMetadata { get; init; }

    /// <summary>
    /// Calculated score (0-1).
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Number of test cases that passed.
    /// </summary>
    public int PassedTests => TestResults.Count(t => t.Passed);

    /// <summary>
    /// Total number of test cases.
    /// </summary>
    public int TotalTests => TestResults.Count;

    /// <summary>
    /// Test pass rate (0-1).
    /// </summary>
    public double TestPassRate => TotalTests > 0 ? (double)PassedTests / TotalTests : 0;
}

/// <summary>
/// Result of a single test case execution.
/// </summary>
public record TestCaseResult
{
    /// <summary>
    /// Index of the test case.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Whether the test passed.
    /// </summary>
    public bool Passed { get; init; }

    /// <summary>
    /// The actual output value (serialized).
    /// </summary>
    public string? ActualOutput { get; init; }

    /// <summary>
    /// The expected output value (serialized).
    /// </summary>
    public string? ExpectedOutput { get; init; }

    /// <summary>
    /// Error message if the test failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Execution time in milliseconds.
    /// </summary>
    public double ExecutionTimeMs { get; init; }

    /// <summary>
    /// Whether execution timed out.
    /// </summary>
    public bool TimedOut { get; init; }

    /// <summary>
    /// Whether a contract violation occurred (expected or unexpected).
    /// </summary>
    public bool ContractViolation { get; init; }
}

/// <summary>
/// Contract verification results for Calor code.
/// </summary>
public record ContractVerificationResults
{
    /// <summary>
    /// Number of contracts proven statically.
    /// </summary>
    public int Proven { get; init; }

    /// <summary>
    /// Number of contracts disproven.
    /// </summary>
    public int Disproven { get; init; }

    /// <summary>
    /// Number of contracts that couldn't be verified.
    /// </summary>
    public int Unproven { get; init; }

    /// <summary>
    /// Total contracts.
    /// </summary>
    public int Total => Proven + Disproven + Unproven;

    /// <summary>
    /// Verification score (0-1).
    /// </summary>
    public double Score => Total > 0 ? (Proven * 1.0 + Unproven * 0.5) / Total : 1.0;
}

/// <summary>
/// Metadata about LLM code generation.
/// </summary>
public record LlmGenerationMetadata
{
    /// <summary>
    /// Provider name (e.g., "claude", "mock").
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// Model name used.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Input token count.
    /// </summary>
    public int InputTokens { get; init; }

    /// <summary>
    /// Output token count.
    /// </summary>
    public int OutputTokens { get; init; }

    /// <summary>
    /// Estimated cost in USD.
    /// </summary>
    public decimal Cost { get; init; }

    /// <summary>
    /// Generation duration in milliseconds.
    /// </summary>
    public double DurationMs { get; init; }

    /// <summary>
    /// Whether this was a cached response.
    /// </summary>
    public bool FromCache { get; init; }

    /// <summary>
    /// Generation timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Complete result of running a single LLM task.
/// </summary>
public record LlmTaskResult
{
    /// <summary>
    /// The task definition.
    /// </summary>
    public required LlmTaskDefinition Task { get; init; }

    /// <summary>
    /// Result for Calor code generation.
    /// </summary>
    public required LlmTaskLanguageResult CalorResult { get; init; }

    /// <summary>
    /// Result for C# code generation.
    /// </summary>
    public required LlmTaskLanguageResult CSharpResult { get; init; }

    /// <summary>
    /// Calor advantage ratio (CalorScore / CSharpScore).
    /// </summary>
    public double AdvantageRatio =>
        CSharpResult.Score > 0 ? CalorResult.Score / CSharpResult.Score : 1.0;

    /// <summary>
    /// Whether Calor outperformed C#.
    /// </summary>
    public bool CalorWins => CalorResult.Score > CSharpResult.Score;

    /// <summary>
    /// Whether C# outperformed Calor.
    /// </summary>
    public bool CSharpWins => CSharpResult.Score > CalorResult.Score;

    /// <summary>
    /// Whether both achieved the same score.
    /// </summary>
    public bool Tie => Math.Abs(CalorResult.Score - CSharpResult.Score) < 0.001;

    /// <summary>
    /// Total cost for this task (both languages).
    /// </summary>
    public decimal TotalCost =>
        (CalorResult.GenerationMetadata?.Cost ?? 0) +
        (CSharpResult.GenerationMetadata?.Cost ?? 0);
}

/// <summary>
/// Aggregated results from running all LLM tasks.
/// </summary>
public record LlmTaskRunResults
{
    /// <summary>
    /// Individual task results.
    /// </summary>
    public List<LlmTaskResult> Results { get; init; } = new();

    /// <summary>
    /// Summary statistics.
    /// </summary>
    public LlmTaskRunSummary Summary { get; init; } = new();

    /// <summary>
    /// Total cost for the run.
    /// </summary>
    public decimal TotalCost => Results.Sum(r => r.TotalCost);

    /// <summary>
    /// Run timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Provider used.
    /// </summary>
    public string? Provider { get; init; }

    /// <summary>
    /// Whether this was a dry run.
    /// </summary>
    public bool IsDryRun { get; init; }
}

/// <summary>
/// Summary statistics for an LLM task run.
/// </summary>
public record LlmTaskRunSummary
{
    /// <summary>
    /// Total number of tasks.
    /// </summary>
    public int TotalTasks { get; init; }

    /// <summary>
    /// Number of tasks where Calor won.
    /// </summary>
    public int CalorWins { get; init; }

    /// <summary>
    /// Number of tasks where C# won.
    /// </summary>
    public int CSharpWins { get; init; }

    /// <summary>
    /// Number of ties.
    /// </summary>
    public int Ties { get; init; }

    /// <summary>
    /// Average Calor score.
    /// </summary>
    public double AverageCalorScore { get; init; }

    /// <summary>
    /// Average C# score.
    /// </summary>
    public double AverageCSharpScore { get; init; }

    /// <summary>
    /// Overall Calor advantage ratio.
    /// </summary>
    public double OverallAdvantageRatio { get; init; }

    /// <summary>
    /// Calor compilation success rate.
    /// </summary>
    public double CalorCompilationRate { get; init; }

    /// <summary>
    /// C# compilation success rate.
    /// </summary>
    public double CSharpCompilationRate { get; init; }

    /// <summary>
    /// Calor test pass rate.
    /// </summary>
    public double CalorTestPassRate { get; init; }

    /// <summary>
    /// C# test pass rate.
    /// </summary>
    public double CSharpTestPassRate { get; init; }

    /// <summary>
    /// Category-wise results.
    /// </summary>
    public Dictionary<string, CategorySummary> ByCategory { get; init; } = new();
}

/// <summary>
/// Summary for a specific task category.
/// </summary>
public record CategorySummary
{
    /// <summary>
    /// Category name.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Number of tasks in this category.
    /// </summary>
    public int TaskCount { get; init; }

    /// <summary>
    /// Average Calor score.
    /// </summary>
    public double AverageCalorScore { get; init; }

    /// <summary>
    /// Average C# score.
    /// </summary>
    public double AverageCSharpScore { get; init; }

    /// <summary>
    /// Category advantage ratio.
    /// </summary>
    public double AdvantageRatio { get; init; }
}
