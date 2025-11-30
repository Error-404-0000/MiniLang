using MiniLang.Interfaces;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.TokenObjects;
using System;
using System.Linq;

namespace MiniLang.GrammarsAnalyers
{
    public class TypeofGrammar : IGrammarAnalyser
    {
        public string GrammarName => "typeof keyword";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.@typeof];

        public bool RequiresTermination => true;

        public int CacheCode { get; set; }

        public TokenType[] TriggerTokenTypes => null;

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens.Length != 2 || tokens[1].TokenType != TokenType.Identifier)
            {
                errorMessage = "error: `typeof` must be followed by a name (identifier)";
                return true;
            }

            return false;
        }

        public Token BuildNode(Token[] tokens,
            ScopeObjectValueManager scopeObjectValueManager,
            ExpressionGrammarAnalyser expressionGrammarAnalyser,
            FunctionDeclarationScopeManager FunctionDeclarationManager,
            IGrammarInterpreter grammarInterpreter,
            int line)
        {
            var identifierToken = tokens[1];
            var type = scopeObjectValueManager.GetTypeOf(identifierToken.Value?.ToString());

         

            return new Token(TokenType.StringLiteralExpression, TokenOperation.None,TokenTree.Single, type.ToString());
        }
    }
}
