using Calor.Compiler.Effects;
using Xunit;

namespace Calor.Compiler.Tests;

/// <summary>
/// Tests for fixes discovered during the C# to Calor conversion campaign.
/// </summary>
public class ConversionCampaignFixTests
{
    #region Issue 310: Async console operations in known effects

    [Fact]
    public void BuiltInEffects_TextWriterWriteLineAsync_IsKnown()
    {
        Assert.True(BuiltInEffects.IsKnown("System.IO.TextWriter::WriteLineAsync(System.String)"));
    }

    [Fact]
    public void BuiltInEffects_StreamReaderReadLineAsync_IsKnown()
    {
        Assert.True(BuiltInEffects.IsKnown("System.IO.StreamReader::ReadLineAsync()"));
    }

    [Fact]
    public void BuiltInEffects_TextWriterFlushAsync_IsKnown()
    {
        Assert.True(BuiltInEffects.IsKnown("System.IO.TextWriter::FlushAsync()"));
    }

    #endregion

    #region Issue 311: Math functions as known pure methods

    [Fact]
    public void BuiltInEffects_MathFloor_IsPure()
    {
        Assert.True(BuiltInEffects.IsKnownPure("System.Math::Floor(System.Double)"));
    }

    [Fact]
    public void BuiltInEffects_MathClamp_IsPure()
    {
        Assert.True(BuiltInEffects.IsKnownPure("System.Math::Clamp(System.Int32,System.Int32,System.Int32)"));
    }

    [Fact]
    public void BuiltInEffects_MathSin_IsPure()
    {
        Assert.True(BuiltInEffects.IsKnownPure("System.Math::Sin(System.Double)"));
    }

    [Fact]
    public void BuiltInEffects_MathRound_IsPure()
    {
        Assert.True(BuiltInEffects.IsKnownPure("System.Math::Round(System.Double,System.Int32)"));
    }

    [Fact]
    public void BuiltInEffects_MathLog_IsPure()
    {
        Assert.True(BuiltInEffects.IsKnownPure("System.Math::Log(System.Double)"));
    }

    #endregion
}
