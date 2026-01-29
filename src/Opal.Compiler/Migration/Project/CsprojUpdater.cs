using System.Xml.Linq;

namespace Opal.Compiler.Migration.Project;

/// <summary>
/// Updates .csproj files for OPAL integration.
/// </summary>
public sealed class CsprojUpdater
{
    /// <summary>
    /// Updates a .csproj file to include OPAL compilation support.
    /// </summary>
    public async Task<CsprojUpdateResult> UpdateForOpalAsync(string csprojPath)
    {
        if (!File.Exists(csprojPath))
        {
            return new CsprojUpdateResult
            {
                Success = false,
                ErrorMessage = $"Project file not found: {csprojPath}"
            };
        }

        try
        {
            var content = await File.ReadAllTextAsync(csprojPath);
            var doc = XDocument.Parse(content);

            var changes = new List<string>();

            // Add Opal.Runtime reference if not present
            if (!HasPackageReference(doc, "Opal.Runtime"))
            {
                AddPackageReference(doc, "Opal.Runtime", "*");
                changes.Add("Added Opal.Runtime package reference");
            }

            // Add opalc tool reference if not present
            if (!HasDotNetTool(doc, "opalc"))
            {
                changes.Add("Consider adding 'dotnet tool install -g opalc' to enable compilation");
            }

            // Add OPAL file compilation items if there are .opal files
            var projectDir = Path.GetDirectoryName(csprojPath) ?? ".";
            var opalFiles = Directory.GetFiles(projectDir, "*.opal", SearchOption.AllDirectories);

            if (opalFiles.Length > 0 && !HasOpalCompileTarget(doc))
            {
                AddOpalCompileTarget(doc);
                changes.Add("Added OPAL pre-build compilation target");
            }

            // Save changes
            if (changes.Count > 0)
            {
                var backupPath = csprojPath + ".bak";
                File.Copy(csprojPath, backupPath, overwrite: true);

                await using var stream = File.Create(csprojPath);
                await doc.SaveAsync(stream, SaveOptions.None, CancellationToken.None);

                return new CsprojUpdateResult
                {
                    Success = true,
                    Changes = changes,
                    BackupPath = backupPath
                };
            }

            return new CsprojUpdateResult
            {
                Success = true,
                Changes = new List<string> { "No changes needed" }
            };
        }
        catch (Exception ex)
        {
            return new CsprojUpdateResult
            {
                Success = false,
                ErrorMessage = $"Failed to update project: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Creates a new .csproj file with OPAL support.
    /// </summary>
    public async Task<CsprojUpdateResult> CreateOpalProjectAsync(string projectPath, string projectName)
    {
        var csprojPath = Path.Combine(projectPath, $"{projectName}.csproj");

        if (File.Exists(csprojPath))
        {
            return new CsprojUpdateResult
            {
                Success = false,
                ErrorMessage = $"Project file already exists: {csprojPath}"
            };
        }

        try
        {
            Directory.CreateDirectory(projectPath);

            var csprojContent = $"""
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>net8.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>

                  <ItemGroup>
                    <PackageReference Include="Opal.Runtime" Version="*" />
                  </ItemGroup>

                  <!-- Compile OPAL files before build -->
                  <Target Name="CompileOpal" BeforeTargets="BeforeBuild">
                    <Exec Command="opalc --input %(OpalFiles.Identity) --output %(OpalFiles.RootDir)%(OpalFiles.Directory)%(OpalFiles.Filename).g.cs"
                          Condition="'@(OpalFiles)' != ''" />
                  </Target>

                  <ItemGroup>
                    <OpalFiles Include="**/*.opal" />
                    <Compile Include="**/*.g.cs" Condition="Exists('**/*.g.cs')" />
                  </ItemGroup>

                </Project>
                """;

            await File.WriteAllTextAsync(csprojPath, csprojContent);

            return new CsprojUpdateResult
            {
                Success = true,
                Changes = new List<string>
                {
                    $"Created project file: {csprojPath}",
                    "Added Opal.Runtime reference",
                    "Added OPAL pre-build compilation target"
                }
            };
        }
        catch (Exception ex)
        {
            return new CsprojUpdateResult
            {
                Success = false,
                ErrorMessage = $"Failed to create project: {ex.Message}"
            };
        }
    }

    private static bool HasPackageReference(XDocument doc, string packageName)
    {
        return doc.Descendants("PackageReference")
            .Any(e => e.Attribute("Include")?.Value.Equals(packageName, StringComparison.OrdinalIgnoreCase) == true);
    }

    private static bool HasDotNetTool(XDocument doc, string toolName)
    {
        return doc.Descendants("DotNetCliToolReference")
            .Any(e => e.Attribute("Include")?.Value.Contains(toolName, StringComparison.OrdinalIgnoreCase) == true);
    }

    private static bool HasOpalCompileTarget(XDocument doc)
    {
        return doc.Descendants("Target")
            .Any(e => e.Attribute("Name")?.Value.Equals("CompileOpal", StringComparison.OrdinalIgnoreCase) == true);
    }

    private static void AddPackageReference(XDocument doc, string packageName, string version)
    {
        var itemGroup = doc.Descendants("ItemGroup")
            .FirstOrDefault(g => g.Elements("PackageReference").Any());

        if (itemGroup == null)
        {
            itemGroup = new XElement("ItemGroup");
            doc.Root?.Add(itemGroup);
        }

        itemGroup.Add(new XElement("PackageReference",
            new XAttribute("Include", packageName),
            new XAttribute("Version", version)));
    }

    private static void AddOpalCompileTarget(XDocument doc)
    {
        var target = new XElement("Target",
            new XAttribute("Name", "CompileOpal"),
            new XAttribute("BeforeTargets", "BeforeBuild"),
            new XElement("Exec",
                new XAttribute("Command", "opalc --input %(OpalFiles.Identity) --output %(OpalFiles.RootDir)%(OpalFiles.Directory)%(OpalFiles.Filename).g.cs"),
                new XAttribute("Condition", "'@(OpalFiles)' != ''")));

        var opalFilesItemGroup = new XElement("ItemGroup",
            new XElement("OpalFiles",
                new XAttribute("Include", "**/*.opal")),
            new XElement("Compile",
                new XAttribute("Include", "**/*.g.cs"),
                new XAttribute("Condition", "Exists('**/*.g.cs')")));

        doc.Root?.Add(target);
        doc.Root?.Add(opalFilesItemGroup);
    }
}

/// <summary>
/// Result of updating a .csproj file.
/// </summary>
public sealed class CsprojUpdateResult
{
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> Changes { get; init; } = new();
    public string? BackupPath { get; init; }
}
