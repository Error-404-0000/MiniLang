using MiniLang.SyntaxObjects.FunctionBuilder;

namespace MiniLang.Runtime.StackObjects.StackFunctionFrame
{
    public class RuntimeFunction
    {
        public string Name { get; }
        public int ArgCount { get; }
        public FunctionDeclarationSyntaxObject Declaration { get; }

        public RuntimeFunction(string name, int argCount, FunctionDeclarationSyntaxObject declaration)
        {
            Name = name;
            ArgCount = argCount;
            Declaration = declaration;
        }
    }
}
