using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.Runtime.StackObjects.StackFunctionFrame;
using MiniLang.SyntaxObjects.FunctionBuilder;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Interpreter.RuntimeExecutors.Builtins
{
    /// <summary>
    /// Represents an executable token for defining and registering functions within a runtime context.
    /// </summary>
    /// <remarks>This class is used to process tokens that represent function declarations and register them
    /// in the runtime's function table. It ensures that the token is valid and contains the necessary information to
    /// create a runtime function.</remarks>
    public class FunctionBuilderExecuteable : IExecutableToken
    {
        public TokenType[] InvokeType =>[TokenType.NewFunction ];

        public TokenOperation[] InvokeOperation => [TokenOperation.None];

        public RuntimeValue Dispatch(Token token, RuntimeContext context)
        {
            if(token == null) throw new ArgumentNullException("execution error: fn token was null.");
            if(token.Value is not FunctionDeclarationSyntaxObject fds)
            {
              throw new ArgumentNullException("execution error: fn token was not a FunctionDeclarationSyntaxObject.");
            }
            RuntimeFunction runtimeFunction = new RuntimeFunction(fds.FunctionName, fds.FunctionArgmentsCount, fds);
            context.FunctionTable.Declare(runtimeFunction);
            return null;
        }
    }
}
