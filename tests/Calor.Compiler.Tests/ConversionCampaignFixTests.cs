using Calor.Compiler.Diagnostics;
using Calor.Compiler.Effects;
using Calor.Compiler.Parsing;
using Xunit;

namespace Calor.Compiler.Tests;

/// <summary>
/// Tests for fixes discovered during the C# to Calor conversion campaign.
/// </summary>
public class ConversionCampaignFixTests
{
    #region Issue 314: LINQ extension method effect recognition

    private static Ast.ModuleNode ParseCalor(string source)
    {
        var diag = new DiagnosticBag();
        var lexer = new Lexer(source, diag);
        var tokens = lexer.TokenizeAll();
        Assert.Empty(diag.Errors);
        var parser = new Parser(tokens, diag);
        var ast = parser.Parse();
        Assert.Empty(diag.Errors);
        return ast;
    }

    [Fact]
    public void LinqMethods_DoNotTriggerCalor0411_Errors()
    {
        var source = @"
§M{m001:TestModule}
§F{f001:process}
  §E{cw}
  §I{List<i32>:items}
  §O{i32}
  §B{filtered} §C{items.Where} §A §LAM{lam001:x:i32} (> x 5) §/LAM{lam001} §/C
  §B{count} §C{filtered.Count} §/C
  §P count
  §R count
§/F{f001}
§/M{m001}";

        var ast = ParseCalor(source);

        var diag = new DiagnosticBag();
        var pass = new EffectEnforcementPass(diag);
        pass.Enforce(ast);

        // Should have no Calor0411 errors
        var errors = diag.Errors.Where(d =>
            d.Code == DiagnosticCode.UnknownExternalCall).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void LinqToList_DoesNotTriggerCalor0411_Errors()
    {
        var source = @"
§M{m001:TestModule}
§F{f001:sortItems}
  §I{List<i32>:items}
  §O{List<i32>}
  §B{sorted} §C{items.OrderBy} §A §LAM{lam001:x:i32} x §/LAM{lam001} §/C
  §R §C{sorted.ToList} §/C
§/F{f001}
§/M{m001}";

        var ast = ParseCalor(source);

        var diag = new DiagnosticBag();
        var pass = new EffectEnforcementPass(diag);
        pass.Enforce(ast);

        var errors = diag.Errors.Where(d =>
            d.Code == DiagnosticCode.UnknownExternalCall).ToList();
        Assert.Empty(errors);
    }

    #endregion
}
