using System.CommandLine;
using Opal.Compiler.Migration;

namespace Opal.Compiler.Commands;

/// <summary>
/// CLI command for single-file conversion.
/// </summary>
public static class ConvertCommand
{
    public static Command Create()
    {
        var inputArgument = new Argument<FileInfo>(
            name: "input",
            description: "The source file to convert (.cs or .opal)");

        var outputOption = new Option<FileInfo?>(
            aliases: new[] { "--output", "-o" },
            description: "The output file path (auto-detected if not specified)");

        var benchmarkOption = new Option<bool>(
            aliases: new[] { "--benchmark", "-b" },
            description: "Include benchmark metrics comparison");

        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "Enable verbose output");

        var command = new Command("convert", "Convert a single file between C# and OPAL")
        {
            inputArgument,
            outputOption,
            benchmarkOption,
            verboseOption
        };

        command.SetHandler(ExecuteAsync, inputArgument, outputOption, benchmarkOption, verboseOption);

        return command;
    }

    private static async Task ExecuteAsync(FileInfo input, FileInfo? output, bool benchmark, bool verbose)
    {
        if (!input.Exists)
        {
            Console.Error.WriteLine($"Error: Input file not found: {input.FullName}");
            Environment.ExitCode = 1;
            return;
        }

        var direction = CSharpToOpalConverter.DetectDirection(input.FullName);

        if (direction == ConversionDirection.Unknown)
        {
            Console.Error.WriteLine($"Error: Unknown file type. Expected .cs or .opal extension.");
            Environment.ExitCode = 1;
            return;
        }

        var outputPath = output?.FullName ?? GetDefaultOutputPath(input.FullName, direction);

        if (verbose)
        {
            Console.WriteLine($"Converting: {input.Name}");
            Console.WriteLine($"Direction: {(direction == ConversionDirection.CSharpToOpal ? "C# → OPAL" : "OPAL → C#")}");
        }

        try
        {
            if (direction == ConversionDirection.CSharpToOpal)
            {
                await ConvertCSharpToOpalAsync(input.FullName, outputPath, benchmark, verbose);
            }
            else
            {
                await ConvertOpalToCSharpAsync(input.FullName, outputPath, verbose);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }

    private static async Task ConvertCSharpToOpalAsync(string inputPath, string outputPath, bool benchmark, bool verbose)
    {
        var converter = new CSharpToOpalConverter(new ConversionOptions
        {
            Verbose = verbose,
            IncludeBenchmark = benchmark
        });

        var result = await converter.ConvertFileAsync(inputPath);

        if (!result.Success)
        {
            Console.Error.WriteLine("Conversion failed:");
            foreach (var issue in result.Issues.Where(i => i.Severity == ConversionIssueSeverity.Error))
            {
                Console.Error.WriteLine($"  {issue}");
            }
            Environment.ExitCode = 1;
            return;
        }

        // Write output
        await File.WriteAllTextAsync(outputPath, result.OpalSource);

        Console.WriteLine($"✓ Conversion successful");

        // Show warnings
        var warnings = result.Issues.Where(i => i.Severity == ConversionIssueSeverity.Warning).ToList();
        if (warnings.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"Warnings ({warnings.Count}):");
            foreach (var warning in warnings.Take(5))
            {
                Console.WriteLine($"  ⚠ {warning.Message}");
            }
            if (warnings.Count > 5)
            {
                Console.WriteLine($"  ... and {warnings.Count - 5} more");
            }
        }

        // Show benchmark if requested
        if (benchmark && result.OpalSource != null)
        {
            var originalSource = await File.ReadAllTextAsync(inputPath);
            var metrics = BenchmarkIntegration.CalculateMetrics(originalSource, result.OpalSource);

            Console.WriteLine();
            Console.WriteLine(BenchmarkIntegration.FormatComparison(metrics));
        }

        Console.WriteLine();
        Console.WriteLine($"Output: {outputPath}");
    }

    private static async Task ConvertOpalToCSharpAsync(string inputPath, string outputPath, bool verbose)
    {
        var source = await File.ReadAllTextAsync(inputPath);
        var result = Program.Compile(source, inputPath, verbose);

        if (result.HasErrors)
        {
            Console.Error.WriteLine("Compilation failed:");
            foreach (var diag in result.Diagnostics.Errors)
            {
                Console.Error.WriteLine($"  {diag}");
            }
            Environment.ExitCode = 1;
            return;
        }

        await File.WriteAllTextAsync(outputPath, result.GeneratedCode);

        Console.WriteLine($"✓ Conversion successful");
        Console.WriteLine($"Output: {outputPath}");
    }

    private static string GetDefaultOutputPath(string inputPath, ConversionDirection direction)
    {
        return direction == ConversionDirection.CSharpToOpal
            ? Path.ChangeExtension(inputPath, ".opal")
            : Path.ChangeExtension(inputPath, ".g.cs");
    }
}
