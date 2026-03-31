using MiniLang.Collections;
using MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins
{
    /// <summary>
    /// Represents the execution logic for invoking a function call within the runtime context.
    /// </summary>
    public class FunctionCallExecution : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.FunctionCall];

        public TokenOperation[] InvokeOperation => [TokenOperation.None];

        public RuntimeValue Dispatch(Token Token, RuntimeContext context)
        {
            if (Token.Value is not FunctionCallTokenObject fc)
            {
                throw new InvalidOperationException("execution error: invalid token body for calling a function.");
            }

            RuntimeValue[] argments = new RuntimeValue[fc.FunctionArgmentsCount];
            var argms = fc.FunctionArgments.ToArray();
            for (int i = 0; i < fc.FunctionArgmentsCount; i++)
            {
                argments[i] = context.RuntimeExpressionEvaluator.Evaluate(argms[i].Argment.ToList());
            }

            if (CollectionBuiltins.Exists(fc.FunctionName))
            {
                return CollectionBuiltins.Invoke(fc, argments);
            }

            var function = context.FunctionTable.Resolve(fc.FunctionName, fc.FunctionArgmentsCount);
            context.PushScope();
            context.PushFunctionTable();
            context.PushStructTable();
            context.PushEnumTable();

            if (function.Declaration.OnFunctionOpened != null)
            {
                function.Declaration.OnFunctionOpened(context);
            }

            var funcArgs = function.Declaration.FunctionArgments.ToArray();
            for (int i = 0; i < fc.FunctionArgmentsCount; i++)
            {
                var argTokens = funcArgs[i].Argment.ToArray();
                if (argTokens.Length is < 1 or > 2)
                {
                    throw new Exception("invalid function parameter shape.");
                }

                var parameterToken = argTokens[^1];
                var declaredType = ResolveParameterType(argTokens);
                if (declaredType is not null && argments[i].Type != declaredType)
                {
                    throw new Exception($"Function '{function.Name}' expected argument '{parameterToken.Value}' to be '{declaredType}' but got '{argments[i].Type}'.");
                }

                context.RuntimeScopeFrame.Declare(new RuntimeVariable(
                    parameterToken.Value?.ToString() ?? throw new Exception("invalid function parameter name."),
                    declaredType ?? TokenType.Identifier,
                    argments[i]));
            }

            var @return = context.RuntimeEngine.Execute(function.Declaration.Body.ToList());
            if (fc.OnFunctionClosed != null)
            {
                fc.OnFunctionClosed(context);
            }

            context.PopEnumTable();
            context.PopStructTable();
            context.PopFunctionTable();
            context.PopScope();

            if (@return is null && function.Declaration.ReturnType is not TokenOperation.ReturnsNothing)
            {
                throw new Exception($"{function.Name} function returned nothing instead of {function.Declaration.ReturnType}");
            }

            if (@return is not null && !MatchReturnType(@return.Type, @return.Operator, function.Declaration.ReturnType))
            {
                throw new Exception($"{function.Name} function returned {@return.Operator} instead of {function.Declaration.ReturnType}");
            }

            if (@return is not null)
            {
                context.SetReturn(@return.Type, @return.Operator, @return.Value);
            }

            return @return;
        }

        public bool MatchReturnType(TokenType operation, TokenOperation @operator, TokenOperation expected)
        {
            return expected switch
            {
                TokenOperation.ReturnsNothing => operation == TokenType.ReturnType && @operator == TokenOperation.ReturnsNothing,
                TokenOperation.ReturnsNumber => operation == TokenType.Number,
                TokenOperation.ReturnsString => operation == TokenType.StringLiteralExpression,
                TokenOperation.ReturnsObject => true,
                TokenOperation.Enum => operation == TokenType.Enum,
                TokenOperation.ReturnsArray => operation == TokenType.Array,
                _ => false
            };
        }

        private static TokenType? ResolveParameterType(Token[] argTokens)
        {
            if (argTokens.Length != 2 || argTokens[0].TokenType != TokenType.ReturnType)
            {
                return null;
            }

            return argTokens[0].TokenOperation switch
            {
                TokenOperation.ReturnsNumber => TokenType.Number,
                TokenOperation.ReturnsString => TokenType.StringLiteralExpression,
                TokenOperation.ReturnsObject => TokenType.Object,
                TokenOperation.ReturnsArray => TokenType.Array,
                _ => null
            };
        }
    }
}
