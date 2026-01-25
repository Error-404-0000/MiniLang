using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.TokenObjects;

namespace MiniLang.GrammarAnalyzers
{
    /// <summary>
    /// Valid syntaxes:
    ///     <Identifier><++/-->
    ///     <++/--><Identifier>
    /// </summary>
    public class ShortenOperatorGrammar : IGrammarAnalyser
    {
        public string GrammarName => "Shorten Operator";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.Increment,TokenOperation.Decrement];

        public TokenType[] TriggerTokenTypes =>
            new[] { TokenType.ShortenOperator, TokenType.Identifier };

        public bool RequiresTermination => true;

        public int CacheCode { get; set; }

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            // Valid patterns always have exactly 2 tokens:
            //   identifier ++
            //   ++ identifier
            if (tokens.Length != 2)
            {
                errorMessage = $"Invalid syntax. Expected <Identifier><Operator> or <Operator><Identifier>, " +
                               $"but got: {string.Join(' ', tokens.Select(t => t.Value))}";
                return true;
            }

            var first = tokens[0].TokenType;
            var second = tokens[1].TokenType;

            bool pattern1 = first == TokenType.Identifier && second == TokenType.ShortenOperator;
            bool pattern2 = first == TokenType.ShortenOperator && second == TokenType.Identifier;

            if (!pattern1 && !pattern2)
            {
                errorMessage = $"Invalid short operator expression: {string.Join(' ', tokens.Select(t => t.Value))}. " +
                               $"Expected either <Identifier><Operator> or <Operator><Identifier>.";
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
            int line, Action<Token> PushToken)
        {
            var idetifier = tokens.First(x => x.TokenType == TokenType.Identifier);
            var shortenOperator = tokens.First(y => y.TokenType == TokenType.ShortenOperator);
            var sugarTokens = grammarInterpreter.Interpret(BuildSugarTokens(shortenOperator.TokenOperation, idetifier),
                scopeObjectValueManager, functionDeclarationManager, expressionGrammarAnalyser);


            if (tokens[0]== shortenOperator)
            {
                foreach (var token in sugarTokens)
                {
                    PushToken(token);
                }
                return idetifier;
            }
            else
            {
                PushToken(idetifier);
                foreach (var token in sugarTokens)
                {
                    PushToken(token);
                }
                return idetifier;
            }


        }
        private static List<Token> BuildSugarTokens(TokenOperation ShortenOperator, Token Identifier)
        {
            //varName <+=/-=> 1 ;
            return [
               Identifier,//varName
               new Token(TokenType.SETTERS,GetShortenType(ShortenOperator),TokenTree.Single,null),//+=/-=
               new Token(TokenType.Number,TokenOperation.None,TokenTree.Single,1),//1
               new Token(TokenType.Semicolon,TokenOperation.None,TokenTree.Single,';')//;

            ];
        }

        private static TokenOperation GetShortenType(TokenOperation ShortenOperator) =>
            ShortenOperator switch
            {
                TokenOperation.Increment => TokenOperation.SETTERAddOperation,
                TokenOperation.Decrement => TokenOperation.SETTERSubtractOperation,
                _ => throw new NotImplementedException()
            };
    }
}
