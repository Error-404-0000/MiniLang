using MiniLang.Interfaces;
using MiniLang.Interop;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.SyntaxObjects.Csharp;
using MiniLang.TokenObjects;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins;

public sealed class CSharpExecutable : IExecutableToken
{
    public TokenType[] InvokeType => [TokenType.CSharp];
    public TokenOperation[] InvokeOperation => [TokenOperation.Cscall, TokenOperation.Win];

    public RuntimeValue Dispatch(Token yourToken, RuntimeContext context)
    {
        if (yourToken.Value is not CSharpCallSyntaxObject interopCall)
        {
            throw new InvalidOperationException("Invalid interop token payload.");
        }

        var arguments = interopCall.FunctionCall.FunctionArgments
            .Select(arg => context.RuntimeExpressionEvaluator.Evaluate(arg.Argment.ToList()))
            .ToArray();

        var result = InteropBridgeRegistry.Invoke(interopCall.NameSpace, interopCall.FunctionCall.FunctionName, arguments);
        context.ReturnedHandled();
        return result;
    }
}
