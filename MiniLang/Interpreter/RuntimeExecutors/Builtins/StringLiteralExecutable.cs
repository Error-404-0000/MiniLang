using MiniLang.Interfaces;
using MiniLang.Interpreter.Expression;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.RuntimeExecutors.Singles
{
    public class StringInterpolatedExecutable : IExecutableToken
    {
        public TokenType[] InvokeType => new[] { TokenType.StringLiteralExpression };
        public TokenOperation[] InvokeOperation => [TokenOperation.None];

        
        public RuntimeValue? Dispatch(Token token, RuntimeContext context)
        {
            var expression = new InterpolatedStringEvaluator(context.RuntimeExpressionEvaluator);
            return new RuntimeValue(TokenType.StringLiteralExpression, TokenOperation.None, expression.Evaluate(token, context));
        }
    }

}
