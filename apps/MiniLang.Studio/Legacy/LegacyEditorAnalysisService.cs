using System.Text;
using System.Text.RegularExpressions;

namespace MiniLang.Studio.Legacy;

public sealed record ClassificationSpan(int Start, int Length, string Classification);

public sealed record HoverInfo(string Title, string Detail, string DocumentationPath);

public sealed class LegacyEditorAnalysis
{
    public required string SyntaxSummary { get; init; }
    public required string SymbolsSummary { get; init; }
    public required string SemanticSummary { get; init; }
    public required IReadOnlyList<ClassificationSpan> Classifications { get; init; }
    public required IReadOnlyList<LegacyOutlineItemDto> Outline { get; init; }
    public required IReadOnlyList<LegacyCompletionItemDto> Completions { get; init; }
    public required IReadOnlyDictionary<string, int> Definitions { get; init; }
    public required IReadOnlyDictionary<string, HoverInfo> HoverMap { get; init; }
}

public static class LegacyEditorAnalysisService
{
    private static readonly HashSet<string> Keywords = ["fn", "struct", "enum", "make", "give", "say", "use", "if", "else", "while", "foreach", "in", "done", "new", "win", "cscall", "public", "private"];
    private static readonly HashSet<string> Types = ["number", "string", "object", "nothing", "array"];
    private static readonly Dictionary<string, HoverInfo> BuiltinDocs = new(StringComparer.Ordinal)
    {
        ["number"] = new("number", "Numeric value type used for arithmetic and comparisons.", "builtin://number"),
        ["string"] = new("string", "Text value type used for strings and interpolation.", "builtin://string"),
        ["object"] = new("object", "General-purpose object return type in legacy MiniLang.", "builtin://object"),
        ["nothing"] = new("nothing", "Void-like return marker for functions with no result.", "builtin://nothing"),
        ["array"] = new("array", "Mutable zero-based dynamic array type.", "builtin://array"),
        ["foreach"] = new("foreach", "Loops over each value in an array expression.", "keyword://foreach"),
        ["in"] = new("in", "Separates the foreach loop variable from the array expression.", "keyword://in"),
        ["Length"] = new("Length", "Returns the current number of items in an array.", "builtin://array/length"),
        ["Push"] = new("Push", "Appends a value to an array and returns the new length.", "builtin://array/push"),
        ["Pop"] = new("Pop", "Removes and returns the last item in an array.", "builtin://array/pop"),
        ["Clear"] = new("Clear", "Removes every item from an array.", "builtin://array/clear"),
        ["Contains"] = new("Contains", "Returns 1 when an array contains the supplied value; otherwise 0.", "builtin://array/contains"),
        ["win"] = new("win", "Approved Windows interop bridge keyword.", "interop://win"),
        ["cscall"] = new("cscall", "Managed interop bridge keyword.", "interop://cscall"),
        ["win.io"] = new("win.io", "Approved file and directory IO bridge namespace.", "interop://win.io")
    };

    public static LegacyEditorAnalysis Analyze(string text)
    {
        var definitions = new Dictionary<string, int>(StringComparer.Ordinal);
        var hover = new Dictionary<string, HoverInfo>(BuiltinDocs, StringComparer.Ordinal);
        var outline = new List<LegacyOutlineItemDto>();
        var completions = new Dictionary<string, LegacyCompletionItemDto>(StringComparer.Ordinal);
        var syntax = new StringBuilder();
        var symbols = new StringBuilder();

        foreach (var item in BuiltinDocs)
        {
            completions[item.Key] = new LegacyCompletionItemDto { Label = item.Key, Detail = "builtin" };
        }

        ParseDeclarations(text, definitions, hover, outline, completions, syntax, symbols);

        return new LegacyEditorAnalysis
        {
            SyntaxSummary = syntax.ToString(),
            SymbolsSummary = symbols.ToString(),
            SemanticSummary = string.Join(Environment.NewLine, outline.Select(static item => $"{item.Kind}: {item.Label} @ {item.Start}")),
            Classifications = Classify(text, definitions.Keys),
            Outline = outline,
            Completions = completions.Values.OrderBy(static x => x.Label, StringComparer.Ordinal).ToArray(),
            Definitions = definitions,
            HoverMap = hover
        };
    }

    public static string? GetWordAt(string text, int offset)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        offset = Math.Clamp(offset, 0, Math.Max(0, text.Length - 1));
        if (!IsIdentifierChar(text[offset]) && offset > 0 && IsIdentifierChar(text[offset - 1]))
        {
            offset--;
        }

        if (!IsIdentifierChar(text[offset]))
        {
            return null;
        }

        var start = offset;
        while (start > 0 && IsIdentifierChar(text[start - 1]))
        {
            start--;
        }

        var end = offset;
        while (end < text.Length && IsIdentifierChar(text[end]))
        {
            end++;
        }

        return text[start..end];
    }

    public static bool IsIdentifierChar(char ch) => char.IsLetterOrDigit(ch) || ch is '_' or '.';

    private static void ParseDeclarations(string text, Dictionary<string, int> definitions, Dictionary<string, HoverInfo> hover, List<LegacyOutlineItemDto> outline, Dictionary<string, LegacyCompletionItemDto> completions, StringBuilder syntax, StringBuilder symbols)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n');
        var offset = 0;
        string? currentEnum = null;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.StartsWith("fn ", StringComparison.Ordinal))
            {
                var match = Regex.Match(line, @"^fn\s+([A-Za-z_][A-Za-z0-9_\.]*)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(");
                if (match.Success)
                {
                    var returnType = match.Groups[1].Value;
                    var name = match.Groups[2].Value;
                    var absoluteOffset = offset + rawLine.IndexOf(name, StringComparison.Ordinal);
                    definitions[name] = absoluteOffset;
                    outline.Add(new LegacyOutlineItemDto { Kind = "function", Label = name, Start = absoluteOffset });
                    hover[name] = new HoverInfo(name, $"Function returning {returnType}.", $"symbol://fn/{name}");
                    completions[name] = new LegacyCompletionItemDto { Label = name, Detail = "function" };
                    syntax.AppendLine($"fn {name} -> {returnType}");
                    symbols.AppendLine($"function {name}");
                }
            }
            else if (line.StartsWith("struct ", StringComparison.Ordinal))
            {
                var match = Regex.Match(line, @"^struct\s+([A-Za-z_][A-Za-z0-9_]*)");
                if (match.Success)
                {
                    var name = match.Groups[1].Value;
                    var absoluteOffset = offset + rawLine.IndexOf(name, StringComparison.Ordinal);
                    definitions[name] = absoluteOffset;
                    outline.Add(new LegacyOutlineItemDto { Kind = "struct", Label = name, Start = absoluteOffset });
                    hover[name] = new HoverInfo(name, "Struct declaration.", $"symbol://struct/{name}");
                    completions[name] = new LegacyCompletionItemDto { Label = name, Detail = "struct" };
                    syntax.AppendLine($"struct {name}");
                    symbols.AppendLine($"struct {name}");
                }
            }
            else if (line.StartsWith("enum ", StringComparison.Ordinal))
            {
                var match = Regex.Match(line, @"^enum\s+([A-Za-z_][A-Za-z0-9_]*)");
                if (match.Success)
                {
                    var name = match.Groups[1].Value;
                    var absoluteOffset = offset + rawLine.IndexOf(name, StringComparison.Ordinal);
                    definitions[name] = absoluteOffset;
                    outline.Add(new LegacyOutlineItemDto { Kind = "enum", Label = name, Start = absoluteOffset });
                    hover[name] = new HoverInfo(name, "Enum declaration.", $"symbol://enum/{name}");
                    completions[name] = new LegacyCompletionItemDto { Label = name, Detail = "enum" };
                    syntax.AppendLine($"enum {name}");
                    symbols.AppendLine($"enum {name}");
                    currentEnum = name;
                }
            }
            else if (currentEnum is not null)
            {
                if (line.StartsWith("}", StringComparison.Ordinal) || string.Equals(line, "done", StringComparison.Ordinal))
                {
                    currentEnum = null;
                }
                else
                {
                    var match = Regex.Match(line, @"^([A-Za-z_][A-Za-z0-9_]*)\s*;");
                    if (match.Success)
                    {
                        var member = match.Groups[1].Value;
                        var fullName = $"{currentEnum}.{member}";
                        var absoluteOffset = offset + rawLine.IndexOf(member, StringComparison.Ordinal);
                        definitions[fullName] = absoluteOffset;
                        hover[fullName] = new HoverInfo(fullName, $"Enum member of {currentEnum}.", $"symbol://enum/{fullName}");
                        completions[fullName] = new LegacyCompletionItemDto { Label = fullName, Detail = "enum member" };
                        symbols.AppendLine($"enum-member {fullName}");
                    }
                }
            }

            offset += rawLine.Length + 1;
        }

        foreach (var item in new[]
                 {
                     new LegacyCompletionItemDto { Label = "win.process", Detail = "interop namespace" },
                     new LegacyCompletionItemDto { Label = "win.time", Detail = "interop namespace" },
                     new LegacyCompletionItemDto { Label = "win.user", Detail = "interop namespace" },
                     new LegacyCompletionItemDto { Label = "win.console", Detail = "interop namespace" },
                     new LegacyCompletionItemDto { Label = "win.io", Detail = "interop namespace" },
                     new LegacyCompletionItemDto { Label = "GetCurrentProcessId", Detail = "interop function" },
                     new LegacyCompletionItemDto { Label = "GetTickCount", Detail = "interop function" },
                     new LegacyCompletionItemDto { Label = "Sleep", Detail = "interop function" },
                     new LegacyCompletionItemDto { Label = "MessageBox", Detail = "interop function" },
                     new LegacyCompletionItemDto { Label = "SetTitle", Detail = "interop function" },
                     new LegacyCompletionItemDto { Label = "GetTitle", Detail = "interop function" },
                     new LegacyCompletionItemDto { Label = "FileExists", Detail = "interop function" },
                     new LegacyCompletionItemDto { Label = "ReadText", Detail = "interop function" },
                     new LegacyCompletionItemDto { Label = "WriteText", Detail = "interop function" },
                     new LegacyCompletionItemDto { Label = "EnsureDirectory", Detail = "interop function" },
                     new LegacyCompletionItemDto { Label = "array", Detail = "type" },
                     new LegacyCompletionItemDto { Label = "foreach", Detail = "keyword" },
                     new LegacyCompletionItemDto { Label = "in", Detail = "keyword" },
                     new LegacyCompletionItemDto { Label = "Length", Detail = "array builtin" },
                     new LegacyCompletionItemDto { Label = "Push", Detail = "array builtin" },
                     new LegacyCompletionItemDto { Label = "Pop", Detail = "array builtin" },
                     new LegacyCompletionItemDto { Label = "Clear", Detail = "array builtin" },
                     new LegacyCompletionItemDto { Label = "Contains", Detail = "array builtin" }
                 })
        {
            completions[item.Label] = item;
        }
    }

    private static IReadOnlyList<ClassificationSpan> Classify(string text, IEnumerable<string> knownDefinitions)
    {
        var spans = new List<ClassificationSpan>();
        var known = new HashSet<string>(knownDefinitions, StringComparer.Ordinal);

        for (var index = 0; index < text.Length;)
        {
            if (text[index] == '"')
            {
                var end = index + 1;
                while (end < text.Length && text[end] != '"')
                {
                    end++;
                }

                end = Math.Min(text.Length, end + 1);
                spans.Add(new ClassificationSpan(index, end - index, "string"));
                index = end;
                continue;
            }

            if (char.IsDigit(text[index]))
            {
                var start = index;
                while (index < text.Length && (char.IsDigit(text[index]) || text[index] == '.'))
                {
                    index++;
                }

                spans.Add(new ClassificationSpan(start, index - start, "number"));
                continue;
            }

            if (IsIdentifierChar(text[index]))
            {
                var start = index;
                while (index < text.Length && IsIdentifierChar(text[index]))
                {
                    index++;
                }

                var word = text[start..index];
                var classification = Keywords.Contains(word)
                    ? "keyword"
                    : Types.Contains(word)
                        ? "type"
                        : BuiltinDocs.ContainsKey(word)
                            ? "builtin"
                        : known.Contains(word) && word.Contains('.')
                            ? "builtin"
                            : "symbol";

                spans.Add(new ClassificationSpan(start, index - start, classification));
                continue;
            }

            index++;
        }

        return spans;
    }
}
