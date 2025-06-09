using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.StackObjects.StackFrame
{
    public class RuntimeScopeFrame
    {
        private readonly List<RuntimeVariable> _locals = new();

        public RuntimeScopeFrame? Parent { get; set; }

        public void Declare(RuntimeVariable variable)
        {
            if (_locals.Exists(x => x.Name == variable.Name))
                throw new InvalidOperationException($"Variable '{variable.Name}' is already declared in this scope.");

            _locals.Add(variable);
        }
        public bool Exists(string name)
        {
            return _locals.Any(x => x.Name == name) ||(Parent?.Exists(name) ??false);
        }
        public void Assign(string name, RuntimeValue value)
        {
            var target = Resolve(name);
            if (target == null)
                throw new Exception($"Variable '{name}' not declared.");

            target.Value = value;
        }

        public RuntimeValue Get(string name)
        {
            var target = Resolve(name);
            if (target == null)
                throw new Exception($"Variable '{name}' not found.");

            return target.Value!;
        }

        private RuntimeVariable? Resolve(string name)
        {
            var frame = this;
            while (frame != null)
            {
                var match = frame._locals.FirstOrDefault(v => v.Name == name);
                if (match != null)
                    return match;

                frame = frame.Parent;
            }
            return null;
        }
    }
}
