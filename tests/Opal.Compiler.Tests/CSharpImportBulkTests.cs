using Opal.Compiler.CSharpImport;
using Opal.Compiler.Parsing;
using Opal.Compiler.CodeGen;
using Opal.Compiler.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Opal.Compiler.Tests;

/// <summary>
/// Bulk tests for the C# to OPAL converter using the test data files.
/// </summary>
public class CSharpImportBulkTests
{
    private readonly ITestOutputHelper _output;

    public CSharpImportBulkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Level 1 - Basic Tests

    [Theory]
    [MemberData(nameof(GetLevel1TestFiles))]
    public void Level1_Basic_FilesConvertSuccessfully(TestFileInfo file)
    {
        AssertConversionSucceeds(file);
    }

    public static IEnumerable<object[]> GetLevel1TestFiles()
    {
        return TestDataLoader.GetTestFilesByLevel(1)
            .Select(f => new object[] { f });
    }

    #endregion

    #region Level 2 - Control Flow Tests

    [Theory]
    [MemberData(nameof(GetLevel2TestFiles))]
    public void Level2_ControlFlow_FilesConvertSuccessfully(TestFileInfo file)
    {
        AssertConversionSucceeds(file);
    }

    public static IEnumerable<object[]> GetLevel2TestFiles()
    {
        return TestDataLoader.GetTestFilesByLevel(2)
            .Select(f => new object[] { f });
    }

    #endregion

    #region Level 3 - Types & Data Tests

    [Theory]
    [MemberData(nameof(GetLevel3TestFiles))]
    public void Level3_Types_FilesConvertSuccessfully(TestFileInfo file)
    {
        AssertConversionSucceeds(file);
    }

    public static IEnumerable<object[]> GetLevel3TestFiles()
    {
        return TestDataLoader.GetTestFilesByLevel(3)
            .Select(f => new object[] { f });
    }

    #endregion

    #region Level 4 - Methods Tests

    [Theory]
    [MemberData(nameof(GetLevel4TestFiles))]
    public void Level4_Methods_FilesConvertSuccessfully(TestFileInfo file)
    {
        AssertConversionSucceeds(file);
    }

    public static IEnumerable<object[]> GetLevel4TestFiles()
    {
        return TestDataLoader.GetTestFilesByLevel(4)
            .Select(f => new object[] { f });
    }

    #endregion

    #region Level 5 - Advanced Tests

    [Theory]
    [MemberData(nameof(GetLevel5TestFiles))]
    public void Level5_Advanced_FilesConvertSuccessfully(TestFileInfo file)
    {
        AssertConversionSucceeds(file);
    }

    public static IEnumerable<object[]> GetLevel5TestFiles()
    {
        return TestDataLoader.GetTestFilesByLevel(5)
            .Select(f => new object[] { f });
    }

    #endregion

    #region All Files Tests

    [Theory]
    [MemberData(nameof(GetAllTestFiles))]
    public void AllFiles_ProduceValidOpal(TestFileInfo file)
    {
        // Load and convert the C# file
        var csharpCode = TestDataLoader.LoadTestFile(file);
        var converter = new CSharpToOpalConverter();
        var result = converter.Convert(csharpCode);

        _output.WriteLine($"File: {file.DisplayName}");
        _output.WriteLine($"Expected: {file.ExpectedResult}");
        _output.WriteLine($"Features: {string.Join(", ", file.Features)}");
        _output.WriteLine("---");

        if (!result.Success)
        {
            _output.WriteLine($"Conversion errors:");
            foreach (var error in result.Errors)
            {
                _output.WriteLine($"  {error}");
            }

            if (file.ExpectedResult == "success")
            {
                Assert.Fail($"Conversion failed for {file.DisplayName}: {result.Errors.FirstOrDefault()}");
            }
            return;
        }

        var opalCode = result.OpalCode!;
        _output.WriteLine($"OPAL Output ({opalCode.Length} chars):");
        _output.WriteLine(opalCode);

        // Basic validation - output should not be empty
        Assert.False(string.IsNullOrWhiteSpace(opalCode),
            $"Converter produced empty output for {file.DisplayName}");

        // Try to parse the generated OPAL to validate syntax
        var opalParseResult = TryParseOpal(opalCode);
        if (!opalParseResult.Success)
        {
            _output.WriteLine($"OPAL Parse Warning: {opalParseResult.Error}");

            // For files expected to succeed, this is a failure
            if (file.ExpectedResult == "success")
            {
                Assert.Fail($"Generated OPAL failed to parse: {opalParseResult.Error}");
            }
        }
    }

    public static IEnumerable<object[]> GetAllTestFiles()
    {
        return TestDataLoader.GetAllTestFiles()
            .Select(f => new object[] { f });
    }

    #endregion

    #region Round Trip Tests

    [Theory]
    [MemberData(nameof(GetRoundTripTestFiles))]
    public void RoundTrip_OpalCompilesToCSharp(TestFileInfo file)
    {
        // Load and convert the C# file to OPAL
        var originalCSharp = TestDataLoader.LoadTestFile(file);
        var converter = new CSharpToOpalConverter();
        var conversionResult = converter.Convert(originalCSharp);

        _output.WriteLine($"File: {file.DisplayName}");
        _output.WriteLine("--- Original C# ---");
        _output.WriteLine(originalCSharp);

        if (!conversionResult.Success)
        {
            _output.WriteLine($"Conversion failed: {conversionResult.Errors.FirstOrDefault()}");
            if (file.ExpectedResult == "success")
            {
                Assert.Fail($"Conversion failed: {conversionResult.Errors.FirstOrDefault()}");
            }
            return;
        }

        var opalCode = conversionResult.OpalCode!;
        _output.WriteLine("--- OPAL ---");
        _output.WriteLine(opalCode);

        // Try to compile the OPAL back to C#
        var roundTripResult = TryCompileOpalToCSharp(opalCode);

        _output.WriteLine("--- Round-trip C# ---");
        _output.WriteLine(roundTripResult.CSharpCode ?? "(failed)");

        if (!roundTripResult.Success)
        {
            _output.WriteLine($"Round-trip Warning: {roundTripResult.Error}");

            // Only fail for files expected to fully succeed
            if (file.ExpectedResult == "success")
            {
                Assert.Fail($"Round-trip compilation failed: {roundTripResult.Error}");
            }
        }
        else
        {
            Assert.False(string.IsNullOrWhiteSpace(roundTripResult.CSharpCode),
                "Round-trip produced empty C# code");
        }
    }

    public static IEnumerable<object[]> GetRoundTripTestFiles()
    {
        // Only include files expected to succeed for round-trip tests
        return TestDataLoader.GetTestFilesByExpectedResult("success")
            .Select(f => new object[] { f });
    }

    #endregion

    #region Feature-specific Tests

    [Theory]
    [MemberData(nameof(GetRecursionTestFiles))]
    public void Feature_Recursion_ConvertsCorrectly(TestFileInfo file)
    {
        AssertConversionSucceeds(file);
    }

    public static IEnumerable<object[]> GetRecursionTestFiles()
    {
        return TestDataLoader.GetTestFilesByFeature("recursion")
            .Select(f => new object[] { f });
    }

    [Theory]
    [MemberData(nameof(GetLoopTestFiles))]
    public void Feature_Loops_ConvertsCorrectly(TestFileInfo file)
    {
        AssertConversionSucceeds(file);
    }

    public static IEnumerable<object[]> GetLoopTestFiles()
    {
        return TestDataLoader.GetAllTestFiles()
            .Where(f => f.Features.Any(feat =>
                feat.Contains("loop") || feat == "for_loop" || feat == "while_loop"))
            .Select(f => new object[] { f });
    }

    #endregion

    #region Summary Tests

    [Fact]
    public void Summary_AllFilesLoaded()
    {
        var files = TestDataLoader.GetAllTestFiles().ToList();
        _output.WriteLine($"Total test files: {files.Count}");

        var byLevel = files.GroupBy(f => f.Level).OrderBy(g => g.Key);
        foreach (var group in byLevel)
        {
            _output.WriteLine($"  Level {group.Key}: {group.Count()} files");
        }

        var byExpected = files.GroupBy(f => f.ExpectedResult);
        foreach (var group in byExpected)
        {
            _output.WriteLine($"  Expected '{group.Key}': {group.Count()} files");
        }

        Assert.Equal(100, files.Count);
    }

    [Fact]
    public void Summary_ConversionSuccessRates()
    {
        var files = TestDataLoader.GetAllTestFiles().ToList();
        var results = new Dictionary<int, (int total, int success, int partial, int failed)>();

        for (int level = 1; level <= 5; level++)
        {
            var levelFiles = files.Where(f => f.Level == level).ToList();
            int success = 0, partial = 0, failed = 0;

            foreach (var file in levelFiles)
            {
                try
                {
                    var csharpCode = TestDataLoader.LoadTestFile(file);
                    var converter = new CSharpToOpalConverter();
                    var conversionResult = converter.Convert(csharpCode);

                    if (conversionResult.Success && !string.IsNullOrWhiteSpace(conversionResult.OpalCode))
                    {
                        var parseResult = TryParseOpal(conversionResult.OpalCode);
                        if (parseResult.Success)
                        {
                            success++;
                        }
                        else
                        {
                            partial++;
                        }
                    }
                    else
                    {
                        failed++;
                    }
                }
                catch
                {
                    failed++;
                }
            }

            results[level] = (levelFiles.Count, success, partial, failed);
        }

        _output.WriteLine("=== Conversion Success Rates ===");
        foreach (var kvp in results.OrderBy(k => k.Key))
        {
            var (total, success, partial, failed) = kvp.Value;
            var successRate = total > 0 ? (success * 100.0 / total) : 0;
            _output.WriteLine($"Level {kvp.Key}: {success}/{total} ({successRate:F1}%) success, {partial} partial, {failed} failed");
        }

        var overall = results.Values.Aggregate(
            (0, 0, 0, 0),
            (acc, val) => (acc.Item1 + val.total, acc.Item2 + val.success, acc.Item3 + val.partial, acc.Item4 + val.failed));

        var overallRate = overall.Item1 > 0 ? (overall.Item2 * 100.0 / overall.Item1) : 0;
        _output.WriteLine($"Overall: {overall.Item2}/{overall.Item1} ({overallRate:F1}%) success");
    }

    #endregion

    #region Helper Methods

    private void AssertConversionSucceeds(TestFileInfo file)
    {
        var csharpCode = TestDataLoader.LoadTestFile(file);

        _output.WriteLine($"File: {file.DisplayName}");
        _output.WriteLine($"Level: {file.Level}");
        _output.WriteLine($"Expected: {file.ExpectedResult}");
        _output.WriteLine($"Features: {string.Join(", ", file.Features)}");
        _output.WriteLine("--- C# Input ---");
        _output.WriteLine(csharpCode);

        ConversionResult conversionResult;
        try
        {
            var converter = new CSharpToOpalConverter();
            conversionResult = converter.Convert(csharpCode);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Conversion failed with exception: {ex.Message}");

            if (file.ExpectedResult == "unsupported")
            {
                // Expected to fail
                return;
            }

            throw;
        }

        if (!conversionResult.Success)
        {
            _output.WriteLine($"Conversion errors:");
            foreach (var error in conversionResult.Errors)
            {
                _output.WriteLine($"  {error}");
            }

            if (file.ExpectedResult == "unsupported")
            {
                return; // Expected to fail
            }

            Assert.Fail($"Conversion failed for {file.DisplayName}");
            return;
        }

        var opalCode = conversionResult.OpalCode!;
        _output.WriteLine("--- OPAL Output ---");
        _output.WriteLine(opalCode);

        Assert.False(string.IsNullOrWhiteSpace(opalCode),
            $"Converter produced empty output for {file.DisplayName}");
    }

    private static (bool Success, string? Error) TryParseOpal(string opalCode)
    {
        try
        {
            // Use the OPAL lexer and parser to validate the generated code
            var diagnostics = new DiagnosticBag();
            var lexer = new Lexer(opalCode, diagnostics);
            var tokens = lexer.Tokenize().ToList();

            // Check for lexer errors
            if (diagnostics.HasErrors)
            {
                return (false, $"Lexer errors: {string.Join(", ", diagnostics.Errors.Select(d => d.Message))}");
            }

            // Try to parse
            var parser = new Parser(tokens, diagnostics);
            var module = parser.Parse();

            // Check for parse errors
            if (diagnostics.HasErrors)
            {
                return (false, $"Parser errors: {string.Join(", ", diagnostics.Errors.Select(d => d.Message))}");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Parse exception: {ex.Message}");
        }
    }

    private static (bool Success, string? CSharpCode, string? Error) TryCompileOpalToCSharp(string opalCode)
    {
        try
        {
            // Tokenize
            var diagnostics = new DiagnosticBag();
            var lexer = new Lexer(opalCode, diagnostics);
            var tokens = lexer.Tokenize().ToList();

            if (diagnostics.HasErrors)
            {
                return (false, null, $"Lexer errors: {string.Join(", ", diagnostics.Errors.Select(d => d.Message))}");
            }

            // Parse
            var parser = new Parser(tokens, diagnostics);
            var module = parser.Parse();

            if (diagnostics.HasErrors)
            {
                return (false, null, $"Parse errors: {string.Join(", ", diagnostics.Errors.Select(d => d.Message))}");
            }

            // Generate C#
            var emitter = new CSharpEmitter();
            var csharpCode = emitter.Emit(module);

            return (true, csharpCode, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Compilation exception: {ex.Message}");
        }
    }

    #endregion
}
