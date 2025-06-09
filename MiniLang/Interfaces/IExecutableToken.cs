using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Interfaces
{
    /// <summary>
    /// Represents a token that can be executed within a runtime context.
    /// </summary>
    /// <remarks>This interface defines the contract for handling and dispatching tokens based on their type
    /// and operation. Implementations of this interface determine whether a token can be handled and provide the logic
    /// for executing the token.</remarks>
    public interface IExecutableToken
    {
        TokenType[] InvokeType { get; }
        TokenOperation[] InvokeOperation { get; }

        bool CanHandle(Token token)
        {
            return InvokeType.Contains(token.TokenType)
                && (InvokeOperation.Length == 0 || InvokeOperation.Contains(token.TokenOperation));
        }
        /// <summary>
        /// Dispatches the specified token within the given runtime context and returns the resulting value.
        /// </summary>
        /// <param name="yourToken">The token to be processed. This represents an input that drives the dispatch operation.</param>
        /// <param name="context">The runtime context in which the token is evaluated. Provides necessary state and environment for the
        /// operation.</param>
        /// <returns>The resulting <see cref="RuntimeValue"/> produced by processing the token within the context.</returns>
        RuntimeValue Dispatch(Token yourToken, RuntimeContext context);
    }


}
