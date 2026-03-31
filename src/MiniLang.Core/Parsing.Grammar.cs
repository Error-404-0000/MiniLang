namespace MiniLang.Core;

internal sealed partial class Parser
{
    private StructDeclarationSyntax ParseStruct(IReadOnlyList<AttributeListSyntax> attributes)
    {
        var keyword = Match(SyntaxKind.StructKeyword);
        var identifier = Match(SyntaxKind.IdentifierToken);
        var genericParameters = ParseGenericParameters();
        var openBrace = Match(SyntaxKind.OpenBraceToken);
        var fields = new List<FieldDeclarationSyntax>();
        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            fields.Add(ParseField());
        }
        var closeBrace = Match(SyntaxKind.CloseBraceToken);
        return new StructDeclarationSyntax(attributes, keyword, identifier, genericParameters, openBrace, fields, closeBrace);
    }

    private EnumDeclarationSyntax ParseEnum(IReadOnlyList<AttributeListSyntax> attributes)
    {
        var keyword = Match(SyntaxKind.EnumKeyword);
        var identifier = Match(SyntaxKind.IdentifierToken);
        var openBrace = Match(SyntaxKind.OpenBraceToken);
        var members = new List<EnumMemberSyntax>();
        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            members.Add(new EnumMemberSyntax(Match(SyntaxKind.IdentifierToken)));
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                _ = Match(SyntaxKind.CommaToken);
            }
            else
            {
                break;
            }
        }
        var closeBrace = Match(SyntaxKind.CloseBraceToken);
        return new EnumDeclarationSyntax(attributes, keyword, identifier, openBrace, members, closeBrace);
    }

    private TraitDeclarationSyntax ParseTrait(IReadOnlyList<AttributeListSyntax> attributes)
    {
        var keyword = Match(SyntaxKind.TraitKeyword);
        var identifier = Match(SyntaxKind.IdentifierToken);
        var openBrace = Match(SyntaxKind.OpenBraceToken);
        var members = new List<FunctionDeclarationSyntax>();
        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var nestedAttributes = ParseAttributeLists();
            members.Add(ParseFunction(nestedAttributes, true, null));
        }
        var closeBrace = Match(SyntaxKind.CloseBraceToken);
        return new TraitDeclarationSyntax(attributes, keyword, identifier, openBrace, members, closeBrace);
    }

    private ImplDeclarationSyntax ParseImpl(IReadOnlyList<AttributeListSyntax> attributes)
    {
        var keyword = Match(SyntaxKind.ImplKeyword);
        var firstType = ParseTypeClause();
        TypeSyntax? traitType = null;
        TypeSyntax targetType = firstType;
        SyntaxToken? forKeyword = null;
        if (Current.Kind == SyntaxKind.ForKeyword)
        {
            forKeyword = Match(SyntaxKind.ForKeyword);
            traitType = firstType;
            targetType = ParseTypeClause();
        }
        var openBrace = Match(SyntaxKind.OpenBraceToken);
        var members = new List<FunctionDeclarationSyntax>();
        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var nestedAttributes = ParseAttributeLists();
            members.Add(ParseFunction(nestedAttributes, false, null));
        }
        var closeBrace = Match(SyntaxKind.CloseBraceToken);
        return new ImplDeclarationSyntax(attributes, keyword, targetType, traitType, forKeyword, openBrace, members, closeBrace);
    }

    private IReadOnlyList<AttributeListSyntax> ParseAttributeLists()
    {
        var lists = new List<AttributeListSyntax>();
        while (Current.Kind == SyntaxKind.HashToken && Peek(1).Kind == SyntaxKind.OpenBracketToken)
        {
            var hash = Match(SyntaxKind.HashToken);
            var open = Match(SyntaxKind.OpenBracketToken);
            var attributes = new List<AttributeSyntax>();
            while (Current.Kind != SyntaxKind.CloseBracketToken && Current.Kind != SyntaxKind.EndOfFileToken)
            {
                attributes.Add(ParseAttribute());
                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    _ = Match(SyntaxKind.CommaToken);
                }
                else
                {
                    break;
                }
            }
            var close = Match(SyntaxKind.CloseBracketToken);
            lists.Add(new AttributeListSyntax(hash, open, attributes, close));
        }
        return lists;
    }

    private AttributeSyntax ParseAttribute()
    {
        var name = Match(SyntaxKind.IdentifierToken);
        if (Current.Kind != SyntaxKind.OpenParenToken)
        {
            return new AttributeSyntax(name, [], null, null);
        }

        var openParen = Match(SyntaxKind.OpenParenToken);
        var arguments = new List<AttributeArgumentSyntax>();
        while (Current.Kind != SyntaxKind.CloseParenToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var argumentName = Match(SyntaxKind.IdentifierToken);
            var colon = Match(SyntaxKind.ColonToken);
            var expression = ParseExpression();
            arguments.Add(new AttributeArgumentSyntax(argumentName, colon, expression));
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                _ = Match(SyntaxKind.CommaToken);
            }
            else
            {
                break;
            }
        }
        var closeParen = Match(SyntaxKind.CloseParenToken);
        return new AttributeSyntax(name, arguments, openParen, closeParen);
    }

    private IReadOnlyList<SyntaxToken> ParseDottedPath()
    {
        var parts = new List<SyntaxToken> { Match(SyntaxKind.IdentifierToken) };
        while (Current.Kind == SyntaxKind.DotToken)
        {
            _ = Match(SyntaxKind.DotToken);
            parts.Add(Match(SyntaxKind.IdentifierToken));
        }
        return parts;
    }

    private IReadOnlyList<GenericParameterSyntax> ParseGenericParameters()
    {
        var parameters = new List<GenericParameterSyntax>();
        if (Current.Kind != SyntaxKind.LessToken)
        {
            return parameters;
        }

        _ = Match(SyntaxKind.LessToken);
        while (Current.Kind != SyntaxKind.GreaterToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            parameters.Add(new GenericParameterSyntax(Match(SyntaxKind.IdentifierToken)));
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                _ = Match(SyntaxKind.CommaToken);
            }
            else
            {
                break;
            }
        }
        _ = Match(SyntaxKind.GreaterToken);
        return parameters;
    }

    private FieldDeclarationSyntax ParseField()
    {
        var attributes = ParseAttributeLists();
        SyntaxToken? visibility = null;
        if (Current.Kind is SyntaxKind.PublicKeyword or SyntaxKind.PrivateKeyword)
        {
            visibility = NextToken();
        }
        var identifier = Match(SyntaxKind.IdentifierToken);
        var colon = Match(SyntaxKind.ColonToken);
        var type = ParseTypeClause();
        var semicolon = Match(SyntaxKind.SemicolonToken);
        return new FieldDeclarationSyntax(attributes, visibility, identifier, colon, type, semicolon);
    }

    private ParameterSyntax ParseParameter()
    {
        var attributes = ParseAttributeLists();
        var identifier = Match(SyntaxKind.IdentifierToken);
        var colon = Match(SyntaxKind.ColonToken);
        var type = ParseTypeClause();
        return new ParameterSyntax(attributes, identifier, colon, type);
    }

    private TypeSyntax ParseTypeClause()
    {
        var identifier = Match(SyntaxKind.IdentifierToken);
        var typeArguments = new List<TypeSyntax>();
        if (Current.Kind == SyntaxKind.LessToken)
        {
            _ = Match(SyntaxKind.LessToken);
            while (Current.Kind != SyntaxKind.GreaterToken && Current.Kind != SyntaxKind.EndOfFileToken)
            {
                typeArguments.Add(ParseTypeClause());
                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    _ = Match(SyntaxKind.CommaToken);
                }
                else
                {
                    break;
                }
            }
            _ = Match(SyntaxKind.GreaterToken);
        }

        TypeSyntax type = new NamedTypeSyntax(identifier, typeArguments);
        while (Current.Kind == SyntaxKind.StarToken)
        {
            type = new PointerTypeSyntax(type, Match(SyntaxKind.StarToken));
        }
        return type;
    }

    private BlockStatementSyntax ParseBlock()
    {
        var openBrace = Match(SyntaxKind.OpenBraceToken);
        var statements = new List<StatementSyntax>();
        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            statements.Add(ParseStatement());
        }
        var closeBrace = Match(SyntaxKind.CloseBraceToken);
        return new BlockStatementSyntax(openBrace, statements, closeBrace);
    }

    private StatementSyntax ParseStatement()
    {
        if (Current.Kind == SyntaxKind.ReturnKeyword)
        {
            var returnKeyword = Match(SyntaxKind.ReturnKeyword);
            var expression = ParseExpression();
            return new ReturnStatementSyntax(returnKeyword, expression, Match(SyntaxKind.SemicolonToken));
        }

        var expr = ParseExpression();
        return new ExpressionStatementSyntax(expr, Match(SyntaxKind.SemicolonToken));
    }

    private ExpressionSyntax ParseExpression(int parentPrecedence = 0)
    {
        ExpressionSyntax left = ParsePrimaryExpression();
        while (true)
        {
            var precedence = GetBinaryPrecedence(Current.Kind);
            if (precedence == 0 || precedence <= parentPrecedence)
            {
                break;
            }
            var operatorToken = NextToken();
            var right = ParseExpression(precedence);
            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }
        return left;
    }

    private ExpressionSyntax ParsePrimaryExpression()
    {
        ExpressionSyntax expression = Current.Kind switch
        {
            SyntaxKind.NumberToken or SyntaxKind.StringToken or SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => new LiteralExpressionSyntax(NextToken()),
            SyntaxKind.IdentifierToken => new NameExpressionSyntax(NextToken()),
            SyntaxKind.OpenParenToken => ParseParenthesized(),
            _ => new LiteralExpressionSyntax(Match(SyntaxKind.StringToken))
        };

        while (Current.Kind is SyntaxKind.DotToken or SyntaxKind.OpenParenToken)
        {
            if (Current.Kind == SyntaxKind.DotToken)
            {
                expression = new MemberAccessExpressionSyntax(expression, Match(SyntaxKind.DotToken), Match(SyntaxKind.IdentifierToken));
            }
            else
            {
                var openParen = Match(SyntaxKind.OpenParenToken);
                var arguments = new List<ExpressionSyntax>();
                while (Current.Kind != SyntaxKind.CloseParenToken && Current.Kind != SyntaxKind.EndOfFileToken)
                {
                    arguments.Add(ParseExpression());
                    if (Current.Kind == SyntaxKind.CommaToken)
                    {
                        _ = Match(SyntaxKind.CommaToken);
                    }
                    else
                    {
                        break;
                    }
                }
                expression = new CallExpressionSyntax(expression, openParen, arguments, Match(SyntaxKind.CloseParenToken));
            }
        }

        return expression;
    }

    private ParenthesizedExpressionSyntax ParseParenthesized()
    {
        var openParen = Match(SyntaxKind.OpenParenToken);
        var expr = ParseExpression();
        var closeParen = Match(SyntaxKind.CloseParenToken);
        return new ParenthesizedExpressionSyntax(openParen, expr, closeParen);
    }

    private static int GetBinaryPrecedence(SyntaxKind kind) => kind switch
    {
        SyntaxKind.StarToken or SyntaxKind.SlashToken or SyntaxKind.PercentToken => 6,
        SyntaxKind.PlusToken or SyntaxKind.MinusToken => 5,
        SyntaxKind.DoubleEqualsToken or SyntaxKind.BangEqualsToken or SyntaxKind.LessToken or SyntaxKind.LessOrEqualsToken or SyntaxKind.GreaterToken or SyntaxKind.GreaterOrEqualsToken => 4,
        SyntaxKind.AndAndToken => 3,
        SyntaxKind.OrOrToken => 2,
        _ => 0
    };

    private SyntaxToken Match(SyntaxKind expectedKind)
    {
        if (Current.Kind == expectedKind)
        {
            return NextToken();
        }
        _diagnostics.Add(new Diagnostic("ML0003", DiagnosticSeverity.Error, $"Expected {expectedKind} but found {Current.Kind}.", _document.Path, Current.Span));
        return new SyntaxToken(expectedKind, Current.Position, string.Empty);
    }

    private SyntaxToken Current => Peek(0);
    private SyntaxToken Peek(int offset)
    {
        var index = _position + offset;
        return index >= _tokens.Count ? _tokens[^1] : _tokens[index];
    }

    private SyntaxToken NextToken()
    {
        var current = Current;
        _position++;
        return current;
    }
}
