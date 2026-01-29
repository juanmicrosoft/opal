using System.CommandLine;
using Opal.Compiler.Migration;

namespace Opal.Compiler.Commands;

/// <summary>
/// CLI command for standalone benchmarking.
/// </summary>
public static class BenchmarkCommand
{
    public static Command Create()
    {
        var opalOption = new Option<FileInfo?>(
            aliases: new[] { "--opal" },
            description: "The OPAL file to benchmark");

        var csharpOption = new Option<FileInfo?>(
            aliases: new[] { "--csharp", "--cs" },
            description: "The C# file to benchmark");

        var projectArgument = new Argument<DirectoryInfo?>(
            name: "project",
            description: "The project directory to benchmark (optional)")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

        var formatOption = new Option<string>(
            aliases: new[] { "--format", "-f" },
            description: "Output format (console, markdown, json)",
            getDefaultValue: () => "console");

        var outputOption = new Option<FileInfo?>(
            aliases: new[] { "--output", "-o" },
            description: "Save benchmark results to file");

        var command = new Command("benchmark", "Compare token economics between C# and OPAL")
        {
            projectArgument,
            opalOption,
            csharpOption,
            formatOption,
            outputOption
        };

        command.SetHandler(ExecuteAsync, projectArgument, opalOption, csharpOption, formatOption, outputOption);

        return command;
    }

    private static async Task ExecuteAsync(
        DirectoryInfo? project,
        FileInfo? opalFile,
        FileInfo? csharpFile,
        string format,
        FileInfo? output)
    {
        try
        {
            if (opalFile != null && csharpFile != null)
            {
                // Single file comparison
                await BenchmarkFilesAsync(opalFile, csharpFile, format, output);
            }
            else if (project != null)
            {
                // Project-level comparison
                await BenchmarkProjectAsync(project, format, output);
            }
            else
            {
                Console.Error.WriteLine("Error: Provide either --opal and --csharp files, or a project directory.");
                Console.Error.WriteLine();
                Console.Error.WriteLine("Examples:");
                Console.Error.WriteLine("  opalc benchmark --opal file.opal --csharp file.cs");
                Console.Error.WriteLine("  opalc benchmark ./MyProject");
                Environment.ExitCode = 1;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }

    private static async Task BenchmarkFilesAsync(FileInfo opalFile, FileInfo csharpFile, string format, FileInfo? output)
    {
        if (!opalFile.Exists)
        {
            Console.Error.WriteLine($"Error: OPAL file not found: {opalFile.FullName}");
            Environment.ExitCode = 1;
            return;
        }

        if (!csharpFile.Exists)
        {
            Console.Error.WriteLine($"Error: C# file not found: {csharpFile.FullName}");
            Environment.ExitCode = 1;
            return;
        }

        var opalSource = await File.ReadAllTextAsync(opalFile.FullName);
        var csharpSource = await File.ReadAllTextAsync(csharpFile.FullName);

        var result = BenchmarkIntegration.RunQuickBenchmark(csharpSource, opalSource);

        var outputContent = format.ToLowerInvariant() switch
        {
            "markdown" or "md" => FormatMarkdown(opalFile.Name, csharpFile.Name, result),
            "json" => FormatJson(opalFile.Name, csharpFile.Name, result),
            _ => FormatConsole(opalFile.Name, csharpFile.Name, result)
        };

        if (output != null)
        {
            await File.WriteAllTextAsync(output.FullName, outputContent);
            Console.WriteLine($"Results saved to: {output.FullName}");
        }
        else
        {
            Console.WriteLine(outputContent);
        }
    }

    private static async Task BenchmarkProjectAsync(DirectoryInfo project, string format, FileInfo? output)
    {
        if (!project.Exists)
        {
            Console.Error.WriteLine($"Error: Project directory not found: {project.FullName}");
            Environment.ExitCode = 1;
            return;
        }

        Console.WriteLine($"Scanning project: {project.FullName}");
        Console.WriteLine();

        // Find paired files
        var opalFiles = Directory.GetFiles(project.FullName, "*.opal", SearchOption.AllDirectories);
        var pairs = new List<(string opal, string cs, FileMetrics metrics)>();

        foreach (var opalPath in opalFiles)
        {
            var baseName = Path.GetFileNameWithoutExtension(opalPath);
            var directory = Path.GetDirectoryName(opalPath) ?? ".";

            // Look for matching C# file
            var csPath = Path.Combine(directory, baseName + ".cs");
            var gcsPath = Path.Combine(directory, baseName + ".g.cs");

            string? matchingCs = null;
            if (File.Exists(csPath))
                matchingCs = csPath;
            else if (File.Exists(gcsPath))
                matchingCs = gcsPath;

            if (matchingCs != null)
            {
                var opalSource = await File.ReadAllTextAsync(opalPath);
                var csSource = await File.ReadAllTextAsync(matchingCs);
                var metrics = BenchmarkIntegration.CalculateMetrics(csSource, opalSource);

                pairs.Add((opalPath, matchingCs, metrics));
            }
        }

        if (pairs.Count == 0)
        {
            Console.WriteLine("No paired .opal and .cs files found.");
            Console.WriteLine("Looking for files with the same base name (e.g., foo.opal and foo.cs)");
            return;
        }

        // Calculate summary
        var summary = BenchmarkIntegration.CreateSummary(pairs.Select(p => p.metrics));

        // Output results
        var outputContent = format.ToLowerInvariant() switch
        {
            "markdown" or "md" => FormatProjectMarkdown(project.Name, pairs, summary),
            "json" => FormatProjectJson(project.Name, pairs, summary),
            _ => FormatProjectConsole(project.Name, pairs, summary)
        };

        if (output != null)
        {
            await File.WriteAllTextAsync(output.FullName, outputContent);
            Console.WriteLine($"Results saved to: {output.FullName}");
        }
        else
        {
            Console.WriteLine(outputContent);
        }
    }

    private static string FormatConsole(string opalName, string csName, BenchmarkResult result)
    {
        return $"""
            Benchmark: {csName} vs {opalName}

            {result.Summary}
            """;
    }

    private static string FormatMarkdown(string opalName, string csName, BenchmarkResult result)
    {
        var m = result.Metrics;
        return $"""
            # Benchmark Results

            **Comparison:** `{csName}` vs `{opalName}`

            | Metric | C# | OPAL | Savings |
            |--------|-----|------|---------|
            | Tokens | {m.OriginalTokens:N0} | {m.OutputTokens:N0} | {m.TokenReduction:F1}% |
            | Lines | {m.OriginalLines:N0} | {m.OutputLines:N0} | {m.LineReduction:F1}% |
            | Characters | {m.OriginalCharacters:N0} | {m.OutputCharacters:N0} | {m.CharReduction:F1}% |

            **Overall OPAL Advantage:** {result.AdvantageRatio:F2}x
            """;
    }

    private static string FormatJson(string opalName, string csName, BenchmarkResult result)
    {
        var m = result.Metrics;
        return $$"""
            {
              "comparison": {
                "csharpFile": "{{csName}}",
                "opalFile": "{{opalName}}"
              },
              "metrics": {
                "tokens": { "csharp": {{m.OriginalTokens}}, "opal": {{m.OutputTokens}}, "savings": {{m.TokenReduction:F1}} },
                "lines": { "csharp": {{m.OriginalLines}}, "opal": {{m.OutputLines}}, "savings": {{m.LineReduction:F1}} },
                "characters": { "csharp": {{m.OriginalCharacters}}, "opal": {{m.OutputCharacters}}, "savings": {{m.CharReduction:F1}} }
              },
              "advantageRatio": {{result.AdvantageRatio:F2}}
            }
            """;
    }

    private static string FormatProjectConsole(string projectName, List<(string opal, string cs, FileMetrics metrics)> pairs, BenchmarkSummary summary)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"Project Benchmark: {projectName}");
        sb.AppendLine($"Files compared: {pairs.Count}");
        sb.AppendLine();
        sb.AppendLine("Summary:");
        sb.AppendLine($"  Total tokens: {summary.TotalOriginalTokens:N0} → {summary.TotalOutputTokens:N0} ({summary.TokenSavingsPercent:F1}% savings)");
        sb.AppendLine($"  Total lines: {summary.TotalOriginalLines:N0} → {summary.TotalOutputLines:N0} ({summary.LineSavingsPercent:F1}% savings)");
        sb.AppendLine($"  Overall OPAL advantage: {summary.OverallAdvantage:F2}x");
        sb.AppendLine();
        sb.AppendLine("By File:");

        foreach (var (opal, cs, metrics) in pairs.OrderByDescending(p => BenchmarkIntegration.CalculateAdvantageRatio(p.metrics)))
        {
            var advantage = BenchmarkIntegration.CalculateAdvantageRatio(metrics);
            var indicator = advantage > 1 ? "+" : "";
            sb.AppendLine($"  {Path.GetFileName(opal)}: {indicator}{(advantage - 1) * 100:F0}% tokens ({metrics.OriginalTokens} → {metrics.OutputTokens})");
        }

        return sb.ToString();
    }

    private static string FormatProjectMarkdown(string projectName, List<(string opal, string cs, FileMetrics metrics)> pairs, BenchmarkSummary summary)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"# Benchmark Results: {projectName}");
        sb.AppendLine();
        sb.AppendLine($"**Files compared:** {pairs.Count}");
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine("| Metric | C# | OPAL | Savings |");
        sb.AppendLine("|--------|-----|------|---------|");
        sb.AppendLine($"| Tokens | {summary.TotalOriginalTokens:N0} | {summary.TotalOutputTokens:N0} | {summary.TokenSavingsPercent:F1}% |");
        sb.AppendLine($"| Lines | {summary.TotalOriginalLines:N0} | {summary.TotalOutputLines:N0} | {summary.LineSavingsPercent:F1}% |");
        sb.AppendLine();
        sb.AppendLine($"**Overall OPAL Advantage:** {summary.OverallAdvantage:F2}x");
        sb.AppendLine();
        sb.AppendLine("## By File");
        sb.AppendLine();
        sb.AppendLine("| File | C# Tokens | OPAL Tokens | Advantage |");
        sb.AppendLine("|------|-----------|-------------|-----------|");

        foreach (var (opal, cs, metrics) in pairs.OrderByDescending(p => BenchmarkIntegration.CalculateAdvantageRatio(p.metrics)))
        {
            var advantage = BenchmarkIntegration.CalculateAdvantageRatio(metrics);
            sb.AppendLine($"| {Path.GetFileName(opal)} | {metrics.OriginalTokens} | {metrics.OutputTokens} | {advantage:F2}x |");
        }

        return sb.ToString();
    }

    private static string FormatProjectJson(string projectName, List<(string opal, string cs, FileMetrics metrics)> pairs, BenchmarkSummary summary)
    {
        var filesJson = string.Join(",\n    ", pairs.Select(p =>
        {
            var advantage = BenchmarkIntegration.CalculateAdvantageRatio(p.metrics);
            return $$"""
                {
                      "opal": "{{Path.GetFileName(p.opal)}}",
                      "csharp": "{{Path.GetFileName(p.cs)}}",
                      "csharpTokens": {{p.metrics.OriginalTokens}},
                      "opalTokens": {{p.metrics.OutputTokens}},
                      "advantage": {{advantage:F2}}
                    }
                """;
        }));

        return $$"""
            {
              "project": "{{projectName}}",
              "fileCount": {{pairs.Count}},
              "summary": {
                "totalCSharpTokens": {{summary.TotalOriginalTokens}},
                "totalOpalTokens": {{summary.TotalOutputTokens}},
                "tokenSavings": {{summary.TokenSavingsPercent:F1}},
                "totalCSharpLines": {{summary.TotalOriginalLines}},
                "totalOpalLines": {{summary.TotalOutputLines}},
                "lineSavings": {{summary.LineSavingsPercent:F1}},
                "overallAdvantage": {{summary.OverallAdvantage:F2}}
              },
              "files": [
                {{filesJson}}
              ]
            }
            """;
    }
}
