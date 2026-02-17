using Calor.Evaluation.Core;
using Calor.Evaluation.LlmTasks;
using Calor.Evaluation.LlmTasks.Caching;
using Calor.Evaluation.LlmTasks.Providers;

namespace Calor.Evaluation.Metrics;

/// <summary>
/// Calculates effect discipline metrics by measuring side effect management quality.
/// This wraps the EffectDisciplineBenchmarkRunner to produce MetricResult compatible
/// with the main benchmark dashboard.
///
/// This measures how well code prevents real-world bugs caused by hidden side effects:
/// - Flaky tests (non-determinism)
/// - Security violations (unauthorized I/O)
/// - Side effect transparency (hidden effects)
/// - Cache safety (memoization correctness)
/// </summary>
public class EffectDisciplineCalculator : IMetricCalculator
{
    public string Category => "EffectDiscipline";

    public string Description =>
        "Measures side effect management quality and bug prevention for effect-related issues";

    private readonly ILlmProvider? _provider;
    private readonly LlmResponseCache? _cache;
    private readonly LlmTaskManifest? _manifest;
    private readonly EffectDisciplineOptions _options;
    private EffectDisciplineResults? _lastResults;

    /// <summary>
    /// Creates a calculator with default settings (uses estimation if no provider configured).
    /// </summary>
    public EffectDisciplineCalculator()
    {
        _options = new EffectDisciplineOptions
        {
            DryRun = true, // Default to dry run to avoid API costs
            UseCache = true
        };
    }

    /// <summary>
    /// Creates a calculator with specified provider and options.
    /// </summary>
    public EffectDisciplineCalculator(
        ILlmProvider provider,
        LlmTaskManifest manifest,
        EffectDisciplineOptions? options = null,
        LlmResponseCache? cache = null)
    {
        _provider = provider;
        _manifest = manifest;
        _cache = cache;
        _options = options ?? new EffectDisciplineOptions();
    }

    /// <summary>
    /// Gets the results from the last calculation.
    /// </summary>
    public EffectDisciplineResults? LastResults => _lastResults;

    public async Task<MetricResult> CalculateAsync(EvaluationContext context)
    {
        // If no provider or manifest configured, return estimation based on context
        if (_provider == null || _manifest == null)
        {
            return CalculateEstimatedMetric(context);
        }

        // Run actual effect discipline benchmark
        using var runner = new EffectDisciplineBenchmarkRunner(_provider, _cache);
        _lastResults = await runner.RunAllAsync(_manifest, _options);

        var summary = _lastResults.Summary;

        var details = new Dictionary<string, object>
        {
            ["totalTasks"] = summary.TotalTasks,
            ["calorWins"] = summary.CalorWins,
            ["csharpWins"] = summary.CSharpWins,
            ["ties"] = summary.Ties,
            ["calorBugPreventionRate"] = summary.CalorBugPreventionRate,
            ["csharpBugPreventionRate"] = summary.CSharpBugPreventionRate,
            ["calorCorrectnessRate"] = summary.CalorCorrectnessRate,
            ["csharpCorrectnessRate"] = summary.CSharpCorrectnessRate,
            ["provider"] = _lastResults.Provider ?? "unknown",
            ["isDryRun"] = _options.DryRun,
            ["byCategory"] = summary.ByCategory
        };

        return MetricResult.CreateHigherIsBetter(
            Category,
            "DisciplineScore",
            summary.AverageCalorDisciplineScore,
            summary.AverageCSharpDisciplineScore,
            details);
    }

    /// <summary>
    /// Calculates an estimated effect discipline metric based on code characteristics when
    /// actual LLM-based evaluation is not available.
    /// </summary>
    private MetricResult CalculateEstimatedMetric(EvaluationContext context)
    {
        var calorScore = EstimateCalorDisciplineScore(context);
        var csharpScore = EstimateCSharpDisciplineScore(context);

        var details = new Dictionary<string, object>
        {
            ["estimated"] = true,
            ["reason"] = "No LLM provider configured - using structural estimation",
            ["calorFactors"] = new Dictionary<string, object>
            {
                ["compiles"] = context.CalorCompilation.Success,
                ["hasEffectDeclarations"] = context.CalorSource.Contains("§E{"),
                ["hasPureAnnotation"] = context.CalorSource.Contains("pure") ||
                                        context.CalorSource.Contains("§E{}"),
                ["hasIoEffects"] = context.CalorSource.Contains("io") ||
                                   context.CalorSource.Contains("net") ||
                                   context.CalorSource.Contains("fs")
            },
            ["csharpFactors"] = new Dictionary<string, object>
            {
                ["compiles"] = context.CSharpCompilation.Success,
                ["hasPureAttribute"] = context.CSharpSource.Contains("[Pure]"),
                ["usesReadonly"] = context.CSharpSource.Contains("readonly"),
                ["usesStaticMethods"] = context.CSharpSource.Contains("static ")
            }
        };

        return MetricResult.CreateHigherIsBetter(
            Category,
            "EstimatedDiscipline",
            calorScore,
            csharpScore,
            details);
    }

    /// <summary>
    /// Estimates Calor discipline score based on compilation success.
    /// With outcome-based scoring, both languages get the same base score.
    /// </summary>
    private static double EstimateCalorDisciplineScore(EvaluationContext context)
    {
        // Outcome-based: compilation success = 1.0, failure = 0.0
        // No syntax-based bonuses to avoid language bias
        return context.CalorCompilation.Success ? 1.0 : 0.0;
    }

    /// <summary>
    /// Estimates C# discipline score based on compilation success.
    /// With outcome-based scoring, both languages get the same base score.
    /// </summary>
    private static double EstimateCSharpDisciplineScore(EvaluationContext context)
    {
        // Outcome-based: compilation success = 1.0, failure = 0.0
        // No cap, no syntax-based penalties - fair comparison
        return context.CSharpCompilation.Success ? 1.0 : 0.0;
    }

    /// <summary>
    /// Creates a calculator configured for actual LLM-based effect discipline evaluation.
    /// </summary>
    public static EffectDisciplineCalculator CreateWithProvider(
        ILlmProvider provider,
        LlmTaskManifest manifest,
        EffectDisciplineOptions? options = null)
    {
        return new EffectDisciplineCalculator(provider, manifest, options);
    }

    /// <summary>
    /// Creates a calculator using the Claude API (requires ANTHROPIC_API_KEY).
    /// </summary>
    public static EffectDisciplineCalculator CreateWithClaude(
        LlmTaskManifest manifest,
        EffectDisciplineOptions? options = null)
    {
        var provider = new ClaudeProvider();
        if (!provider.IsAvailable)
        {
            throw new InvalidOperationException(
                $"Claude provider unavailable: {provider.UnavailabilityReason}");
        }

        return new EffectDisciplineCalculator(provider, manifest, options);
    }
}
