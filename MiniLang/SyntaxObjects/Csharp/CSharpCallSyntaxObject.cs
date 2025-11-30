using MiniLang.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.SyntaxObjects.Csharp
{
    public record class CSharpCallSyntaxObject(string NameSpace, FunctionCallTokenObject FunctionCall);
   
}
