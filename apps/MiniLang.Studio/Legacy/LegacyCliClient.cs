using System.Diagnostics;
using System.Text.Json;

namespace MiniLang.Studio.Legacy;

public sealed class LegacyCliClient
{
    private readonly string _repoRoot;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LegacyCliClient(string repoRoot)
    {
        _repoRoot = repoRoot;
    }

    public async Task<LegacyAnalysisResultDto> InspectAsync(string sourcePath, string sourceText)
        => await InspectAsync(sourcePath, sourceText, "Debug");

    public async Task<LegacyAnalysisResultDto> InspectAsync(string sourcePath, string sourceText, string configuration)
    {
        await EnsureCliBuiltAsync(configuration);
        var previewPath = await WritePreviewFileAsync(sourcePath, sourceText);
        try
        {
            var stdout = await RunProcessAsync(GetCliExecutablePath(configuration), $"inspect-json \"{previewPath}\"");
            return JsonSerializer.Deserialize<LegacyAnalysisResultDto>(stdout, _jsonOptions) ?? new LegacyAnalysisResultDto
            {
                Success = false,
                Diagnostics = [new LegacyDiagnosticDto { Id = "STUDIO0001", Severity = "Error", Message = "Legacy CLI returned invalid analysis JSON." }]
            };
        }
        finally
        {
            TryDeletePreview(previewPath, sourcePath);
        }
    }

    public async Task<LegacyRunResultDto> RunAsync(string sourcePath, string sourceText)
        => await RunAsync(sourcePath, sourceText, "Debug");

    public async Task<LegacyRunResultDto> RunAsync(string sourcePath, string sourceText, string configuration)
    {
        await EnsureCliBuiltAsync(configuration);
        var previewPath = await WritePreviewFileAsync(sourcePath, sourceText);
        try
        {
            var stdout = await RunProcessAsync(GetCliExecutablePath(configuration), $"run-json \"{previewPath}\"");
            return JsonSerializer.Deserialize<LegacyRunResultDto>(stdout, _jsonOptions) ?? new LegacyRunResultDto
            {
                Success = false,
                Diagnostics = [new LegacyDiagnosticDto { Id = "STUDIO0002", Severity = "Error", Message = "Legacy CLI returned invalid run JSON." }]
            };
        }
        finally
        {
            TryDeletePreview(previewPath, sourcePath);
        }
    }

    public Task BuildCliAsync(string configuration = "Debug")
    {
        configuration = NormalizeConfiguration(configuration);
        return RunProcessAsync("dotnet", $"build \"MiniLangTest\\MiniLangCLI.csproj\" -c {configuration}", _repoRoot);
    }

    public async Task<LegacyExternalRunLaunchDto> LaunchExternalRunAsync(string sourcePath, string configuration = "Release")
    {
        configuration = NormalizeConfiguration(configuration);
        await BuildCliAsync(configuration);
        var executablePath = GetCliExecutablePath(configuration);
        var commandLine = $"\"{executablePath}\" run \"{sourcePath}\"";
        var shellCommand = $"\"{commandLine}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/k {shellCommand}",
            WorkingDirectory = _repoRoot,
            UseShellExecute = true
        };

        Process.Start(startInfo);

        return new LegacyExternalRunLaunchDto
        {
            CommandLine = $"cmd.exe /k {shellCommand}",
            WorkingDirectory = _repoRoot
        };
    }

    private async Task EnsureCliBuiltAsync(string configuration)
    {
        configuration = NormalizeConfiguration(configuration);
        if (File.Exists(GetCliExecutablePath(configuration)))
        {
            return;
        }

        await BuildCliAsync(configuration);
    }

    private string GetCliExecutablePath(string configuration) =>
        Path.Combine(_repoRoot, "MiniLangTest", "bin", NormalizeConfiguration(configuration), "net10.0", "MiniLangCLI.exe");

    private static async Task<string> RunProcessAsync(string fileName, string arguments, string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(fileName)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(stderr) ? stdout : stderr);
        }

        return stdout;
    }

    private static string NormalizeConfiguration(string configuration) =>
        string.Equals(configuration, "Release", StringComparison.OrdinalIgnoreCase)
            ? "Release"
            : "Debug";

    private static async Task<string> WritePreviewFileAsync(string sourcePath, string sourceText)
    {
        var directory = Path.GetDirectoryName(sourcePath) ?? Path.GetTempPath();
        var extension = Path.GetExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".mini";
        }

        var previewPath = Path.Combine(directory, $".studio-preview{extension}");
        await File.WriteAllTextAsync(previewPath, sourceText);
        return previewPath;
    }

    private static void TryDeletePreview(string previewPath, string sourcePath)
    {
        if (string.Equals(previewPath, sourcePath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            if (File.Exists(previewPath))
            {
                File.Delete(previewPath);
            }
        }
        catch
        {
        }
    }
}
