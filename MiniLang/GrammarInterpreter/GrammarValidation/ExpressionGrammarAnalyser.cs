using MiniLang.Functions;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniLang.GrammarInterpreter.GrammarValidation
{
    public class ExpressionGrammarAnalyser
    {
        private  ScopeObjectValueManager _scope;
        private readonly FunctionDeclarationScopeManager _functionManager;

        public ExpressionGrammarAnalyser(ScopeObjectValueManager scope, FunctionDeclarationScopeManager functionManager)
        {
            _scope = scope;
            _functionManager = functionManager;
        }
        public ExpressionGrammarAnalyser(ref ScopeObjectValueManager scope, FunctionDeclarationScopeManager functionManager)
        {
            _scope = scope;
            _functionManager = functionManager;
        }
        public void UpdateScope(ScopeObjectValueManager scope)
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

            if (tokens[0].TokenType is TokenType.CSharp)
            {
                i = tokens.Length;
                return true;//can't verify csharp calls

            }

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

            switch (token.TokenType)
            {
                case TokenType.Number:
                case TokenType.StringLiteralExpression:
                case TokenType.ReturnType:
                case TokenType.CSharp:
                    i++;
                    return true;

                case TokenType.Identifier:
                    string name = token.Value?.ToString() ?? "";
                    if (!_scope.Exists(name)  )
                    {
                        error = $"Undeclared identifier: '{name}'.";
                        return false;
                    }
                    i++;
                    return true;

                case TokenType.FunctionCall:
                    if (token.Value is FunctionCallTokenObject f)
                    {
                        if (_functionManager == null || !f.FunctionName.Contains('.')&&!_functionManager.Exists(f.FunctionName, f.FunctionArgmentsCount))
                        {
                            error = $"Undeclared function: '{f.FunctionName}' with {f.FunctionArgmentsCount} arguments.";
                            return false;
                        }

                        foreach (var arg in f.FunctionArgments)
                        {
                            var argTokens = arg.Argment.ToArray();
                            int argIndex = 0;
                            if (!ParseExpression(argTokens, ref argIndex, out error))
                                return false;
                            if (argIndex != argTokens.Length)
                            {
                                error = $"Invalid argument at position {arg.Index} in function '{f.FunctionName}'.";
                                return false;
                            }
                        }
                        i++;
                        return true;
                    }
                    error = "Malformed function token.";
                    return false;

                default:
                    error = $"Unexpected token: {token.TokenType}";
                    return false;
            }
        }
    }
}
