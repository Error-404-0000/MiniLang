using MiniLang.Functions;
using MiniLang.TokenObjects;

namespace MiniLang.SyntaxObjects.Csharp
{
    public record class CSharpCallSyntaxObject(string NameSpace, FunctionCallTokenObject FunctionCall, TokenOperation InvokeOperation);
}
