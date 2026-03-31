using MiniLang.Interop;
using MiniLang.LanguageServices;
using MiniLang.Runtime;
using MiniLang.Workspace;

if (args.Length == 0)
{
    PrintUsage();
    return;
}

try
{
    switch (args[0].ToLowerInvariant())
    {
        case "check":
            RequireArgumentCount(args, 2);
            RunCheck(args[1]);
            break;
        case "inspect":
            RequireArgumentCount(args, 2);
            RunInspect(args[1]);
            break;
        case "bindings":
            RequireArgumentCount(args, 3);
            RunBindings(args[1], args[2]);
            break;
        case "run":
            RequireArgumentCount(args, 2);
            RunProject(args[1]);
            break;
        default:
            PrintUsage();
            break;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 1;
}

static void RunCheck(string projectPath)
{
    var project = MiniWorkspaceLoader.LoadProject(projectPath);
    var diagnostics = project.Compilation.Diagnostics;
    Console.WriteLine($"MiniLang check: {project.Manifest.Name}");
    if (diagnostics.Count == 0)
    {
        Console.WriteLine("No diagnostics.");
        return;
    }

    foreach (var diagnostic in diagnostics)
    {
        Console.WriteLine($"{diagnostic.Severity} {diagnostic.Id} {Path.GetFileName(diagnostic.DocumentPath)}:{diagnostic.Span.Start} {diagnostic.Message}");
    }
}

static void RunInspect(string projectPath)
{
    var project = MiniWorkspaceLoader.LoadProject(projectPath);
    foreach (var document in project.Documents)
    {
        var snapshot = DocumentAnalysisService.CreateSnapshot(project.Compilation, document.Path);
        Console.WriteLine($"== {Path.GetFileName(document.Path)} ==");
        Console.WriteLine("-- Syntax --");
        Console.WriteLine(snapshot.SyntaxTree);
        Console.WriteLine("-- Symbols --");
        Console.WriteLine(snapshot.Symbols);
        Console.WriteLine("-- Lowered --");
        Console.WriteLine(snapshot.Lowered);
        Console.WriteLine("-- Diagnostics --");
        foreach (var diagnostic in snapshot.Diagnostics)
        {
            Console.WriteLine($"{diagnostic.Severity} {diagnostic.Id}: {diagnostic.Message}");
        }
    }
}

static void RunBindings(string projectPath, string outputDirectory)
{
    var project = MiniWorkspaceLoader.LoadProject(projectPath);
    Directory.CreateDirectory(outputDirectory);
    File.WriteAllText(Path.Combine(outputDirectory, "minilang-bindings.json"), InteropBindingGenerator.GenerateManifest(project.Compilation));
    File.WriteAllText(Path.Combine(outputDirectory, "MiniLangExports.g.cs"), InteropBindingGenerator.GenerateCSharpStubs(project.Compilation));
    File.WriteAllText(Path.Combine(outputDirectory, "docs-index.json"), DocsExport.ExportBuiltinsAndSymbols(project));
    Console.WriteLine($"Bindings written to {Path.GetFullPath(outputDirectory)}");
}

static void RunProject(string projectPath)
{
    var project = MiniWorkspaceLoader.LoadProject(projectPath);
    var exportName = project.Compilation.Symbols.Values.OfType<MiniLang.Core.FunctionSymbol>().FirstOrDefault(static x => x.IsExported)?.Name
        ?? "main";
    var result = MiniRuntimeHost.InvokeExport(project.Compilation, exportName);
    Console.WriteLine(result);
}

static void RequireArgumentCount(string[] args, int count)
{
    if (args.Length < count)
    {
        PrintUsage();
        Environment.Exit(1);
    }
}

static void PrintUsage()
{
    Console.WriteLine("MiniLang CLI");
    Console.WriteLine("  check <project.miniproj>");
    Console.WriteLine("  inspect <project.miniproj>");
    Console.WriteLine("  bindings <project.miniproj> <output-dir>");
    Console.WriteLine("  run <project.miniproj>");
}
