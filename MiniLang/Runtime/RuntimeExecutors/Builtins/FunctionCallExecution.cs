using MiniLang.Functions;
using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins
{
    /// <summary>
    /// Represents the execution logic for invoking a function call within the runtime context.
    /// </summary>
    /// <remarks>This class is responsible for resolving function definitions, evaluating arguments, and
    /// executing the function body. It ensures that the function call adheres to the expected argument count and return
    /// type.</remarks>
    public class FunctionCallExecution : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.FunctionCall];

        public TokenOperation[] InvokeOperation => [TokenOperation.None];
        /// <summary>
        /// Dispatches a function call based on the provided token and runtime context.
        /// </summary>
        /// <remarks>This method evaluates the arguments of the function, sets up the necessary runtime
        /// scope and function table,  and executes the function body. It ensures that the function returns a value
        /// consistent with its declared return type.</remarks>
        /// <param name="Token">The token representing the function call, including its name and arguments.</param>
        /// <param name="context">The runtime context used to resolve the function, evaluate arguments, and manage scope.</param>
        /// <returns>The result of the function execution as a <see cref="RuntimeValue"/>.  If the function has no return value,
        /// the method returns <c>null</c>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the provided token does not contain a valid function body.</exception>
        /// <exception cref="Exception">Thrown if the function returns a value inconsistent with its declared return type or if a function parameter
        /// name is invalid.</exception>
        public RuntimeValue Dispatch(Token Token, RuntimeContext context)
        {
            if (Token.Value is not FunctionTokenObject fc)
            {
                throw new InvalidOperationException("execution error: invalid token body for calling a function.");
            }
            var function = context.FunctionTable.Resolve(fc.FunctionName, fc.FunctionArgmentsCount);
            RuntimeValue[] argments = new RuntimeValue[fc.FunctionArgmentsCount];
            var argms = fc.FunctionArgments.ToArray();
            //setting up the arg expressions
            for (int i = 0; i < fc.FunctionArgmentsCount; i++)
            {
                argments[i] = context.RuntimeExpressionEvaluator.Evaluate(argms[i].Argment.ToList());
            }
            context.PushScope();
            context.PushFunctionTable();
            //creating the arg local values
            var funcArgs = function.Declaration.FunctionArgments.ToArray();
            for (int i = 0;i < fc.FunctionArgmentsCount; i++)
            {
                context.RuntimeScopeFrame.Declare(new RuntimeVariable(funcArgs[i].Argment.ToArray()[0].Value.ToString() ?? throw new Exception("invalid function parameter name."),
                    TokenType.Identifier, argments[i]));
            }
            var @return = context.RuntimeEngine.Execute(function.Declaration.Body.ToList());
            context.PopFunctionTable();
            context.PopScope();
            if(@return is null  && function.Declaration.ReturnType is not TokenOperation.ReturnsNothing)
            {
                throw new Exception($"{function.Name} function returned nothing instead of {function.Declaration.ReturnType}");
            }
            else if(@return is not null && !MatchReturnType(@return.Type ,@return.Operator, function.Declaration.ReturnType))
            {
                throw new Exception($"{function.Name} function returned {@return.Operator} instead of {function.Declaration.ReturnType}");
            }
            if(@return is not null)
            {
                context.SetReturn(@return?.Type ?? TokenType.ReturnType, @return?.Operator ?? TokenOperation.ReturnsNothing, @return?.Value);

            }
         
            return @return;
        }
        public bool MatchReturnType(TokenType operation,TokenOperation @operator, TokenOperation expected)
        {
            return expected switch
            {
                TokenOperation.ReturnsNothing => operation == TokenType.ReturnType && @operator == TokenOperation.ReturnsNothing,
                TokenOperation.ReturnsNumber => operation == TokenType.Number,
                TokenOperation.ReturnsString => operation == TokenType.StringLiteralExpression,
                TokenOperation.ReturnsObject => true,
                _ => false
            };
        }
    }
}
