using System.Text;

namespace MiniLang.Core;

public sealed class EnumMemberSyntax : SyntaxNode
{
    public EnumMemberSyntax(SyntaxToken identifier) => Identifier = identifier;
    public SyntaxToken Identifier { get; }
    public override SyntaxKind Kind => SyntaxKind.EnumMember;
    public override TextSpan Span => Identifier.Span;
}

public sealed class FunctionDeclarationSyntax : MemberSyntax
{
    public FunctionDeclarationSyntax(
        SyntaxKind declarationKind,
        IReadOnlyList<AttributeListSyntax> attributes,
        SyntaxToken? externKeyword,
        SyntaxToken fnKeyword,
        SyntaxToken identifier,
        IReadOnlyList<GenericParameterSyntax> genericParameters,
        SyntaxToken openParenToken,
        IReadOnlyList<ParameterSyntax> parameters,
        SyntaxToken closeParenToken,
        SyntaxToken? arrowToken,
        TypeSyntax? returnType,
        BlockStatementSyntax? body,
        SyntaxToken terminator)
    {
        DeclarationKind = declarationKind;
        Attributes = attributes;
        ExternKeyword = externKeyword;
        FnKeyword = fnKeyword;
        Identifier = identifier;
        GenericParameters = genericParameters;
        OpenParenToken = openParenToken;
        Parameters = parameters;
        CloseParenToken = closeParenToken;
        ArrowToken = arrowToken;
        ReturnType = returnType;
        Body = body;
        Terminator = terminator;
    }

    public SyntaxKind DeclarationKind { get; }
    public IReadOnlyList<AttributeListSyntax> Attributes { get; }
    public SyntaxToken? ExternKeyword { get; }
    public SyntaxToken FnKeyword { get; }
    public SyntaxToken Identifier { get; }
    public IReadOnlyList<GenericParameterSyntax> GenericParameters { get; }
    public SyntaxToken OpenParenToken { get; }
    public IReadOnlyList<ParameterSyntax> Parameters { get; }
    public SyntaxToken CloseParenToken { get; }
    public SyntaxToken? ArrowToken { get; }
    public TypeSyntax? ReturnType { get; }
    public BlockStatementSyntax? Body { get; }
    public SyntaxToken Terminator { get; }
    public bool IsExtern => DeclarationKind == SyntaxKind.ExternFunctionDeclaration;
    public override SyntaxKind Kind => DeclarationKind;
    public override TextSpan Span => TextSpan.FromBounds(Attributes.Count > 0 ? Attributes[0].Span.Start : (ExternKeyword?.Span.Start ?? FnKeyword.Span.Start), Terminator.Span.End);
}

public sealed class ExternTypeDeclarationSyntax : MemberSyntax
{
    public ExternTypeDeclarationSyntax(IReadOnlyList<AttributeListSyntax> attributes, SyntaxToken externKeyword, SyntaxToken typeKeyword, SyntaxToken identifier, SyntaxToken semicolonToken)
    {
        Attributes = attributes;
        ExternKeyword = externKeyword;
        TypeKeyword = typeKeyword;
        Identifier = identifier;
        SemicolonToken = semicolonToken;
    }

    public IReadOnlyList<AttributeListSyntax> Attributes { get; }
    public SyntaxToken ExternKeyword { get; }
    public SyntaxToken TypeKeyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.ExternTypeDeclaration;
    public override TextSpan Span => TextSpan.FromBounds(Attributes.Count > 0 ? Attributes[0].Span.Start : ExternKeyword.Span.Start, SemicolonToken.Span.End);
}

public sealed class StructDeclarationSyntax : MemberSyntax
{
    public StructDeclarationSyntax(IReadOnlyList<AttributeListSyntax> attributes, SyntaxToken structKeyword, SyntaxToken identifier, IReadOnlyList<GenericParameterSyntax> genericParameters, SyntaxToken openBraceToken, IReadOnlyList<FieldDeclarationSyntax> fields, SyntaxToken closeBraceToken)
    {
        Attributes = attributes;
        StructKeyword = structKeyword;
        Identifier = identifier;
        GenericParameters = genericParameters;
        OpenBraceToken = openBraceToken;
        Fields = fields;
        CloseBraceToken = closeBraceToken;
    }

    public IReadOnlyList<AttributeListSyntax> Attributes { get; }
    public SyntaxToken StructKeyword { get; }
    public SyntaxToken Identifier { get; }
    public IReadOnlyList<GenericParameterSyntax> GenericParameters { get; }
    public SyntaxToken OpenBraceToken { get; }
    public IReadOnlyList<FieldDeclarationSyntax> Fields { get; }
    public SyntaxToken CloseBraceToken { get; }
    public override SyntaxKind Kind => SyntaxKind.StructDeclaration;
    public override TextSpan Span => TextSpan.FromBounds(Attributes.Count > 0 ? Attributes[0].Span.Start : StructKeyword.Span.Start, CloseBraceToken.Span.End);
}

public sealed class EnumDeclarationSyntax : MemberSyntax
{
    public EnumDeclarationSyntax(IReadOnlyList<AttributeListSyntax> attributes, SyntaxToken enumKeyword, SyntaxToken identifier, SyntaxToken openBraceToken, IReadOnlyList<EnumMemberSyntax> members, SyntaxToken closeBraceToken)
    {
        Attributes = attributes;
        EnumKeyword = enumKeyword;
        Identifier = identifier;
        OpenBraceToken = openBraceToken;
        Members = members;
        CloseBraceToken = closeBraceToken;
    }

    public IReadOnlyList<AttributeListSyntax> Attributes { get; }
    public SyntaxToken EnumKeyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenBraceToken { get; }
    public IReadOnlyList<EnumMemberSyntax> Members { get; }
    public SyntaxToken CloseBraceToken { get; }
    public override SyntaxKind Kind => SyntaxKind.EnumDeclaration;
    public override TextSpan Span => TextSpan.FromBounds(Attributes.Count > 0 ? Attributes[0].Span.Start : EnumKeyword.Span.Start, CloseBraceToken.Span.End);
}

public sealed class TraitDeclarationSyntax : MemberSyntax
{
    public TraitDeclarationSyntax(IReadOnlyList<AttributeListSyntax> attributes, SyntaxToken traitKeyword, SyntaxToken identifier, SyntaxToken openBraceToken, IReadOnlyList<FunctionDeclarationSyntax> members, SyntaxToken closeBraceToken)
    {
        Attributes = attributes;
        TraitKeyword = traitKeyword;
        Identifier = identifier;
        OpenBraceToken = openBraceToken;
        Members = members;
        CloseBraceToken = closeBraceToken;
    }

    public IReadOnlyList<AttributeListSyntax> Attributes { get; }
    public SyntaxToken TraitKeyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenBraceToken { get; }
    public IReadOnlyList<FunctionDeclarationSyntax> Members { get; }
    public SyntaxToken CloseBraceToken { get; }
    public override SyntaxKind Kind => SyntaxKind.TraitDeclaration;
    public override TextSpan Span => TextSpan.FromBounds(Attributes.Count > 0 ? Attributes[0].Span.Start : TraitKeyword.Span.Start, CloseBraceToken.Span.End);
}

public sealed class ImplDeclarationSyntax : MemberSyntax
{
    public ImplDeclarationSyntax(IReadOnlyList<AttributeListSyntax> attributes, SyntaxToken implKeyword, TypeSyntax targetType, TypeSyntax? traitType, SyntaxToken? forKeyword, SyntaxToken openBraceToken, IReadOnlyList<FunctionDeclarationSyntax> members, SyntaxToken closeBraceToken)
    {
        Attributes = attributes;
        ImplKeyword = implKeyword;
        TargetType = targetType;
        TraitType = traitType;
        ForKeyword = forKeyword;
        OpenBraceToken = openBraceToken;
        Members = members;
        CloseBraceToken = closeBraceToken;
    }

    public IReadOnlyList<AttributeListSyntax> Attributes { get; }
    public SyntaxToken ImplKeyword { get; }
    public TypeSyntax TargetType { get; }
    public TypeSyntax? TraitType { get; }
    public SyntaxToken? ForKeyword { get; }
    public SyntaxToken OpenBraceToken { get; }
    public IReadOnlyList<FunctionDeclarationSyntax> Members { get; }
    public SyntaxToken CloseBraceToken { get; }
    public override SyntaxKind Kind => SyntaxKind.ImplDeclaration;
    public override TextSpan Span => TextSpan.FromBounds(Attributes.Count > 0 ? Attributes[0].Span.Start : ImplKeyword.Span.Start, CloseBraceToken.Span.End);
}

public sealed class BlockStatementSyntax : SyntaxNode
{
    public BlockStatementSyntax(SyntaxToken openBraceToken, IReadOnlyList<StatementSyntax> statements, SyntaxToken closeBraceToken)
    {
        OpenBraceToken = openBraceToken;
        Statements = statements;
        CloseBraceToken = closeBraceToken;
    }

    public SyntaxToken OpenBraceToken { get; }
    public IReadOnlyList<StatementSyntax> Statements { get; }
    public SyntaxToken CloseBraceToken { get; }
    public override SyntaxKind Kind => SyntaxKind.BlockStatement;
    public override TextSpan Span => TextSpan.FromBounds(OpenBraceToken.Span.Start, CloseBraceToken.Span.End);
}

public sealed class ReturnStatementSyntax : StatementSyntax
{
    public ReturnStatementSyntax(SyntaxToken returnKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
    {
        ReturnKeyword = returnKeyword;
        Expression = expression;
        SemicolonToken = semicolonToken;
    }

    public SyntaxToken ReturnKeyword { get; }
    public ExpressionSyntax Expression { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
    public override TextSpan Span => TextSpan.FromBounds(ReturnKeyword.Span.Start, SemicolonToken.Span.End);
}

public sealed class ExpressionStatementSyntax : StatementSyntax
{
    public ExpressionStatementSyntax(ExpressionSyntax expression, SyntaxToken semicolonToken)
    {
        Expression = expression;
        SemicolonToken = semicolonToken;
    }

    public ExpressionSyntax Expression { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
    public override TextSpan Span => TextSpan.FromBounds(Expression.Span.Start, SemicolonToken.Span.End);
}

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
    public LiteralExpressionSyntax(SyntaxToken literalToken) => LiteralToken = literalToken;
    public SyntaxToken LiteralToken { get; }
    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    public override TextSpan Span => LiteralToken.Span;
}

public sealed class NameExpressionSyntax : ExpressionSyntax
{
    public NameExpressionSyntax(SyntaxToken identifier) => Identifier = identifier;
    public SyntaxToken Identifier { get; }
    public override SyntaxKind Kind => SyntaxKind.NameExpression;
    public override TextSpan Span => Identifier.Span;
}

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
    {
        Left = left;
        OperatorToken = operatorToken;
        Right = right;
    }

    public ExpressionSyntax Left { get; }
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Right { get; }
    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
    public override TextSpan Span => TextSpan.FromBounds(Left.Span.Start, Right.Span.End);
}

public sealed class CallExpressionSyntax : ExpressionSyntax
{
    public CallExpressionSyntax(ExpressionSyntax target, SyntaxToken openParenToken, IReadOnlyList<ExpressionSyntax> arguments, SyntaxToken closeParenToken)
    {
        Target = target;
        OpenParenToken = openParenToken;
        Arguments = arguments;
        CloseParenToken = closeParenToken;
    }

    public ExpressionSyntax Target { get; }
    public SyntaxToken OpenParenToken { get; }
    public IReadOnlyList<ExpressionSyntax> Arguments { get; }
    public SyntaxToken CloseParenToken { get; }
    public override SyntaxKind Kind => SyntaxKind.CallExpression;
    public override TextSpan Span => TextSpan.FromBounds(Target.Span.Start, CloseParenToken.Span.End);
}

public sealed class MemberAccessExpressionSyntax : ExpressionSyntax
{
    public MemberAccessExpressionSyntax(ExpressionSyntax target, SyntaxToken dotToken, SyntaxToken memberName)
    {
        Target = target;
        DotToken = dotToken;
        MemberName = memberName;
    }

    public ExpressionSyntax Target { get; }
    public SyntaxToken DotToken { get; }
    public SyntaxToken MemberName { get; }
    public override SyntaxKind Kind => SyntaxKind.MemberAccessExpression;
    public override TextSpan Span => TextSpan.FromBounds(Target.Span.Start, MemberName.Span.End);
}

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    public ParenthesizedExpressionSyntax(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
    {
        OpenParenToken = openParenToken;
        Expression = expression;
        CloseParenToken = closeParenToken;
    }

    public SyntaxToken OpenParenToken { get; }
    public ExpressionSyntax Expression { get; }
    public SyntaxToken CloseParenToken { get; }
    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
    public override TextSpan Span => TextSpan.FromBounds(OpenParenToken.Span.Start, CloseParenToken.Span.End);
}

public sealed class NamedTypeSyntax : TypeSyntax
{
    public NamedTypeSyntax(SyntaxToken identifier, IReadOnlyList<TypeSyntax> typeArguments)
    {
        Identifier = identifier;
        TypeArguments = typeArguments;
    }

    public SyntaxToken Identifier { get; }
    public IReadOnlyList<TypeSyntax> TypeArguments { get; }
    public override SyntaxKind Kind => TypeArguments.Count == 0 ? SyntaxKind.NamedTypeClause : SyntaxKind.GenericNameClause;
    public override TextSpan Span => Identifier.Span;
}

public sealed class PointerTypeSyntax : TypeSyntax
{
    public PointerTypeSyntax(TypeSyntax elementType, SyntaxToken starToken)
    {
        ElementType = elementType;
        StarToken = starToken;
    }

    public TypeSyntax ElementType { get; }
    public SyntaxToken StarToken { get; }
    public override SyntaxKind Kind => SyntaxKind.PointerTypeClause;
    public override TextSpan Span => TextSpan.FromBounds(ElementType.Span.Start, StarToken.Span.End);
}

public static class SyntaxPrinter
{
    public static string PrettyPrint(SyntaxNode node)
    {
        var builder = new StringBuilder();
        WriteNode(builder, node, 0);
        return builder.ToString();
    }

    public static void WriteNode(StringBuilder builder, SyntaxNode node, int indentLevel)
    {
        builder.Append(' ', indentLevel * 2);
        builder.Append(node.Kind);
        builder.Append(' ');
        builder.Append('[');
        builder.Append(node.Span.Start);
        builder.Append("..");
        builder.Append(node.Span.End);
        builder.Append(']');
        if (node is SyntaxToken token && !string.IsNullOrWhiteSpace(token.Text))
        {
            builder.Append(" \"");
            builder.Append(token.Text.Replace("\"", "\\\"", StringComparison.Ordinal));
            builder.Append('"');
        }
        builder.AppendLine();
    }
}
