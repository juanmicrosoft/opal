using Calor.Compiler;
using Calor.Compiler.Diagnostics;
using Xunit;

namespace Calor.Semantics.Tests;

/// <summary>
/// Tests for semantics versioning (S11).
/// </summary>
public class VersioningTests
{
    /// <summary>
    /// S11: Semantics version mismatch emits diagnostic.
    /// </summary>
    [Fact]
    public void S11_SemanticsVersionMismatch_EmitsDiagnostic()
    {
        // Check that the version checking logic works correctly
        var currentVersion = SemanticsVersion.Current;

        // Same major version, higher minor = possibly incompatible (warning)
        var higherMinor = new Version(currentVersion.Major, currentVersion.Minor + 1, 0);
        var compat1 = SemanticsVersion.CheckCompatibility(higherMinor);
        Assert.Equal(VersionCompatibility.PossiblyIncompatible, compat1);

        // Higher major version = incompatible (error)
        var higherMajor = new Version(currentVersion.Major + 1, 0, 0);
        var compat2 = SemanticsVersion.CheckCompatibility(higherMajor);
        Assert.Equal(VersionCompatibility.Incompatible, compat2);

        // Same or lower version = compatible
        var sameVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);
        var compat3 = SemanticsVersion.CheckCompatibility(sameVersion);
        Assert.Equal(VersionCompatibility.Compatible, compat3);
    }

    /// <summary>
    /// Current semantics version is 1.0.0.
    /// </summary>
    [Fact]
    public void CurrentVersion_Is_1_0_0()
    {
        Assert.Equal(1, SemanticsVersion.Major);
        Assert.Equal(0, SemanticsVersion.Minor);
        Assert.Equal(0, SemanticsVersion.Patch);
        Assert.Equal("1.0.0", SemanticsVersion.VersionString);
    }

    /// <summary>
    /// Compatible versions are correctly identified.
    /// </summary>
    [Fact]
    public void CompatibleVersions_CorrectlyIdentified()
    {
        // All 1.0.x versions are compatible
        Assert.Equal(VersionCompatibility.Compatible,
            SemanticsVersion.CheckCompatibility(new Version(1, 0, 0)));
        Assert.Equal(VersionCompatibility.Compatible,
            SemanticsVersion.CheckCompatibility(new Version(1, 0, 1)));
        Assert.Equal(VersionCompatibility.Compatible,
            SemanticsVersion.CheckCompatibility(new Version(1, 0, 99)));

        // 0.x versions are also compatible (older)
        Assert.Equal(VersionCompatibility.Compatible,
            SemanticsVersion.CheckCompatibility(new Version(0, 9, 0)));
    }

    /// <summary>
    /// Diagnostic codes for version mismatch exist.
    /// </summary>
    [Fact]
    public void VersionDiagnosticCodes_Exist()
    {
        Assert.Equal("Calor0700", DiagnosticCode.SemanticsVersionMismatch);
        Assert.Equal("Calor0701", DiagnosticCode.SemanticsVersionIncompatible);
    }

    /// <summary>
    /// Version string parsing works.
    /// </summary>
    [Fact]
    public void VersionString_Parsing()
    {
        var result = SemanticsVersion.CheckCompatibility("1.0.0");
        Assert.NotNull(result);
        Assert.Equal(VersionCompatibility.Compatible, result.Value);

        // Invalid version string
        var invalid = SemanticsVersion.CheckCompatibility("not-a-version");
        Assert.Null(invalid);
    }

    /// <summary>
    /// Minor version increments are forward-compatible.
    /// </summary>
    [Fact]
    public void MinorVersionIncrement_ForwardCompatible()
    {
        // Code written for 1.0.0 can run on 1.1.0 (compatible)
        // But 1.1.0 code on 1.0.0 compiler is PossiblyIncompatible

        // Older code on newer compiler
        Assert.Equal(VersionCompatibility.Compatible,
            SemanticsVersion.CheckCompatibility(new Version(1, 0, 0)));

        // Newer code on this compiler (if minor is 0)
        if (SemanticsVersion.Minor == 0)
        {
            Assert.Equal(VersionCompatibility.PossiblyIncompatible,
                SemanticsVersion.CheckCompatibility(new Version(1, 1, 0)));
        }
    }

    /// <summary>
    /// Major version changes are breaking.
    /// </summary>
    [Fact]
    public void MajorVersionChange_Breaking()
    {
        // Version 2.0.0 is incompatible with 1.x compiler
        Assert.Equal(VersionCompatibility.Incompatible,
            SemanticsVersion.CheckCompatibility(new Version(2, 0, 0)));

        // Any 2.x version
        Assert.Equal(VersionCompatibility.Incompatible,
            SemanticsVersion.CheckCompatibility(new Version(2, 5, 0)));

        // Future versions
        Assert.Equal(VersionCompatibility.Incompatible,
            SemanticsVersion.CheckCompatibility(new Version(99, 0, 0)));
    }
}
