using System.Text;

namespace MiniLang.Core;

public sealed class SyntaxTree
{
    internal SyntaxTree(SourceDocument document, CompilationUnitSyntax root, IReadOnlyList<SyntaxToken> tokens, IReadOnlyList<Diagnostic> diagnostics)
    {
        Document = document;
        Root = root;
        Tokens = tokens;
        Diagnostics = diagnostics;
    }

    public SourceDocument Document { get; }
    public CompilationUnitSyntax Root { get; }
    public IReadOnlyList<SyntaxToken> Tokens { get; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    public static SyntaxTree Parse(SourceDocument document)
    {
        var lexer = new Lexer(document);
        var (tokens, diagnostics) = lexer.Lex();
        var parser = new Parser(document, tokens, diagnostics);
        return parser.Parse();
    }
}

internal sealed class Lexer
{
    private static readonly Dictionary<string, SyntaxKind> Keywords = new(StringComparer.Ordinal)
    {
        ["module"] = SyntaxKind.ModuleKeyword,
        ["use"] = SyntaxKind.UseKeyword,
        ["fn"] = SyntaxKind.FnKeyword,
        ["struct"] = SyntaxKind.StructKeyword,
        ["enum"] = SyntaxKind.EnumKeyword,
        ["trait"] = SyntaxKind.TraitKeyword,
        ["impl"] = SyntaxKind.ImplKeyword,
        ["extern"] = SyntaxKind.ExternKeyword,
        ["type"] = SyntaxKind.TypeKeyword,
        ["for"] = SyntaxKind.ForKeyword,
        ["return"] = SyntaxKind.ReturnKeyword,
        ["public"] = SyntaxKind.PublicKeyword,
        ["private"] = SyntaxKind.PrivateKeyword,
        ["true"] = SyntaxKind.TrueKeyword,
        ["false"] = SyntaxKind.FalseKeyword
    };

    private readonly SourceDocument _document;
    private readonly string _text;
    private readonly List<Diagnostic> _diagnostics = [];
    private int _position;

    public Lexer(SourceDocument document)
    {
        _document = document;
        _text = document.Text;
    }

    public (IReadOnlyList<SyntaxToken> Tokens, IReadOnlyList<Diagnostic> Diagnostics) Lex()
    {
        var tokens = new List<SyntaxToken>();
        SyntaxToken token;
        do
        {
            token = LexToken();
            if (token.Kind != SyntaxKind.BadToken)
            {
                tokens.Add(token);
            }
        }
        while (token.Kind != SyntaxKind.EndOfFileToken);

        return (tokens, _diagnostics);
    }

    private SyntaxToken LexToken()
    {
        var leading = ReadTrivia();
        var start = _position;
        if (_position >= _text.Length)
        {
            return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, string.Empty, leadingTrivia: leading);
        }

        if (char.IsLetter(Current) || Current == '_')
        {
            while (char.IsLetterOrDigit(Current) || Current == '_' || Current == '.')
            {
                _position++;
            }

            var text = _text[start.._position];
            return new SyntaxToken(Keywords.TryGetValue(text, out var keywordKind) ? keywordKind : SyntaxKind.IdentifierToken, start, text, text, leading);
        }

        if (char.IsDigit(Current))
        {
            while (char.IsDigit(Current))
            {
                _position++;
            }

            var text = _text[start.._position];
            _ = int.TryParse(text, out var value);
            return new SyntaxToken(SyntaxKind.NumberToken, start, text, value, leading);
        }

        if (Current == '"')
        {
            _position++;
            var content = new StringBuilder();
            var terminated = false;
            while (_position < _text.Length)
            {
                if (Current == '"')
                {
                    _position++;
                    terminated = true;
                    break;
                }

                content.Append(Current);
                _position++;
            }

            if (!terminated)
            {
                _diagnostics.Add(new Diagnostic("ML0001", DiagnosticSeverity.Error, "Unterminated string literal.", _document.Path, new TextSpan(start, Math.Max(1, _position - start))));
            }

            return new SyntaxToken(SyntaxKind.StringToken, start, _text[start.._position], content.ToString(), leading);
        }

        var twoChars = _position + 1 < _text.Length ? _text.Substring(_position, 2) : string.Empty;
        switch (twoChars)
        {
            case "->":
                _position += 2;
                return new SyntaxToken(SyntaxKind.ArrowToken, start, "->", leadingTrivia: leading);
            case "==":
                _position += 2;
                return new SyntaxToken(SyntaxKind.DoubleEqualsToken, start, "==", leadingTrivia: leading);
            case "!=":
                _position += 2;
                return new SyntaxToken(SyntaxKind.BangEqualsToken, start, "!=", leadingTrivia: leading);
            case "<=":
                _position += 2;
                return new SyntaxToken(SyntaxKind.LessOrEqualsToken, start, "<=", leadingTrivia: leading);
            case ">=":
                _position += 2;
                return new SyntaxToken(SyntaxKind.GreaterOrEqualsToken, start, ">=", leadingTrivia: leading);
            case "&&":
                _position += 2;
                return new SyntaxToken(SyntaxKind.AndAndToken, start, "&&", leadingTrivia: leading);
            case "||":
                _position += 2;
                return new SyntaxToken(SyntaxKind.OrOrToken, start, "||", leadingTrivia: leading);
        }

        var kind = Current switch
        {
            '#' => SyntaxKind.HashToken,
            '[' => SyntaxKind.OpenBracketToken,
            ']' => SyntaxKind.CloseBracketToken,
            '(' => SyntaxKind.OpenParenToken,
            ')' => SyntaxKind.CloseParenToken,
            '{' => SyntaxKind.OpenBraceToken,
            '}' => SyntaxKind.CloseBraceToken,
            ':' => SyntaxKind.ColonToken,
            ';' => SyntaxKind.SemicolonToken,
            ',' => SyntaxKind.CommaToken,
            '.' => SyntaxKind.DotToken,
            '+' => SyntaxKind.PlusToken,
            '-' => SyntaxKind.MinusToken,
            '*' => SyntaxKind.StarToken,
            '/' => SyntaxKind.SlashToken,
            '%' => SyntaxKind.PercentToken,
            '=' => SyntaxKind.EqualsToken,
            '!' => SyntaxKind.BangToken,
            '<' => SyntaxKind.LessToken,
            '>' => SyntaxKind.GreaterToken,
            _ => SyntaxKind.BadToken
        };

        if (kind == SyntaxKind.BadToken)
        {
            _diagnostics.Add(new Diagnostic("ML0002", DiagnosticSeverity.Error, $"Unrecognized character '{Current}'.", _document.Path, new TextSpan(_position, 1)));
            _position++;
            return new SyntaxToken(SyntaxKind.BadToken, start, _text[start.._position], leadingTrivia: leading);
        }

        _position++;
        return new SyntaxToken(kind, start, _text[start.._position], leadingTrivia: leading);
    }

    private List<SyntaxTrivia> ReadTrivia()
    {
        var trivia = new List<SyntaxTrivia>();
        while (_position < _text.Length)
        {
            if (char.IsWhiteSpace(Current))
            {
                var start = _position;
                while (_position < _text.Length && char.IsWhiteSpace(Current))
                {
                    _position++;
                }
                trivia.Add(new SyntaxTrivia(SyntaxKind.BadToken, _text[start.._position]));
                continue;
            }

            if (Current == '/' && Peek(1) == '/')
            {
                var start = _position;
                while (_position < _text.Length && Current != '\n')
                {
                    _position++;
                }
                trivia.Add(new SyntaxTrivia(SyntaxKind.BadToken, _text[start.._position]));
                continue;
            }

            break;
        }
        return trivia;
    }

    private char Current => _position >= _text.Length ? '\0' : _text[_position];
    private char Peek(int offset) => _position + offset >= _text.Length ? '\0' : _text[_position + offset];
}

internal sealed partial class Parser
{
    private readonly SourceDocument _document;
    private readonly IReadOnlyList<SyntaxToken> _tokens;
    private readonly List<Diagnostic> _diagnostics;
    private int _position;

    public Parser(SourceDocument document, IReadOnlyList<SyntaxToken> tokens, IReadOnlyList<Diagnostic> diagnostics)
    {
        _document = document;
        _tokens = tokens;
        _diagnostics = diagnostics.ToList();
    }

    public SyntaxTree Parse()
    {
        var members = new List<MemberSyntax>();
        ModuleDeclarationSyntax? module = null;
        while (Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var attributes = ParseAttributeLists();
            if (Current.Kind == SyntaxKind.ModuleKeyword)
            {
                module ??= ParseModule(attributes);
                continue;
            }

            var member = ParseMember(attributes);
            if (member is not null)
            {
                members.Add(member);
            }
            else
            {
                _diagnostics.Add(new Diagnostic("ML0004", DiagnosticSeverity.Error, $"Unexpected token '{Current.Text}'.", _document.Path, Current.Span));
                NextToken();
            }
        }
        return new SyntaxTree(_document, new CompilationUnitSyntax(module, members, Match(SyntaxKind.EndOfFileToken)), _tokens, _diagnostics);
    }

    private ModuleDeclarationSyntax ParseModule(IReadOnlyList<AttributeListSyntax> attributes)
    {
        var keyword = Match(SyntaxKind.ModuleKeyword);
        var parts = ParseDottedPath();
        return new ModuleDeclarationSyntax(attributes, keyword, parts, Match(SyntaxKind.SemicolonToken));
    }

    private MemberSyntax? ParseMember(IReadOnlyList<AttributeListSyntax> attributes) => Current.Kind switch
    {
        SyntaxKind.UseKeyword => ParseUse(attributes),
        SyntaxKind.FnKeyword => ParseFunction(attributes, false, null),
        SyntaxKind.StructKeyword => ParseStruct(attributes),
        SyntaxKind.EnumKeyword => ParseEnum(attributes),
        SyntaxKind.TraitKeyword => ParseTrait(attributes),
        SyntaxKind.ImplKeyword => ParseImpl(attributes),
        SyntaxKind.ExternKeyword => ParseExtern(attributes),
        _ => null
    };

    private UseDeclarationSyntax ParseUse(IReadOnlyList<AttributeListSyntax> attributes)
    {
        var keyword = Match(SyntaxKind.UseKeyword);
        var parts = ParseDottedPath();
        return new UseDeclarationSyntax(attributes, keyword, parts, Match(SyntaxKind.SemicolonToken));
    }

    private MemberSyntax ParseExtern(IReadOnlyList<AttributeListSyntax> attributes)
    {
        var externKeyword = Match(SyntaxKind.ExternKeyword);
        if (Current.Kind == SyntaxKind.TypeKeyword)
        {
            var typeKeyword = Match(SyntaxKind.TypeKeyword);
            var identifier = Match(SyntaxKind.IdentifierToken);
            return new ExternTypeDeclarationSyntax(attributes, externKeyword, typeKeyword, identifier, Match(SyntaxKind.SemicolonToken));
        }

        return ParseFunction(attributes, true, externKeyword);
    }

    private FunctionDeclarationSyntax ParseFunction(IReadOnlyList<AttributeListSyntax> attributes, bool isExtern, SyntaxToken? externKeyword)
    {
        var fnKeyword = Match(SyntaxKind.FnKeyword);
        var identifier = Match(SyntaxKind.IdentifierToken);
        var genericParameters = ParseGenericParameters();
        var openParen = Match(SyntaxKind.OpenParenToken);
        var parameters = new List<ParameterSyntax>();
        while (Current.Kind != SyntaxKind.CloseParenToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            parameters.Add(ParseParameter());
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
        SyntaxToken? arrow = null;
        TypeSyntax? returnType = null;
        if (Current.Kind == SyntaxKind.ArrowToken)
        {
            arrow = Match(SyntaxKind.ArrowToken);
            returnType = ParseTypeClause();
        }
        if (isExtern)
        {
            var semicolon = Match(SyntaxKind.SemicolonToken);
            return new FunctionDeclarationSyntax(SyntaxKind.ExternFunctionDeclaration, attributes, externKeyword, fnKeyword, identifier, genericParameters, openParen, parameters, closeParen, arrow, returnType, null, semicolon);
        }
        var body = ParseBlock();
        return new FunctionDeclarationSyntax(SyntaxKind.FunctionDeclaration, attributes, null, fnKeyword, identifier, genericParameters, openParen, parameters, closeParen, arrow, returnType, body, body.CloseBraceToken);
    }
}
