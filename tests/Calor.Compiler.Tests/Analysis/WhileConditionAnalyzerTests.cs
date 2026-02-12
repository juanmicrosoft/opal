using Calor.Compiler.Ast;
using Calor.Compiler.Binding;
using Calor.Compiler.Verification.Z3.KInduction;
using Xunit;

namespace Calor.Compiler.Tests.Analysis;

/// <summary>
/// Tests for WhileConditionAnalyzer which extracts loop information for k-induction.
/// </summary>
public class WhileConditionAnalyzerTests
{
    #region Helpers

    private static BoundBinaryExpression MakeBinaryExpr(
        BinaryOperator op,
        BoundExpression left,
        BoundExpression right)
    {
        return new BoundBinaryExpression(default, op, left, right, "BOOL");
    }

    private static BoundVariableExpression MakeVar(string name)
    {
        return new BoundVariableExpression(default, new VariableSymbol(name, "INT", true));
    }

    private static BoundIntLiteral MakeInt(int value)
    {
        return new BoundIntLiteral(default, value);
    }

    #endregion

    #region Analyze - Less Than

    [Fact]
    public void Analyze_LessThan_VariableLessThanValue_ReturnsIncrementing()
    {
        // i < 10
        var condition = MakeBinaryExpr(
            BinaryOperator.LessThan,
            MakeVar("i"),
            MakeInt(10));

        var result = WhileConditionAnalyzer.Analyze(condition);

        Assert.NotNull(result);
        Assert.Equal("i", result.LoopVariable);
        Assert.Equal(10, result.UpperBound);
        Assert.False(result.IsDecrementing);
        Assert.Equal("<", result.ConditionOperator);
        Assert.True(result.IsAnalyzable);
    }

    [Fact]
    public void Analyze_LessThan_VariableLessThanVariable_ReturnsVariable()
    {
        // i < n
        var condition = MakeBinaryExpr(
            BinaryOperator.LessThan,
            MakeVar("i"),
            MakeVar("n"));

        var result = WhileConditionAnalyzer.Analyze(condition);

        Assert.NotNull(result);
        Assert.Equal("i", result.LoopVariable);
        Assert.Null(result.UpperBound); // n is variable, not constant
        Assert.False(result.IsDecrementing);
        Assert.Equal("<", result.ConditionOperator);
    }

    #endregion

    #region Analyze - Less Or Equal

    [Fact]
    public void Analyze_LessOrEqual_VariableLessOrEqualValue_ReturnsIncrementing()
    {
        // i <= 10
        var condition = MakeBinaryExpr(
            BinaryOperator.LessOrEqual,
            MakeVar("i"),
            MakeInt(10));

        var result = WhileConditionAnalyzer.Analyze(condition);

        Assert.NotNull(result);
        Assert.Equal("i", result.LoopVariable);
        Assert.Equal(10, result.UpperBound);
        Assert.False(result.IsDecrementing);
        Assert.Equal("<=", result.ConditionOperator);
    }

    #endregion

    #region Analyze - Greater Than

    [Fact]
    public void Analyze_GreaterThan_VariableGreaterThanZero_ReturnsDecrementing()
    {
        // i > 0
        var condition = MakeBinaryExpr(
            BinaryOperator.GreaterThan,
            MakeVar("i"),
            MakeInt(0));

        var result = WhileConditionAnalyzer.Analyze(condition);

        Assert.NotNull(result);
        Assert.Equal("i", result.LoopVariable);
        Assert.Equal(1, result.LowerBound); // > 0 means >= 1
        Assert.True(result.IsDecrementing);
        Assert.Equal(">", result.ConditionOperator);
    }

    [Fact]
    public void Analyze_GreaterThan_VariableGreaterThanFive_ReturnsLowerBound()
    {
        // i > 5
        var condition = MakeBinaryExpr(
            BinaryOperator.GreaterThan,
            MakeVar("i"),
            MakeInt(5));

        var result = WhileConditionAnalyzer.Analyze(condition);

        Assert.NotNull(result);
        Assert.Equal("i", result.LoopVariable);
        Assert.Equal(6, result.LowerBound); // > 5 means >= 6
        Assert.True(result.IsDecrementing);
    }

    #endregion

    #region Analyze - Greater Or Equal

    [Fact]
    public void Analyze_GreaterOrEqual_VariableGreaterOrEqualOne_ReturnsDecrementing()
    {
        // i >= 1
        var condition = MakeBinaryExpr(
            BinaryOperator.GreaterOrEqual,
            MakeVar("i"),
            MakeInt(1));

        var result = WhileConditionAnalyzer.Analyze(condition);

        Assert.NotNull(result);
        Assert.Equal("i", result.LoopVariable);
        Assert.Equal(1, result.LowerBound);
        Assert.True(result.IsDecrementing);
        Assert.Equal(">=", result.ConditionOperator);
    }

    #endregion

    #region Analyze - Not Equal

    [Fact]
    public void Analyze_NotEqual_VariableNotEqualValue_ReturnsVariable()
    {
        // i != 10
        var condition = MakeBinaryExpr(
            BinaryOperator.NotEqual,
            MakeVar("i"),
            MakeInt(10));

        var result = WhileConditionAnalyzer.Analyze(condition);

        Assert.NotNull(result);
        Assert.Equal("i", result.LoopVariable);
        Assert.Equal(10, result.UpperBound);
        Assert.Equal("!=", result.ConditionOperator);
    }

    #endregion

    #region Analyze - Conjunction

    [Fact]
    public void Analyze_And_BothBounds_CombinesBounds()
    {
        // i < 10 && i >= 0
        var left = MakeBinaryExpr(BinaryOperator.LessThan, MakeVar("i"), MakeInt(10));
        var right = MakeBinaryExpr(BinaryOperator.GreaterOrEqual, MakeVar("i"), MakeInt(0));
        var condition = MakeBinaryExpr(BinaryOperator.And, left, right);

        var result = WhileConditionAnalyzer.Analyze(condition);

        Assert.NotNull(result);
        Assert.Equal("i", result.LoopVariable);
        Assert.Equal(0, result.LowerBound);
        Assert.Equal(10, result.UpperBound);
    }

    #endregion

    #region Analyze - Null Cases

    [Fact]
    public void Analyze_UnsupportedOperator_ReturnsNull()
    {
        // i + 1 (not a condition)
        var condition = MakeBinaryExpr(
            BinaryOperator.Add,
            MakeVar("i"),
            MakeInt(1));

        var result = WhileConditionAnalyzer.Analyze(condition);

        Assert.Null(result);
    }

    [Fact]
    public void Analyze_IntLiteral_ReturnsNull()
    {
        // Just a literal value, not analyzable
        var result = WhileConditionAnalyzer.Analyze(MakeInt(5));

        Assert.Null(result);
    }

    #endregion

    #region AnalyzeTransition

    [Fact]
    public void AnalyzeTransition_Increment_ReturnsAddConstant()
    {
        // i = i + 1
        var stmt = new BoundBindStatement(
            default,
            new VariableSymbol("i", "INT", true),
            MakeBinaryExpr(BinaryOperator.Add, MakeVar("i"), MakeInt(1)));

        var result = WhileConditionAnalyzer.AnalyzeTransition(new[] { stmt }, "i");

        Assert.NotNull(result);
        Assert.Equal("i", result.Variable);
        Assert.Equal(WhileConditionAnalyzer.TransitionKind.AddConstant, result.Kind);
        Assert.Equal(1, result.Delta);
    }

    [Fact]
    public void AnalyzeTransition_Decrement_ReturnsSubConstant()
    {
        // i = i - 1
        var stmt = new BoundBindStatement(
            default,
            new VariableSymbol("i", "INT", true),
            MakeBinaryExpr(BinaryOperator.Subtract, MakeVar("i"), MakeInt(1)));

        var result = WhileConditionAnalyzer.AnalyzeTransition(new[] { stmt }, "i");

        Assert.NotNull(result);
        Assert.Equal("i", result.Variable);
        Assert.Equal(WhileConditionAnalyzer.TransitionKind.SubConstant, result.Kind);
        Assert.Equal(1, result.Delta);
    }

    [Fact]
    public void AnalyzeTransition_IncrementByTwo_ReturnsAddConstantWithDelta()
    {
        // i = i + 2
        var stmt = new BoundBindStatement(
            default,
            new VariableSymbol("i", "INT", true),
            MakeBinaryExpr(BinaryOperator.Add, MakeVar("i"), MakeInt(2)));

        var result = WhileConditionAnalyzer.AnalyzeTransition(new[] { stmt }, "i");

        Assert.NotNull(result);
        Assert.Equal(WhileConditionAnalyzer.TransitionKind.AddConstant, result.Kind);
        Assert.Equal(2, result.Delta);
    }

    [Fact]
    public void AnalyzeTransition_CommutativeAdd_ReturnsAddConstant()
    {
        // i = 1 + i
        var stmt = new BoundBindStatement(
            default,
            new VariableSymbol("i", "INT", true),
            MakeBinaryExpr(BinaryOperator.Add, MakeInt(1), MakeVar("i")));

        var result = WhileConditionAnalyzer.AnalyzeTransition(new[] { stmt }, "i");

        Assert.NotNull(result);
        Assert.Equal(WhileConditionAnalyzer.TransitionKind.AddConstant, result.Kind);
        Assert.Equal(1, result.Delta);
    }

    [Fact]
    public void AnalyzeTransition_NoTransition_ReturnsNull()
    {
        // j = 5 (not the loop variable)
        var stmt = new BoundBindStatement(
            default,
            new VariableSymbol("j", "INT", true),
            MakeInt(5));

        var result = WhileConditionAnalyzer.AnalyzeTransition(new[] { stmt }, "i");

        Assert.Null(result);
    }

    [Fact]
    public void AnalyzeTransition_EmptyBody_ReturnsNull()
    {
        var result = WhileConditionAnalyzer.AnalyzeTransition(Array.Empty<BoundStatement>(), "i");

        Assert.Null(result);
    }

    [Fact]
    public void AnalyzeTransition_NestedInIf_FindsTransition()
    {
        // if (cond) { i = i + 1 }
        var innerStmt = new BoundBindStatement(
            default,
            new VariableSymbol("i", "INT", true),
            MakeBinaryExpr(BinaryOperator.Add, MakeVar("i"), MakeInt(1)));

        var ifStmt = new BoundIfStatement(
            default,
            MakeVar("cond"),
            new[] { innerStmt },
            Array.Empty<BoundElseIfClause>(),
            null);

        var result = WhileConditionAnalyzer.AnalyzeTransition(new BoundStatement[] { ifStmt }, "i");

        Assert.NotNull(result);
        Assert.Equal(WhileConditionAnalyzer.TransitionKind.AddConstant, result.Kind);
    }

    #endregion

    #region WhileLoopInfo

    [Fact]
    public void WhileLoopInfo_IsAnalyzable_WithVariable_ReturnsTrue()
    {
        var info = new WhileConditionAnalyzer.WhileLoopInfo("i", null, 10, false, "<", null);
        Assert.True(info.IsAnalyzable);
    }

    [Fact]
    public void WhileLoopInfo_IsAnalyzable_WithoutBounds_ReturnsFalse()
    {
        var info = new WhileConditionAnalyzer.WhileLoopInfo("i", null, null, false, "<", null);
        Assert.False(info.IsAnalyzable);
    }

    [Fact]
    public void WhileLoopInfo_IsAnalyzable_WithoutVariable_ReturnsFalse()
    {
        var info = new WhileConditionAnalyzer.WhileLoopInfo(null, 0, 10, false, "<", null);
        Assert.False(info.IsAnalyzable);
    }

    #endregion

    #region TransitionInfo

    [Fact]
    public void TransitionInfo_IsWellFormed_WithDelta_ReturnsTrue()
    {
        var info = new WhileConditionAnalyzer.TransitionInfo("i", WhileConditionAnalyzer.TransitionKind.AddConstant, 1);
        Assert.True(info.IsWellFormed);
    }

    [Fact]
    public void TransitionInfo_IsWellFormed_Unknown_ReturnsFalse()
    {
        // Unknown kind is not well-formed (can't be used for k-induction)
        var info = new WhileConditionAnalyzer.TransitionInfo("i", WhileConditionAnalyzer.TransitionKind.Unknown, null);
        Assert.False(info.IsWellFormed);
    }

    #endregion
}
