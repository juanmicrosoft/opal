using Calor.Compiler.Diagnostics;
using Calor.Compiler.Parsing;
using Xunit;

namespace Calor.Compiler.Tests;

/// <summary>
/// Tests for fixes discovered during the C# to Calor conversion campaign.
/// </summary>
public class ConversionCampaignFixTests
{
    #region Issue 312: Single quotes and line comments in lexer

    [Fact]
    public void Lexer_LineComment_SkipsContent()
    {
        var source = "§M{m001:Test}\n// This is a comment with apostrophe: it's fine\n§/M{m001}";
        var diag = new DiagnosticBag();
        var lexer = new Lexer(source, diag);
        var tokens = lexer.TokenizeAll();
        // Should have no errors — comment is skipped
        Assert.Empty(diag.Errors);
    }

    [Fact]
    public void Lexer_LineCommentAtEndOfFile_SkipsContent()
    {
        var source = "§M{m001:Test}\n§/M{m001}\n// trailing comment";
        var diag = new DiagnosticBag();
        var lexer = new Lexer(source, diag);
        var tokens = lexer.TokenizeAll();
        Assert.Empty(diag.Errors);
    }

    [Fact]
    public void Lexer_CharLiteral_DoesNotCrash()
    {
        var source = "§M{m001:Test}\n§F{f001:hello}\n§O{str}\n§R 'x'\n§/F{f001}\n§/M{m001}";
        var diag = new DiagnosticBag();
        var lexer = new Lexer(source, diag);
        var tokens = lexer.TokenizeAll();
        Assert.NotNull(tokens);
        Assert.True(tokens.Count > 0);
    }

    [Fact]
    public void Lexer_SlashNotFollowedBySlash_IsSlashToken()
    {
        var source = "§M{m001:Test}\n§F{f001:div}\n§O{i32}\n§R (/ 10 2)\n§/F{f001}\n§/M{m001}";
        var diag = new DiagnosticBag();
        var lexer = new Lexer(source, diag);
        var tokens = lexer.TokenizeAll();
        Assert.Contains(tokens, t => t.Kind == TokenKind.Slash);
    }

    #endregion
}
