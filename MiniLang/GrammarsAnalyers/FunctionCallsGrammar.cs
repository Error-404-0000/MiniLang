using MiniLang.Interfaces;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;
using MiniLang.Attributes.GrammarAttribute;
using MiniLang.Functions;
namespace MiniLang.GrammarsAnalyers
{

    /// <summary>
    /// Provides grammar analysis for function call tokens within a minilang language syntax.
    /// </summary>
    /// <remarks>This class is responsible for analyzing tokens to determine whether they represent valid
    /// function calls. It supports syntax validation, token transformation, and node building for function
    /// calls.</remarks>
    /// <example>
    /// 
    ///     Func(<!-- FunctionCallTokenObject -->arg1, arg2, arg3); // Valid function call syntax-->
    ///     funcName(<!--Expression-->);
    ///     
    /// </example>


    [TriggerTokenType(TriggerType.Type)]
    public class FunctionCallsGrammar : IGrammarAnalyser
    {
        public string GrammarName => "Function Caller";

        public TokenOperation[] TriggerTokensOperator => [];

        public TokenType[] TriggerTokenTypes => [TokenType.FunctionCall];

        public bool RequiresTermination => true;

        public int CacheCode { get ; set ; }

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            if(tokens == null || tokens.Length == 0)
            {
                errorMessage = "Syntax error: incomplete function call.";
                return true;
            }
            if (tokens.Length > 1 || tokens[0].TokenType is not TokenType.FunctionCall)
            {
                errorMessage = "Syntax error: incorrect function call syntax.";
                return true;
            }
            errorMessage = null;
            return false;
            
        }

        public Token BuildNode(Token[] tokens, ScopeObjectValueManager scopeObjectValueManager, ExpressionGrammarAnalyser expressionGrammarAnalyser, FunctionDeclarationScopeManager FunctionDeclarationManager, IGrammarInterpreter grammarInterpreter, int Line)
        {
            if (tokens.Length > 1 || tokens[0].TokenType is not TokenType.FunctionCall || tokens[0].Value is  FunctionTokenObject func&&
                FunctionDeclarationManager.Get(func.FunctionName,func.FunctionArgmentsCount) is  null)
            {

                throw new Exception("Syntax error: incorrect function call syntax.");
            }
            if (tokens[0].Value is FunctionTokenObject func1)
            foreach (var parm in func1.FunctionArgments)
            {
                    if(!expressionGrammarAnalyser.IsValidExpression(parm.Argment.ToArray(), out string error))
                    {
                        throw new Exception(error);
                    }
            }
            return tokens[0];//it's is already a FunctionTokenObject
        }
    }
}
