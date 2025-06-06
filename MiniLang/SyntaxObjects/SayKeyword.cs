using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.SyntaxObjects
{
    public class SayFunctionSyntaxObject
    {
        public string? FunctionName { get; set; }
        public int ArgmentCounts { get; set; }
        public IEnumerable<Token>? Argments { get; set; }
    }
}
