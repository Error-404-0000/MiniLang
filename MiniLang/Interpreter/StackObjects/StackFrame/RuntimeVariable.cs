using MiniLang.TokenObjects;

namespace MiniLang.Runtime.StackObjects.StackFrame
{
    public class RuntimeVariable
    {
        public string Name { get; }
        public TokenType DeclaredType { get; }
        public RuntimeValue? Value { get; set; }

        public RuntimeVariable(string name, TokenType declaredType, RuntimeValue? value = null)
        {
            Name = name;
            DeclaredType = declaredType;
            Value = value;
        }
        public override string ToString()
        {
            return Value?.Value?.ToString()??"";
        }
    }
}
