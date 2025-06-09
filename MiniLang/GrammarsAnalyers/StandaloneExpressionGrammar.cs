using MiniLang.Attributes.GrammarAttribute;
using MiniLang.Interfaces;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{
    [TriggerTokenType(TriggerType.Type)]
    public class StandaloneExpressionGrammar : IGrammarAnalyser
    {
        public string GrammarName => "StandaloneExpression";

        public TokenOperation[] TriggerTokensOperator => [];

        public TokenType[] TriggerTokenTypes => [TokenType.None];

        public bool RequiresTermination => true;

        public int CacheCode { get ; set; }

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            errorMessage = "StandaloneExpression error: an expression can't be called as a standalone.";
            return true;
        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager, ExpressionGrammarAnalyser expressionGrammarAnalyser, FunctionDeclarationScopeManager FunctionDeclarationManager, IGrammarInterpreter grammarInterpreter, int Line)
        {
            return tokens[0];//makes the complier happy, but this is not used in this context.
        }
    }
}
