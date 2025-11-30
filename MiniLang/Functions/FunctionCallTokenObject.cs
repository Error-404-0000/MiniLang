using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Functions
{
    public record class FunctionCallTokenObject
    {
        public delegate void OnFunctionOpen(RuntimeContext  runtimeContext);
        public delegate void OnFunctionClose(RuntimeContext runtimeContext);

        public FunctionCallTokenObject(string functionName, int functionArgmentsCount, IEnumerable<FunctionArgments> functionArgments,
            OnFunctionOpen? OnFunctionOpened = default, OnFunctionClose? OnFunctionClosed = default)
        {
            FunctionName = functionName;
            FunctionArgmentsCount = functionArgmentsCount;
            FunctionArgments = functionArgments;
            this.OnFunctionOpened = OnFunctionOpened;
            this.OnFunctionClosed = OnFunctionClosed;
        }

        public string FunctionName { get; set; }
        public int FunctionArgmentsCount { get;}
        public IEnumerable<FunctionArgments> FunctionArgments { get;}
        public OnFunctionOpen? OnFunctionOpened { get; set; }
        public OnFunctionClose? OnFunctionClosed { get; set; }
    }
    public record  FunctionArgments(IEnumerable<Token> Argment,int Index)
    {
        public IEnumerable<Token> Argment { get; } = Argment;
        public int Index { get; } = Index;  
    }
}
