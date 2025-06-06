using MiniLang.Interfaces;
using MiniLang.Interpreter;
using MiniLang.Interpreter.GrammarDummyScopes;
using MiniLang.Interpreter.GrammarValidation;
using MiniLang.SyntaxObjects;
using MiniLang.Tokenilzer;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace MiniLang.GrammarsAnalyers
{
    public class UseGrammar : IGrammarAnalyser,IDebugger
    {
        public string GrammarName => "use keyword";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.@use];

        public bool RequiresTermination => true;

        public int CacheCode { get; set; }

        public TokenType[] TriggerTokenTypes => throw new NotImplementedException();

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens.Length != 2 || tokens[1].TokenType != TokenType.StringLiteralExpression)
            {
                errorMessage = "syntax error: expected `use \"filepath\";`";
                return true;
            }

            return false;
        }

        public Token BuildNode(
            Token[] tokens,
            ScopeObjectValueManager scopeObjectValueManager,
            ExpressionGrammarAnalyser expressionGrammarAnalyser,
            IGrammarInterpreter grammarInterpreter,
            int line)
        {
            var pathToken = tokens[1];
            var path = pathToken.Value?.ToString();

            if (!File.Exists(path))
                throw new FileNotFoundException($"use error: file not found → \"{path}\"");

            string fileSource = File.ReadAllText(path);

            var tokensFromFile = Tokenizer.Tokenize(fileSource);
            var parsedTokens = Parser.Parser.Parse(tokensFromFile);

           var Tokens =  grammarInterpreter.Interpret(parsedTokens,scopeObjectValueManager,expressionGrammarAnalyser); // run it

            return new Token(TokenType.Keyword, TokenOperation.@use,TokenTree.Single, new UseSyntaxObject(path, Tokens));
        }

        public string ViewSelf(Token Token, GrammarValidator grammarValidator = null, int indentLevel = 0)
        {
            StringBuilder sb = new StringBuilder();
            if(Token.Value is UseSyntaxObject UseSyntaxObject)
            {
                foreach (Token token in UseSyntaxObject.Tokens)
                {
                    if (grammarValidator.ResolveAnalyser(token) is IGrammarAnalyser gm && gm is IDebugger db)
                    {
                        sb.AppendLine(db.ViewSelf(token, grammarValidator, 0));
                    }
                }
            }
            return sb.ToString();
        }
    }
}

