using MiniLang.StructCreation;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Runtime.StackObjects
{
    public record StructBluePrint(string StructName, Func<StructFieldHandler> WhenCreated);
    public class RuntimeStructScopeFrame
    {
        public RuntimeStructScopeFrame Parent { set; get; }
        public List<StructBluePrint> structBluePrints = new List<StructBluePrint>();
        public RuntimeStructScopeFrame()
        {
            structBluePrints = new();
        }
        public void DeclearStruct(string structName, Func<StructFieldHandler> OnCreation)
        {
            if(structBluePrints.Any(x=> x.StructName == structName))
            {
                throw new Exception($"Struct with the name '{structName}' already exist.");
            }
            structBluePrints.Add(new(structName, OnCreation));
        }
        public StructFieldHandler CreateNewStruct(string structName)
        {
            if (structBluePrints.FirstOrDefault(x => x.StructName == structName) is StructBluePrint structPrint)
            {
                var fresh_struct = structPrint.WhenCreated();
                foreach (var item in fresh_struct.Fields.Where(x => x.IsStruct))
                {
                    item.Value = CreateNewStruct(item.TypeName);
                }

                return fresh_struct;
            }
            else throw new Exception($"No struct with the name '{structName}' was found in the current frame.");
        }

    }
}
