using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.Executor
{
    public class ExecutableTokenDispatcher
    {
        private readonly List<IExecutableToken> _executables;

        public ExecutableTokenDispatcher(IEnumerable<IExecutableToken> executables)
        {
            _executables = executables.ToList();
        }
        public IExecutableToken? Resolve(TokenType tokenType, TokenOperation tokenOperation)
            => _executables.FirstOrDefault(x => x.InvokeOperation.Contains(tokenOperation) && x.InvokeType.Contains(tokenType));
        
    }
}
