using MiniLang.Functions;
using MiniLang.GrammarsAnalyers;
using MiniLang.Runtime.RuntimeExecutors.Builtins;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.Runtime.StackObjects.StackFunctionFrame;
using MiniLang.SyntaxObjects.FunctionBuilder;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using static MiniLang.Functions.FunctionCallTokenObject;

namespace MiniLang.StructCreation
{

    public class StructFieldHandler
    {
        public static FunctionCallExecution functionCallExecution = new FunctionCallExecution();
        private readonly IEnumerable<FieldItem> _fields;
        public IReadOnlyCollection<FieldItem> Fields => _fields.ToList();
        public IEnumerable<RuntimeFunction> Functions = new List<RuntimeFunction>();
        public string? ToStringFunctionName = null;
        public Func<IEnumerable<FieldItem>,OnFunctionOpen>? onFunctionOpen { get; set; }

        public FieldItem GetField(List<string> fields_path)
        {
            if (fields_path.Count == 0)
            {
                throw new ArgumentException("no field path was given.");
            }
            var tryGetValue = _fields.FirstOrDefault(f => f.FieldName == fields_path[0]);
            if (tryGetValue is null)
                throw new InvalidOperationException($"Field with the name '{fields_path[0]}' was not found.");
            if (fields_path.Count > 1)
            {
                if (tryGetValue.Value is not StructFieldHandler tr)
                {
                    throw new ArgumentException("trying to access a field path when the type is not a struct.");
                }
                tryGetValue = tr.GetField(fields_path[1..]);
            }
           
            return tryGetValue;
        }
        public StructFieldHandler(IEnumerable<FieldItem> fields_path)
        {
            this._fields = fields_path;
        }
        public object GetFieldValue(List<string> fields_path) => GetField(fields_path).Value;

        public FieldItem? TryGetField(List<string> fields_path)
        {
            try
            {
                return GetField(fields_path);
            }
            catch { return null; }
        }
        public bool TryGetFieldValue(List<string> fields_path, out object value)
        {
            value = null;
            try
            {
                value = GetFieldValue(fields_path);
                return true;
            }
            catch { return false; }
        }
        public FieldItem SetValue(List<string> fields_path, object value)
        {
            if (TryGetField(fields_path) is FieldItem fieldItem)
            {
                fieldItem.Value = value;
                return fieldItem;
            }
            else throw new InvalidCastException("no field named '{fieldItem}' was found.");
        }


        public RuntimeValue CallMethod(FunctionCallTokenObject functioncall,RuntimeContext context)
        {
            var getFunction = Functions.FirstOrDefault(x => x.Name == functioncall.FunctionName && x.ArgCount == functioncall.FunctionArgmentsCount);
            if (getFunction is null)
            {
                throw new InvalidOperationException("no function with the name '{function.FunctionName}' and argments count was found.");
            }
            context.PushFunctionTable();
            context.FunctionTable.Declare(getFunction);
            var result =  functionCallExecution.Dispatch(Token.DefaultToken() with { Value = functioncall }, context);
            context.PopFunctionTable();
            return result;

        }
    }
}
