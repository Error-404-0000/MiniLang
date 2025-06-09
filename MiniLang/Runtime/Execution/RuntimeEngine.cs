using MiniLang.Runtime.Executor;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.Execution
{
    public class RuntimeEngine
    {
        private readonly ExecutableTokenDispatcher _dispatcher;
        private readonly RuntimeContext _context;

        public RuntimeEngine(ExecutableTokenDispatcher dispatcher, RuntimeContext context)
        {
            _dispatcher = dispatcher;
            _context = context;
        }

        public RuntimeValue Execute(List<Token> tokens)
        {
            RuntimeValue last = new RuntimeValue(TokenType.ReturnType,TokenOperation.ReturnsNothing,null);

            foreach (var token in tokens)
            {
                var handler = _dispatcher.Resolve(token.TokenType, token.TokenOperation);
                if (handler == null)
                {
                    throw new InvalidOperationException(
                        $"[Runtime::RuntimeEngine] No executable found for token type '{token.TokenType}' with operation '{token.TokenOperation}'");
                }

                var result = handler.Dispatch(token, _context);

                if (result is RuntimeValue rv)
                {
                    last = rv;

                    // Return handling (e.g., stop on return if in a function context)
                    if (_context.ReturnValueHolder != null &&
                        _context.ReturnValueHolder.returnOperator != TokenOperation.ReturnsNothing)
                    {
                        return rv;
                    }
                }
            }

            return last;
        }
    }
}
