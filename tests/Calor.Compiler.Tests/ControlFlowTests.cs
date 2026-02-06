using Calor.Compiler.Ast;
using Calor.Compiler.Diagnostics;
using Calor.Compiler.Parsing;
using Xunit;

namespace Calor.Compiler.Tests;

public class ControlFlowTests
{
    private static ModuleNode Parse(string source, out DiagnosticBag diagnostics)
    {
        diagnostics = new DiagnosticBag();
        var lexer = new Lexer(source, diagnostics);
        var tokens = lexer.TokenizeAll();
        var parser = new Parser(tokens, diagnostics);
        return parser.Parse();
    }

    [Fact]
    public void Parse_ForLoop_ReturnsForStatementNode()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §O{void}
              §L{for1:i:0:10:1}
                §C{Console.WriteLine}
                  §A i
                §/C
              §/L{for1}
            §/F{f001}
            §/M{m001}
            """;

        var module = Parse(source, out var diagnostics);

        Assert.False(diagnostics.HasErrors);
        var func = module.Functions[0];
        Assert.Single(func.Body);

        var forStmt = func.Body[0] as ForStatementNode;
        Assert.NotNull(forStmt);
        Assert.Equal("i", forStmt.VariableName);
        Assert.Single(forStmt.Body);
    }

    [Fact]
    public void Parse_ForLoop_MismatchedId_ReportsError()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §L{for1:i:0:10}
              §/L{for999}
            §/F{f001}
            §/M{m001}
            """;

        var module = Parse(source, out var diagnostics);

        Assert.True(diagnostics.HasErrors);
        Assert.Contains(diagnostics, d =>
            d.Code == DiagnosticCode.MismatchedId &&
            d.Message.Contains("for1") &&
            d.Message.Contains("for999"));
    }

    [Fact]
    public void Parse_IfStatement_ReturnsIfStatementNode()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §O{void}
              §IF{if1} (== x INT:0)
                §C{Console.WriteLine}
                  §A "Zero"
                §/C
              §/I{if1}
            §/F{f001}
            §/M{m001}
            """;

        var module = Parse(source, out var diagnostics);

        Assert.False(diagnostics.HasErrors);
        var func = module.Functions[0];
        Assert.Single(func.Body);

        var ifStmt = func.Body[0] as IfStatementNode;
        Assert.NotNull(ifStmt);
        Assert.Single(ifStmt.ThenBody);
        Assert.Empty(ifStmt.ElseIfClauses);
        Assert.Null(ifStmt.ElseBody);
    }

    [Fact]
    public void Parse_IfElseStatement_ReturnsIfStatementNodeWithElse()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §O{void}
              §IF{if1} BOOL:true
                §C{Console.WriteLine}
                  §A "Then"
                §/C
              §EL
                §C{Console.WriteLine}
                  §A "Else"
                §/C
              §/I{if1}
            §/F{f001}
            §/M{m001}
            """;

        var module = Parse(source, out var diagnostics);

        Assert.False(diagnostics.HasErrors);
        var ifStmt = module.Functions[0].Body[0] as IfStatementNode;
        Assert.NotNull(ifStmt);
        Assert.Single(ifStmt.ThenBody);
        Assert.NotNull(ifStmt.ElseBody);
        Assert.Single(ifStmt.ElseBody);
    }

    [Fact]
    public void Parse_IfElseIfElse_ReturnsCorrectStructure()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §O{void}
              §IF{if1} (== x INT:1)
                §C{Console.WriteLine}
                  §A "One"
                §/C
              §EI (== x INT:2)
                §C{Console.WriteLine}
                  §A "Two"
                §/C
              §EL
                §C{Console.WriteLine}
                  §A "Other"
                §/C
              §/I{if1}
            §/F{f001}
            §/M{m001}
            """;

        var module = Parse(source, out var diagnostics);

        Assert.False(diagnostics.HasErrors);
        var ifStmt = module.Functions[0].Body[0] as IfStatementNode;
        Assert.NotNull(ifStmt);
        Assert.Single(ifStmt.ThenBody);
        Assert.Single(ifStmt.ElseIfClauses);
        Assert.NotNull(ifStmt.ElseBody);
    }

    [Fact]
    public void Parse_WhileLoop_ReturnsWhileStatementNode()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §O{void}
              §WH{w1} (< x INT:10)
                §C{Console.WriteLine}
                  §A x
                §/C
              §/WH{w1}
            §/F{f001}
            §/M{m001}
            """;

        var module = Parse(source, out var diagnostics);

        Assert.False(diagnostics.HasErrors);
        var whileStmt = module.Functions[0].Body[0] as WhileStatementNode;
        Assert.NotNull(whileStmt);
        Assert.Single(whileStmt.Body);
    }

    [Fact]
    public void Parse_BindStatement_ReturnsBindStatementNode()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
            §O{void}
            §B{x:i32} 42
            §/F{f001}
            §/M{m001}
            """;

        var module = Parse(source, out var diagnostics);

        Assert.False(diagnostics.HasErrors, string.Join("\n", diagnostics.Select(d => d.Message)));
        var bindStmt = module.Functions[0].Body[0] as BindStatementNode;
        Assert.NotNull(bindStmt);
        Assert.Equal("x", bindStmt.Name);
        Assert.Equal("INT", bindStmt.TypeName);  // i32 expands to INT internally
        Assert.NotNull(bindStmt.Initializer);
    }

    [Fact]
    public void Parse_BinaryOperation_ReturnsBinaryOperationNode()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Add:pub}
              §I{i32:a}
              §I{i32:b}
              §O{i32}
              §R (+ a b)
            §/F{f001}
            §/M{m001}
            """;

        var module = Parse(source, out var diagnostics);

        Assert.False(diagnostics.HasErrors);
        var retStmt = module.Functions[0].Body[0] as ReturnStatementNode;
        Assert.NotNull(retStmt);

        var binOp = retStmt.Expression as BinaryOperationNode;
        Assert.NotNull(binOp);
        Assert.Equal(BinaryOperator.Add, binOp.Operator);
    }

    [Fact]
    public void Compile_ForLoop_GeneratesValidCSharp()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §O{void}
              §E{cw}
              §L{l1:i:1:10:1}
                §C{Console.WriteLine}
                  §A i
                §/C
              §/L{l1}
            §/F{f001}
            §/M{m001}
            """;

        var result = Program.Compile(source);

        Assert.False(result.HasErrors);
        Assert.Contains("for (var i = 1; i <= 10; i++)", result.GeneratedCode);
        Assert.Contains("Console.WriteLine(i)", result.GeneratedCode);
    }

    [Fact]
    public void Compile_IfElse_GeneratesValidCSharp()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §O{void}
              §E{cw}
              §IF{i1} BOOL:true
                §C{Console.WriteLine}
                  §A STR:"Yes"
                §/C
              §EL
                §C{Console.WriteLine}
                  §A STR:"No"
                §/C
              §/I{i1}
            §/F{f001}
            §/M{m001}
            """;

        var result = Program.Compile(source);

        Assert.False(result.HasErrors);
        Assert.Contains("if (true)", result.GeneratedCode);
        Assert.Contains("else", result.GeneratedCode);
    }

    [Fact]
    public void Compile_Bind_GeneratesValidCSharp()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §O{void}
              §E{cw}
              §B{x} INT:42
              §C{Console.WriteLine}
                §A x
              §/C
            §/F{f001}
            §/M{m001}
            """;

        var result = Program.Compile(source);

        Assert.False(result.HasErrors);
        Assert.Contains("var x = 42;", result.GeneratedCode);
        Assert.Contains("Console.WriteLine(x)", result.GeneratedCode);
    }

    [Fact]
    public void Compile_BinaryOperation_GeneratesValidCSharp()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Add:pub}
              §I{i32:a}
              §I{i32:b}
              §O{i32}
              §R (+ a b)
            §/F{f001}
            §/M{m001}
            """;

        var result = Program.Compile(source);

        Assert.False(result.HasErrors);
        Assert.Contains("return (a + b);", result.GeneratedCode);
    }

    [Fact]
    public void Compile_Modulo_GeneratesValidCSharp()
    {
        var source = """
            §M{m001:Test}
            §F{f001:IsEven:pub}
              §I{i32:n}
              §O{bool}
              §R (== (% n INT:2) INT:0)
            §/F{f001}
            §/M{m001}
            """;

        var result = Program.Compile(source);

        Assert.False(result.HasErrors);
        Assert.Contains("return ((n % 2) == 0);", result.GeneratedCode);
    }

    [Fact]
    public void Parse_DoWhileLoop_ReturnsDoWhileStatementNode()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §O{void}
              §DO{do1}
                §C{Console.WriteLine}
                  §A x
                §/C
              §/DO{do1} (< x INT:10)
            §/F{f001}
            §/M{m001}
            """;

        var module = Parse(source, out var diagnostics);

        Assert.False(diagnostics.HasErrors);
        var doStmt = module.Functions[0].Body[0] as DoWhileStatementNode;
        Assert.NotNull(doStmt);
        Assert.Equal("do1", doStmt.Id);
        Assert.Single(doStmt.Body);
        Assert.NotNull(doStmt.Condition);
    }

    [Fact]
    public void Parse_DoWhileLoop_MismatchedId_ReportsError()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §DO{do1}
              §/DO{do999} (< x INT:10)
            §/F{f001}
            §/M{m001}
            """;

        var module = Parse(source, out var diagnostics);

        Assert.True(diagnostics.HasErrors);
        Assert.Contains(diagnostics, d =>
            d.Code == DiagnosticCode.MismatchedId &&
            d.Message.Contains("do1") &&
            d.Message.Contains("do999"));
    }

    [Fact]
    public void Compile_DoWhileLoop_GeneratesValidCSharp()
    {
        var source = """
            §M{m001:Test}
            §F{f001:Main:pub}
              §O{void}
              §E{cw}
              §B{i} INT:0
              §DO{d1}
                §C{Console.WriteLine}
                  §A i
                §/C
              §/DO{d1} (< i INT:10)
            §/F{f001}
            §/M{m001}
            """;

        var result = Program.Compile(source);

        Assert.False(result.HasErrors);
        Assert.Contains("do", result.GeneratedCode);
        Assert.Contains("while ((i < 10));", result.GeneratedCode);
    }
}
