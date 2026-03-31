using System.Text.Json;
using MiniLang.Hosting;

internal static class MiniLangRuntime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public static int Main(string[] args)
    {
        args = ["run", @"C:\Users\Demon\source\repos\MiniLang\MiniLangProjects\Workspace\App\StartupApp.mini.c"];
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: MiniLangCLI <check|check-json|run|run-json|inspect-json> <script-file>");
            return 1;
        }

        var command = args[0];
        var filePath = Path.GetFullPath(args[1]);
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Script file not found: {filePath}");
            return 1;
        }

        var sourceCode = File.ReadAllText(filePath);

        return command switch
        {
            "check" => WriteCheck(filePath, sourceCode, json: false),
            "check-json" => WriteCheck(filePath, sourceCode, json: true),
            "run" => WriteRun(filePath, sourceCode, json: false),
            "run-json" => WriteRun(filePath, sourceCode, json: true),
            "inspect-json" => WriteInspect(filePath, sourceCode),
            _ => UnknownCommand(command)
        };
    }

    private static int WriteCheck(string filePath, string sourceCode, bool json)
    {
        var result = LegacyMiniLangHost.AnalyzeSource(sourceCode, filePath);
        if (json)
        {
            Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
        }
        else if (result.Success)
        {
            Console.WriteLine("Check succeeded.");
        }
        else
        {
            foreach (var diagnostic in result.Diagnostics)
            {
                Console.WriteLine($"{diagnostic.Severity} {diagnostic.Id} ({diagnostic.Line},{diagnostic.Column}): {diagnostic.Message}");
            }
        }

        return result.Success ? 0 : 1;
    }

    private static int WriteRun(string filePath, string sourceCode, bool json)
    {
        var result = LegacyMiniLangHost.RunSource(sourceCode, filePath);
        if (json)
        {
            Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
        }
        else if (result.Success)
        {
            Console.Write(result.Output);
        }
        else
        {
            foreach (var diagnostic in result.Diagnostics)
            {
                Console.WriteLine($"{diagnostic.Severity} {diagnostic.Id} ({diagnostic.Line},{diagnostic.Column}): {diagnostic.Message}");
            }
        }

        return result.Success ? 0 : 1;
    }

    private static int WriteInspect(string filePath, string sourceCode)
    {
        var result = LegacyMiniLangHost.AnalyzeSource(sourceCode, filePath);
        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
        return result.Success ? 0 : 1;
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command '{command}'.");
        return 1;
    }
}
