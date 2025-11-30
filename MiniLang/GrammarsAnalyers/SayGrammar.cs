using MiniLang.Interfaces;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammarValidation;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.SyntaxObjects;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniLang.GrammarsAnalyers
{
    /// <summary>
    /// Represents the grammar rules and analysis logic for the "say" function.
    /// </summary>
    /// <remarks>The "say" function grammar is triggered by the <see cref="TokenOperation.SayKeyword"/> token
    /// and requires a valid argument, such as a string literal, identifier, or expression. This grammar enforces
    /// termination with a semicolon and provides functionality to analyze tokens and build syntax nodes for the "say"
    /// function.</remarks>
    /// <example>
    /// 
    ///            say "Hello, World!"; // Valid usage
    ///            say myVariable; // Valid usage with identifier
    ///            say 1 + 2; // Valid usage with expression
    ///            say ; // Invalid usage, missing argument
    ///            say 123; // Invalid usage, argument must be a string or identifier
    ///            say FunctionCall(); // Valid usage with function call
    ///            say "Hello" + " World"; // Valid usage with concatenation
    ///            say <!--Expression-->; // Valid usage with expression
    /// 
    /// </example>
    public class SayGrammar : IGrammarAnalyser
    {
        public string GrammarName => "say function";

        public TokenOperation[] TriggerTokensOperator => [TokenOperation.SayKeyword];

        public bool RequiresTermination => true;

        public int CacheCode { get; set; }

        public TokenType[] TriggerTokenTypes => throw new NotImplementedException();

        public bool Analyze(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens == null || tokens.Length < 2)
            {
                errorMessage = "[SayGrammar] 'say' must be followed by a value (e.g. string or identifier).";
                return true;
            }

            if (tokens[1].TokenType != TokenType.StringLiteralExpression &&
                tokens[1].TokenType != TokenType.Identifier && tokens[1].TokenType!=TokenType.Expression)
            {
                errorMessage = $"[SayGrammar] Argument to 'say' must be a string or identifier, found: {tokens[1].TokenType}.";
                return true;
            }

          
            return false; // No error
        }

        public Token BuildNode(Token[] tokens,
            ScopeObjectValueManager scopeObjectValueManager,
            ExpressionGrammarAnalyser expressionGrammarAnalyser,
            FunctionDeclarationScopeManager FunctionDeclarationManager,
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
            
            return new Token(TokenType.Function, TokenOperation.SayKeyword,TokenTree.Single, function);
        }
    }

   
}
