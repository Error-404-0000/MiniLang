using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.SyntaxObjects;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Interpreter.RuntimeExecutors.Builtins
{
    public class GiveExacuteable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.Keyword];

        public TokenOperation[] InvokeOperation => [TokenOperation.give];

        /// <summary>
        /// Evaluates the expression contained within the specified token and returns the resulting runtime value.
        /// </summary>
        /// <param name="Token">The token containing the expression to be evaluated. The <see cref="Token.Value"/> must be of type
        /// <c>GiveSyntaxObject</c>.</param>
        /// <param name="context">The runtime context used for evaluating the expression. Provides access to the runtime expression evaluator.</param>
        /// <returns>The result of evaluating the expression contained in the token.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="Token"/> does not have a <see cref="Token.Value"/> of type
        /// <c>GiveSyntaxObject</c>.</exception>
        public RuntimeValue Dispatch(Token Token, RuntimeContext context)
        {
            if (Token.Value is not GiveSyntaxObject giveSyntaxObject)
            {
                throw new ArgumentException("runtime error : Token value must be of type GiveSyntaxObject", nameof(Token));
            }
            var returncontext = context.RuntimeExpressionEvaluator.Evaluate(giveSyntaxObject.expression.ToList());
            context.SetReturn(returncontext.Type, returncontext.Operator, returncontext.Value);
            return returncontext;
        }
    }
}
