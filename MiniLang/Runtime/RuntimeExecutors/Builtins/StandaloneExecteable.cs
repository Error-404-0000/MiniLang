using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins
{
    public class StandaloneExecteable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.Identifier];

        public TokenOperation[] InvokeOperation => [TokenOperation.None];

        public RuntimeValue Dispatch(Token yourToken, RuntimeContext context)
        => null!;
    }
}
