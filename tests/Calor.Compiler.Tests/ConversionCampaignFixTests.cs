using Calor.Compiler.Migration;
using Xunit;

namespace Calor.Compiler.Tests;

/// <summary>
/// Tests for fixes discovered during the C# to Calor conversion campaign.
/// </summary>
public class ConversionCampaignFixTests
{
    private readonly CSharpToCalorConverter _converter = new();

    #region Issue 302: Emit §MT instead of §SIG for interface method signatures

    [Fact]
    public void Convert_InterfaceMethod_EmitsMTNotSIG()
    {
        var result = _converter.Convert(@"
public interface IAnimal
{
    string Speak();
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        Assert.Contains("§MT{", result.CalorSource);
        Assert.Contains("§/MT{", result.CalorSource);
        Assert.DoesNotContain("§SIG", result.CalorSource);
    }

    [Fact]
    public void Convert_InterfaceMethodWithParams_EmitsMTWithParams()
    {
        var result = _converter.Convert(@"
public interface ICalculator
{
    int Add(int a, int b);
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        Assert.Contains("§MT{", result.CalorSource);
        Assert.Contains("§I{", result.CalorSource);
        Assert.Contains("§O{", result.CalorSource);
        Assert.DoesNotContain("§SIG", result.CalorSource);
    }

    #endregion
}
