using Calor.Compiler.Migration;
using Xunit;

namespace Calor.Compiler.Tests;

/// <summary>
/// Tests for fixes discovered during the C# to Calor conversion campaign.
/// </summary>
public class ConversionCampaignFixTests
{
    private readonly CSharpToCalorConverter _converter = new();

    #region Issue 300: Convert postfix/prefix increment to compound assignment

    [Fact]
    public void Convert_PostfixIncrement_ProducesCompoundAssignment()
    {
        var result = _converter.Convert(@"
public class Test
{
    public void Count()
    {
        int i = 0;
        i++;
    }
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        Assert.DoesNotContain("§ERR", result.CalorSource);
        Assert.DoesNotContain("postfix", result.CalorSource?.ToLower() ?? "");
    }

    [Fact]
    public void Convert_PostfixDecrement_ProducesCompoundAssignment()
    {
        var result = _converter.Convert(@"
public class Test
{
    public void CountDown()
    {
        int i = 10;
        i--;
    }
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        Assert.DoesNotContain("§ERR", result.CalorSource);
    }

    [Fact]
    public void Convert_PrefixIncrement_ProducesCompoundAssignment()
    {
        var result = _converter.Convert(@"
public class Test
{
    public void Count()
    {
        int i = 0;
        ++i;
    }
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        Assert.DoesNotContain("§ERR", result.CalorSource);
    }

    #endregion
}
