using MiniLang.Interfaces;
using MiniLang.Interpreter.GrammarDummyScopes;
using MiniLang.Interpreter.GrammarValidation;
using MiniLang.Interpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.SyntaxObjects;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniLang.GrammarsAnalyers
{
    public class SayGrammar : IGrammarAnalyser
    {
        public string GrammarName => "say function";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.say];

        public bool RequiresTermination => true;

        public int CacheCode { get; set; }

        public TokenType[] TriggerTokenTypes => throw new NotImplementedException();

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens == null || tokens.Length < 2)
            {
                errorMessage = "[SayGrammar] 'say' must be followed by a value (e.g. string or identifier).";
                return true;
            }

            if (tokens[1].TokenType != TokenType.StringLiteralExpression &&
                tokens[1].TokenType != TokenType.Identifier)
            {
                errorMessage = $"[SayGrammar] Argument to 'say' must be a string or identifier, found: {tokens[1].TokenType}.";
                return true;
            }

          
            return false; // No error
        }

        public Token BuildNode(Token[] tokens,
            ScopeObjectValueManager scopeObjectValueManager,
            ExpressionGrammarAnalyser expressionGrammarAnalyser,
            FunctionDeclarationManager FunctionDeclarationManager,
            IGrammarInterpreter grammarInterpreter,
            int line)
        {
            var sayToken = tokens[0];
            var args = tokens.Skip(1)
                             .TakeWhile(t => t.TokenType != TokenType.Semicolon)
                             .ToList();

            var function = new SayFunctionSyntaxObject
            {
                FunctionName = "say",
                ArgmentCounts = args.Count,
                Argments = args
            };
            Console.WriteLine(args[0].Value);
            return new Token(TokenType.Function, TokenOperation.say,TokenTree.Single, function);
        }
    }

   
}
