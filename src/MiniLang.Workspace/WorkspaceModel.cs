using System.Text.Json;
using System.Text.Json.Serialization;
using MiniLang.Core;

namespace MiniLang.Workspace;

public sealed record InteropSettings(
    [property: JsonPropertyName("allowUserModeWin32")] bool AllowUserModeWin32,
    [property: JsonPropertyName("allowDotNetBridge")] bool AllowDotNetBridge);

public sealed record MiniProjectManifest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("target")] string Target,
    [property: JsonPropertyName("sources")] string[] Sources,
    [property: JsonPropertyName("references")] string[] References,
    [property: JsonPropertyName("packageSources")] string[] PackageSources,
    [property: JsonPropertyName("defines")] string[] Defines,
    [property: JsonPropertyName("outputKind")] string OutputKind,
    [property: JsonPropertyName("interop")] InteropSettings Interop);

public sealed record MiniWorkspaceManifest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("projects")] string[] Projects);

public sealed record LoadedProject(string ProjectPath, MiniProjectManifest Manifest, IReadOnlyList<SourceDocument> Documents, Compilation Compilation);
public sealed record LoadedWorkspace(string WorkspacePath, MiniWorkspaceManifest Manifest, IReadOnlyList<LoadedProject> Projects);

public static class MiniWorkspaceLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static LoadedProject LoadProject(string projectPath)
    {
        var fullProjectPath = Path.GetFullPath(projectPath);
        var manifest = JsonSerializer.Deserialize<MiniProjectManifest>(File.ReadAllText(fullProjectPath), JsonOptions)
            ?? throw new InvalidOperationException($"Unable to parse project manifest '{fullProjectPath}'.");
        var projectDirectory = Path.GetDirectoryName(fullProjectPath) ?? Environment.CurrentDirectory;
        var documents = manifest.Sources
            .Select(source => new SourceDocument(Path.GetFullPath(Path.Combine(projectDirectory, source)), File.ReadAllText(Path.GetFullPath(Path.Combine(projectDirectory, source)))))
            .ToArray();
        var compilation = Compilation.Create(documents);
        return new LoadedProject(fullProjectPath, manifest, documents, compilation);
    }

    public static LoadedWorkspace LoadWorkspace(string workspacePath)
    {
        var fullWorkspacePath = Path.GetFullPath(workspacePath);
        var manifest = JsonSerializer.Deserialize<MiniWorkspaceManifest>(File.ReadAllText(fullWorkspacePath), JsonOptions)
            ?? throw new InvalidOperationException($"Unable to parse workspace manifest '{fullWorkspacePath}'.");
        var workspaceDirectory = Path.GetDirectoryName(fullWorkspacePath) ?? Environment.CurrentDirectory;
        var projects = manifest.Projects
            .Select(project => LoadProject(Path.GetFullPath(Path.Combine(workspaceDirectory, project))))
            .ToArray();
        return new LoadedWorkspace(fullWorkspacePath, manifest, projects);
    }
}

public static class DocsExport
{
    public static string ExportBuiltinsAndSymbols(LoadedProject project) => project.Compilation.ExportDocumentationIndex();
}
