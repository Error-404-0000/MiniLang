namespace MiniLang.Core;

public readonly record struct TextSpan(int Start, int Length)
{
    public int End => Start + Length;

    public static TextSpan FromBounds(int start, int end) => new(start, Math.Max(0, end - start));

    public bool Contains(int position) => position >= Start && position <= End;
}

public sealed record SourceDocument(string Path, string Text)
{
    public string FileName => System.IO.Path.GetFileName(Path);
}

public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error
}

public sealed record Diagnostic(
    string Id,
    DiagnosticSeverity Severity,
    string Message,
    string DocumentPath,
    TextSpan Span,
    string? Suggestion = null);
