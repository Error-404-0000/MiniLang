using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{
    /// <summary>
    /// Syntax  = <varName><++/-->
    /// Syntax  = <++/--><varName>
    /// </summary>
    public class ShortenOperatorGrammer : IGrammarAnalyser
    {
        public string GrammarName => "Shorten op";

        public TokenOperation[] TriggerTokensOperator => [];

        public TokenType[] TriggerTokenTypes => [TokenType.ShortenOperator, TokenType.Identifier];

        public bool RequiresTermination => true;

        public int CacheCode { get; set; }

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;
            if(tokens.Length is < 3 or > 3)
            {
                errorMessage = $"Invalid operator.expected <Identifier><operator> but got {string.Join(' ',tokens.Select(x=>x.Value))}";
                return true;
            }
            if (tokens[1].TokenType is TokenType.ShortenOperator && tokens[2].TokenType is not TokenType.Identifier)
            {
                errorMessage = $"Invalid operator.expected <operator><Identifier> but got {string.Join(' ', tokens.Select(x => x.Value))}";
                return true;
            }
            if (tokens[1].TokenType is TokenType.Identifier && tokens[2].TokenType is not TokenType.ShortenOperator)
            {
                errorMessage = $"Invalid operator.expected <Identifier><operator> but got {string.Join(' ', tokens.Select(x => x.Value))}";
                return true;
            }
            if(tokens[1].TokenType is not TokenType.Identifier && tokens[1].TokenType is not TokenType.ShortenOperator)
            {

            }
        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager, 
            ExpressionGrammarAnalyser expressionGrammarAnalyser, FunctionDeclarationScopeManager FunctionDeclarationManager, 
            IGrammarInterpreter grammarInterpreter, int Line)
        {
            throw new NotImplementedException();
        }
    }
}
