using MiniLang.Interfaces;
using MiniLang.Runtime.Collections;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.SyntaxObjects.Collections;
using MiniLang.TokenObjects;

namespace MiniLang.Runtime.RuntimeExecutors.Builtins
{
    public class ForeachExecutable : IExecutableToken
    {
        public TokenType[] InvokeType => [TokenType.Conditions];

        public TokenOperation[] InvokeOperation => [TokenOperation.Foreach];

        public RuntimeValue Dispatch(Token token, RuntimeContext context)
        {
            if (token.Value is not ForeachSyntaxObject foreachSyntax)
            {
                throw new Exception("runtime error: expected ForeachSyntaxObject.");
            }

            var collection = context.RuntimeExpressionEvaluator.Evaluate(foreachSyntax.CollectionExpression.ToList());
            if (collection.Type != TokenType.Array || collection.Value is not RuntimeArrayValue array)
            {
                throw new Exception("Cannot foreach over a non-array value.");
            }

            foreach (var item in array.Snapshot().Items)
            {
                context.NewScope();
                context.RuntimeScopeFrame.Declare(new RuntimeVariable(foreachSyntax.Identifier, item.Type, item));
                var result = context.RuntimeEngine.Execute(foreachSyntax.Scope.ToList());
                if (context.ReturnValueHolder != null && context.ReturnValueHolder.returnOperator != TokenOperation.ReturnsNothing)
                {
                    context.EndScope();
                    return result;
                }

                context.EndScope();
            }

            context.ReturnedHandled();
            return null;
        }
    }
}
