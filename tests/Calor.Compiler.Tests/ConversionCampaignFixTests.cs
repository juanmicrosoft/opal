using Calor.Compiler.Migration;
using Xunit;

namespace Calor.Compiler.Tests;

/// <summary>
/// Tests for fixes discovered during the C# to Calor conversion campaign.
/// </summary>
public class ConversionCampaignFixTests
{
    private readonly CSharpToCalorConverter _converter = new();

    #region Issue 303: Convert §DICT/§LIST to §FLD inside class bodies

    [Fact]
    public void Convert_DictionaryField_EmitsFLDNotDICT()
    {
        var result = _converter.Convert(@"
public class Cache
{
    private Dictionary<string, int> _lookup = new();
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        Assert.Contains("§FLD{", result.CalorSource);
        Assert.DoesNotContain("§DICT{", result.CalorSource);
        Assert.DoesNotContain("§/DICT{", result.CalorSource);
    }

    [Fact]
    public void Convert_ListField_EmitsFLDNotLIST()
    {
        var result = _converter.Convert(@"
public class Service
{
    private List<string> _items = new();
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        Assert.Contains("§FLD{", result.CalorSource);
        Assert.DoesNotContain("§LIST{", result.CalorSource);
        Assert.DoesNotContain("§/LIST{", result.CalorSource);
    }

    [Fact]
    public void Convert_ComplexDictionaryField_EmitsFLDNotDICT()
    {
        var result = _converter.Convert(@"
public class Cache
{
    private readonly Dictionary<string, List<int>> _cache = new Dictionary<string, List<int>>();
}");
        Assert.True(result.Success, string.Join("\n", result.Issues));
        Assert.Contains("§FLD{", result.CalorSource);
        Assert.DoesNotContain("§DICT{", result.CalorSource);
        Assert.DoesNotContain("§/DICT{", result.CalorSource);
    }

    #endregion
}
