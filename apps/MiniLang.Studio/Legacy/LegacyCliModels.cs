namespace MiniLang.Studio.Legacy;

public sealed class LegacyDiagnosticDto
{
    public string Id { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Start { get; set; }
    public int Length { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
}

public sealed class LegacyOutlineItemDto
{
    public string Kind { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Start { get; set; }
}

public sealed class LegacyCompletionItemDto
{
    public string Label { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}

public sealed class LegacyAnalysisResultDto
{
    public bool Success { get; set; }
    public string SyntaxTree { get; set; } = string.Empty;
    public string Symbols { get; set; } = string.Empty;
    public string InterpretedTree { get; set; } = string.Empty;
    public List<LegacyDiagnosticDto> Diagnostics { get; set; } = [];
    public List<LegacyOutlineItemDto> Outline { get; set; } = [];
    public List<LegacyCompletionItemDto> Completions { get; set; } = [];
}

public sealed class LegacyRunResultDto
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public List<LegacyDiagnosticDto> Diagnostics { get; set; } = [];
}

public sealed class LegacyExternalRunLaunchDto
{
    public string CommandLine { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
}
