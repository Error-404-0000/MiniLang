using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.RuntimeExecutors.Singles
{
    /// <summary>
    /// Represents an executable token that processes number literals.
    /// </summary>
    /// <remarks>This class is responsible for handling tokens that represent numeric values. It validates the
    /// token's value and converts it into a runtime representation.</remarks>
    public class NumberLiteralExecutable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.Number ];
        public TokenOperation[] InvokeOperation => Array.Empty<TokenOperation>();

        public RuntimeValue Dispatch(Token token, RuntimeContext context)
        {
            if (token.Value == null || !double.TryParse(token.Value.ToString(), out double result))
                throw new Exception($"Invalid number literal: {token.Value}");

            return new RuntimeValue(TokenType.Number, TokenOperation.None, result);
        }
    }

}
