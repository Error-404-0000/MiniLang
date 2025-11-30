using MiniLang.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarInterpreter.GrammerdummyScopes
{
    using global::MiniLang.TokenObjects;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace MiniLang.Functions
    {
        /// <summary>
        /// Manages function declarations within a given scope, with optional parent scope access.
        /// </summary>
        public class FunctionDeclarationScopeManager:IDisposable
        {
            private  List<FunctionCallTokenObject> _scopeFunctions = new();

            /// <summary>
            /// Optional reference to the parent function scope, used for hierarchical lookup.
            /// </summary>
            public FunctionDeclarationScopeManager? ParentScope { get; set; }
            private List<FunctionCallTokenObject> _StructFunctions=new();

            private bool _isInStruct;


            public void StructIn()=>_isInStruct = true;
            public void StructOut()
            {
                foreach (var structItem in _StructFunctions)
                    _scopeFunctions.Remove(structItem);
                _StructFunctions.Clear();
                _isInStruct = false;
            }

            /// <summary>
            /// Adds a new function declaration to the current scope.
            /// </summary>
            /// <param name="function">The function token object to add.</param>
            /// <exception cref="Exception">Thrown if a function with the same name already exists in this scope.</exception>
            public void Add(FunctionCallTokenObject function)
            {
                
                if (_scopeFunctions.Any(f => f.FunctionName == function.FunctionName&&f.FunctionArgmentsCount== function.FunctionArgmentsCount))
                    throw new Exception($"Function '{function.FunctionName}' is already declared in this scope.");
                if (_isInStruct)
                    _StructFunctions.Add(function);


                _scopeFunctions.Add(function);
            }
            public void Remove(FunctionCallTokenObject function)
            {
                if (!_scopeFunctions.Any(f => f.FunctionName == function.FunctionName && f.FunctionArgmentsCount == function.FunctionArgmentsCount))
                    throw new Exception($"Function '{function.FunctionName}' is already declared in this scope.");

                _scopeFunctions.Remove(function);
            }
            /// <summary>
            /// Checks if a function is declared in the current scope or any parent scope.
            /// </summary>
            public bool Exists(string functionName,int functionCount) =>
                _scopeFunctions.Any(f => f.FunctionName == functionName&& f.FunctionArgmentsCount == functionCount) ||
                (ParentScope != null && ParentScope._scopeFunctions.Any(x=>x.FunctionName == functionName  &&x.FunctionArgmentsCount == functionCount));
            //public TokenType? GetReturnType(string functionName, int functionCount)
            //{
            //    var ReturnType = _scopeFunctions.FirstOrDefault(f => f.FunctionName == functionName && f.FunctionArgmentsCount == functionCount);
            //    if(ReturnType == null)
            //    {
            //        var ParentReturnType  = ParentScope?.GetReturnType(functionName, functionCount);
            //        if(ParentReturnType is null)
            //        {
            //            throw new Exception($"Function '{functionName}' is not declared in any accessible scope with the argment counts.");
            //        }
            //        return ParentReturnType;
            //    }
            //    return ReturnType.;
            //}
            /// <summary>
            /// Retrieves a function by name from the current or parent scope.
            /// </summary>
            /// <param name="functionName">The name of the function to look up.</param>
            /// <returns>The corresponding FunctionTokenObject.</returns>
            /// <exception cref="Exception">Thrown if the function is not found in any scope.</exception>
            public FunctionCallTokenObject Get(string functionName,int FunctionCount)
            {
                var path = GetStructPath(functionName);
                
                var func = _scopeFunctions.FirstOrDefault(f => f.FunctionName == (path.Count>0?path[^1]:functionName) && f.FunctionArgmentsCount == FunctionCount);

                if (func != null)
                    return func;
                var first = ParentScope?.Get(functionName, FunctionCount);
                if (first != null )
                    return first;

                throw new Exception($"Function '{functionName}' is not declared in any accessible scope with the argment counts.");
            }
            private List<string> GetStructPath(string name_dir)
            {
                List<string> path = new List<string>();


                StringBuilder currentPath = new StringBuilder();
                foreach (char currentChar in name_dir)
                {
                    if (currentChar is '.')
                    {
                        path.Add(currentPath.ToString());
                        currentPath.Length = 0;
                    }
                    else
                    {
                        currentPath.Append(currentChar);
                    }
                }
                if (currentPath.Length > 0)
                    path.Add(currentPath.ToString());

                return path;

            }

            /// <summary>
            /// Clears all functions from this scope. Parent is not affected.
            /// </summary>
            public void Clear() => _scopeFunctions.Clear();

            public void Dispose()
            {
                _scopeFunctions = null;
            }
        }
    }

}
