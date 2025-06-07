using MiniLang.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Interpreter.GrammerdummyScopes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace MiniLang.Functions
    {
        /// <summary>
        /// Manages function declarations within a given scope, with optional parent scope access.
        /// </summary>
        public class FunctionDeclarationManager
        {
            private readonly List<FunctionTokenObject> _scopeFunctions = new();

            /// <summary>
            /// Optional reference to the parent function scope, used for hierarchical lookup.
            /// </summary>
            public FunctionTokenObject? ParentScope { get; set; }

            /// <summary>
            /// Adds a new function declaration to the current scope.
            /// </summary>
            /// <param name="function">The function token object to add.</param>
            /// <exception cref="Exception">Thrown if a function with the same name already exists in this scope.</exception>
            public void Add(FunctionTokenObject function)
            {
                if (_scopeFunctions.Any(f => f.FunctionName == function.FunctionName))
                    throw new Exception($"Function '{function.FunctionName}' is already declared in this scope.");

                _scopeFunctions.Add(function);
            }

            /// <summary>
            /// Checks if a function is declared in the current scope or any parent scope.
            /// </summary>
            public bool Exists(string functionName) =>
                _scopeFunctions.Any(f => f.FunctionName == functionName) ||
                (ParentScope != null && ParentScope.FunctionName == functionName);

            /// <summary>
            /// Retrieves a function by name from the current or parent scope.
            /// </summary>
            /// <param name="functionName">The name of the function to look up.</param>
            /// <returns>The corresponding FunctionTokenObject.</returns>
            /// <exception cref="Exception">Thrown if the function is not found in any scope.</exception>
            public FunctionTokenObject Get(string functionName)
            {
                var func = _scopeFunctions.FirstOrDefault(f => f.FunctionName == functionName);

                if (func != null)
                    return func;

                if (ParentScope != null && ParentScope.FunctionName == functionName)
                    return ParentScope;

                throw new Exception($"Function '{functionName}' is not declared in any accessible scope.");
            }

            /// <summary>
            /// Clears all functions from this scope. Parent is not affected.
            /// </summary>
            public void Clear() => _scopeFunctions.Clear();
        }
    }

}
