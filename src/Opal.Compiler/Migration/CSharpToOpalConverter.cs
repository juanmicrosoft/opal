using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Opal.Compiler.Ast;

namespace Opal.Compiler.Migration;

/// <summary>
/// Result of a C# to OPAL conversion.
/// </summary>
public sealed class ConversionResult
{
    public bool Success { get; init; }
    public string? OpalSource { get; init; }
    public ModuleNode? Ast { get; init; }
    public ConversionContext Context { get; init; } = new();
    public TimeSpan Duration { get; init; }

    public bool HasErrors => Context.HasErrors;
    public bool HasWarnings => Context.HasWarnings;
    public IReadOnlyList<ConversionIssue> Issues => Context.Issues;
}

/// <summary>
/// Options for C# to OPAL conversion.
/// </summary>
public sealed class ConversionOptions
{
    /// <summary>
    /// The module name to use in the generated OPAL code.
    /// If not specified, derived from the source file name.
    /// </summary>
    public string? ModuleName { get; set; }

    /// <summary>
    /// Whether to preserve original comments in the output.
    /// </summary>
    public bool PreserveComments { get; set; } = true;

    /// <summary>
    /// Whether to include benchmark metrics comparison.
    /// </summary>
    public bool IncludeBenchmark { get; set; }

    /// <summary>
    /// Whether to enable verbose output.
    /// </summary>
    public bool Verbose { get; set; }

    /// <summary>
    /// Whether to auto-generate unique IDs for OPAL elements.
    /// </summary>
    public bool AutoGenerateIds { get; set; } = true;
}

/// <summary>
/// Main converter that orchestrates the C# to OPAL conversion pipeline.
///
/// Pipeline: C# Source → Roslyn Parse → RoslynSyntaxVisitor → OPAL AST → OpalEmitter → OPAL Source
/// </summary>
public sealed class CSharpToOpalConverter
{
    private readonly ConversionOptions _options;

    public CSharpToOpalConverter(ConversionOptions? options = null)
    {
        _options = options ?? new ConversionOptions();
    }

    /// <summary>
    /// Converts C# source code to OPAL source code.
    /// </summary>
    public ConversionResult Convert(string csharpSource, string? sourceFile = null)
    {
        var startTime = DateTime.UtcNow;
        var context = CreateContext(sourceFile);
        context.OriginalSource = csharpSource;

        try
        {
            // Step 1: Parse C# with Roslyn
            var syntaxTree = CSharpSyntaxTree.ParseText(csharpSource);
            var root = syntaxTree.GetCompilationUnitRoot();

            // Check for parse errors
            var diagnostics = root.GetDiagnostics()
                .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                .ToList();

            if (diagnostics.Count > 0)
            {
                foreach (var diag in diagnostics)
                {
                    var lineSpan = diag.Location.GetLineSpan();
                    context.AddError(
                        $"C# parse error: {diag.GetMessage()}",
                        line: lineSpan.StartLinePosition.Line + 1,
                        column: lineSpan.StartLinePosition.Character + 1);
                }

                return new ConversionResult
                {
                    Success = false,
                    Context = context,
                    Duration = DateTime.UtcNow - startTime
                };
            }

            // Step 2: Visit C# AST and build OPAL AST
            var moduleName = _options.ModuleName ?? DeriveModuleName(sourceFile, root);
            var visitor = new RoslynSyntaxVisitor(context);
            var opalAst = visitor.Convert(root, moduleName);

            if (context.HasErrors)
            {
                return new ConversionResult
                {
                    Success = false,
                    Ast = opalAst,
                    Context = context,
                    Duration = DateTime.UtcNow - startTime
                };
            }

            // Step 3: Emit OPAL source code
            var emitter = new OpalEmitter(context);
            var opalSource = emitter.Emit(opalAst);

            if (_options.Verbose)
            {
                Console.WriteLine($"Converted {context.Stats.ConvertedNodes} nodes");
                Console.WriteLine($"  Classes: {context.Stats.ClassesConverted}");
                Console.WriteLine($"  Interfaces: {context.Stats.InterfacesConverted}");
                Console.WriteLine($"  Methods: {context.Stats.MethodsConverted}");
                Console.WriteLine($"  Properties: {context.Stats.PropertiesConverted}");
                Console.WriteLine($"  Fields: {context.Stats.FieldsConverted}");
            }

            return new ConversionResult
            {
                Success = true,
                OpalSource = opalSource,
                Ast = opalAst,
                Context = context,
                Duration = DateTime.UtcNow - startTime
            };
        }
        catch (Exception ex)
        {
            context.AddError($"Conversion failed: {ex.Message}");

            return new ConversionResult
            {
                Success = false,
                Context = context,
                Duration = DateTime.UtcNow - startTime
            };
        }
    }

    /// <summary>
    /// Converts a C# file to OPAL.
    /// </summary>
    public async Task<ConversionResult> ConvertFileAsync(string csharpFilePath)
    {
        if (!File.Exists(csharpFilePath))
        {
            var context = new ConversionContext { SourceFile = csharpFilePath };
            context.AddError($"Source file not found: {csharpFilePath}");
            return new ConversionResult { Success = false, Context = context };
        }

        var source = await File.ReadAllTextAsync(csharpFilePath);
        return Convert(source, csharpFilePath);
    }

    /// <summary>
    /// Converts a C# file and writes the output to an OPAL file.
    /// </summary>
    public async Task<ConversionResult> ConvertFileAndSaveAsync(string csharpFilePath, string? outputPath = null)
    {
        var result = await ConvertFileAsync(csharpFilePath);

        if (result.Success && result.OpalSource != null)
        {
            var opalPath = outputPath ?? Path.ChangeExtension(csharpFilePath, ".opal");
            await File.WriteAllTextAsync(opalPath, result.OpalSource);
        }

        return result;
    }

    /// <summary>
    /// Detects the direction of conversion based on file extension.
    /// </summary>
    public static ConversionDirection DetectDirection(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".cs" => ConversionDirection.CSharpToOpal,
            ".opal" => ConversionDirection.OpalToCSharp,
            _ => ConversionDirection.Unknown
        };
    }

    private ConversionContext CreateContext(string? sourceFile)
    {
        return new ConversionContext
        {
            SourceFile = sourceFile,
            Verbose = _options.Verbose,
            IncludeBenchmark = _options.IncludeBenchmark,
            PreserveComments = _options.PreserveComments,
            AutoGenerateIds = _options.AutoGenerateIds,
            ModuleName = _options.ModuleName
        };
    }

    private static string DeriveModuleName(string? sourceFile, CompilationUnitSyntax root)
    {
        // Try to get namespace from the source
        var namespaceDecl = root.DescendantNodes()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        if (namespaceDecl != null)
        {
            return namespaceDecl.Name.ToString();
        }

        // Fall back to file name
        if (!string.IsNullOrEmpty(sourceFile))
        {
            return Path.GetFileNameWithoutExtension(sourceFile);
        }

        return "ConvertedModule";
    }
}

/// <summary>
/// Direction of conversion.
/// </summary>
public enum ConversionDirection
{
    Unknown,
    CSharpToOpal,
    OpalToCSharp
}

/// <summary>
/// Provides a simple facade for bidirectional conversion.
/// </summary>
public static class Converter
{
    /// <summary>
    /// Converts a file in the detected direction.
    /// </summary>
    public static async Task<object> ConvertFileAsync(string filePath, string? outputPath = null)
    {
        var direction = CSharpToOpalConverter.DetectDirection(filePath);

        return direction switch
        {
            ConversionDirection.CSharpToOpal => await ConvertCSharpToOpalAsync(filePath, outputPath),
            ConversionDirection.OpalToCSharp => await ConvertOpalToCSharpAsync(filePath, outputPath),
            _ => throw new ArgumentException($"Unknown file type: {filePath}")
        };
    }

    /// <summary>
    /// Converts C# to OPAL.
    /// </summary>
    public static async Task<ConversionResult> ConvertCSharpToOpalAsync(string csharpPath, string? outputPath = null)
    {
        var converter = new CSharpToOpalConverter();
        var result = await converter.ConvertFileAsync(csharpPath);

        if (result.Success && result.OpalSource != null)
        {
            var opalPath = outputPath ?? Path.ChangeExtension(csharpPath, ".opal");
            await File.WriteAllTextAsync(opalPath, result.OpalSource);
        }

        return result;
    }

    /// <summary>
    /// Converts OPAL to C# using the existing compiler.
    /// </summary>
    public static async Task<CompilationResult> ConvertOpalToCSharpAsync(string opalPath, string? outputPath = null)
    {
        var source = await File.ReadAllTextAsync(opalPath);
        var result = Program.Compile(source, opalPath);

        if (!result.HasErrors && !string.IsNullOrEmpty(result.GeneratedCode))
        {
            var csPath = outputPath ?? Path.ChangeExtension(opalPath, ".g.cs");
            await File.WriteAllTextAsync(csPath, result.GeneratedCode);
        }

        return result;
    }
}
