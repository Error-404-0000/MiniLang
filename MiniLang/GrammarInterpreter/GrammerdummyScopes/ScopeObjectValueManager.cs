using MiniLang.GrammarInterpreter.GrammerdummyScopes;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniLang.GrammarInterpreter.GrammarDummyScopes
{
    public class ScopeObjectValueManager : IDisposable
    {
        private List<ScopeObjectValue> _scopes = new();
        private List<ScopeObjectValue> _structScopeRef = new();
        private bool _is_in_struct = false;

        /// <summary>
        /// The parent scope — walk up this chain to access outer-scope values.
        /// </summary>
        public ScopeObjectValueManager? Parent { get; set; }

        public void IsInStruct() => _is_in_struct = true;
        //struct scope out
        public void StructOut()
        {
            for (int i = 0; i < _structScopeRef.Count(); i++)
            {
                _scopes.Remove(_structScopeRef[i]);
            }

            _is_in_struct = false;
        }
        public void Add(ScopeObjectValue value)
        {
            //this fixes the Name->TypeName if they are the same
            var ObjectName = (_is_in_struct?$"_field_":null)+value.Identifier;
            if (_scopes.Any(x => x.Identifier == ObjectName ))
                throw new Exception($"Identifier '{value.Identifier}' is already declared in this scope.");
            _scopes.Add(value);
            if (_is_in_struct)
            {
                _structScopeRef.Add(value);
            }
        }
        public TokenType GetTypeOf(string identifier)
        {
            if (FindScopeWith(identifier) is ScopeObjectValue scopeObjectValue) return scopeObjectValue.TokenType;
            throw new Exception($"Cannot find typeof '{identifier}' — it was not declared.");

        }
        public void MarkAssigned(string identifier)
        {
            var scope = FindScopeWith(identifier);

            if (scope == null)
                throw new Exception($"Cannot assign to '{identifier}' — it was not declared in any scope.");

            scope.IsAssigned = true;
        }

        public bool Exists(string identifier) => FindScopeWith(identifier) != null;

        public bool IsAssigned(string identifier)
        {
            identifier = (_is_in_struct ? $"_field_" : null)+ identifier;
            var p = GetStructPath(identifier);
            var scope = FindScopeWith(p.Count>0 ?p[0]:identifier);

            if (scope == null)
                throw new Exception($"Cannot check assignment of '{identifier}' — it was not declared.");

            return scope.IsAssigned;
        }

        private ScopeObjectValue? FindScopeWith(string identifier)
        {
            if (_is_in_struct)
            {
                var IfFieldName = (_is_in_struct ? $"_field_" : null) + identifier;
                var _field_path = GetStructPath(IfFieldName);

                var match_field_path = _scopes.FirstOrDefault(x => x.Identifier == (_field_path.Count > 0 ? _field_path[0] : identifier));
                if (match_field_path != null)
                    return match_field_path;
            }

            var p = GetStructPath(identifier);

            var match = _scopes.FirstOrDefault(x => x.Identifier == (p.Count > 0 ? p[0] : identifier));
            if (match != null)
                return match;


            return Parent?.FindScopeWith(identifier);
        }

        public void Clear()
        {
            _scopes.Clear();
            // Don't clear parent — just this instance
        }

        public void Dispose()
        {
            _scopes = null;
        }
        private static List<string> GetStructPath(string name_dir)
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
    }
}
