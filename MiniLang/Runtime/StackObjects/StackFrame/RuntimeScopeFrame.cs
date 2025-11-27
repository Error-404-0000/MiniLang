using MiniLang.StructCreation;
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
            var paths = GetStructPath(name);

            if (paths.Count < 2)
            {
                return _locals.Any(x => x.Name == name) || (Parent?.Exists(name) ?? false);
            }
            if (paths.Count > 1 )
            {
                var target = Resolve(paths[0]);
                if(target?.Value?.Value is StructFieldHandler @struct)
                {
                    return @struct.TryGetField(paths[1..]) is not null;
                }
               
            }
            return false;
        }
        public void Assign(string name, RuntimeValue value)
        {
            var paths = GetStructPath(name);
            var target = Resolve(paths.Count > 0 ? paths[0] : throw new Exception($"no value with the name '{name}' was found."));
            if (target == null)
                throw new Exception($"Variable '{name}' not declared.");
            if (paths.Count > 1 && target.Value?.Value is StructFieldHandler @struct)
            {
                var field =   GetStructField(paths[1..], @struct);
                if(value.Type != field.FieldType)
                {
                    throw new Exception($"trying to assign another type of '{value.Type}' but expected '{field.FieldType}' to a field.");
                }
                field.Value = value.Value;
                return;
            }
            else if (paths.Count > 1 && target.Value?.Value is not StructFieldHandler)
            {
                throw new Exception($"Variable '{name}' fields can't be 'get' because it is not  a struct.");
            }
            target.Value = value;
        }

        public RuntimeValue Get(string name)
        {
            var paths = GetStructPath(name);
            var target = Resolve(paths.Count > 0 ? paths[0] : throw new Exception($"no value with the name '{name}' was found."));
            if (target == null)
                throw new Exception($"Variable '{name}' not found.");
            if (paths.Count > 1 && target.Value?.Value is StructFieldHandler @struct)
            {
                return GetStructValue(paths[1..], @struct);
            }
            else if (paths.Count > 1 && target.Value?.Value is not StructFieldHandler)
            {
                throw new Exception($"Variable '{name}' fields can't be 'get' because it is not  a struct.");
            }
            return target.Value!;
        }
        public RuntimeValue GetStructValue(List<string> path, StructFieldHandler structHandler)
        {
            var field = structHandler.GetField(path);
            return new RuntimeValue(field.FieldType, TokenObjects.TokenOperation.None, field.Value);
        }
        public  FieldItem GetStructField(List<string> path, StructFieldHandler structHandler)
        {
            var field = structHandler.GetField(path);
            return   field;
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
