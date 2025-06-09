using CacheLily;
using MiniLang.GrammarInterpreter;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Interfaces
{
    public interface IGrammarAnalyser:ICacheable
    {
        string GrammarName { get; }

        /// <summary>
        /// Performs syntax validation on a segment of tokens.
        /// </summary>
       public bool Analyse(Token[] tokens, out string errorMessage);

        /// <summary>
        /// Specifies which keyword(s) or token this analyser is responsible for.
        /// Helps dispatcher know when to call it.
        /// </summary>
        TokenOperation[] TriggerTokensOperator { get; }
        TokenType[] TriggerTokenTypes { get; }
        /// <summary>
        /// Whether the grammar analyser expects a semicolon (or another delimiter) to terminate.
        /// </summary>
        bool RequiresTermination { get; }

        /// <summary>
        /// Optionally returns a parsed result object (AST node or structure) if grammar is valid.
        /// </summary>
       public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager, ExpressionGrammarAnalyser expressionGrammarAnalyser,
           FunctionDeclarationScopeManager FunctionDeclarationManager, IGrammarInterpreter grammarInterpreter, int Line);
    }

}
