using MiniLang.SyntaxObjects.FunctionBuilder;

namespace MiniLang.Runtime.StackObjects.StackFunctionFrame
{
    public record class RuntimeFunction
    {
        public string Name { get; set; }
        public int ArgCount { get;  }
        public FunctionDeclarationSyntaxObject Declaration { get; set; }

        public RuntimeFunction(string name, int argCount, FunctionDeclarationSyntaxObject declaration)
        {
            Name = name;
            ArgCount = argCount;
            Declaration = declaration;
        }
    }
}
