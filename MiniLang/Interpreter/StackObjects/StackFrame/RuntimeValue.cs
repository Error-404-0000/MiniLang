using MiniLang.TokenObjects;

namespace MiniLang.Runtime.StackObjects.StackFrame
{
    public class RuntimeValue
    {
        public TokenType Type { get; }
        public TokenOperation Operator { get; }
        public object Value { get; }

        public RuntimeValue(TokenType type,TokenOperation tokenOperation, object value)
        {
            Type = type;
            Value = value;
            Operator = tokenOperation;
        }


        public override string ToString() => $"{Value} ({Type})";
    }
}
