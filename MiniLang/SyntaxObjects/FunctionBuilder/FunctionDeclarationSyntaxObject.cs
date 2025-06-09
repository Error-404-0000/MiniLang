using MiniLang.Functions;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.SyntaxObjects.FunctionBuilder
{
    public class FunctionDeclarationSyntaxObject : FunctionTokenObject
    {
        public IEnumerable<Token> Body { get;  }
        public TokenOperation ReturnType {  get; }
        public FunctionDeclarationSyntaxObject(string functionName, int functionArgmentsCount, TokenOperation returnType, IEnumerable<FunctionArgments> functionArgments,
           IEnumerable<Token> Body) :
            base(functionName, functionArgmentsCount, functionArgments)
        {

            this.Body = Body;
            ReturnType = returnType;
        }
    }
}
