namespace MiniLang.Hosting;

public sealed record LegacyDiagnostic(
    string Id,
    string Severity,
    string Message,
    int Start,
    int Length,
    int Line,
    int Column);

public sealed record LegacyOutlineItem(string Kind, string Label, int Start);

public sealed record LegacyCompletionItem(string Label, string Detail);

public sealed record LegacyAnalysisResult(
    bool Success,
    string SyntaxTree,
    string Symbols,
    string InterpretedTree,
    IReadOnlyList<LegacyDiagnostic> Diagnostics,
    IReadOnlyList<LegacyOutlineItem> Outline,
    IReadOnlyList<LegacyCompletionItem> Completions);

public sealed record LegacyRunResult(
    bool Success,
    string Output,
    IReadOnlyList<LegacyDiagnostic> Diagnostics);
