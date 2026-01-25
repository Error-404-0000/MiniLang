using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Interfaces
{
    public interface IGrammarInterpreter
    {
        public IEnumerable<Token> Interpret(List<Token> tokens, ScopeObjectValueManager scopeObjectValueManagerParent, FunctionDeclarationScopeManager FunctiondeclarationManager, ExpressionGrammarAnalyser expressionGrammarAnalyser);
        /// <summary>
        /// pushes token on top before the Interpret return a new buildToken
        /// </summary>
        /// <param name="token">the token to push</param>
        public void PushToken(List<Token> builtTokens,Token token);

    }
}
