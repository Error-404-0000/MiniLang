using MiniLang.GrammarsAnalyers;
using MiniLang.Interfaces;
using MiniLang.Runtime.Collections;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.SyntaxObjects.Collections;
using MiniLang.TokenObjects;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins
{
    public class SetterExecutable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.SETTERS];

        public TokenOperation[] InvokeOperation => [TokenOperation.SETTER, TokenOperation.SETTERAddOperation, TokenOperation.SETTERSubtractOperation];

        public RuntimeValue Dispatch(Token token, RuntimeContext context)
        {
            if (token.Value is not SetterSyntaxObject setterSyntaxObject)
            {
                throw new ArgumentException("runtime error: Token value must be of type SetterSyntaxObject.", nameof(token));
            }

            var value = context.RuntimeExpressionEvaluator.Evaluate(setterSyntaxObject.Expression.ToList());
            switch (setterSyntaxObject.Target.Value)
            {
                case ArrayAccessSyntaxObject arrayAccess:
                    AssignArrayItem(arrayAccess, value, setterSyntaxObject.SetterOperator, context);
                    context.ReturnedHandled();
                    return null;

                default:
                    AssignVariable(setterSyntaxObject.Target, value, setterSyntaxObject.SetterOperator, context);
                    context.ReturnedHandled();
                    return null;
            }
        }

        private static void AssignVariable(Token target, RuntimeValue value, SetterOperator setterOperator, RuntimeContext context)
        {
            var identifier = target.Value?.ToString() ?? string.Empty;
            if (!context.RuntimeScopeFrame.Exists(identifier))
            {
                throw new Exception($"Variable '{identifier}' not declared in the current scope.");
            }

            var currentValue = context.RuntimeScopeFrame.Get(identifier);
            if (currentValue.Type != value.Type && setterOperator != SetterOperator.SETTER)
            {
                throw new Exception($"Type mismatch: Cannot apply '{setterOperator}' with {value.Type} to {currentValue.Type}.");
            }

            context.RuntimeScopeFrame.Assign(identifier, new RuntimeValue(currentValue.Type, currentValue.Operator, RenewValue(currentValue.Value, value.Value, setterOperator)));
        }

        private static void AssignArrayItem(ArrayAccessSyntaxObject access, RuntimeValue value, SetterOperator setterOperator, RuntimeContext context)
        {
            var target = access.Target.TokenType switch
            {
                TokenType.Identifier => context.RuntimeScopeFrame.Get(access.Target.Value?.ToString() ?? string.Empty),
                TokenType.FunctionCall => context.RuntimeExpressionEvaluator.Evaluate(new List<Token> { access.Target }),
                TokenType.Array => context.RuntimeExpressionEvaluator.Evaluate(new List<Token> { access.Target }),
                _ => throw new Exception("Array assignment target is invalid.")
            };

            if (target.Type != TokenType.Array || target.Value is not RuntimeArrayValue array)
            {
                throw new Exception("Cannot assign through index on a non-array value.");
            }

            var indexValue = context.RuntimeExpressionEvaluator.Evaluate(access.IndexExpression.ToList());
            if (indexValue.Type != TokenType.Number)
            {
                throw new Exception("Array index must evaluate to a number.");
            }

            var index = Convert.ToInt32(indexValue.Value);
            if (index < 0 || index >= array.Count)
            {
                throw new IndexOutOfRangeException($"Array index {index} is out of range.");
            }

            var currentValue = array[index];
            if (currentValue.Type != value.Type && setterOperator != SetterOperator.SETTER)
            {
                throw new Exception($"Type mismatch: Cannot apply '{setterOperator}' with {value.Type} to array element {currentValue.Type}.");
            }

            array[index] = new RuntimeValue(
                setterOperator == SetterOperator.SETTER ? value.Type : currentValue.Type,
                TokenOperation.None,
                RenewValue(currentValue.Value, value.Value, setterOperator));
        }

        private static object RenewValue(object left, object right, SetterOperator setterOperator)
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
