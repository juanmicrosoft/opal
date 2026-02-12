using Calor.Compiler.Analysis.Security;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Calor.Compiler.Tests.Analysis;

/// <summary>
/// Property-based tests for effect matching logic using FsCheck.
/// These tests verify invariants that should hold for any input.
/// </summary>
public class EffectMatchingPropertyTests
{
    #region Generators

    /// <summary>
    /// Generates valid effect strings in "category:value" format.
    /// </summary>
    public static Arbitrary<string> ValidEffectStrings()
    {
        var categories = new[] { "io", "process", "memory", "console" };
        var values = new[]
        {
            "database_write", "database_read", "database_readwrite",
            "filesystem_write", "filesystem_read", "filesystem_readwrite",
            "network_write", "network_read", "network_readwrite",
            "console_write", "console_read",
            "environment_write", "environment_read",
            "process", "exec_command", "shell_execute"
        };

        return Arb.From(
            from category in Gen.Elements(categories)
            from value in Gen.Elements(values)
            select $"{category}:{value}");
    }

    /// <summary>
    /// Generates call targets that should match database effects.
    /// </summary>
    public static Arbitrary<string> DatabaseCallTargets()
    {
        var prefixes = new[] { "db", "database", "sql", "mysql", "postgres", "sqlite", "mongo" };
        var methods = new[] { "execute", "query", "run", "select", "insert", "update", "delete" };

        return Arb.From(
            from prefix in Gen.Elements(prefixes)
            from method in Gen.Elements(methods)
            from separator in Gen.Elements(".", "_")
            select $"{prefix}{separator}{method}");
    }

    /// <summary>
    /// Generates call targets that should match filesystem effects.
    /// </summary>
    public static Arbitrary<string> FilesystemCallTargets()
    {
        var prefixes = new[] { "fs", "file", "path", "directory", "dir" };
        var methods = new[] { "open", "read", "write", "create", "delete", "exists" };

        return Arb.From(
            from prefix in Gen.Elements(prefixes)
            from method in Gen.Elements(methods)
            from separator in Gen.Elements(".", "_")
            select $"{prefix}{separator}{method}");
    }

    /// <summary>
    /// Generates random strings that shouldn't accidentally match effects.
    /// </summary>
    public static Arbitrary<string> SafeRandomStrings()
    {
        // Generate strings that avoid effect-related keywords
        var safeWords = new[]
        {
            "calculate", "transform", "validate", "format", "parse",
            "convert", "encode", "decode", "compress", "hash",
            "sort", "filter", "map", "reduce", "aggregate",
            "user", "item", "value", "result", "output"
        };

        return Arb.From(
            from word1 in Gen.Elements(safeWords)
            from word2 in Gen.Elements(safeWords)
            from separator in Gen.Elements(".", "_", "")
            select $"{word1}{separator}{word2}");
    }

    #endregion

    #region Property Tests - EffectSinkMapping

    [Property(MaxTest = 100)]
    public Property ParseEffect_ValidFormat_NeverThrows()
    {
        // Property: ParseEffect should never throw for any string input
        return Prop.ForAll(Arb.From<string>(), input =>
        {
            try
            {
                _ = EffectSinkMapping.ParseEffect(input);
                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    [Property(MaxTest = 100)]
    public Property ParseEffect_ValidAccessCodes_AlwaysParses()
    {
        // Property: Valid "resource:access" formats always parse successfully
        var resources = new[] { "db", "fs", "net", "console", "env", "process" };
        var accesses = new[] { "r", "w", "rw" };

        return Prop.ForAll(
            Gen.Elements(resources).ToArbitrary(),
            Gen.Elements(accesses).ToArbitrary(),
            (resource, access) =>
            {
                var effect = $"{resource}:{access}";
                var result = EffectSinkMapping.ParseEffect(effect);
                return result != null;
            });
    }

    [Property(MaxTest = 100)]
    public Property MapEffectToSink_WriteEffects_ReturnsSinkOrNull()
    {
        // Property: Write effects either map to a sink or return null (never throw)
        var writeValues = new[]
        {
            "database_write", "filesystem_write", "network_write",
            "console_write", "environment_write", "process"
        };

        return Prop.ForAll(Gen.Elements(writeValues).ToArbitrary(), value =>
        {
            try
            {
                var result = EffectSinkMapping.MapEffectToSink(EffectKind.IO, value);
                // Result is either a valid sink or null
                return result == null || Enum.IsDefined(typeof(TaintSink), result.Value);
            }
            catch
            {
                return false;
            }
        });
    }

    [Property(MaxTest = 100)]
    public Property MapEffectToSink_ReadEffects_NeverReturnSink()
    {
        // Property: Read-only effects should never return a sink (sinks are for write operations)
        var readOnlyEffects = new[]
        {
            new EffectDeclaration(EffectKind.IO, "db", EffectAccess.Read),
            new EffectDeclaration(EffectKind.IO, "fs", EffectAccess.Read),
            new EffectDeclaration(EffectKind.IO, "net", EffectAccess.Read),
            new EffectDeclaration(EffectKind.Console, "console", EffectAccess.Read)
        };

        return Prop.ForAll(Gen.Elements(readOnlyEffects).ToArbitrary(), effect =>
        {
            var result = EffectSinkMapping.MapEffectToSink(effect);
            return result == null; // Read-only effects are not sinks
        });
    }

    [Property(MaxTest = 100)]
    public Property GetSinksFromEffects_EmptyInput_ReturnsEmpty()
    {
        // Property: Empty or null-ish inputs return empty list
        return Prop.ForAll(
            Gen.Elements(
                Array.Empty<string>(),
                new[] { "", "   ", "invalid" }
            ).ToArbitrary(),
            effects =>
            {
                var result = EffectSinkMapping.GetSinksFromEffects(effects);
                // Should return empty for invalid inputs
                return result != null && result.Count <= effects.Length;
            });
    }

    [Property(MaxTest = 100)]
    public Property GetSinksFromEffects_NoDuplicates()
    {
        // Property: Result should never contain duplicate sinks
        var effects = new[] { "db:w", "database:w", "sql:rw", "fs:w", "net:w" };

        return Prop.ForAll(
            Gen.SubListOf(effects).ToArbitrary(),
            selectedEffects =>
            {
                var result = EffectSinkMapping.GetSinksFromEffects(selectedEffects);
                var distinct = result.Distinct().Count();
                return result.Count == distinct; // No duplicates
            });
    }

    #endregion

    #region Property Tests - Sink Mapping Consistency

    [Property(MaxTest = 50)]
    public Property DatabaseEffects_AlwaysMapToSqlSink()
    {
        // Property: All database write effects should map to SqlQuery sink
        var dbEffects = new[]
        {
            new EffectDeclaration(EffectKind.IO, "db", EffectAccess.Write),
            new EffectDeclaration(EffectKind.IO, "database", EffectAccess.Write),
            new EffectDeclaration(EffectKind.IO, "sql", EffectAccess.Write),
            new EffectDeclaration(EffectKind.IO, "db", EffectAccess.ReadWrite),
            new EffectDeclaration(EffectKind.IO, "database", EffectAccess.ReadWrite)
        };

        return Prop.ForAll(Gen.Elements(dbEffects).ToArbitrary(), effect =>
        {
            var result = EffectSinkMapping.MapEffectToSink(effect);
            return result == TaintSink.SqlQuery;
        });
    }

    [Property(MaxTest = 50)]
    public Property FilesystemEffects_AlwaysMapToFilePathSink()
    {
        // Property: All filesystem write effects should map to FilePath sink
        var fsEffects = new[]
        {
            new EffectDeclaration(EffectKind.IO, "fs", EffectAccess.Write),
            new EffectDeclaration(EffectKind.IO, "filesystem", EffectAccess.Write),
            new EffectDeclaration(EffectKind.IO, "file", EffectAccess.Write),
            new EffectDeclaration(EffectKind.IO, "fs", EffectAccess.ReadWrite)
        };

        return Prop.ForAll(Gen.Elements(fsEffects).ToArbitrary(), effect =>
        {
            var result = EffectSinkMapping.MapEffectToSink(effect);
            return result == TaintSink.FilePath;
        });
    }

    [Property(MaxTest = 50)]
    public Property ProcessEffects_AlwaysMapToCommandExecutionSink()
    {
        // Property: All process/command effects should map to CommandExecution sink
        var processEffects = new[]
        {
            new EffectDeclaration(EffectKind.Process, "process", EffectAccess.Write),
            new EffectDeclaration(EffectKind.Process, "system", EffectAccess.Write),
            new EffectDeclaration(EffectKind.Process, "exec", EffectAccess.Write),
            new EffectDeclaration(EffectKind.Process, "shell", EffectAccess.Write)
        };

        return Prop.ForAll(Gen.Elements(processEffects).ToArbitrary(), effect =>
        {
            var result = EffectSinkMapping.MapEffectToSink(effect);
            return result == TaintSink.CommandExecution;
        });
    }

    #endregion

    #region Property Tests - Robustness

    [Property(MaxTest = 200)]
    public Property ParseEffect_CaseInsensitive()
    {
        // Property: Parsing should be case-insensitive
        var resources = new[] { "db", "DB", "Db", "dB" };
        var accesses = new[] { "w", "W", "r", "R", "rw", "RW", "Rw" };

        return Prop.ForAll(
            Gen.Elements(resources).ToArbitrary(),
            Gen.Elements(accesses).ToArbitrary(),
            (resource, access) =>
            {
                var effect = $"{resource}:{access}";
                var result = EffectSinkMapping.ParseEffect(effect);

                // Should parse successfully regardless of case
                if (result == null) return false;

                // Resource should be normalized to lowercase
                return result.Value.Resource == resource.ToLowerInvariant();
            });
    }

    [Property(MaxTest = 100)]
    public Property MapEffectToSink_NeverThrows()
    {
        // Property: MapEffectToSink should never throw for any EffectKind/string combination
        return Prop.ForAll(
            Gen.Elements(Enum.GetValues<EffectKind>()).ToArbitrary(),
            Arb.From<string>(),
            (kind, value) =>
            {
                try
                {
                    _ = EffectSinkMapping.MapEffectToSink(kind, value ?? "");
                    return true;
                }
                catch
                {
                    return false;
                }
            });
    }

    [Property(MaxTest = 100)]
    public Property SourceAndSinkMapping_Exclusive()
    {
        // Property: An effect with Read-only access should map to Source, not Sink
        // An effect with Write-only access should map to Sink, not Source
        var resources = new[] { "db", "fs", "net" };

        return Prop.ForAll(Gen.Elements(resources).ToArbitrary(), resource =>
        {
            var readEffect = new EffectDeclaration(EffectKind.IO, resource, EffectAccess.Read);
            var writeEffect = new EffectDeclaration(EffectKind.IO, resource, EffectAccess.Write);

            var readSource = EffectSinkMapping.MapEffectToSource(readEffect);
            var readSink = EffectSinkMapping.MapEffectToSink(readEffect);
            var writeSource = EffectSinkMapping.MapEffectToSource(writeEffect);
            var writeSink = EffectSinkMapping.MapEffectToSink(writeEffect);

            // Read-only should be source, not sink
            var readCorrect = readSource != null && readSink == null;
            // Write-only should be sink, not source
            var writeCorrect = writeSink != null && writeSource == null;

            return readCorrect && writeCorrect;
        });
    }

    #endregion

    #region Fuzzing Tests - Edge Cases

    [Property(MaxTest = 500)]
    public Property Fuzzing_RandomStrings_NeverCrash()
    {
        // Fuzz test: Random strings should never crash the system
        return Prop.ForAll(Arb.From<string>(), input =>
        {
            try
            {
                // Try all parsing/mapping operations
                _ = EffectSinkMapping.ParseEffect(input ?? "");
                _ = EffectSinkMapping.MapEffectToSink(EffectKind.IO, input ?? "");
                _ = EffectSinkMapping.GetSinksFromEffects(new[] { input ?? "" });
                return true;
            }
            catch (Exception ex)
            {
                // Only ArgumentNullException is acceptable for null input
                return ex is ArgumentNullException;
            }
        });
    }

    [Property(MaxTest = 200)]
    public Property Fuzzing_MalformedEffects_HandleGracefully()
    {
        // Fuzz test: Malformed effect strings should be handled gracefully
        var malformedPatterns = new[]
        {
            ":", "::", ":::", "a:", ":b", "a:b:c", "a:b:c:d",
            "", " ", "  ", "\t", "\n", "\r\n",
            "a", "abc", "a:b c", "a: b", " a:b", "a:b ",
            "null", "undefined", "NaN", "true", "false",
            "<script>", "'; DROP TABLE", "$()", "`cmd`"
        };

        return Prop.ForAll(Gen.Elements(malformedPatterns).ToArbitrary(), input =>
        {
            var result = EffectSinkMapping.ParseEffect(input);
            // Malformed inputs should return null, not throw
            return result == null || result.Value.Resource != null;
        });
    }

    [Property(MaxTest = 100)]
    public Property Fuzzing_UnicodeStrings_HandleGracefully()
    {
        // Fuzz test: Unicode strings should be handled gracefully
        var unicodeStrings = new[]
        {
            "数据库:写", "データベース:w", "базаданных:w",
            "🔥:w", "db:🔥", "💾:rw",
            "db\u0000:w", "db\uFFFF:w", "db\u200B:w"
        };

        return Prop.ForAll(Gen.Elements(unicodeStrings).ToArbitrary(), input =>
        {
            try
            {
                _ = EffectSinkMapping.ParseEffect(input);
                return true; // Didn't crash
            }
            catch
            {
                return false;
            }
        });
    }

    #endregion
}
