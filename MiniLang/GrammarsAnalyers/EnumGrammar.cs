using MiniLang.Attributes.GrammarAttribute;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.SyntaxObjects.Enum;
using MiniLang.TokenObjects;

namespace MiniLang.GrammarsAnalyers;

[TriggerTokenType(TriggerType.Type), RequiresBody]
public sealed class EnumGrammar : IGrammarAnalyser
{
    public string GrammarName => "enum";
    public TokenOperation[] TriggerTokensOperator => [];
    public TokenType[] TriggerTokenTypes => [TokenType.Enum];
    public bool RequiresTermination => false;
    public int CacheCode { get; set; }

    public bool Analyze(Token[] tokens, out string errorMessage)
    {
        errorMessage = null;
        if (tokens.Length != 3)
        {
            errorMessage = "Invalid enum declaration. Expected 'enum <Name> { <members> }'.";
            return true;
        }

        if (tokens[1].TokenType != TokenType.Identifier)
        {
            errorMessage = "Enum name must be an identifier.";
            return true;
        }

        if (tokens[2].TokenType != TokenType.Scope)
        {
            errorMessage = "Enum declaration requires a body.";
            return true;
        }

        return false;
    }

    public Token BuildNode(
        Token[] tokens,
        ScopeObjectValueManager scopeObjectValueManager,
        ExpressionGrammarAnalyser expressionGrammarAnalyser,
        FunctionDeclarationScopeManager functionDeclarationManager,
        IGrammarInterpreter grammarInterpreter,
        int line,
        Action<Token> pushToken)
    {
        var enumName = tokens[1].Value.ToString() ?? throw new InvalidOperationException($"Enum missing name at index {line}.");
        var scopeTokens = tokens[2].Value as IEnumerable<Token> ?? throw new InvalidOperationException("Enum body is missing.");
        var body = scopeTokens.ToList();
        var members = new List<EnumMemberSyntax>();
        var segment = new List<Token>();

        void FlushMember()
        {
            if (segment.Count == 0)
            {
                return;
            }

            if (segment.Count != 1 || segment[0].TokenType != TokenType.Identifier)
            {
                throw new InvalidOperationException($"Enum '{enumName}' only supports named members in v1.");
            }

            var memberName = segment[0].Value.ToString() ?? throw new InvalidOperationException("Enum member name was empty.");
            if (members.Any(x => string.Equals(x.Name, memberName, StringComparison.Ordinal)))
            {
                throw new InvalidOperationException($"Enum '{enumName}' already contains member '{memberName}'.");
            }

            members.Add(new EnumMemberSyntax(memberName, members.Count));
            segment.Clear();
        }

        foreach (var token in body)
        {
            if (token.TokenType == TokenType.Semicolon)
            {
                FlushMember();
                continue;
            }

            segment.Add(token);
        }

        FlushMember();

        if (members.Count == 0)
        {
            throw new InvalidOperationException($"Enum '{enumName}' must declare at least one member.");
        }

        scopeObjectValueManager.Add(new GrammarInterpreter.GrammerdummyScopes.ScopeObjectValue
        {
            Identifier = enumName,
            IsAssigned = true,
            TokenType = TokenType.Enum
        });

        foreach (var member in members)
        {
            scopeObjectValueManager.Add(new GrammarInterpreter.GrammerdummyScopes.ScopeObjectValue
            {
                Identifier = $"{enumName}.{member.Name}",
                IsAssigned = true,
                TokenType = TokenType.Enum
            });
        }

        return new Token(
            TokenType.Enum,
            TokenOperation.Enum,
            TokenTree.Single,
            new EnumSyntaxObject
            {
                EnumName = enumName,
                Members = members
            });
    }
}
