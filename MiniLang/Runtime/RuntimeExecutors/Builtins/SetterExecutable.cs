using MiniLang.GrammarsAnalyers;
using MiniLang.Interfaces;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins
{
    public class SetterExecutable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.SETTERS];

        public TokenOperation[] InvokeOperation => [   TokenOperation.SETTER,
            TokenOperation.SETTERAddOperation,
            TokenOperation.SETTERSubtractOperation];

        public RuntimeValue Dispatch(Token token, RuntimeContext context)
        {
            if(token.Value is not SetterSyntaxObject setterSyntaxObject)
            {
                throw new ArgumentException("runtime error: Token value must be of type SetterSyntaxObject.", nameof(token));
            }
            if(!context.RuntimeScopeFrame.Exists(setterSyntaxObject.Identifier))
            {
                throw new Exception($"Variable '{setterSyntaxObject.Identifier}' not declared in the current scope.");
            }
            var value = context.RuntimeExpressionEvaluator.Evaluate(setterSyntaxObject.Expression.ToList());
            var currentValue = context.RuntimeScopeFrame.Get(setterSyntaxObject.Identifier);
            if (currentValue.Type != value.Type)
            {
                throw new Exception($"Type mismatch: Cannot add {value.Type} to {currentValue.Type}.");
            }
            context.RuntimeScopeFrame.Assign(setterSyntaxObject.Identifier, new RuntimeValue(currentValue.Type,currentValue.Operator, RenewValue(currentValue.Value, value.Value, setterSyntaxObject.SetterOperator)));
            return null; // No return value for setter operations
        }
        private object RenewValue(object left, object right, SetterOperator setterOperator)
        {
            return setterOperator switch
            {
                SetterOperator.SETTER => right,
                SetterOperator.SETTERAddOperation => (dynamic)left + (dynamic)right,
                SetterOperator.SETTERSubtractOperation => (dynamic)left - (dynamic)right,
                _ => throw new NotSupportedException($"runtime error : setter operator: {setterOperator}"),
            };
        }



    }
}
