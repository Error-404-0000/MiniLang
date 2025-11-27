using MiniLang.TokenObjects;

namespace MiniLang.Runtime.StackObjects.StackFrame
{
    public class RuntimeVariable
    {
        public string Name { get; }
        public TokenType DeclaredType { get; }
        public RuntimeValue? Value { get; set; }
        public bool IsStruct { get; }
        public RuntimeVariable(string name, TokenType declaredType, RuntimeValue? value = null, bool isStruct = false)
        {
            Name = name;
            DeclaredType = declaredType;
            Value = value;
            IsStruct = isStruct;
        }
        public override string ToString()
        {
            return Value?.Value?.ToString()??"";
        }
    }
}
