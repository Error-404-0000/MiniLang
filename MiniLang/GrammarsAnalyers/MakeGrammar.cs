using MiniLang.Interfaces;
using MiniLang.GrammarInterpreter;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.SyntaxObjects.Make;
using MiniLang.TokenObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{

    /// <summary>
    /// Provides functionality for analyzing and processing grammar related to the "make" keyword.
    /// </summary>
    /// <remarks>This class implements grammar analysis for statements that begin with the "make" or "var"
    /// keywords. It validates the syntax of such statements and builds corresponding syntax nodes for further
    /// processing.</remarks>
    ///  <example>
    ///   make x = 5; // Valid statement
    ///   make y; // Valid statement, but value is not set
    ///   make k = 10 + 20; // Valid statement with an expression
    ///   make z = someFunction(); // Valid statement with a function call
    ///   make k =<!--Expression-->; // Valid statement with an expression
    ///  </example>
    ///

    public class MakeGrammar : IGrammarAnalyser,IDebugger
    {
        public string GrammarName => "make keyword";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.make];

        public bool RequiresTermination => true;

        public int CacheCode { get ; set ; }

        public TokenType[] TriggerTokenTypes => throw new NotImplementedException();

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens.Length < 4)
            {
                errorMessage = "Incomplete 'make' statement.";
                return true;
            }

            if (tokens[0].TokenType != TokenType.Keyword && (tokens[0].Value != "make" || tokens[0].Value != "var"))
            {
                errorMessage = "Make statement must begin with 'make' or 'var'.";
                return true;
            }

            if (tokens[1].TokenType != TokenType.Identifier)
            {
                errorMessage = $"Expected variable name after 'make' but got token type '{tokens[1].TokenType}'.";
                return true;
            }

            if (tokens[2].TokenType != TokenType.SETTERS)
            {
                errorMessage = $"Expected '=' after variable name  but got '{tokens[2].TokenType}.";
                return true;
            }


            return false;
        }

        public Token BuildNode(Token[] tokens,ScopeObjectValueManager objectValueManager,
             ExpressionGrammarAnalyser expressionGrammar,
             FunctionDeclarationScopeManager FunctionDeclarationManager
            , IGrammarInterpreter grammarInterpreter, int line)
        {
            string identifier = (string)tokens[1].Value;
            //FunctionTokenObject valueToken = tokens[2..^1];
            
            objectValueManager.Add(new GrammarInterpreter.GrammerdummyScopes.ScopeObjectValue()
            {
                TokenType = tokens.Length - 3 > 2 ? TokenType.Expression : tokens[3].TokenType,
                Identifier=identifier,
                IsAssigned = tokens.Length<3? true:false,//make obj; means value is not set and make obj =...;
            });
            var expression = tokens[3..(tokens.Length)];
            var IsStruct = false;
            var structNameIfWasStruct = default(string);
            if (tokens.Length>3)
            {
                if (tokens[3].TokenType is TokenType.New)
                {
                    expression = tokens[4..(tokens.Length)];
                    if (tokens.Count() is < 3 or > 5)
                    {
                        throw new Exception("Invalid struct name");
                    }
                    else if (tokens[4].TokenType is not TokenType.Identifier)
                    {
                        throw new Exception($"Invalid struct name '{tokens[4].Value}'");

                    }
                    structNameIfWasStruct = tokens[4].Value.ToString();

                    IsStruct = true;
                }
                else if(!expressionGrammar.IsValidExpression(expression, out string errorMessage))
                {
                    throw new Exception((IsStruct?"when creating struct: ":null)+ errorMessage);
                }
            }
            var makeObject = new MakeSyntaxObject(identifier, tokens[3..tokens.Length], line,IsStruct, structNameIfWasStruct);
            return new Token(TokenType.Keyword, TokenOperation.make, TokenTree.Single, makeObject);
        }

        public string ViewSelf(Token Token,GrammarValidator validator, int indentLevel)
        {
            if(Token.TokenOperation is TokenOperation.make)
            {
                if(Token.Value is MakeSyntaxObject makeSyntaxObject)
                {
                    return makeSyntaxObject.ToTreeString(string.Join(" ", Enumerable.Repeat(" ", indentLevel -  indentLevel >0?-1:0))+" ",true);
                }
            }
            return string.Empty;
        }
    }
}
