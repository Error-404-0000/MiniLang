using MiniLang.Interfaces;
using MiniLang.Interpreter;
using MiniLang.Interpreter.GrammarDummyScopes;
using MiniLang.Interpreter.GrammarValidation;
using MiniLang.SyntaxObjects.Make;
using MiniLang.TokenObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{

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
                return false;
            }

            if (tokens[0].TokenType != TokenType.Keyword || (tokens[0].Value != "make" && tokens[0].Value != "var"))
            {
                errorMessage = "Statement must begin with 'make' or 'var'.";
                return false;
            }

            if (tokens[1].TokenType != TokenType.Identifier)
            {
                errorMessage = "Expected variable name after 'make'.";
                return false;
            }

            if (tokens[2].TokenType != TokenType.SETTERS)
            {
                errorMessage = "Expected '=' after variable name.";
                return false;
            }

            if (tokens[^1].TokenType != TokenType.Semicolon)
            {
                errorMessage = "Expected ';' at end of statement.";
                return false;
            }

            return true;
        }

        public Token BuildNode(Token[] tokens,ScopeObjectValueManager objectValueManager, ExpressionGrammarAnalyser expressionGrammar
            , IGrammarInterpreter grammarInterpreter, int line)
        {
            string identifier = (string)tokens[1].Value;
            //FunctionTokenObject valueToken = tokens[2..^1];
            
            objectValueManager.Add(new Interpreter.GrammerdummyScopes.ScopeObjectValue()
            {
                TokenType = tokens.Length - 3 > 2 ? TokenType.Expression : tokens[3].TokenType,
                Identifier=identifier,
                IsAssigned = tokens.Length<3? true:false,//make obj; means value is not set and make obj =...;
            });
            if (tokens.Length>3&&!expressionGrammar.IsValidExpression(tokens[3..(tokens.Length)],out string errorMessage))
            {
                throw new Exception(errorMessage);
            }
            var makeObject = new MakeSyntaxObject(identifier, tokens[2..tokens.Length], line);
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
