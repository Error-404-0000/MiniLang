using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MiniLang.StructCreation
{
    public class StructFieldHandler
    {
        private readonly IEnumerable<FieldItem> _fields;
        public IReadOnlyCollection<FieldItem> Fields => _fields.ToList();
        public FieldItem GetField(List<string> fields_path)
        {
            if(fields_path.Count == 0)
            {
                throw new ArgumentException("no field path was given.");
            }
            var tryGetValue = _fields.FirstOrDefault(f => f.FieldName == fields_path[0]);
            if (tryGetValue is null)
                throw new InvalidOperationException($"Field with the name '{fields_path[0]}' was not found.");
            if (fields_path.Count > 1)
            {
                if(tryGetValue.Value is not StructFieldHandler tr)
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
        public object GetFieldValue(List<string> fields_path) =>GetField(fields_path).Value;

        public FieldItem? TryGetField(List<string> fields_path)
        {
            try
            {
                return GetField(fields_path);
            }
            catch { return null; }
        }
        public bool  TryGetFieldValue(List<string> fields_path, out object value)
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
    }
}
