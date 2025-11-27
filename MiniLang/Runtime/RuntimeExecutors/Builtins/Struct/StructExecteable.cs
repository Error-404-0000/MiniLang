using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.SyntaxObjects.Structure;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins.Struct
{
    public class StructExecteable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.Struct];

        public TokenOperation[] InvokeOperation => [TokenOperation.None];

        public RuntimeValue Dispatch(Token currentToken, RuntimeContext context)
        {
            if(currentToken.Value is not StructSyntaxObject structObject) 
            {
                throw new InvalidOperationException($"Invalid struct token was give to Struct.Dispatch, Dispatch got '{currentToken.TokenType}'");
            }
            context.StructFrame.DeclearStruct(structObject.StructName, () => structObject.StructHandler);
            return null;

        }
    }
}
