using MiniLang.Functions;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.SyntaxObjects.FunctionBuilder
{
    public record class FunctionDeclarationSyntaxObject : FunctionCallTokenObject
    {
        public IEnumerable<Token> Body { get;  }
        public TokenOperation ReturnType {  get; }
        public FunctionDeclarationSyntaxObject(string functionName, int functionArgmentsCount, TokenOperation returnType, IEnumerable<FunctionArgments> functionArgments,
           IEnumerable<Token> Body, OnFunctionOpen? onFunctionOpen = null,OnFunctionClose? onFunctionClose = null) :
            base(functionName, functionArgmentsCount, functionArgments, onFunctionOpen, onFunctionClose)
        {

            this.Body = Body;
            ReturnType = returnType;
        }
    }
}
