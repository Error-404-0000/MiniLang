using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Functions
{
    public class FunctionTokenObject
    {
        public FunctionTokenObject(string functionName, int functionArgmentsCount, IEnumerable<FunctionArgments> functionArgments)
        {
            FunctionName = functionName;
            FunctionArgmentsCount = functionArgmentsCount;
            FunctionArgments = functionArgments;
        }

        public string FunctionName { get; }
        public int FunctionArgmentsCount { get;}
        public IEnumerable<FunctionArgments> FunctionArgments { get;}
       
    }
    public record  FunctionArgments(IEnumerable<Token> Argment,int Index)
    {
        public IEnumerable<Token> Argment { get; } = Argment;
        public int Index { get; } = Index;  
    }
}
