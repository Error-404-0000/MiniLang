using MiniLang.Attributes.GrammarAttribute;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.SyntaxObjects.Collections;
using MiniLang.TokenObjects;

namespace MiniLang.GrammarsAnalyers
{
    [RequiresBody]
    public class ForeachGrammar : IGrammarAnalyser
    {
        public string GrammarName => "Foreach loop";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.Foreach];

        public TokenType[] TriggerTokenTypes => [];

        public bool RequiresTermination => false;

        public int CacheCode { get; set; }

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;
            if (tokens.Length < 5)
            {
                errorMessage = "foreach requires an identifier, the 'in' keyword, a collection expression, and a body.";
                return true;
            }

            if (tokens[1].TokenType != TokenType.Identifier)
            {
                errorMessage = "foreach requires an iteration variable name after 'foreach'.";
                return true;
            }

            if (tokens[2].TokenOperation != TokenOperation.In)
            {
                errorMessage = "foreach requires the 'in' keyword before the collection expression.";
                return true;
            }

            if (tokens[^1].TokenType != TokenType.Scope)
            {
                errorMessage = "foreach requires a body block.";
                return true;
            }

            return false;
        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager, ExpressionGrammarAnalyser expressionGrammarAnalyser, FunctionDeclarationScopeManager FunctionDeclarationManager, IGrammarInterpreter grammarInterpreter, int line, Action<Token> PushToken)
        {
            var identifier = tokens[1].Value?.ToString() ?? throw new InvalidOperationException("foreach variable name is missing.");
            var collectionExpression = tokens[3..^1];
            if (!expressionGrammarAnalyser.IsValidExpression(collectionExpression, out var error))
            {
                throw new Exception(error);
            }

            if (collectionExpression.Length == 1 && collectionExpression[0].TokenType == TokenType.Identifier)
            {
                var collectionName = collectionExpression[0].Value?.ToString() ?? string.Empty;
                if (scopeObjectValueManager.Exists(collectionName) && scopeObjectValueManager.GetTypeOf(collectionName) != TokenType.Array)
                {
                    throw new Exception($"Cannot foreach over non-array target '{collectionName}'.");
                }
            }

            var body = tokens[^1].Value as IEnumerable<Token> ?? [];
            using var bodyScope = new ScopeObjectValueManager { Parent = scopeObjectValueManager };
            bodyScope.Add(new GrammarInterpreter.GrammerdummyScopes.ScopeObjectValue
            {
                Identifier = identifier,
                IsAssigned = true,
                TokenType = TokenType.Identifier
            });
            var bodyExpressionGrammar = new ExpressionGrammarAnalyser(bodyScope, FunctionDeclarationManager);
            var interpretedBody = grammarInterpreter.Interpret(body.ToList(), bodyScope, FunctionDeclarationManager, bodyExpressionGrammar).ToList();

            return new Token(
                TokenType.Conditions,
                TokenOperation.Foreach,
                TokenTree.Single,
                new ForeachSyntaxObject(identifier, collectionExpression.ToList(), interpretedBody));
        }
    }
}
