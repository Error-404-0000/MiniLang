using System.Collections.ObjectModel;
using System.Text.Json;

namespace MiniLang.Core;

public enum SymbolKind
{
    Module,
    Import,
    Function,
    Struct,
    Enum,
    Trait,
    Impl,
    Field,
    Parameter,
    EnumMember,
    TypeParameter,
    BuiltinType,
    ExternFunction,
    ExternType
}

public enum AttributeTarget
{
    Module,
    Use,
    Function,
    Struct,
    Enum,
    Trait,
    Impl,
    Field,
    Parameter,
    ExternFunction,
    ExternType,
    Any
}

public enum TypeKind
{
    Builtin,
    Struct,
    Enum,
    Trait,
    Extern,
    GenericParameter,
    Pointer
}

public sealed record AttributeDefinition(string Name, IReadOnlySet<AttributeTarget> AllowedTargets, string Documentation, string DocsPath, IReadOnlyList<string> RequiredArguments);
public sealed record AttributeData(AttributeDefinition Definition, IReadOnlyDictionary<string, string> Arguments, TextSpan Span);
public sealed record BoundReference(string DocumentPath, TextSpan Span, string DisplayText, Symbol? Symbol, string? DocumentationPath = null);
public sealed record ExternSignature(string Name, string Library, string EntryPoint, string CallingConvention, string Charset, IReadOnlyList<string> Parameters, string ReturnType);
public sealed record MarshallingRule(string TargetName, string Rule);
public sealed record LoweredProgram(IReadOnlyList<string> Instructions);

public class Symbol
{
    public Symbol(string name, SymbolKind kind, string documentPath, TextSpan span, string documentation, string? docsPath = null)
    {
        Name = name;
        Kind = kind;
        DocumentPath = documentPath;
        Span = span;
        Documentation = documentation;
        DocsPath = docsPath;
        Attributes = [];
    }

    public string Name { get; }
    public SymbolKind Kind { get; }
    public string DocumentPath { get; }
    public TextSpan Span { get; }
    public string Documentation { get; }
    public string? DocsPath { get; }
    public List<AttributeData> Attributes { get; }
}

public sealed class TypeSymbol : Symbol
{
    public TypeSymbol(string name, SymbolKind kind, string documentPath, TextSpan span, string documentation, TypeKind typeKind, string? docsPath = null)
        : base(name, kind, documentPath, span, documentation, docsPath)
    {
        TypeKind = typeKind;
        Members = [];
    }

    public TypeKind TypeKind { get; }
    public List<Symbol> Members { get; }
}

public sealed class ParameterSymbol : Symbol
{
    public ParameterSymbol(string name, string documentPath, TextSpan span, TypeSymbol type, string documentation)
        : base(name, SymbolKind.Parameter, documentPath, span, documentation)
    {
        Type = type;
    }

    public TypeSymbol Type { get; }
}

public sealed class FunctionSymbol : Symbol
{
    public FunctionSymbol(string name, SymbolKind kind, string documentPath, TextSpan span, string documentation, TypeSymbol returnType, IReadOnlyList<ParameterSymbol> parameters, IReadOnlyList<string> genericParameters, bool isExported, bool isExtern, string? docsPath = null)
        : base(name, kind, documentPath, span, documentation, docsPath)
    {
        ReturnType = returnType;
        Parameters = parameters;
        GenericParameters = genericParameters;
        IsExported = isExported;
        IsExtern = isExtern;
    }

    public TypeSymbol ReturnType { get; }
    public IReadOnlyList<ParameterSymbol> Parameters { get; }
    public IReadOnlyList<string> GenericParameters { get; }
    public bool IsExported { get; }
    public bool IsExtern { get; }
}

public sealed class SemanticModel
{
    public SemanticModel(SyntaxTree syntaxTree, IReadOnlyList<Diagnostic> diagnostics, IReadOnlyList<Symbol> declaredSymbols, IReadOnlyList<BoundReference> references, LoweredProgram loweredProgram)
    {
        SyntaxTree = syntaxTree;
        Diagnostics = diagnostics;
        DeclaredSymbols = declaredSymbols;
        References = references;
        LoweredProgram = loweredProgram;
    }

    public SyntaxTree SyntaxTree { get; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; }
    public IReadOnlyList<Symbol> DeclaredSymbols { get; }
    public IReadOnlyList<BoundReference> References { get; }
    public LoweredProgram LoweredProgram { get; }

    public Symbol? GetDeclaredSymbolAt(int position) => DeclaredSymbols.FirstOrDefault(x => x.Span.Contains(position));
    public BoundReference? GetReferenceAt(int position) => References.FirstOrDefault(x => x.Span.Contains(position));
}

public sealed class Compilation
{
    internal Compilation(
        IReadOnlyList<SourceDocument> documents,
        IReadOnlyList<SyntaxTree> syntaxTrees,
        IReadOnlyDictionary<string, SemanticModel> semanticModels,
        IReadOnlyDictionary<string, Symbol> symbols,
        IReadOnlyDictionary<string, AttributeDefinition> attributeDefinitions,
        IReadOnlyList<ExternSignature> externSignatures,
        IReadOnlyList<MarshallingRule> marshallingRules,
        LoweredProgram loweredProgram)
    {
        Documents = documents;
        SyntaxTrees = syntaxTrees;
        SemanticModels = semanticModels;
        Symbols = symbols;
        AttributeDefinitions = attributeDefinitions;
        ExternSignatures = externSignatures;
        MarshallingRules = marshallingRules;
        LoweredProgram = loweredProgram;
    }

    public IReadOnlyList<SourceDocument> Documents { get; }
    public IReadOnlyList<SyntaxTree> SyntaxTrees { get; }
    public IReadOnlyDictionary<string, SemanticModel> SemanticModels { get; }
    public IReadOnlyDictionary<string, Symbol> Symbols { get; }
    public IReadOnlyDictionary<string, AttributeDefinition> AttributeDefinitions { get; }
    public IReadOnlyList<ExternSignature> ExternSignatures { get; }
    public IReadOnlyList<MarshallingRule> MarshallingRules { get; }
    public LoweredProgram LoweredProgram { get; }
    public IReadOnlyList<Diagnostic> Diagnostics => SemanticModels.Values.SelectMany(static x => x.Diagnostics).OrderBy(static x => x.DocumentPath).ThenBy(static x => x.Span.Start).ToArray();

    public static Compilation Create(IEnumerable<SourceDocument> documents)
    {
        var trees = documents.Select(SyntaxTree.Parse).ToArray();
        return new Binder(trees).Bind();
    }

    public SemanticModel? GetSemanticModel(string documentPath) => SemanticModels.TryGetValue(documentPath, out var model) ? model : null;

    public string ExportDocumentationIndex()
    {
        var payload = new
        {
            builtins = Symbols.Values.Where(static x => x.Kind == SymbolKind.BuiltinType).Select(static x => new { x.Name, x.Documentation, x.DocsPath }),
            symbols = Symbols.Values.Where(static x => x.Kind != SymbolKind.BuiltinType).Select(static x => new { x.Name, kind = x.Kind.ToString(), x.Documentation, x.DocsPath, x.DocumentPath })
        };
        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }
}

internal sealed class Binder
{
    private static readonly IReadOnlyDictionary<string, AttributeDefinition> BuiltinAttributes = new ReadOnlyDictionary<string, AttributeDefinition>(
        new Dictionary<string, AttributeDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["export"] = new("export", new HashSet<AttributeTarget> { AttributeTarget.Function }, "Marks a function as callable from generated C# bindings.", "/docs/interop", []),
            ["docs"] = new("docs", new HashSet<AttributeTarget> { AttributeTarget.Any }, "Attaches docs metadata surfaced by tooling and the docs site.", "/docs/attributes", ["summary"]),
            ["dll_import"] = new("dll_import", new HashSet<AttributeTarget> { AttributeTarget.ExternFunction }, "Declares the native user-mode library and entry point for an extern function.", "/docs/interop", ["library"]),
            ["calling_convention"] = new("calling_convention", new HashSet<AttributeTarget> { AttributeTarget.ExternFunction }, "Controls the unmanaged calling convention for extern functions.", "/docs/interop", ["value"]),
            ["layout"] = new("layout", new HashSet<AttributeTarget> { AttributeTarget.Struct }, "Controls struct layout for interop.", "/docs/attributes", ["kind"]),
            ["marshal_as"] = new("marshal_as", new HashSet<AttributeTarget> { AttributeTarget.Parameter, AttributeTarget.Field }, "Declares marshaling intent for fields or parameters.", "/docs/interop", ["value"]),
            ["intrinsic"] = new("intrinsic", new HashSet<AttributeTarget> { AttributeTarget.Function, AttributeTarget.Struct, AttributeTarget.Enum, AttributeTarget.Trait }, "Marks a language-known declaration.", "/docs/reference/language-reference", [])
        });

    private static readonly IReadOnlyDictionary<string, TypeSymbol> BuiltinTypes = new ReadOnlyDictionary<string, TypeSymbol>(
        new Dictionary<string, TypeSymbol>(StringComparer.OrdinalIgnoreCase)
        {
            ["int"] = new("int", SymbolKind.BuiltinType, "<builtin>", new TextSpan(0, 0), "Signed 32-bit integer.", TypeKind.Builtin, "/docs/reference/type-system#int"),
            ["string"] = new("string", SymbolKind.BuiltinType, "<builtin>", new TextSpan(0, 0), "UTF-16 string value.", TypeKind.Builtin, "/docs/reference/type-system#string"),
            ["bool"] = new("bool", SymbolKind.BuiltinType, "<builtin>", new TextSpan(0, 0), "Boolean true/false value.", TypeKind.Builtin, "/docs/reference/type-system#bool"),
            ["void"] = new("void", SymbolKind.BuiltinType, "<builtin>", new TextSpan(0, 0), "No value returned.", TypeKind.Builtin, "/docs/reference/type-system#void"),
            ["handle"] = new("handle", SymbolKind.BuiltinType, "<builtin>", new TextSpan(0, 0), "Opaque handle for user-mode platform APIs.", TypeKind.Builtin, "/docs/interop#handles"),
            ["usize"] = new("usize", SymbolKind.BuiltinType, "<builtin>", new TextSpan(0, 0), "Pointer-sized unsigned integer.", TypeKind.Builtin, "/docs/reference/type-system#usize")
        });

    private readonly IReadOnlyList<SyntaxTree> _trees;
    private readonly Dictionary<string, Symbol> _symbols = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, SemanticModel> _semanticModels = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ExternSignature> _externSignatures = [];
    private readonly List<MarshallingRule> _marshallingRules = [];
    private readonly List<string> _loweredInstructions = [];

    public Binder(IReadOnlyList<SyntaxTree> trees)
    {
        _trees = trees;
        foreach (var builtin in BuiltinTypes)
        {
            _symbols[builtin.Key] = builtin.Value;
        }
    }

    public Compilation Bind()
    {
        foreach (var tree in _trees)
        {
            DeclareTopLevelSymbols(tree);
        }
        foreach (var tree in _trees)
        {
            BindTree(tree);
        }
        return new Compilation(
            _trees.Select(static x => x.Document).ToArray(),
            _trees,
            _semanticModels,
            _symbols,
            BuiltinAttributes,
            _externSignatures,
            _marshallingRules,
            new LoweredProgram(_loweredInstructions.ToArray()));
    }

    private void DeclareTopLevelSymbols(SyntaxTree tree)
    {
        foreach (var member in tree.Root.Members)
        {
            switch (member)
            {
                case FunctionDeclarationSyntax function:
                    var genericParameters = function.GenericParameters.Select(static x => x.Identifier.Text).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var returnType = ResolveType(function.ReturnType, tree.Document.Path, function.Identifier.Span, null, genericParameters) ?? BuiltinTypes["void"];
                    var parameters = function.Parameters.Select(parameter =>
                        new ParameterSymbol(
                            parameter.Identifier.Text,
                            tree.Document.Path,
                            parameter.Identifier.Span,
                            ResolveType(parameter.Type, tree.Document.Path, parameter.Type.Span, null, genericParameters) ?? BuiltinTypes["void"],
                            $"Parameter {parameter.Identifier.Text}.")).ToArray();
                    AddSymbol(new FunctionSymbol(
                        function.Identifier.Text,
                        function.IsExtern ? SymbolKind.ExternFunction : SymbolKind.Function,
                        tree.Document.Path,
                        function.Identifier.Span,
                        $"Function {function.Identifier.Text}({string.Join(", ", parameters.Select(static x => $"{x.Name}: {x.Type.Name}"))}) -> {returnType.Name}.",
                        returnType,
                        parameters,
                        function.GenericParameters.Select(static x => x.Identifier.Text).ToArray(),
                        HasAttribute(function.Attributes, "export"),
                        function.IsExtern,
                        function.IsExtern ? "/docs/interop" : "/docs/reference/functions"));
                    break;
                case StructDeclarationSyntax @struct:
                    AddSymbol(new TypeSymbol(@struct.Identifier.Text, SymbolKind.Struct, tree.Document.Path, @struct.Identifier.Span, $"Struct {@struct.Identifier.Text}.", TypeKind.Struct, "/docs/reference/type-system"));
                    break;
                case EnumDeclarationSyntax @enum:
                    AddSymbol(new TypeSymbol(@enum.Identifier.Text, SymbolKind.Enum, tree.Document.Path, @enum.Identifier.Span, $"Enum {@enum.Identifier.Text}.", TypeKind.Enum, "/docs/reference/type-system"));
                    break;
                case TraitDeclarationSyntax trait:
                    AddSymbol(new TypeSymbol(trait.Identifier.Text, SymbolKind.Trait, tree.Document.Path, trait.Identifier.Span, $"Trait {trait.Identifier.Text}.", TypeKind.Trait, "/docs/reference/type-system"));
                    break;
                case ExternTypeDeclarationSyntax externType:
                    AddSymbol(new TypeSymbol(externType.Identifier.Text, SymbolKind.ExternType, tree.Document.Path, externType.Identifier.Span, $"Extern type {externType.Identifier.Text}.", TypeKind.Extern, "/docs/interop"));
                    break;
            }
        }
    }

    private void BindTree(SyntaxTree tree)
    {
        var diagnostics = tree.Diagnostics.ToList();
        var declaredSymbols = new List<Symbol>();
        var references = new List<BoundReference>();

        foreach (var member in tree.Root.Members)
        {
            switch (member)
            {
                case UseDeclarationSyntax use:
                    ValidateAttributes(use.Attributes, AttributeTarget.Use, tree.Document.Path, diagnostics, null);
                    foreach (var part in use.PathParts)
                    {
                        references.Add(new BoundReference(tree.Document.Path, part.Span, part.Text, null, "/docs/reference/modules"));
                    }
                    _loweredInstructions.Add($"import {use.QualifiedPath}");
                    break;
                case FunctionDeclarationSyntax function when _symbols.TryGetValue(function.Identifier.Text, out var fnSymbol) && fnSymbol is FunctionSymbol fn:
                    declaredSymbols.Add(fn);
                    BindAttributes(function.Attributes, fn, function.IsExtern ? AttributeTarget.ExternFunction : AttributeTarget.Function, tree.Document.Path, diagnostics);
                    var functionGenerics = function.GenericParameters.Select(static x => x.Identifier.Text).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    if (function.ReturnType is not null)
                    {
                        var returnType = ResolveType(function.ReturnType, tree.Document.Path, function.ReturnType.Span, diagnostics, functionGenerics);
                        if (returnType is not null)
                        {
                            references.Add(new BoundReference(tree.Document.Path, function.ReturnType.Span, returnType.Name, returnType, returnType.DocsPath));
                        }
                    }
                    foreach (var parameter in function.Parameters)
                    {
                        ValidateAttributes(parameter.Attributes, AttributeTarget.Parameter, tree.Document.Path, diagnostics, null);
                        var parameterType = ResolveType(parameter.Type, tree.Document.Path, parameter.Type.Span, diagnostics, functionGenerics);
                        if (parameterType is not null)
                        {
                            references.Add(new BoundReference(tree.Document.Path, parameter.Type.Span, parameterType.Name, parameterType, parameterType.DocsPath));
                            if (parameter.Type is PointerTypeSyntax && !function.IsExtern)
                            {
                                diagnostics.Add(new Diagnostic("ML1005", DiagnosticSeverity.Error, "Pointer types are restricted to interop-facing declarations in this build.", tree.Document.Path, parameter.Type.Span));
                            }
                        }
                    }
                    if (function.IsExtern)
                    {
                        ValidateExternFunction(tree.Document.Path, function, fn, diagnostics);
                    }
                    else if (function.Body is not null)
                    {
                        BindFunctionBody(tree.Document.Path, function, diagnostics, references);
                    }
                    _loweredInstructions.Add(function.IsExtern ? $"extern {fn.Name} -> {fn.ReturnType.Name}" : $"fn {fn.Name} -> {fn.ReturnType.Name}");
                    break;
                case StructDeclarationSyntax @struct when _symbols.TryGetValue(@struct.Identifier.Text, out var structSymbol) && structSymbol is TypeSymbol structType:
                    declaredSymbols.Add(structType);
                    BindAttributes(@struct.Attributes, structType, AttributeTarget.Struct, tree.Document.Path, diagnostics);
                    var structGenerics = @struct.GenericParameters.Select(static x => x.Identifier.Text).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    foreach (var field in @struct.Fields)
                    {
                        ValidateAttributes(field.Attributes, AttributeTarget.Field, tree.Document.Path, diagnostics, null);
                        var fieldType = ResolveType(field.Type, tree.Document.Path, field.Type.Span, diagnostics, structGenerics);
                        if (fieldType is not null)
                        {
                            references.Add(new BoundReference(tree.Document.Path, field.Type.Span, fieldType.Name, fieldType, fieldType.DocsPath));
                        }
                    }
                    _loweredInstructions.Add($"type struct {structType.Name}");
                    break;
                case EnumDeclarationSyntax @enum when _symbols.TryGetValue(@enum.Identifier.Text, out var enumSymbol) && enumSymbol is TypeSymbol enumType:
                    declaredSymbols.Add(enumType);
                    BindAttributes(@enum.Attributes, enumType, AttributeTarget.Enum, tree.Document.Path, diagnostics);
                    _loweredInstructions.Add($"type enum {enumType.Name}");
                    break;
                case TraitDeclarationSyntax trait when _symbols.TryGetValue(trait.Identifier.Text, out var traitSymbol) && traitSymbol is TypeSymbol traitType:
                    declaredSymbols.Add(traitType);
                    BindAttributes(trait.Attributes, traitType, AttributeTarget.Trait, tree.Document.Path, diagnostics);
                    _loweredInstructions.Add($"type trait {traitType.Name}");
                    break;
                case ImplDeclarationSyntax impl:
                    ValidateAttributes(impl.Attributes, AttributeTarget.Impl, tree.Document.Path, diagnostics, null);
                    var targetType = ResolveType(impl.TargetType, tree.Document.Path, impl.TargetType.Span, diagnostics);
                    if (targetType is not null)
                    {
                        references.Add(new BoundReference(tree.Document.Path, impl.TargetType.Span, targetType.Name, targetType, targetType.DocsPath));
                    }
                    if (impl.TraitType is not null)
                    {
                        var traitType = ResolveType(impl.TraitType, tree.Document.Path, impl.TraitType.Span, diagnostics);
                        if (traitType is not null)
                        {
                            references.Add(new BoundReference(tree.Document.Path, impl.TraitType.Span, traitType.Name, traitType, traitType.DocsPath));
                        }
                    }
                    _loweredInstructions.Add("impl block");
                    break;
                case ExternTypeDeclarationSyntax externType when _symbols.TryGetValue(externType.Identifier.Text, out var externSymbol):
                    declaredSymbols.Add(externSymbol);
                    BindAttributes(externType.Attributes, externSymbol, AttributeTarget.ExternType, tree.Document.Path, diagnostics);
                    _loweredInstructions.Add($"extern type {externType.Identifier.Text}");
                    break;
            }
        }

        _semanticModels[tree.Document.Path] = new SemanticModel(tree, diagnostics, declaredSymbols, references, new LoweredProgram(_loweredInstructions.ToArray()));
    }

    private void BindFunctionBody(string documentPath, FunctionDeclarationSyntax function, List<Diagnostic> diagnostics, List<BoundReference> references)
    {
        var localSymbols = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);
        if (_symbols.TryGetValue(function.Identifier.Text, out var symbol) && symbol is FunctionSymbol fn)
        {
            foreach (var parameter in fn.Parameters)
            {
                localSymbols[parameter.Name] = parameter;
            }
        }

        foreach (var statement in function.Body!.Statements)
        {
            var expression = statement switch
            {
                ReturnStatementSyntax @return => @return.Expression,
                ExpressionStatementSyntax expr => expr.Expression,
                _ => null
            };

            if (expression is not null)
            {
                BindExpression(documentPath, expression, diagnostics, references, localSymbols);
            }
        }
    }

    private void BindExpression(string documentPath, ExpressionSyntax expression, List<Diagnostic> diagnostics, List<BoundReference> references, IReadOnlyDictionary<string, Symbol> locals)
    {
        switch (expression)
        {
            case LiteralExpressionSyntax:
                return;
            case NameExpressionSyntax name when locals.TryGetValue(name.Identifier.Text, out var local):
                references.Add(new BoundReference(documentPath, name.Identifier.Span, local.Name, local, local.DocsPath));
                return;
            case NameExpressionSyntax name when _symbols.TryGetValue(name.Identifier.Text, out var symbol):
                references.Add(new BoundReference(documentPath, name.Identifier.Span, symbol.Name, symbol, symbol.DocsPath));
                return;
            case NameExpressionSyntax name:
                diagnostics.Add(new Diagnostic("ML3001", DiagnosticSeverity.Error, $"Undefined symbol '{name.Identifier.Text}'.", documentPath, name.Identifier.Span));
                return;
            case BinaryExpressionSyntax binary:
                BindExpression(documentPath, binary.Left, diagnostics, references, locals);
                BindExpression(documentPath, binary.Right, diagnostics, references, locals);
                return;
            case ParenthesizedExpressionSyntax parenthesized:
                BindExpression(documentPath, parenthesized.Expression, diagnostics, references, locals);
                return;
            case MemberAccessExpressionSyntax member:
                BindExpression(documentPath, member.Target, diagnostics, references, locals);
                references.Add(new BoundReference(documentPath, member.MemberName.Span, member.MemberName.Text, _symbols.TryGetValue(member.MemberName.Text, out var target) ? target : null, target?.DocsPath ?? "/docs/reference/modules"));
                return;
            case CallExpressionSyntax call:
                BindExpression(documentPath, call.Target, diagnostics, references, locals);
                foreach (var arg in call.Arguments)
                {
                    BindExpression(documentPath, arg, diagnostics, references, locals);
                }
                return;
        }
    }

    private void ValidateExternFunction(string documentPath, FunctionDeclarationSyntax syntax, FunctionSymbol symbol, List<Diagnostic> diagnostics)
    {
        if (!symbol.Attributes.Any(static x => string.Equals(x.Definition.Name, "dll_import", StringComparison.OrdinalIgnoreCase)))
        {
            diagnostics.Add(new Diagnostic("ML2001", DiagnosticSeverity.Error, "Extern functions require #[dll_import(...)] metadata.", documentPath, syntax.Identifier.Span));
        }
        var import = symbol.Attributes.FirstOrDefault(static x => string.Equals(x.Definition.Name, "dll_import", StringComparison.OrdinalIgnoreCase));
        var callingConvention = symbol.Attributes.FirstOrDefault(static x => string.Equals(x.Definition.Name, "calling_convention", StringComparison.OrdinalIgnoreCase));
        var library = import?.Arguments.TryGetValue("library", out var lib) == true ? lib : "unknown";
        var entryPoint = import?.Arguments.TryGetValue("entrypoint", out var entry) == true ? entry : symbol.Name;
        var charset = import?.Arguments.TryGetValue("charset", out var cs) == true ? cs : "utf16";
        var convention = callingConvention?.Arguments.TryGetValue("value", out var cc) == true ? cc : "platform";
        _externSignatures.Add(new ExternSignature(symbol.Name, library, entryPoint, convention, charset, symbol.Parameters.Select(static x => $"{x.Name}: {x.Type.Name}").ToArray(), symbol.ReturnType.Name));
    }

    private void BindAttributes(IReadOnlyList<AttributeListSyntax> attributeLists, Symbol symbol, AttributeTarget target, string documentPath, List<Diagnostic> diagnostics)
    {
        foreach (var list in attributeLists)
        {
            foreach (var attribute in list.Attributes)
            {
                if (!BuiltinAttributes.TryGetValue(attribute.NameToken.Text, out var definition))
                {
                    diagnostics.Add(new Diagnostic("ML1001", DiagnosticSeverity.Error, $"Unknown attribute '{attribute.NameToken.Text}'.", documentPath, attribute.Span));
                    continue;
                }
                if (!definition.AllowedTargets.Contains(AttributeTarget.Any) && !definition.AllowedTargets.Contains(target))
                {
                    diagnostics.Add(new Diagnostic("ML1002", DiagnosticSeverity.Error, $"Attribute '{definition.Name}' is not valid on {target}.", documentPath, attribute.Span));
                }
                var arguments = attribute.Arguments.ToDictionary(static x => x.NameToken.Text, static x => ExtractAttributeValue(x.Expression), StringComparer.OrdinalIgnoreCase);
                foreach (var required in definition.RequiredArguments)
                {
                    if (!arguments.ContainsKey(required))
                    {
                        diagnostics.Add(new Diagnostic("ML1003", DiagnosticSeverity.Error, $"Attribute '{definition.Name}' requires argument '{required}'.", documentPath, attribute.Span));
                    }
                }
                symbol.Attributes.Add(new AttributeData(definition, arguments, attribute.Span));
            }
        }
    }

    private void ValidateAttributes(IReadOnlyList<AttributeListSyntax> attributeLists, AttributeTarget target, string documentPath, List<Diagnostic> diagnostics, Symbol? owner)
    {
        if (owner is not null)
        {
            BindAttributes(attributeLists, owner, target, documentPath, diagnostics);
            return;
        }
        foreach (var list in attributeLists)
        {
            foreach (var attribute in list.Attributes)
            {
                if (!BuiltinAttributes.TryGetValue(attribute.NameToken.Text, out var definition))
                {
                    diagnostics.Add(new Diagnostic("ML1001", DiagnosticSeverity.Error, $"Unknown attribute '{attribute.NameToken.Text}'.", documentPath, attribute.Span));
                }
                else if (!definition.AllowedTargets.Contains(AttributeTarget.Any) && !definition.AllowedTargets.Contains(target))
                {
                    diagnostics.Add(new Diagnostic("ML1002", DiagnosticSeverity.Error, $"Attribute '{definition.Name}' is not valid on {target}.", documentPath, attribute.Span));
                }
            }
        }
    }

    private TypeSymbol? ResolveType(TypeSyntax? syntax, string documentPath, TextSpan errorSpan, List<Diagnostic>? diagnostics = null, IReadOnlySet<string>? genericParameters = null)
    {
        if (syntax is null)
        {
            return BuiltinTypes["void"];
        }
        if (syntax is PointerTypeSyntax pointer)
        {
            var element = ResolveType(pointer.ElementType, documentPath, pointer.ElementType.Span, diagnostics, genericParameters) ?? BuiltinTypes["void"];
            return new TypeSymbol($"{element.Name}*", SymbolKind.BuiltinType, documentPath, syntax.Span, $"Pointer to {element.Name}.", TypeKind.Pointer, "/docs/interop#pointers");
        }
        if (syntax is NamedTypeSyntax named && _symbols.TryGetValue(named.Identifier.Text, out var symbol) && symbol is TypeSymbol typeSymbol)
        {
            return typeSymbol;
        }
        if (syntax is NamedTypeSyntax generic && genericParameters is not null && genericParameters.Contains(generic.Identifier.Text))
        {
            return new TypeSymbol(generic.Identifier.Text, SymbolKind.TypeParameter, documentPath, generic.Span, $"Generic type parameter {generic.Identifier.Text}.", TypeKind.GenericParameter);
        }
        if (syntax is NamedTypeSyntax unknown)
        {
            diagnostics?.Add(new Diagnostic("ML3002", DiagnosticSeverity.Error, $"Unknown type '{unknown.Identifier.Text}'.", documentPath, errorSpan));
        }
        return null;
    }

    private void AddSymbol(Symbol symbol)
    {
        if (!_symbols.ContainsKey(symbol.Name))
        {
            _symbols.Add(symbol.Name, symbol);
        }
    }

    private static bool HasAttribute(IReadOnlyList<AttributeListSyntax> attributes, string name) =>
        attributes.SelectMany(static x => x.Attributes).Any(x => string.Equals(x.NameToken.Text, name, StringComparison.OrdinalIgnoreCase));

    private static string ExtractAttributeValue(ExpressionSyntax expression) => expression switch
    {
        LiteralExpressionSyntax literal when literal.LiteralToken.Value is not null => literal.LiteralToken.Value.ToString() ?? string.Empty,
        NameExpressionSyntax name => name.Identifier.Text,
        _ => string.Empty
    };
}
