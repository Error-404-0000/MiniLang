using MiniLang.Attributes.GrammarAttribute;
using MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.Interpreter.GrammarDummyScopes;
using MiniLang.Interpreter.GrammarValidation;
using MiniLang.SyntaxObjects.FunctionBuilder;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarsAnalyers
{
    /// <summary>
    /// Grammar rule for function declarations.
    /// Syntax: fn <Function> <Scope>
    /// </summary>
    [TriggerTokenType(TriggerType.Type), RequiresBody]
    public class FunctionDeclarationGrammar : IGrammarAnalyser
    {
        public string GrammarName => "Function Declaration";

        public TokenOperation[] TriggerTokensOperator => Array.Empty<TokenOperation>();

        public TokenType[] TriggerTokenTypes => new[] { TokenType.NewFunction };

        public bool RequiresTermination => false;

        public int CacheCode { get; set; }

        public bool Analyse(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens == null || tokens.Length != 3)
            {
                errorMessage = "Syntax error: function declaration must follow the form 'fn <Function> <Body>'.";
                return true;
            }

            if (tokens[1].TokenType != TokenType.Function)
            {
                errorMessage = "Syntax error: 'fn' must be followed by a function signature.";
                return true;
            }

            if (tokens[2].TokenType != TokenType.Scope)
            {
                errorMessage = "Syntax error: function declaration must include a body block.";
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
            if (tokens[1].Value is FunctionTokenObject funcToken)
            {
                // Check that all function arguments are identifiers
                var invalidArg = funcToken.FunctionArgments
                    .FirstOrDefault(arg =>
                        arg.Argment.FirstOrDefault(token => token.TokenType != TokenType.Identifier) != null);

                if (invalidArg != null)
                {
                    throw new InvalidOperationException("Syntax error: function signature requires identifiers as argument names, but an expression was found.");
                }

                return new Token(
                    TokenType.NewFunction,
                    TokenOperation.None,
                    TokenTree.Single,
                    new FunctionDeclarationSyntaxObject(
                        funcToken.FunctionName,
                        funcToken.FunctionArgmentsCount,
                        funcToken.FunctionArgments,
                        grammarInterpreter.Interpret((tokens[2].Value as IEnumerable<Token>).ToList(),scopeObjectValueManager,expressionGrammarAnalyser)
                    )
                );
            }

            throw new InvalidOperationException($"Invalid function token object at line {line}. Expected FunctionTokenObject but got {tokens[1].Value?.GetType().Name ?? "null"}.");
        }
    }
}
