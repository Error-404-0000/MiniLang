using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.SyntaxObjects;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{
    /// <summary>
    /// Represents a grammar analyser for the "give" operation, which processes tokens related to the "give" keyword.
    /// </summary>
    /// <remarks>This grammar analyser is triggered by the "give" token and requires termination. It validates
    /// the syntax and builds a corresponding syntax tree node for the "give" operation.</remarks>
    ///<example>
    ///        
    ///             give 5 + 3; // This will be processed by the GiveGrammar analyser
    ///             give <!--Expression-->;
    /// 
    ///</example> 

    public class GiveGrammar : IGrammarAnalyser
    {
        public string GrammarName => "returner grammar";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.give];

        public TokenType[] TriggerTokenTypes => [];

        public bool RequiresTermination => true;

        public int CacheCode { get ; set ; }

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            if (tokens.Length < 2)
            {
                errorMessage = "[give] must be followed by an expression.";
                return true;
            }
            errorMessage = null;
            return false;

        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager, ExpressionGrammarAnalyser expressionGrammarAnalyser, FunctionDeclarationScopeManager FunctionDeclarationManager, IGrammarInterpreter grammarInterpreter, int Line)
        {
            var expressionTokens = tokens[1..];
            if(!new ExpressionGrammarAnalyser(scopeObjectValueManager,FunctionDeclarationManager).IsValidExpression(expressionTokens,out string error))
            {
                throw new Exception(error);
            }
            return new Token(TokenType.Keyword, TokenOperation.give, TokenTree.Single, new GiveSyntaxObject(expressionTokens));
        }
    }
}
