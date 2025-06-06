using MiniLang.Interpreter.GrammarDummyScopes;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Interpreter.GrammarValidation
{
    public class ExpressionGrammarAnalyser
    {
        private readonly ScopeObjectValueManager _scope;

        public ExpressionGrammarAnalyser(ScopeObjectValueManager scope)
        {
            _scope = scope;
        }

        public bool IsValidExpression(Token[] tokens, out string errorMessage)
        {
            errorMessage = null;

            if (tokens == null || tokens.Length == 0)
            {
                errorMessage = "Expression is empty.";
                return false;
            }

            int i = 0;
            bool valid = ParseExpression(tokens, ref i, out errorMessage);

            if (!valid)
                return false;

            if (i != tokens.Length)
            {
                errorMessage = $"Unexpected token at position {i}: '{tokens[i].Value}'";
                return false;
            }

            return true;
        }

        private bool ParseExpression(Token[] tokens, ref int i, out string error)
        {
            error = null;

            if (!ParseOperand(tokens, ref i, out error))
                return false;

            while (i < tokens.Length && tokens[i].TokenType == TokenType.Operation)
            {
                i++; // skip operator

                if (!ParseOperand(tokens, ref i, out error))
                    return false;
            }

            return true;
        }

        private bool ParseOperand(Token[] tokens, ref int i, out string error)
        {
            error = null;
            if (i >= tokens.Length)
            {
                error = "Expected operand but reached end of tokens.";
                return false;
            }

            var token = tokens[i];

            if (token.TokenTree == TokenTree.Group && token.Value is IEnumerable<Token> groupTokens)
            {
                var inner = groupTokens.ToArray();
                int innerIndex = 0;
                if (!ParseExpression(inner, ref innerIndex, out error))
                    return false;

                if (innerIndex != inner.Length)
                {
                    error = "Unconsumed tokens inside group.";
                    return false;
                }

                i++;
                return true;
            }

            // Other standard cases
            switch (token.TokenType)
            {
                case TokenType.Number:
                case TokenType.StringLiteralExpression:
                    i++;
                    return true;

                case TokenType.Identifier:
                    if (!_scope.Exists(token.Value?.ToString() ?? ""))
                    {
                        error = $"Undeclared identifier '{token.Value}'.";
                        return false;
                    }
                    
                    i++;
                    return true;

                default:
                    error = $"Unexpected token: {token.TokenType}";
                    return false;
            }
        }

    }
}
