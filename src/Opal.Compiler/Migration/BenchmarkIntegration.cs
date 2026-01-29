namespace Opal.Compiler.Migration;

/// <summary>
/// Integrates with the evaluation framework to provide benchmark metrics during migration.
/// </summary>
public sealed class BenchmarkIntegration
{
    /// <summary>
    /// Calculates token and line metrics for source code comparison.
    /// </summary>
    public static FileMetrics CalculateMetrics(string originalSource, string convertedSource)
    {
        var originalTokens = TokenizeSource(originalSource);
        var convertedTokens = TokenizeSource(convertedSource);

        return new FileMetrics
        {
            OriginalLines = CountLines(originalSource),
            OutputLines = CountLines(convertedSource),
            OriginalTokens = originalTokens.Count,
            OutputTokens = convertedTokens.Count,
            OriginalCharacters = CountNonWhitespaceChars(originalSource),
            OutputCharacters = CountNonWhitespaceChars(convertedSource)
        };
    }

    /// <summary>
    /// Simple tokenizer that approximates LLM tokenization.
    /// </summary>
    private static List<string> TokenizeSource(string source)
    {
        var tokens = new List<string>();
        var currentToken = "";

        foreach (var ch in source)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    tokens.Add(currentToken);
                    currentToken = "";
                }
            }
            else if (char.IsPunctuation(ch) || char.IsSymbol(ch))
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    tokens.Add(currentToken);
                    currentToken = "";
                }
                tokens.Add(ch.ToString());
            }
            else
            {
                currentToken += ch;
            }
        }

        if (!string.IsNullOrEmpty(currentToken))
        {
            tokens.Add(currentToken);
        }

        return tokens;
    }

    private static int CountLines(string source)
    {
        if (string.IsNullOrEmpty(source))
            return 0;

        var lines = source.Split('\n');
        // Count non-empty lines
        return lines.Count(line => !string.IsNullOrWhiteSpace(line));
    }

    private static int CountNonWhitespaceChars(string source)
    {
        return source.Count(c => !char.IsWhiteSpace(c));
    }

    /// <summary>
    /// Calculates the advantage ratio (higher = more compact OPAL).
    /// </summary>
    public static double CalculateAdvantageRatio(FileMetrics metrics)
    {
        if (metrics.OutputTokens == 0)
            return 1.0;

        // Calculate geometric mean of ratios
        var tokenRatio = (double)metrics.OriginalTokens / metrics.OutputTokens;
        var lineRatio = metrics.OutputLines > 0 ? (double)metrics.OriginalLines / metrics.OutputLines : 1.0;
        var charRatio = metrics.OutputCharacters > 0 ? (double)metrics.OriginalCharacters / metrics.OutputCharacters : 1.0;

        return Math.Pow(tokenRatio * lineRatio * charRatio, 1.0 / 3.0);
    }

    /// <summary>
    /// Formats a benchmark comparison for console output.
    /// </summary>
    public static string FormatComparison(FileMetrics metrics)
    {
        var advantage = CalculateAdvantageRatio(metrics);

        return $"""
            Token Economics:
              Before: {metrics.OriginalTokens:N0} tokens, {metrics.OriginalLines:N0} lines
              After:  {metrics.OutputTokens:N0} tokens, {metrics.OutputLines:N0} lines
              Token Savings: {metrics.TokenReduction:F1}%
              Line Savings: {metrics.LineReduction:F1}%
              Overall Advantage: {advantage:F2}x
            """;
    }

    /// <summary>
    /// Creates a benchmark summary from multiple file metrics.
    /// </summary>
    public static BenchmarkSummary CreateSummary(IEnumerable<FileMetrics> metricsCollection)
    {
        var metricsList = metricsCollection.ToList();

        return new BenchmarkSummary
        {
            TotalOriginalTokens = metricsList.Sum(m => m.OriginalTokens),
            TotalOutputTokens = metricsList.Sum(m => m.OutputTokens),
            TotalOriginalLines = metricsList.Sum(m => m.OriginalLines),
            TotalOutputLines = metricsList.Sum(m => m.OutputLines)
        };
    }

    /// <summary>
    /// Runs a quick benchmark comparison between C# and OPAL source.
    /// </summary>
    public static BenchmarkResult RunQuickBenchmark(string csharpSource, string opalSource)
    {
        var metrics = CalculateMetrics(csharpSource, opalSource);
        var advantage = CalculateAdvantageRatio(metrics);

        return new BenchmarkResult
        {
            Metrics = metrics,
            AdvantageRatio = advantage,
            Summary = FormatComparison(metrics)
        };
    }
}

/// <summary>
/// Result of a benchmark comparison.
/// </summary>
public sealed class BenchmarkResult
{
    public required FileMetrics Metrics { get; init; }
    public required double AdvantageRatio { get; init; }
    public required string Summary { get; init; }
}
