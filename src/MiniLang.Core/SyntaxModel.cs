using System.Text;

namespace MiniLang.Core;

public enum SyntaxKind
{
    BadToken,
    EndOfFileToken,
    IdentifierToken,
    NumberToken,
    StringToken,
    HashToken,
    OpenBracketToken,
    CloseBracketToken,
    OpenParenToken,
    CloseParenToken,
    OpenBraceToken,
    CloseBraceToken,
    ColonToken,
    SemicolonToken,
    CommaToken,
    DotToken,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    PercentToken,
    EqualsToken,
    DoubleEqualsToken,
    BangToken,
    BangEqualsToken,
    LessToken,
    LessOrEqualsToken,
    GreaterToken,
    GreaterOrEqualsToken,
    ArrowToken,
    AndAndToken,
    OrOrToken,
    ModuleKeyword,
    UseKeyword,
    FnKeyword,
    StructKeyword,
    EnumKeyword,
    TraitKeyword,
    ImplKeyword,
    ExternKeyword,
    TypeKeyword,
    ForKeyword,
    ReturnKeyword,
    PublicKeyword,
    PrivateKeyword,
    TrueKeyword,
    FalseKeyword,
    CompilationUnit,
    ModuleDeclaration,
    UseDeclaration,
    FunctionDeclaration,
    ExternFunctionDeclaration,
    ExternTypeDeclaration,
    StructDeclaration,
    EnumDeclaration,
    TraitDeclaration,
    ImplDeclaration,
    Parameter,
    FieldDeclaration,
    EnumMember,
    GenericParameter,
    AttributeList,
    Attribute,
    AttributeArgument,
    BlockStatement,
    ReturnStatement,
    ExpressionStatement,
    LiteralExpression,
    NameExpression,
    BinaryExpression,
    CallExpression,
    MemberAccessExpression,
    ParenthesizedExpression,
    NamedTypeClause,
    PointerTypeClause,
    GenericNameClause
}

public sealed record SyntaxTrivia(SyntaxKind Kind, string Text);

public abstract class SyntaxNode
{
    public abstract SyntaxKind Kind { get; }
    public abstract TextSpan Span { get; }
}

public sealed class SyntaxToken : SyntaxNode
{
    public SyntaxToken(
        SyntaxKind kind,
        int position,
        string text,
        object? value = null,
        IReadOnlyList<SyntaxTrivia>? leadingTrivia = null,
        IReadOnlyList<SyntaxTrivia>? trailingTrivia = null)
    {
        Kind = kind;
        Position = position;
        Text = text;
        Value = value;
        LeadingTrivia = leadingTrivia ?? Array.Empty<SyntaxTrivia>();
        TrailingTrivia = trailingTrivia ?? Array.Empty<SyntaxTrivia>();
    }

    public override SyntaxKind Kind { get; }
    public int Position { get; }
    public string Text { get; }
    public object? Value { get; }
    public IReadOnlyList<SyntaxTrivia> LeadingTrivia { get; }
    public IReadOnlyList<SyntaxTrivia> TrailingTrivia { get; }
    public override TextSpan Span => new(Position, Text.Length);
}

public abstract class MemberSyntax : SyntaxNode;
public abstract class StatementSyntax : SyntaxNode;
public abstract class ExpressionSyntax : SyntaxNode;
public abstract class TypeSyntax : SyntaxNode;

public sealed class AttributeListSyntax : SyntaxNode
{
    public AttributeListSyntax(SyntaxToken hashToken, SyntaxToken openBracketToken, IReadOnlyList<AttributeSyntax> attributes, SyntaxToken closeBracketToken)
    {
        HashToken = hashToken;
        OpenBracketToken = openBracketToken;
        Attributes = attributes;
        CloseBracketToken = closeBracketToken;
    }

    public SyntaxToken HashToken { get; }
    public SyntaxToken OpenBracketToken { get; }
    public IReadOnlyList<AttributeSyntax> Attributes { get; }
    public SyntaxToken CloseBracketToken { get; }
    public override SyntaxKind Kind => SyntaxKind.AttributeList;
    public override TextSpan Span => TextSpan.FromBounds(HashToken.Span.Start, CloseBracketToken.Span.End);
}

public sealed class AttributeSyntax : SyntaxNode
{
    public AttributeSyntax(SyntaxToken nameToken, IReadOnlyList<AttributeArgumentSyntax> arguments, SyntaxToken? openParenToken, SyntaxToken? closeParenToken)
    {
        NameToken = nameToken;
        Arguments = arguments;
        OpenParenToken = openParenToken;
        CloseParenToken = closeParenToken;
    }

    public SyntaxToken NameToken { get; }
    public IReadOnlyList<AttributeArgumentSyntax> Arguments { get; }
    public SyntaxToken? OpenParenToken { get; }
    public SyntaxToken? CloseParenToken { get; }
    public override SyntaxKind Kind => SyntaxKind.Attribute;
    public override TextSpan Span => CloseParenToken is null
        ? NameToken.Span
        : TextSpan.FromBounds(NameToken.Span.Start, CloseParenToken.Span.End);
}

public sealed class AttributeArgumentSyntax : SyntaxNode
{
    public AttributeArgumentSyntax(SyntaxToken nameToken, SyntaxToken colonToken, ExpressionSyntax expression)
    {
        NameToken = nameToken;
        ColonToken = colonToken;
        Expression = expression;
    }

    public SyntaxToken NameToken { get; }
    public SyntaxToken ColonToken { get; }
    public ExpressionSyntax Expression { get; }
    public override SyntaxKind Kind => SyntaxKind.AttributeArgument;
    public override TextSpan Span => TextSpan.FromBounds(NameToken.Span.Start, Expression.Span.End);
}

public sealed class CompilationUnitSyntax : SyntaxNode
{
    public CompilationUnitSyntax(ModuleDeclarationSyntax? module, IReadOnlyList<MemberSyntax> members, SyntaxToken endOfFileToken)
    {
        Module = module;
        Members = members;
        EndOfFileToken = endOfFileToken;
    }

    public ModuleDeclarationSyntax? Module { get; }
    public IReadOnlyList<MemberSyntax> Members { get; }
    public SyntaxToken EndOfFileToken { get; }
    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    public override TextSpan Span => Members.Count > 0
        ? TextSpan.FromBounds(Module?.Span.Start ?? Members[0].Span.Start, Members[^1].Span.End)
        : Module?.Span ?? EndOfFileToken.Span;
}

public sealed class ModuleDeclarationSyntax : MemberSyntax
{
    public ModuleDeclarationSyntax(IReadOnlyList<AttributeListSyntax> attributes, SyntaxToken moduleKeyword, IReadOnlyList<SyntaxToken> nameParts, SyntaxToken semicolonToken)
    {
        Attributes = attributes;
        ModuleKeyword = moduleKeyword;
        NameParts = nameParts;
        SemicolonToken = semicolonToken;
    }

    public IReadOnlyList<AttributeListSyntax> Attributes { get; }
    public SyntaxToken ModuleKeyword { get; }
    public IReadOnlyList<SyntaxToken> NameParts { get; }
    public SyntaxToken SemicolonToken { get; }
    public string QualifiedName => string.Join(".", NameParts.Select(static x => x.Text));
    public override SyntaxKind Kind => SyntaxKind.ModuleDeclaration;
    public override TextSpan Span => TextSpan.FromBounds(Attributes.Count > 0 ? Attributes[0].Span.Start : ModuleKeyword.Span.Start, SemicolonToken.Span.End);
}

public sealed class UseDeclarationSyntax : MemberSyntax
{
    public UseDeclarationSyntax(IReadOnlyList<AttributeListSyntax> attributes, SyntaxToken useKeyword, IReadOnlyList<SyntaxToken> pathParts, SyntaxToken semicolonToken)
    {
        Attributes = attributes;
        UseKeyword = useKeyword;
        PathParts = pathParts;
        SemicolonToken = semicolonToken;
    }

    public IReadOnlyList<AttributeListSyntax> Attributes { get; }
    public SyntaxToken UseKeyword { get; }
    public IReadOnlyList<SyntaxToken> PathParts { get; }
    public SyntaxToken SemicolonToken { get; }
    public string QualifiedPath => string.Join(".", PathParts.Select(static x => x.Text));
    public override SyntaxKind Kind => SyntaxKind.UseDeclaration;
    public override TextSpan Span => TextSpan.FromBounds(Attributes.Count > 0 ? Attributes[0].Span.Start : UseKeyword.Span.Start, SemicolonToken.Span.End);
}

public sealed class GenericParameterSyntax : SyntaxNode
{
    public GenericParameterSyntax(SyntaxToken identifier) => Identifier = identifier;
    public SyntaxToken Identifier { get; }
    public override SyntaxKind Kind => SyntaxKind.GenericParameter;
    public override TextSpan Span => Identifier.Span;
}

public sealed class ParameterSyntax : SyntaxNode
{
    public ParameterSyntax(IReadOnlyList<AttributeListSyntax> attributes, SyntaxToken identifier, SyntaxToken colonToken, TypeSyntax type)
    {
        Attributes = attributes;
        Identifier = identifier;
        ColonToken = colonToken;
        Type = type;
    }

    public IReadOnlyList<AttributeListSyntax> Attributes { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken ColonToken { get; }
    public TypeSyntax Type { get; }
    public override SyntaxKind Kind => SyntaxKind.Parameter;
    public override TextSpan Span => TextSpan.FromBounds(Attributes.Count > 0 ? Attributes[0].Span.Start : Identifier.Span.Start, Type.Span.End);
}

public sealed class FieldDeclarationSyntax : SyntaxNode
{
    public FieldDeclarationSyntax(IReadOnlyList<AttributeListSyntax> attributes, SyntaxToken? visibilityToken, SyntaxToken identifier, SyntaxToken colonToken, TypeSyntax type, SyntaxToken semicolonToken)
    {
        Attributes = attributes;
        VisibilityToken = visibilityToken;
        Identifier = identifier;
        ColonToken = colonToken;
        Type = type;
        SemicolonToken = semicolonToken;
    }

    public IReadOnlyList<AttributeListSyntax> Attributes { get; }
    public SyntaxToken? VisibilityToken { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken ColonToken { get; }
    public TypeSyntax Type { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.FieldDeclaration;
    public override TextSpan Span => TextSpan.FromBounds(Attributes.Count > 0 ? Attributes[0].Span.Start : (VisibilityToken?.Span.Start ?? Identifier.Span.Start), SemicolonToken.Span.End);
}
