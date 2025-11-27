using MiniLang.TokenObjects;

namespace MiniLang.StructCreation
{
    public class FieldItem
    {
        public readonly string FieldName;
        public readonly TokenType FieldType;
        public object Value;
        public bool IsStruct { get; set; }
        public string TypeName { get; set; }
        public FieldItem(string fieldName, TokenType fieldType, object value, bool IsStruct, string typeName)
        {
            this.FieldName = fieldName;
            this.FieldType = fieldType;
            this.Value = value;
            this.IsStruct = IsStruct;
            TypeName = typeName;
        }
    }
}
