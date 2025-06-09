using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.SyntaxObjects;
using MiniLang.SyntaxObjects.Make;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins
{
    public class SayExecutable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.Function];

        public TokenOperation[] InvokeOperation => [TokenOperation.SayKeyword];

        public RuntimeValue Dispatch(Token yourToken, RuntimeContext context)
        {
            if(yourToken.Value is not SayFunctionSyntaxObject SayFunctionSyntaxObject)
                throw new InvalidOperationException("Invalid 'Say keyword' token payload.");
            var value = context.RuntimeExpressionEvaluator.Evaluate(SayFunctionSyntaxObject.Argments?.ToList() ?? []);
            Console.WriteLine(value.Value);
            return null;
        }
    }
}
