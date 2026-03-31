using System.Text;
using MiniLang.Core;

namespace MiniLang.LanguageServices;

public sealed record CompletionItem(string Label, string Kind, string Detail);
public sealed record HoverInfo(string Title, string Detail, string? DocumentationPath);
public sealed record DefinitionLocation(string DocumentPath, TextSpan Span, string? DocumentationPath);
public sealed record ClassificationSpan(TextSpan Span, string Classification);
public sealed record OutlineItem(string Label, string Kind, TextSpan Span);
public sealed record InspectorSnapshot(string SyntaxTree, string Symbols, string Lowered, IReadOnlyList<Diagnostic> Diagnostics);

public static class ClassificationService
{
    public static IReadOnlyList<ClassificationSpan> Classify(SemanticModel model)
    {
        var spans = new List<ClassificationSpan>();
        foreach (var token in model.SyntaxTree.Tokens)
        {
            var classification = token.Kind switch
            {
                SyntaxKind.ModuleKeyword or SyntaxKind.UseKeyword or SyntaxKind.FnKeyword or SyntaxKind.StructKeyword or SyntaxKind.EnumKeyword or SyntaxKind.TraitKeyword or SyntaxKind.ImplKeyword or SyntaxKind.ExternKeyword or SyntaxKind.TypeKeyword => "keyword",
                SyntaxKind.StringToken => "string",
                SyntaxKind.NumberToken => "number",
                SyntaxKind.IdentifierToken => "identifier",
                _ => string.Empty
            };
            if (!string.IsNullOrEmpty(classification))
            {
                spans.Add(new ClassificationSpan(token.Span, classification));
            }
        }
        foreach (var reference in model.References.Where(static x => x.Symbol is not null))
        {
            var semanticKind = reference.Symbol!.Kind switch
            {
                SymbolKind.BuiltinType => "builtin",
                SymbolKind.Function or SymbolKind.ExternFunction => "function",
                SymbolKind.Struct or SymbolKind.Enum or SymbolKind.Trait or SymbolKind.ExternType => "type",
                SymbolKind.Parameter => "parameter",
                _ => "symbol"
            };
            spans.Add(new ClassificationSpan(reference.Span, semanticKind));
        }
        return spans;
    }
}

public static class CompletionService
{
    private static readonly CompletionItem[] Keywords =
    [
        new("module", "keyword", "Declare the module name for the current file."),
        new("use", "keyword", "Import another module into the current file."),
        new("fn", "keyword", "Declare a function."),
        new("struct", "keyword", "Declare a struct type."),
        new("enum", "keyword", "Declare an enum type."),
        new("trait", "keyword", "Declare an interface-like trait."),
        new("impl", "keyword", "Implement inherent members or a trait."),
        new("extern", "keyword", "Declare an interop-facing symbol.")
    ];

    public static IReadOnlyList<CompletionItem> GetCompletions(Compilation compilation, string documentPath, int position)
    {
        var items = new List<CompletionItem>(Keywords);
        foreach (var symbol in compilation.Symbols.Values.OrderBy(static x => x.Name))
        {
            items.Add(new CompletionItem(symbol.Name, symbol.Kind.ToString().ToLowerInvariant(), symbol.Documentation));
        }
        foreach (var attribute in compilation.AttributeDefinitions.Values.OrderBy(static x => x.Name))
        {
            items.Add(new CompletionItem(attribute.Name, "attribute", attribute.Documentation));
        }
        return items;
    }
}

public static class HoverService
{
    public static HoverInfo? GetHover(Compilation compilation, string documentPath, int position)
    {
        var model = compilation.GetSemanticModel(documentPath);
        var declared = model?.GetDeclaredSymbolAt(position);
        if (declared is not null)
        {
            return new HoverInfo(declared.Name, declared.Documentation, declared.DocsPath);
        }
        var reference = model?.GetReferenceAt(position);
        if (reference?.Symbol is not null)
        {
            return new HoverInfo(reference.Symbol.Name, reference.Symbol.Documentation, reference.Symbol.DocsPath);
        }
        return null;
    }
}

public static class DefinitionService
{
    public static DefinitionLocation? GetDefinition(Compilation compilation, string documentPath, int position)
    {
        var model = compilation.GetSemanticModel(documentPath);
        var reference = model?.GetReferenceAt(position);
        if (reference?.Symbol is not null)
        {
            return new DefinitionLocation(reference.Symbol.DocumentPath, reference.Symbol.Span, reference.Symbol.DocsPath);
        }
        var declared = model?.GetDeclaredSymbolAt(position);
        return declared is null ? null : new DefinitionLocation(declared.DocumentPath, declared.Span, declared.DocsPath);
    }
}

public static class NavigationService
{
    public static IReadOnlyList<OutlineItem> GetOutline(SemanticModel model) =>
        model.DeclaredSymbols
            .Select(static symbol => new OutlineItem(symbol.Name, symbol.Kind.ToString(), symbol.Span))
            .OrderBy(static x => x.Span.Start)
            .ToArray();
}

public static class DocumentAnalysisService
{
    public static InspectorSnapshot CreateSnapshot(Compilation compilation, string documentPath)
    {
        var model = compilation.GetSemanticModel(documentPath) ?? throw new InvalidOperationException($"Document '{documentPath}' is not part of the compilation.");
        var symbols = new StringBuilder();
        foreach (var symbol in model.DeclaredSymbols)
        {
            symbols.Append(symbol.Kind);
            symbols.Append(' ');
            symbols.Append(symbol.Name);
            symbols.Append(" -> ");
            symbols.AppendLine(symbol.Documentation);
        }
        return new InspectorSnapshot(
            SyntaxPrinter.PrettyPrint(model.SyntaxTree.Root),
            symbols.ToString(),
            string.Join(Environment.NewLine, model.LoweredProgram.Instructions),
            model.Diagnostics);
    }
}
