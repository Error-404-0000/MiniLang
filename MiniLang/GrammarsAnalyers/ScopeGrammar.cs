using MiniLang.Attributes.GrammarAttribute;
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

    [TriggerTokenType(TriggerType.Type)]
    public class ScopeGrammar : IGrammarAnalyser
    {
        public string GrammarName =>"Scope Grammar";

        public TokenOperation[] TriggerTokensOperator => [];

        public TokenType[] TriggerTokenTypes =>[TokenType.Scope];

        public bool RequiresTermination => false;

        public int CacheCode { get ; set ; }

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            if(tokens == null || tokens.Length is 0 or > 1 )
            {
                errorMessage = "Syntax error: invalid scope body";
                return true;
            }
            if (tokens[0].TokenType != TokenType.Scope)
            {
                errorMessage = "Syntax error: invalid scope body";
                return true;
            }
            errorMessage = "";
            return false;
        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager, ExpressionGrammarAnalyser expressionGrammarAnalyser, FunctionDeclarationScopeManager FunctionDeclarationManager, IGrammarInterpreter grammarInterpreter, int Line)
        {
            ScopeObjectValueManager subscope = new ScopeObjectValueManager();
            subscope.Parent = scopeObjectValueManager;
            FunctionDeclarationScopeManager subFuncScope = new FunctionDeclarationScopeManager();
            subFuncScope.ParentScope = FunctionDeclarationManager;
            grammarInterpreter.Interpret(tokens.ToList(),subscope,subFuncScope,expressionGrammarAnalyser);
            return tokens[0];
        }
    }
}
