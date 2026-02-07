using Calor.Compiler.Ids;
using Calor.Compiler.Parsing;
using Xunit;

namespace Calor.Ids.Tests;

public class IdCheckerTests
{
    [Fact]
    public void Check_ValidIds_ReturnsNoIssues()
    {
        var entries = new List<IdEntry>
        {
            new("f_01J5X7K9M2NPQRSTABWXYZ1234", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
            new("m_01J5X7K9M2NPQRSTABWXYZ1312", IdKind.Module, "Module1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
        };

        var result = IdChecker.Check(entries);

        Assert.True(result.IsValid);
        Assert.Equal(0, result.TotalIssues);
    }

    [Fact]
    public void Check_MissingIds_ReturnsIssues()
    {
        var entries = new List<IdEntry>
        {
            new("", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
        };

        var result = IdChecker.Check(entries);

        Assert.False(result.IsValid);
        Assert.Single(result.MissingIds);
    }

    [Fact]
    public void Check_InvalidFormatIds_ReturnsIssues()
    {
        var entries = new List<IdEntry>
        {
            new("f_INVALID", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
        };

        var result = IdChecker.Check(entries);

        Assert.False(result.IsValid);
        Assert.Single(result.InvalidFormatIds);
    }

    [Fact]
    public void Check_WrongPrefixIds_ReturnsIssues()
    {
        var entries = new List<IdEntry>
        {
            new("m_01J5X7K9M2NPQRSTABWXYZ1234", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
        };

        var result = IdChecker.Check(entries);

        Assert.False(result.IsValid);
        Assert.Single(result.WrongPrefixIds);
    }

    [Fact]
    public void Check_TestIdsInProduction_ReturnsIssues()
    {
        var entries = new List<IdEntry>
        {
            new("f001", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
        };

        var result = IdChecker.Check(entries);

        Assert.False(result.IsValid);
        Assert.Single(result.TestIdsInProduction);
    }

    [Fact]
    public void Check_TestIdsInTestPath_ReturnsNoIssues()
    {
        var entries = new List<IdEntry>
        {
            new("f001", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/tests/test.calr"),
        };

        var result = IdChecker.Check(entries);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Check_DuplicateIds_ReturnsIssues()
    {
        var entries = new List<IdEntry>
        {
            new("f_01J5X7K9M2NPQRSTABWXYZ1234", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/file1.calr"),
            new("f_01J5X7K9M2NPQRSTABWXYZ1234", IdKind.Function, "Func2", new TextSpan(0, 0, 1, 1), "/src/file2.calr"),
        };

        var result = IdChecker.Check(entries);

        Assert.False(result.IsValid);
        Assert.Single(result.DuplicateGroups);
        Assert.Equal(2, result.DuplicateGroups[0].Count);
    }

    [Fact]
    public void Check_AllowTestIds_AllowsTestIdsEverywhere()
    {
        var entries = new List<IdEntry>
        {
            new("f001", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
        };

        var result = IdChecker.Check(entries, allowTestIds: true);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Check_GeneratesDiagnostics()
    {
        var entries = new List<IdEntry>
        {
            new("", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
            new("f_INVALID", IdKind.Function, "Func2", new TextSpan(0, 0, 2, 1), "/src/test.calr"),
        };

        var result = IdChecker.Check(entries);
        var diagnostics = IdChecker.GenerateDiagnostics(result).ToList();

        Assert.Equal(2, diagnostics.Count);
    }

    [Fact]
    public void DetectIdChurn_DetectsChangedIds()
    {
        var oldEntries = new List<IdEntry>
        {
            new("f_01J5X7K9M2NPQRSTABWXYZ1234", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
        };
        var newEntries = new List<IdEntry>
        {
            new("f_01J5X7K9M2NPQRSTABWXYZ9912", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
        };

        var churn = IdChecker.DetectIdChurn(oldEntries, newEntries);

        Assert.Single(churn);
        Assert.Equal("f_01J5X7K9M2NPQRSTABWXYZ1234", churn[0].Old.Id);
        Assert.Equal("f_01J5X7K9M2NPQRSTABWXYZ9912", churn[0].New.Id);
    }

    [Fact]
    public void DetectIdChurn_IgnoresNewFunctions()
    {
        var oldEntries = new List<IdEntry>
        {
            new("f_01J5X7K9M2NPQRSTABWXYZ1234", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
        };
        var newEntries = new List<IdEntry>
        {
            new("f_01J5X7K9M2NPQRSTABWXYZ1234", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
            new("f_01J5X7K9M2NPQRSTABWXYZ9912", IdKind.Function, "NewFunc", new TextSpan(0, 0, 5, 1), "/src/test.calr"),
        };

        var churn = IdChecker.DetectIdChurn(oldEntries, newEntries);

        Assert.Empty(churn);
    }

    [Fact]
    public void DetectIdChurn_IgnoresDeletedFunctions()
    {
        var oldEntries = new List<IdEntry>
        {
            new("f_01J5X7K9M2NPQRSTABWXYZ1234", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
            new("f_01J5X7K9M2NPQRSTABWXYZ9912", IdKind.Function, "DeletedFunc", new TextSpan(0, 0, 5, 1), "/src/test.calr"),
        };
        var newEntries = new List<IdEntry>
        {
            new("f_01J5X7K9M2NPQRSTABWXYZ1234", IdKind.Function, "Func1", new TextSpan(0, 0, 1, 1), "/src/test.calr"),
        };

        var churn = IdChecker.DetectIdChurn(oldEntries, newEntries);

        Assert.Empty(churn);
    }
}
