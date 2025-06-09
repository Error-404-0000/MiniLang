using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.SyntaxObjects.Condition;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins
{
    /// <summary>
    /// Represents an executable token that evaluates conditional expressions and executes associated code blocks.
    /// </summary>
    /// <remarks>This class is responsible for processing tokens that represent conditional logic, such as
    /// "if" statements. It evaluates the condition expression and executes the corresponding code block if the
    /// condition is met. If the condition evaluates to false and an "else" block is defined, the "else" block is
    /// executed instead.</remarks>
    public class ConditionExecuteable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.Conditions];

        public TokenOperation[] InvokeOperation => [TokenOperation.If];

        /// <summary>
        /// Executes a conditional dispatch based on the evaluation of a condition expression.
        /// </summary>
        /// <remarks>This method evaluates the condition expression within the provided token. If the
        /// expression evaluates to a non-zero numeric value,  the associated conditional scope is executed. If the
        /// condition evaluates to zero and an "else" scope is defined, the "else" scope is executed.  The method
        /// creates a new runtime scope for the execution of conditional or "else" scopes and ensures proper scope
        /// management.</remarks>
        /// <param name="Token">The token containing the condition syntax object to be evaluated.</param>
        /// <param name="context">The runtime context used for evaluating expressions and executing scopes.</param>
        /// <returns>The result of executing the conditional scope, or <see langword="null"/> if the condition evaluates to false
        /// and no "else" scope is defined.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the token value is not a valid <c>ConditionSyntaxObject</c>, or if the condition expression does
        /// not evaluate to a numeric value.</exception>
        public RuntimeValue Dispatch(Token Token, RuntimeContext context)
        {
            if (Token.Value is not ConditionSyntaxObject cso)
            {
                throw new InvalidOperationException($"runtime exception: Token value is not a valid ConditionSyntaxObject: {Token.Value}");
            }
            var expression_result = context.RuntimeExpressionEvaluator.Evaluate(cso.Expression.ToList());
            if (expression_result.Type is not TokenType.Number)
            {
                throw new InvalidOperationException($"runtime exception: Condition expression must evaluate to a number, but got: {expression_result.Type}");
            }
            if (expression_result.Value is not double resultValue)
            {
                throw new InvalidOperationException($"runtime exception: Condition expression must evaluate to a number, but got: {expression_result.Value}");
            }
            if (resultValue != 0)
            {
                if (cso.HasBody)
                {
                    context.NewScope();
                    var @return = context.RuntimeEngine.Execute(cso.Scope.ToList());
                    context.EndScope();
                    if (@return is not null)
                    {
                        context.SetReturn(TokenType.ReturnType, TokenOperation.ReturnsNothing, @return);
                    }
                    return @return;

                }

            }
            else if(cso.Else?.Scope is not null  && cso.Else.Scope.Count()>0)
            {
                context.NewScope();
                var @return = context.RuntimeEngine.Execute(cso.Else.Scope.ToList());
                context.EndScope();
                if (@return is not null)
                {
                    context.SetReturn(TokenType.ReturnType, TokenOperation.ReturnsNothing, @return);
                }
                return @return;
            }
            return null;
        }
    }
}
