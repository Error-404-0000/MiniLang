using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.SyntaxObjects.Csharp;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{

    /// <summary>
    /// : cscall System.Y Function(parms); no parms yet
    /// </summary>
    public class CSharpGrammer : IGrammarAnalyser
    {
        public string GrammarName =>"cscall";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.Cscall];

        public TokenType[] TriggerTokenTypes => [TokenType.CSharp];

        public bool RequiresTermination => true;

        public int CacheCode { get ; set ; }

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            errorMessage = "Invalid c# call, expected cscall <namespace> <function> ";

            if (tokens.Length is < 3 or > 3)
            {
                return true;
            }
            if (tokens[1].TokenType is not TokenType.Identifier || tokens[2].TokenType is not TokenType.FunctionCall) return true;
            
            return false;//everything checked out
        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager, ExpressionGrammarAnalyser expressionGrammarAnalyser, FunctionDeclarationScopeManager FunctionDeclarationManager, IGrammarInterpreter grammarInterpreter, int Line)
        {

            return null;
        }
    }
}
