using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.SyntaxObjects.Enum;
using MiniLang.TokenObjects;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins;

public sealed class EnumExecutable : IExecutableToken
{
    public TokenType[] InvokeType => [TokenType.Enum];
    public TokenOperation[] InvokeOperation => [TokenOperation.Enum];

    public RuntimeValue Dispatch(Token yourToken, RuntimeContext context)
    {
        if (yourToken.Value is not EnumSyntaxObject enumSyntax)
        {
            throw new InvalidOperationException("Invalid enum token payload.");
        }

        var members = enumSyntax.Members.ToDictionary(
            static member => member.Name,
            member => new RuntimeEnumValue(enumSyntax.EnumName, member.Name, member.Ordinal),
            StringComparer.Ordinal);

        context.EnumFrame.Declare(new RuntimeEnumDefinition(enumSyntax.EnumName, members));
        return new RuntimeValue(TokenType.Enum, TokenOperation.Enum, enumSyntax.EnumName);
    }
}
