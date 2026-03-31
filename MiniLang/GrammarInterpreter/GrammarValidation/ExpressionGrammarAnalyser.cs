using MiniLang.Collections;
using MiniLang.Functions;
using MiniLang.GrammarInterpreter.GrammarDummyScopes;
using MiniLang.GrammarInterpreter.GrammerdummyScopes.MiniLang.Functions;
using MiniLang.SyntaxObjects.Collections;
using MiniLang.TokenObjects;

namespace MiniLang.GrammarInterpreter.GrammarValidation
{
    public class ExpressionGrammarAnalyser
    {
        private ScopeObjectValueManager _scope;
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

            var index = 0;
            var valid = ParseExpression(tokens, ref index, out errorMessage);

            if (!valid)
            {
                return false;
            }

            if (index != tokens.Length)
            {
                errorMessage = $"Unexpected token at position {index}: '{tokens[index].Value}'";
                return false;
            }

            return true;
        }

        private bool ParseExpression(Token[] tokens, ref int index, out string error)
        {
            error = null;

            if (tokens[0].TokenType == TokenType.CSharp)
            {
                index = tokens.Length;
                return true;
            }

            if (!ParseOperand(tokens, ref index, out error))
            {
                return false;
            }

            while (index < tokens.Length && tokens[index].TokenType == TokenType.Operation)
            {
                index++;
                if (!ParseOperand(tokens, ref index, out error))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ParseOperand(Token[] tokens, ref int index, out string error)
        {
            error = null;
            if (index >= tokens.Length)
            {
                error = "Expected operand but reached end of tokens.";
                return false;
            }

            var token = tokens[index];

            if (token.TokenTree == TokenTree.Group && token.Value is IEnumerable<Token> groupTokens)
            {
                var inner = groupTokens.ToArray();
                var innerIndex = 0;
                if (!ParseExpression(inner, ref innerIndex, out error))
                {
                    return false;
                }

                if (innerIndex != inner.Length)
                {
                    error = "Unconsumed tokens inside group.";
                    return false;
                }

                index++;
                return true;
            }

            switch (token.TokenType)
            {
                case TokenType.Number:
                case TokenType.StringLiteralExpression:
                case TokenType.ReturnType:
                case TokenType.CSharp:
                case TokenType.ShortenOperator:
                    index++;
                    return true;

                case TokenType.Array:
                    if (!ValidateArrayToken(token, out error))
                    {
                        return false;
                    }

                    index++;
                    return true;

                case TokenType.Identifier:
                    var name = token.Value?.ToString() ?? string.Empty;
                    if (!_scope.Exists(name))
                    {
                        error = $"Undeclared identifier: '{name}'.";
                        return false;
                    }

                    index++;
                    return true;

                case TokenType.FunctionCall:
                    return ValidateFunctionCall(token, ref index, out error);

                default:
                    error = $"Unexpected token: {token.TokenType}";
                    return false;
            }
        }

        private bool ValidateFunctionCall(Token token, ref int index, out string error)
        {
            error = null;
            if (token.Value is not FunctionCallTokenObject function)
            {
                error = "Malformed function token.";
                return false;
            }

            if ((_functionManager == null || !function.FunctionName.Contains('.') && !_functionManager.Exists(function.FunctionName, function.FunctionArgmentsCount))
                && !CollectionBuiltins.Exists(function.FunctionName))
            {
                error = $"Undeclared function: '{function.FunctionName}' with {function.FunctionArgmentsCount} arguments.";
                return false;
            }

            foreach (var arg in function.FunctionArgments)
            {
                var argTokens = arg.Argment.ToArray();
                var argIndex = 0;
                if (!ParseExpression(argTokens, ref argIndex, out error))
                {
                    return false;
                }

                if (argIndex != argTokens.Length)
                {
                    error = $"Invalid argument at position {arg.Index} in function '{function.FunctionName}'.";
                    return false;
                }
            }

            index++;
            return true;
        }

        private bool ValidateArrayToken(Token token, out string error)
        {
            error = null;

            if (token.Value is ArrayLiteralSyntaxObject literal)
            {
                foreach (var element in literal.Elements)
                {
                    if (element.Count == 0)
                    {
                        continue;
                    }

                    if (!IsValidExpression(element.ToArray(), out error))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (token.Value is ArrayAccessSyntaxObject access)
            {
                if (!ValidateArrayTarget(access.Target, out error))
                {
                    return false;
                }

                if (!IsValidExpression(access.IndexExpression.ToArray(), out error))
                {
                    return false;
                }

                var indexType = TryInferExpressionType(access.IndexExpression.ToArray());
                if (indexType is not null && indexType != TokenType.Number)
                {
                    error = "Array index must evaluate to a number.";
                    return false;
                }

                return true;
            }

            error = "Malformed array syntax.";
            return false;
        }

        private bool ValidateArrayTarget(Token target, out string error)
        {
            error = null;

            switch (target.TokenType)
            {
                case TokenType.Identifier:
                {
                    var name = target.Value?.ToString() ?? string.Empty;
                    if (!_scope.Exists(name))
                    {
                        error = $"Undeclared identifier: '{name}'.";
                        return false;
                    }

                    var declaredType = _scope.GetTypeOf(name);
                    if (declaredType != TokenType.Array && declaredType != TokenType.Object && declaredType != TokenType.Identifier)
                    {
                        error = $"Cannot index non-array target '{name}'.";
                        return false;
                    }

                    return true;
                }

                case TokenType.FunctionCall:
                {
                    var localIndex = 0;
                    if (!ValidateFunctionCall(target, ref localIndex, out error))
                    {
                        return false;
                    }

                    if (target.Value is FunctionCallTokenObject function &&
                        CollectionBuiltins.TryGetReturnType(function.FunctionName, out var returnType, out var returnsNothing))
                    {
                        if (returnsNothing)
                        {
                            error = $"Cannot index the result of '{function.FunctionName}' because it does not return a value.";
                            return false;
                        }

                        if (returnType != TokenType.Array && returnType != TokenType.Object)
                        {
                            error = $"Cannot index non-array result from '{function.FunctionName}'.";
                            return false;
                        }
                    }

                    return true;
                }

                case TokenType.Array:
                    return ValidateArrayToken(target, out error);

                default:
                    error = $"Cannot index token '{target.TokenType}'.";
                    return false;
            }
        }

        private TokenType? TryInferExpressionType(Token[] tokens)
        {
            if (tokens.Length != 1)
            {
                return null;
            }

            var token = tokens[0];
            return token.TokenType switch
            {
                TokenType.Number => TokenType.Number,
                TokenType.StringLiteralExpression => TokenType.StringLiteralExpression,
                TokenType.Array when token.Value is ArrayLiteralSyntaxObject => TokenType.Array,
                TokenType.Identifier when _scope.Exists(token.Value?.ToString() ?? string.Empty) => _scope.GetTypeOf(token.Value?.ToString() ?? string.Empty),
                TokenType.FunctionCall when token.Value is FunctionCallTokenObject function &&
                                            CollectionBuiltins.TryGetReturnType(function.FunctionName, out var returnType, out _) => returnType,
                _ => null
            };
        }
    }
}
