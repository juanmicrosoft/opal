namespace Calor.Evaluation.LlmTasks.Providers;

/// <summary>
/// Interface for LLM code generation providers.
/// Implementations connect to different AI services (Claude, GPT, etc.)
/// to generate code from prompts.
/// </summary>
public interface ILlmProvider
{
    /// <summary>
    /// Provider name (e.g., "claude", "openai", "mock").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Default model to use if not specified.
    /// </summary>
    string DefaultModel { get; }

    /// <summary>
    /// Generates code from a prompt.
    /// </summary>
    /// <param name="prompt">The code generation prompt.</param>
    /// <param name="language">Target language ("calor" or "csharp").</param>
    /// <param name="options">Generation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generation result.</returns>
    Task<LlmGenerationResult> GenerateCodeAsync(
        string prompt,
        string language,
        LlmGenerationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the cost for a generation request.
    /// </summary>
    /// <param name="inputTokens">Estimated input token count.</param>
    /// <param name="outputTokens">Estimated output token count.</param>
    /// <param name="model">Model name (uses default if null).</param>
    /// <returns>Estimated cost in USD.</returns>
    decimal EstimateCost(int inputTokens, int outputTokens, string? model = null);

    /// <summary>
    /// Estimates the token count for a string.
    /// </summary>
    /// <param name="text">The text to count tokens for.</param>
    /// <returns>Estimated token count.</returns>
    int EstimateTokenCount(string text);

    /// <summary>
    /// Checks if the provider is available (e.g., API key configured).
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Gets the reason why the provider is unavailable.
    /// </summary>
    string? UnavailabilityReason { get; }
}

/// <summary>
/// Options for LLM code generation.
/// </summary>
public record LlmGenerationOptions
{
    /// <summary>
    /// Model to use (provider-specific).
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Maximum tokens to generate.
    /// </summary>
    public int MaxTokens { get; init; } = 2048;

    /// <summary>
    /// Temperature for generation (0-1).
    /// </summary>
    public double Temperature { get; init; } = 0.0;

    /// <summary>
    /// System prompt to use.
    /// </summary>
    public string? SystemPrompt { get; init; }

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 60;

    /// <summary>
    /// Whether to include thinking/reasoning in the response.
    /// </summary>
    public bool IncludeThinking { get; init; }

    /// <summary>
    /// Default options for code generation.
    /// </summary>
    public static LlmGenerationOptions Default => new()
    {
        Temperature = 0.0, // Deterministic for reproducibility
        MaxTokens = 2048,
        SystemPrompt = "You are an expert programmer. Generate only the requested code without any explanation or markdown. Output only valid source code."
    };
}

/// <summary>
/// Result of an LLM code generation request.
/// </summary>
public record LlmGenerationResult
{
    /// <summary>
    /// Whether the generation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The generated code.
    /// </summary>
    public string GeneratedCode { get; init; } = "";

    /// <summary>
    /// Error message if generation failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Provider name.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// Model used.
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
    /// Cost in USD.
    /// </summary>
    public decimal Cost { get; init; }

    /// <summary>
    /// Generation duration in milliseconds.
    /// </summary>
    public double DurationMs { get; init; }

    /// <summary>
    /// Whether this was served from cache.
    /// </summary>
    public bool FromCache { get; init; }

    /// <summary>
    /// Stop reason from the API.
    /// </summary>
    public string? StopReason { get; init; }

    /// <summary>
    /// Raw API response for debugging.
    /// </summary>
    public string? RawResponse { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static LlmGenerationResult Successful(
        string generatedCode,
        string provider,
        string? model,
        int inputTokens,
        int outputTokens,
        decimal cost,
        double durationMs,
        bool fromCache = false,
        string? stopReason = null) =>
        new()
        {
            Success = true,
            GeneratedCode = generatedCode,
            Provider = provider,
            Model = model,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            Cost = cost,
            DurationMs = durationMs,
            FromCache = fromCache,
            StopReason = stopReason
        };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static LlmGenerationResult Failed(string provider, string error) =>
        new()
        {
            Success = false,
            Error = error,
            Provider = provider
        };
}
