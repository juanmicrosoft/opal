using Calor.Compiler.Migration;
using Xunit;

namespace Calor.Compiler.Tests;

/// <summary>
/// Tests for fixes discovered during the C# to Calor conversion campaign.
/// </summary>
public class ConversionCampaignFixTests
{
    private readonly CSharpToCalorConverter _converter = new();

    #region Issue 301: Convert nameof() to string literal and string.Empty to ""

    [Fact]
    public void Convert_Nameof_ProducesStringLiteral()
    {
        var result = _converter.Convert(@"
public class Test
{
    public void Check(string name)
    {
        var x = nameof(name);
    }
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        Assert.Contains(@"""name""", result.CalorSource);
        Assert.DoesNotContain("nameof", result.CalorSource);
    }

    [Fact]
    public void Convert_StringEmpty_ProducesEmptyStringLiteral()
    {
        var result = _converter.Convert(@"
public class Test
{
    public string GetDefault()
    {
        return string.Empty;
    }
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        Assert.Contains(@"""""", result.CalorSource);
        Assert.DoesNotContain("§ERR", result.CalorSource);
    }

    #endregion
}
