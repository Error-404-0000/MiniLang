using MiniLang.Runtime.Execution;
using MiniLang.Runtime.Executor;
using MiniLang.Runtime.Expression;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.Runtime.StackObjects.StackFunctionFrame;
using MiniLang.TokenObjects;

namespace MiniLang.Runtime.RuntimeObjectStack
{
    public record class RuntimeContext
    {
        public RuntimeScopeFrame RuntimeScopeFrame { get; set; }
        public RuntimeFunctionTable FunctionTable { get; set; }
        public ReturnObject ReturnValueHolder { get; set; }
        public ExecutableTokenDispatcher ExecutableTokenDispatcher { get; }
        public RuntimeEngine RuntimeEngine { get; }
        public RuntimeExpressionEvaluator RuntimeExpressionEvaluator { get; set; }
        public RuntimeContext(ExecutableTokenDispatcher dispatcher)
        {
            RuntimeExpressionEvaluator = new(this);
            ExecutableTokenDispatcher = dispatcher;
            RuntimeEngine = new RuntimeEngine(dispatcher, this);
            ReturnValueHolder = new ReturnObject(TokenType.ReturnType, TokenOperation.ReturnsNothing, null);
        }

        public void PushScope()
        {
            var child = new RuntimeScopeFrame { Parent = RuntimeScopeFrame };
            RuntimeScopeFrame = child;
        }

        public void PopScope()
        {
            if (RuntimeScopeFrame.Parent == null)
                throw new InvalidOperationException("Cannot pop the global scope.");
            RuntimeScopeFrame = RuntimeScopeFrame.Parent;
        }

        public void PushFunctionTable()
        {
            var child = new RuntimeFunctionTable { Parent = FunctionTable };
            FunctionTable = child;
        }

        public void PopFunctionTable()
        {
            if (FunctionTable.Parent == null)
                throw new InvalidOperationException("Cannot pop the global function table.");
            FunctionTable = FunctionTable.Parent;
        }
        public void ReturnedHandled()
        {
            ReturnValueHolder = null;
        }
        public void NewScope()
        {
            PushFunctionTable();
            PushScope();
        }
        public void EndScope()
        {
            PopScope();
            PopFunctionTable();
            ReturnedHandled();
        }
        public void SetReturn(TokenType returnType, TokenOperation returnOperator, object? value)
        {
            ReturnValueHolder = new ReturnObject(returnType, returnOperator, value);
        }

    }

    public record ReturnObject(TokenType returnType, TokenOperation returnOperator, object? value);
}
