using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.StackObjects.StackFunctionFrame
{
    public class RuntimeFunctionTable
    {
        private readonly List<RuntimeFunction> _functions = new();
        public RuntimeFunctionTable? Parent { get; set; }

        public void Declare(RuntimeFunction function)
        {
            if (_functions.Any(f => f.Name == function.Name && f.ArgCount == function.ArgCount))
                throw new InvalidOperationException($"Function '{function.Name}' with {function.ArgCount} args already declared.");

            _functions.Add(function);
        }

        public RuntimeFunction Resolve(string name, int argCount)
        {
            var current = this;
            while (current != null)
            {
                var match = current._functions.FirstOrDefault(f => f.Name == name && f.ArgCount == argCount);
                if (match != null)
                    return match;

                current = current.Parent;
            }

            throw new InvalidOperationException($"Function '{name}' with {argCount} args not found.");
        }
    }
}
