using System.Text.Json;

namespace Calor.Evaluation.LlmTasks.Execution;

/// <summary>
/// Verifies execution outputs against expected values.
/// Handles various types and comparison strategies.
/// </summary>
public sealed class OutputVerifier
{
    private readonly JsonSerializerOptions _jsonOptions;

    public OutputVerifier()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Verifies that the actual output matches the expected output.
    /// </summary>
    /// <param name="actual">The actual execution result.</param>
    /// <param name="expected">The expected value as JsonElement.</param>
    /// <returns>Verification result.</returns>
    public VerificationResult Verify(ExecutionResult actual, JsonElement expected)
    {
        if (!actual.Success)
        {
            return new VerificationResult
            {
                Passed = false,
                Reason = actual.Error ?? "Execution failed",
                ActualValue = null,
                ExpectedValue = expected.ToString()
            };
        }

        return VerifyValue(actual.ReturnValue, expected);
    }

    /// <summary>
    /// Verifies a value against an expected JsonElement.
    /// </summary>
    public VerificationResult VerifyValue(object? actual, JsonElement expected)
    {
        try
        {
            var actualJson = actual == null ? "null" : JsonSerializer.Serialize(actual);
            var expectedJson = expected.GetRawText();

            // Handle different JSON value kinds
            return expected.ValueKind switch
            {
                JsonValueKind.Null => VerifyNull(actual, expectedJson),
                JsonValueKind.True or JsonValueKind.False => VerifyBoolean(actual, expected, expectedJson),
                JsonValueKind.Number => VerifyNumber(actual, expected, expectedJson),
                JsonValueKind.String => VerifyString(actual, expected, expectedJson),
                JsonValueKind.Array => VerifyArray(actual, expected, actualJson, expectedJson),
                JsonValueKind.Object => VerifyObject(actual, expected, actualJson, expectedJson),
                _ => new VerificationResult
                {
                    Passed = false,
                    Reason = $"Unsupported expected value kind: {expected.ValueKind}",
                    ActualValue = actualJson,
                    ExpectedValue = expectedJson
                }
            };
        }
        catch (Exception ex)
        {
            return new VerificationResult
            {
                Passed = false,
                Reason = $"Verification error: {ex.Message}",
                ActualValue = actual?.ToString(),
                ExpectedValue = expected.ToString()
            };
        }
    }

    private VerificationResult VerifyNull(object? actual, string expectedJson)
    {
        var passed = actual == null;
        return new VerificationResult
        {
            Passed = passed,
            Reason = passed ? null : "Expected null but got a value",
            ActualValue = actual?.ToString() ?? "null",
            ExpectedValue = expectedJson
        };
    }

    private VerificationResult VerifyBoolean(object? actual, JsonElement expected, string expectedJson)
    {
        var expectedBool = expected.GetBoolean();
        var actualBool = actual switch
        {
            bool b => b,
            int i => i != 0,
            long l => l != 0,
            _ => null as bool?
        };

        if (actualBool == null)
        {
            return new VerificationResult
            {
                Passed = false,
                Reason = $"Expected boolean but got {actual?.GetType().Name ?? "null"}",
                ActualValue = actual?.ToString() ?? "null",
                ExpectedValue = expectedJson
            };
        }

        var passed = actualBool == expectedBool;
        return new VerificationResult
        {
            Passed = passed,
            Reason = passed ? null : $"Expected {expectedBool} but got {actualBool}",
            ActualValue = actualBool.ToString()!.ToLowerInvariant(),
            ExpectedValue = expectedJson
        };
    }

    private VerificationResult VerifyNumber(object? actual, JsonElement expected, string expectedJson)
    {
        // Try to compare as decimal for precision
        if (!TryGetDecimal(actual, out var actualDecimal))
        {
            return new VerificationResult
            {
                Passed = false,
                Reason = $"Expected number but got {actual?.GetType().Name ?? "null"}",
                ActualValue = actual?.ToString() ?? "null",
                ExpectedValue = expectedJson
            };
        }

        var expectedDecimal = expected.GetDecimal();
        var passed = actualDecimal == expectedDecimal;

        // If not exact match, try floating point comparison with tolerance
        if (!passed)
        {
            var tolerance = 1e-10m;
            passed = Math.Abs(actualDecimal - expectedDecimal) < tolerance;
        }

        return new VerificationResult
        {
            Passed = passed,
            Reason = passed ? null : $"Expected {expectedDecimal} but got {actualDecimal}",
            ActualValue = actualDecimal.ToString(),
            ExpectedValue = expectedJson
        };
    }

    private VerificationResult VerifyString(object? actual, JsonElement expected, string expectedJson)
    {
        var expectedString = expected.GetString();
        var actualString = actual?.ToString();

        var passed = actualString == expectedString;
        return new VerificationResult
        {
            Passed = passed,
            Reason = passed ? null : $"Expected \"{expectedString}\" but got \"{actualString}\"",
            ActualValue = actualString ?? "null",
            ExpectedValue = expectedJson
        };
    }

    private VerificationResult VerifyArray(object? actual, JsonElement expected, string actualJson, string expectedJson)
    {
        if (actual == null)
        {
            return new VerificationResult
            {
                Passed = false,
                Reason = "Expected array but got null",
                ActualValue = "null",
                ExpectedValue = expectedJson
            };
        }

        // Serialize actual to JSON and compare
        var actualElement = JsonDocument.Parse(actualJson).RootElement;

        if (actualElement.ValueKind != JsonValueKind.Array)
        {
            return new VerificationResult
            {
                Passed = false,
                Reason = $"Expected array but got {actualElement.ValueKind}",
                ActualValue = actualJson,
                ExpectedValue = expectedJson
            };
        }

        var actualArray = actualElement.EnumerateArray().ToList();
        var expectedArray = expected.EnumerateArray().ToList();

        if (actualArray.Count != expectedArray.Count)
        {
            return new VerificationResult
            {
                Passed = false,
                Reason = $"Array length mismatch: expected {expectedArray.Count} but got {actualArray.Count}",
                ActualValue = actualJson,
                ExpectedValue = expectedJson
            };
        }

        for (var i = 0; i < expectedArray.Count; i++)
        {
            var elementResult = VerifyJsonElements(actualArray[i], expectedArray[i]);
            if (!elementResult.Passed)
            {
                return new VerificationResult
                {
                    Passed = false,
                    Reason = $"Array element [{i}] mismatch: {elementResult.Reason}",
                    ActualValue = actualJson,
                    ExpectedValue = expectedJson
                };
            }
        }

        return new VerificationResult
        {
            Passed = true,
            ActualValue = actualJson,
            ExpectedValue = expectedJson
        };
    }

    private VerificationResult VerifyObject(object? actual, JsonElement expected, string actualJson, string expectedJson)
    {
        if (actual == null)
        {
            return new VerificationResult
            {
                Passed = false,
                Reason = "Expected object but got null",
                ActualValue = "null",
                ExpectedValue = expectedJson
            };
        }

        var actualElement = JsonDocument.Parse(actualJson).RootElement;

        if (actualElement.ValueKind != JsonValueKind.Object)
        {
            return new VerificationResult
            {
                Passed = false,
                Reason = $"Expected object but got {actualElement.ValueKind}",
                ActualValue = actualJson,
                ExpectedValue = expectedJson
            };
        }

        // Compare all expected properties
        foreach (var expectedProperty in expected.EnumerateObject())
        {
            if (!actualElement.TryGetProperty(expectedProperty.Name, out var actualProperty))
            {
                return new VerificationResult
                {
                    Passed = false,
                    Reason = $"Missing property: {expectedProperty.Name}",
                    ActualValue = actualJson,
                    ExpectedValue = expectedJson
                };
            }

            var propertyResult = VerifyJsonElements(actualProperty, expectedProperty.Value);
            if (!propertyResult.Passed)
            {
                return new VerificationResult
                {
                    Passed = false,
                    Reason = $"Property '{expectedProperty.Name}' mismatch: {propertyResult.Reason}",
                    ActualValue = actualJson,
                    ExpectedValue = expectedJson
                };
            }
        }

        return new VerificationResult
        {
            Passed = true,
            ActualValue = actualJson,
            ExpectedValue = expectedJson
        };
    }

    private VerificationResult VerifyJsonElements(JsonElement actual, JsonElement expected)
    {
        if (actual.ValueKind != expected.ValueKind)
        {
            // Special case: number comparison between int/float types
            if (actual.ValueKind == JsonValueKind.Number && expected.ValueKind == JsonValueKind.Number)
            {
                var actualNum = actual.GetDecimal();
                var expectedNum = expected.GetDecimal();
                var passed = actualNum == expectedNum ||
                    Math.Abs(actualNum - expectedNum) < 1e-10m;
                return new VerificationResult
                {
                    Passed = passed,
                    Reason = passed ? null : $"Number mismatch: {actualNum} vs {expectedNum}",
                    ActualValue = actual.GetRawText(),
                    ExpectedValue = expected.GetRawText()
                };
            }

            return new VerificationResult
            {
                Passed = false,
                Reason = $"Type mismatch: {actual.ValueKind} vs {expected.ValueKind}",
                ActualValue = actual.GetRawText(),
                ExpectedValue = expected.GetRawText()
            };
        }

        return expected.ValueKind switch
        {
            JsonValueKind.Null => new VerificationResult { Passed = true },
            JsonValueKind.True or JsonValueKind.False =>
                new VerificationResult
                {
                    Passed = actual.GetBoolean() == expected.GetBoolean(),
                    Reason = actual.GetBoolean() == expected.GetBoolean() ? null : "Boolean mismatch",
                    ActualValue = actual.GetRawText(),
                    ExpectedValue = expected.GetRawText()
                },
            JsonValueKind.Number =>
                VerifyNumber(actual.GetDecimal(), expected, expected.GetRawText()),
            JsonValueKind.String =>
                new VerificationResult
                {
                    Passed = actual.GetString() == expected.GetString(),
                    Reason = actual.GetString() == expected.GetString() ? null : "String mismatch",
                    ActualValue = actual.GetRawText(),
                    ExpectedValue = expected.GetRawText()
                },
            JsonValueKind.Array => VerifyArray(
                JsonSerializer.Deserialize<object[]>(actual.GetRawText()),
                expected,
                actual.GetRawText(),
                expected.GetRawText()),
            JsonValueKind.Object => VerifyObject(
                JsonSerializer.Deserialize<Dictionary<string, object>>(actual.GetRawText()),
                expected,
                actual.GetRawText(),
                expected.GetRawText()),
            _ => new VerificationResult
            {
                Passed = false,
                Reason = $"Unsupported value kind: {expected.ValueKind}"
            }
        };
    }

    private static bool TryGetDecimal(object? value, out decimal result)
    {
        result = 0;
        if (value == null) return false;

        try
        {
            result = value switch
            {
                int i => i,
                long l => l,
                float f => (decimal)f,
                double d => (decimal)d,
                decimal dec => dec,
                short s => s,
                byte b => b,
                _ => Convert.ToDecimal(value)
            };
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Result of output verification.
/// </summary>
public record VerificationResult
{
    public bool Passed { get; init; }
    public string? Reason { get; init; }
    public string? ActualValue { get; init; }
    public string? ExpectedValue { get; init; }
}
