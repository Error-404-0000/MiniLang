using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins
{
    public class ScopeExecutable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.Scope];

        public TokenOperation[] InvokeOperation => [TokenOperation.None];

        public RuntimeValue Dispatch(Token yourToken, RuntimeContext context)
        {
            if(yourToken.Value is not  IEnumerable<Token> tl)
            {
                throw new InvalidOperationException("trying to execute body with valid body tokens.");
            }
            context.PushScope();
            context.PushFunctionTable();
            var @excute =   context.RuntimeEngine.Execute(tl.ToList());
            context.PopScope();
            context.PopFunctionTable();
            context.ReturnedHandled();
            return @excute;

        }
    }
}
