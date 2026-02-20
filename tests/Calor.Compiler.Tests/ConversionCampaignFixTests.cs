using Calor.Compiler.Migration;
using Xunit;

namespace Calor.Compiler.Tests;

/// <summary>
/// Tests for fixes discovered during the C# to Calor conversion campaign.
/// </summary>
public class ConversionCampaignFixTests
{
    private readonly CSharpToCalorConverter _converter = new();

    #region Issue 309: Handle @-prefixed C# parameter names

    [Fact]
    public void Convert_AtPrefixedParam_StripsAtPrefix()
    {
        var result = _converter.Convert(@"
public class Example
{
    public void Process(string @class, int @event)
    {
        var x = @class;
    }
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        var source = result.CalorSource ?? "";
        // Parameters should not have @ prefix in Calor
        Assert.Contains("§I{str:class}", source);
        Assert.Contains("§I{i32:event}", source);
        Assert.DoesNotContain("@class", source);
        Assert.DoesNotContain("@event", source);
    }

    [Fact]
    public void Convert_AtPrefixedLocalVariable_StripsAtPrefix()
    {
        var result = _converter.Convert(@"
public class Example
{
    public void Process()
    {
        var @object = 42;
    }
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        var source = result.CalorSource ?? "";
        Assert.Contains("object", source);
        Assert.DoesNotContain("@object", source);
    }

    #endregion
}
