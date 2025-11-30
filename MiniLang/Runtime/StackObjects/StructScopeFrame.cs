using MiniLang.Functions;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFunctionFrame;
using MiniLang.StructCreation;
using MiniLang.SyntaxObjects.FunctionBuilder;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;
using static MiniLang.Functions.FunctionCallTokenObject;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public StructFieldHandler CreateNewStruct(string structName,string varName,RuntimeContext context)
        {
            if (structBluePrints.FirstOrDefault(x => x.StructName == structName) is StructBluePrint structPrint and not null)
            {
                var fresh_struct = structPrint.WhenCreated();
                foreach (var field in fresh_struct.Fields.Where(x => x.IsStruct))
                {
                    field.Value = CreateNewStruct(field.TypeName, varName+'.'+field.FieldName, context);
                    //if(field.IsStruct && field.Value is StructFieldHandler structFieldHandler && structFieldHandler.ToStringFunctionName != null)
                    //{
                    //    context.FunctionTable.Declare(
                    //        //allows myStruct.<fieldname>.ToString()
                    //       BuildToStringProxy($"{varName}.{field.FieldName}", structFieldHandler.ToStringFunctionName +"ToString", fresh_struct.onFunctionOpen(fresh_struct.Fields)));
                    //}
                }
                foreach(var function in fresh_struct.Functions)
                {
                    //make x = new user;
                    function.Name =varName +'.'+ function.Name; // x.FunctionName
                    if (fresh_struct?.onFunctionOpen is not null)
                    {
                        function.Declaration.FunctionName = function.Name;
                        function.Declaration.OnFunctionOpened = fresh_struct.onFunctionOpen(fresh_struct.Fields);//init the local struct fields

                    }

                    context.FunctionTable.Declare(function);
                }
                //creating default ToString
                if(fresh_struct.ToStringFunctionName is not null)
                {
                    //mystruct.StructNameToString()
                    var redirctToMethod = $"{varName}.{structPrint.StructName}ToString";
                    if (fresh_struct.onFunctionOpen is not null)
                    {
                        context.FunctionTable.Declare(
                            BuildToStringProxy(varName, redirctToMethod, fresh_struct.onFunctionOpen(fresh_struct.Fields)));

                    }
                    else context.FunctionTable.Declare(BuildToStringProxy(varName, redirctToMethod, null));

                }
                return fresh_struct;
            }
            else throw new Exception($"No struct with the name '{structName}' was found in the current frame.");
        }

        public static RuntimeFunction BuildToStringProxy(string FunctionName,string ToStringFunctionNameToCall, OnFunctionOpen OnFunctionOpened)
        {
            FunctionDeclarationSyntaxObject func = null;
            var proxy = new Token(TokenType.NewFunction,
                TokenOperation.None,
                TokenTree.Single,
                func = new FunctionDeclarationSyntaxObject(
                    FunctionName,
                    0,
                    TokenOperation.ReturnsString,
                    functionArgments: [],
                    Body: [
                        new Token(
                            TokenType.FunctionCall,
                            TokenOperation.None,
                            TokenTree.Single, //Call ToString
                            new FunctionCallTokenObject(
                                ToStringFunctionNameToCall,
                                0,
                                functionArgments:[],
                                OnFunctionOpened,
                                null)
                            )
                    ],
                    OnFunctionOpened,
                    null)
                );

            return new RuntimeFunction(FunctionName, 0, func); ;
        }
    }
}

