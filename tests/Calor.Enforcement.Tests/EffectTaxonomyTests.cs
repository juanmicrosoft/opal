using Calor.Compiler.Effects;
using Calor.Compiler.Parsing;
using Xunit;

namespace Calor.Enforcement.Tests;

/// <summary>
/// Tests for the effect taxonomy (new granular effect codes and backward compatibility).
/// </summary>
public class EffectTaxonomyTests
{
    [Theory]
    [InlineData("fs:r", EffectKind.IO, "filesystem_read")]
    [InlineData("fs:w", EffectKind.IO, "filesystem_write")]
    [InlineData("fs:rw", EffectKind.IO, "filesystem_readwrite")]
    [InlineData("net:r", EffectKind.IO, "network_read")]
    [InlineData("net:w", EffectKind.IO, "network_write")]
    [InlineData("net:rw", EffectKind.IO, "network_readwrite")]
    [InlineData("db:r", EffectKind.IO, "database_read")]
    [InlineData("db:w", EffectKind.IO, "database_write")]
    [InlineData("db:rw", EffectKind.IO, "database_readwrite")]
    [InlineData("env:r", EffectKind.IO, "environment_read")]
    [InlineData("env:w", EffectKind.IO, "environment_write")]
    [InlineData("rng", EffectKind.Nondeterminism, "random")]
    [InlineData("unsafe", EffectKind.Memory, "unsafe")]
    public void NewGranularEffectCodes_ParseCorrectly(string code, EffectKind expectedKind, string expectedValue)
    {
        // Test via AttributeHelper
        var (category, value) = AttributeHelper.ExpandEffectCode(code);
        Assert.Equal(expectedValue, value);

        // Test via EffectSet
        var effectSet = EffectSet.From(code);
        Assert.Single(effectSet.Effects);
        var effect = effectSet.Effects.First();
        Assert.Equal(expectedKind, effect.Kind);
        Assert.Equal(expectedValue, effect.Value);
    }

    [Theory]
    [InlineData("cw", EffectKind.IO, "console_write")]
    [InlineData("cr", EffectKind.IO, "console_read")]
    [InlineData("fw", EffectKind.IO, "file_write")]
    [InlineData("fr", EffectKind.IO, "file_read")]
    [InlineData("fd", EffectKind.IO, "file_delete")]
    [InlineData("dbr", EffectKind.IO, "database_read")]
    [InlineData("dbw", EffectKind.IO, "database_write")]
    [InlineData("time", EffectKind.Nondeterminism, "time")]
    [InlineData("rand", EffectKind.Nondeterminism, "random")]
    [InlineData("alloc", EffectKind.Memory, "allocation")]
    [InlineData("proc", EffectKind.IO, "process")]
    public void LegacyEffectCodes_StillWork(string code, EffectKind expectedKind, string expectedValue)
    {
        var effectSet = EffectSet.From(code);
        Assert.Single(effectSet.Effects);
        var effect = effectSet.Effects.First();
        Assert.Equal(expectedKind, effect.Kind);
        Assert.Equal(expectedValue, effect.Value);
    }

    [Theory]
    [InlineData("net", EffectKind.IO, "network_readwrite")]
    [InlineData("db", EffectKind.IO, "database_readwrite")]
    [InlineData("env", EffectKind.IO, "environment_readwrite")]
    public void LegacyBroadCodes_MapToReadWrite(string code, EffectKind expectedKind, string expectedValue)
    {
        var effectSet = EffectSet.From(code);
        Assert.Single(effectSet.Effects);
        var effect = effectSet.Effects.First();
        Assert.Equal(expectedKind, effect.Kind);
        Assert.Equal(expectedValue, effect.Value);
    }

    [Fact]
    public void RngAlias_MapsToRandom()
    {
        var rngSet = EffectSet.From("rng");
        var randSet = EffectSet.From("rand");

        Assert.Equal(rngSet, randSet);
    }

    [Fact]
    public void EffectSet_ToDisplayString_ShowsHumanReadableCodes()
    {
        var effects = EffectSet.From("cw", "fs:r", "rand");
        var display = effects.ToDisplayString();

        // Should contain surface codes, sorted
        Assert.Contains("cw", display);
        Assert.Contains("fs:r", display);
        Assert.Contains("rand", display);
    }

    [Fact]
    public void EffectSet_Empty_DisplaysAsPure()
    {
        var effects = EffectSet.Empty;
        Assert.Equal("[pure]", effects.ToDisplayString());
    }

    [Fact]
    public void EffectSet_Unknown_DisplaysAsUnknown()
    {
        var effects = EffectSet.Unknown;
        Assert.Equal("[unknown]", effects.ToDisplayString());
    }
}
