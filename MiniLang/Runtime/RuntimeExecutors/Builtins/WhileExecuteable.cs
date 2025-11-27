using MiniLang.GrammarsAnalyers;
using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins
{
    public class WhileExecuteable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.Conditions];

        public TokenOperation[] InvokeOperation => [TokenOperation.While];

        public RuntimeValue Dispatch(Token token, RuntimeContext context)
        {
            if(token == null)
                throw new Exception("runtime error: token is null in WhileExecuteable Dispatch method.");
            if(token.Value is not WhileSyntaxObject whileSyntax)
                throw new Exception($"runtime error: expected WhileSyntaxObject, got {token.Value.GetType().Name}.");
            var value = context.RuntimeExpressionEvaluator.Evaluate(whileSyntax.Expression.ToList());
            var body = whileSyntax.Scope.ToList();
            context.ReturnedHandled();

            while (value.Value is double d && d > 0 ||
                   value.Value is int i && i > 0 ||
                   value.Value is decimal dec && dec > 0)
            {
                if (!whileSyntax.hasBody)
                    continue;
                context.NewScope();
                var result  = context.RuntimeEngine.Execute(body);
                if(context.ReturnValueHolder != null)
                {
                    if(context.ReturnValueHolder.returnOperator == TokenOperation.ReturnsNothing)
                    {
                        context.ReturnedHandled();
                        continue;
                    }
                    context.ReturnedHandled();
                    return result;
                }
                context.EndScope();

                value = context.RuntimeExpressionEvaluator.Evaluate(whileSyntax.Expression.ToList());
            }
            return null;
        }
    }
}
