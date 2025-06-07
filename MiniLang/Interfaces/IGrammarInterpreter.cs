using MiniLang.Interpreter.GrammarDummyScopes;
using MiniLang.Interpreter.GrammarValidation;
using MiniLang.Interpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Interfaces
{
    public interface IGrammarInterpreter
    {
        public IEnumerable<Token> Interpret(List<Token> tokens, ScopeObjectValueManager scopeObjectValueManagerParent, FunctionDeclarationManager FunctiondeclarationManager, ExpressionGrammarAnalyser expressionGrammarAnalyser);
    }
}
