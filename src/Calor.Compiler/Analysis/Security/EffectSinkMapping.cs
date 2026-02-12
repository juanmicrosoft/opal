namespace Calor.Compiler.Analysis.Security;

/// <summary>
/// Represents effect kinds from the Calor effect system.
/// </summary>
public enum EffectKind
{
    /// <summary>No effect (pure computation).</summary>
    Pure,
    /// <summary>I/O effects (file, network, database, etc.).</summary>
    IO,
    /// <summary>Process/system effects.</summary>
    Process,
    /// <summary>Memory/allocation effects.</summary>
    Memory,
    /// <summary>Console/UI effects.</summary>
    Console
}

/// <summary>
/// Represents effect access modes.
/// </summary>
public enum EffectAccess
{
    /// <summary>Read-only access.</summary>
    Read,
    /// <summary>Write-only access.</summary>
    Write,
    /// <summary>Read and write access.</summary>
    ReadWrite
}

/// <summary>
/// Parsed effect declaration from Calor code.
/// </summary>
public readonly record struct EffectDeclaration(
    EffectKind Kind,
    string Resource,
    EffectAccess Access);

/// <summary>
/// Maps effect system declarations to taint sinks.
/// This enables automatic derivation of security-sensitive operations
/// from the Calor effect system.
/// </summary>
public static class EffectSinkMapping
{
    /// <summary>
    /// Maps an effect declaration to a taint sink.
    /// Returns null if the effect doesn't correspond to a security sink.
    /// </summary>
    /// <param name="effect">The effect declaration to map.</param>
    /// <returns>The corresponding taint sink, or null if not a sink.</returns>
    public static TaintSink? MapEffectToSink(EffectDeclaration effect)
    {
        // Only write and read-write effects are sinks (read effects are sources, not sinks)
        if (effect.Access == EffectAccess.Read)
            return null;

        return effect.Resource.ToLowerInvariant() switch
        {
            // Database effects
            "db" or "database" or "sql" => TaintSink.SqlQuery,

            // Filesystem effects
            "fs" or "filesystem" or "file" => TaintSink.FilePath,

            // Network effects
            "net" or "network" or "http" => TaintSink.UrlRedirect,

            // Process/command effects
            "process" or "system" or "exec" or "shell" => TaintSink.CommandExecution,

            // HTML/web output effects
            "html" or "web" or "response" => TaintSink.HtmlOutput,

            // Code evaluation effects
            "eval" or "code" or "script" => TaintSink.CodeEval,

            // Serialization effects
            "serialize" or "deserialize" or "marshal" => TaintSink.Deserialization,

            // Logging effects (potential log injection)
            "log" or "audit" => TaintSink.LogOutput,

            _ => null
        };
    }

    /// <summary>
    /// Maps an effect kind and value string to a taint sink.
    /// </summary>
    /// <param name="kind">The effect kind (IO, Process, etc.).</param>
    /// <param name="value">The effect value string (e.g., "database_write").</param>
    /// <returns>The corresponding taint sink, or null if not a sink.</returns>
    public static TaintSink? MapEffectToSink(EffectKind kind, string value)
    {
        var lowerValue = value.ToLowerInvariant();

        return (kind, lowerValue) switch
        {
            // Database effects
            (EffectKind.IO, "database_write") => TaintSink.SqlQuery,
            (EffectKind.IO, "database_readwrite") => TaintSink.SqlQuery,
            (EffectKind.IO, "db:w") => TaintSink.SqlQuery,
            (EffectKind.IO, "db:rw") => TaintSink.SqlQuery,

            // Filesystem effects
            (EffectKind.IO, "filesystem_write") => TaintSink.FilePath,
            (EffectKind.IO, "filesystem_readwrite") => TaintSink.FilePath,
            (EffectKind.IO, "fs:w") => TaintSink.FilePath,
            (EffectKind.IO, "fs:rw") => TaintSink.FilePath,

            // Network effects
            (EffectKind.IO, "network_write") => TaintSink.UrlRedirect,
            (EffectKind.IO, "network_readwrite") => TaintSink.UrlRedirect,
            (EffectKind.IO, "net:w") => TaintSink.UrlRedirect,
            (EffectKind.IO, "net:rw") => TaintSink.UrlRedirect,

            // Process effects
            (EffectKind.Process, _) when lowerValue.Contains("exec") => TaintSink.CommandExecution,
            (EffectKind.Process, _) when lowerValue.Contains("shell") => TaintSink.CommandExecution,
            (EffectKind.Process, _) when lowerValue.Contains("system") => TaintSink.CommandExecution,
            (EffectKind.Process, "process:rw") => TaintSink.CommandExecution,

            _ => null
        };
    }

    /// <summary>
    /// Parses an effect string from Calor source code.
    /// Format: "resource:access" where access is r, w, or rw.
    /// Examples: "db:w", "fs:rw", "net:r"
    /// </summary>
    /// <param name="effectString">The effect string to parse.</param>
    /// <returns>The parsed effect declaration, or null if invalid.</returns>
    public static EffectDeclaration? ParseEffect(string effectString)
    {
        if (string.IsNullOrWhiteSpace(effectString))
            return null;

        var parts = effectString.Split(':');
        if (parts.Length != 2)
            return null;

        var resource = parts[0].Trim().ToLowerInvariant();
        var accessStr = parts[1].Trim().ToLowerInvariant();

        var access = accessStr switch
        {
            "r" => EffectAccess.Read,
            "w" => EffectAccess.Write,
            "rw" => EffectAccess.ReadWrite,
            _ => (EffectAccess?)null
        };

        if (access == null)
            return null;

        var kind = resource switch
        {
            "db" or "database" or "sql" or "fs" or "filesystem" or "file" or "net" or "network" or "http" => EffectKind.IO,
            "process" or "system" or "exec" or "shell" => EffectKind.Process,
            "mem" or "memory" or "alloc" => EffectKind.Memory,
            "console" or "stdin" or "stdout" or "stderr" => EffectKind.Console,
            _ => EffectKind.IO // Default to IO for unknown resources
        };

        return new EffectDeclaration(kind, resource, access.Value);
    }

    /// <summary>
    /// Determines if a function with the given effects has any security-sensitive sinks.
    /// </summary>
    /// <param name="effects">The list of effect strings declared for the function.</param>
    /// <returns>A list of taint sinks derived from the effects.</returns>
    public static IReadOnlyList<TaintSink> GetSinksFromEffects(IEnumerable<string> effects)
    {
        var sinks = new List<TaintSink>();

        foreach (var effectStr in effects)
        {
            var effect = ParseEffect(effectStr);
            if (effect != null)
            {
                var sink = MapEffectToSink(effect.Value);
                if (sink != null && !sinks.Contains(sink.Value))
                {
                    sinks.Add(sink.Value);
                }
            }
        }

        return sinks;
    }

    /// <summary>
    /// Maps an effect declaration to a taint source (for read effects).
    /// </summary>
    /// <param name="effect">The effect declaration to map.</param>
    /// <returns>The corresponding taint source, or null if not a source.</returns>
    public static TaintSource? MapEffectToSource(EffectDeclaration effect)
    {
        // Only read and read-write effects are sources
        if (effect.Access == EffectAccess.Write)
            return null;

        return effect.Resource.ToLowerInvariant() switch
        {
            // Database read effects
            "db" or "database" or "sql" => TaintSource.DatabaseResult,

            // Filesystem read effects
            "fs" or "filesystem" or "file" => TaintSource.FileRead,

            // Network read effects
            "net" or "network" or "http" => TaintSource.NetworkInput,

            // Console input effects
            "console" or "stdin" => TaintSource.UserInput,

            // Environment effects
            "env" or "environment" => TaintSource.Environment,

            _ => null
        };
    }
}
