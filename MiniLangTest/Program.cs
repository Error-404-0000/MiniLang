
#define DebugMode1
using MiniLang.Debugger;
using MiniLang.GrammarsAnalyers;
using MiniLang.Interfaces;
using MiniLang.GrammarInterpreter;
using MiniLang.Parser;
using MiniLang.SyntaxObjects;
using MiniLang.Tokenilzer;
using MiniLang.TokenObjects;
using MiniLang.Runtime.Execution;
using MiniLang.Runtime.Executor;
using MiniLang.Runtime.RuntimeExecutors.Singles;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.RuntimeExecutors.Builtins;
using MiniLang.GrammarsAnalyers.StructDeclaration;
using MiniLang.Runtime.RuntimeExecutors.Builtins.Struct;

class MiniLangRuntime
{
    public static void Main(string[] args)
    {
        args = [null];
        args[0] = @"C:\Users\Demon\source\repos\MiniLang\MiniLangGuide\MiniLang_Syntax_Guide\ClassCreation.mini.c";
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: MiniLangRuntime <script-file>");
            return;
        }

        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Script file not found: {filePath}");
            return;
        }

        string sourceCode = File.ReadAllText(filePath);
        string cleanedCode = MiniLangPreprocessor.RemoveCommentLines(sourceCode);
        try
        {
            RunScript(cleanedCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Runtime Error]: {ex.Message}");
        }
    }

  
    public static void RunScript(string code)
    {
        var tokens = Tokenizer.Tokenize(code);

        var parsedTokens = Parser.Parse(tokens);

#if DebugMode
        foreach (var token in parsedTokens)
        {
            token.Print();
        }
#endif


#if !DebugMode

        var grammarValidator = new GrammarValidator([
            new MakeGrammar(), new ConditionGrammar(), new SayGrammar(), new TypeofGrammar(), new UseGrammar(),
            new SetterGrammar(), new FunctionDeclarationGrammar(), new FunctionCallsGrammar(),
            new StandaloneExpressionGrammar(), new ScopeGrammar(), new GiveGrammar(), new WhileGrammar(),
            new StructGrammer(), new FieldDeclarationGrammer(),new CSharpGrammer(),
        ]);
        // Interpret Grammar
        var grammarInterpreter = new GrammarInterpreter(grammarValidator, parsedTokens);
        var interpreted = grammarInterpreter.Interpret();
        interpreted = grammarInterpreter.InjectUse(interpreted);
       

        // Dispatcher setup
        var dispatcher = new ExecutableTokenDispatcher([
            new NumberLiteralExecutable(),
            new StringInterpolatedExecutable(),
            new MakeExecutable(),
            new SayExecutable(),
            new ScopeExecutable(),
            new FunctionCallExecution(),
            new FunctionBuilderExecuteable(),
            new GiveExacuteable(),
            new ConditionExecuteable(),
            new WhileExecuteable(),
            new SetterExecutable(),
            new StructExecteable(),
            
        ]);

        //  Context
        var context = new RuntimeContext(dispatcher);
        context.PushScope();
        context.PushFunctionTable();
        context.PushStructTable();
        context.RuntimeScopeFrame.Declare(new MiniLang.Runtime.StackObjects.StackFrame.RuntimeVariable(
            "true", TokenType.Number, new MiniLang.Runtime.StackObjects.StackFrame.RuntimeValue(TokenType.Number, TokenOperation.None, 1)
        ));

        context.RuntimeScopeFrame.Declare(new MiniLang.Runtime.StackObjects.StackFrame.RuntimeVariable(
            "false", TokenType.Number, new MiniLang.Runtime.StackObjects.StackFrame.RuntimeValue(TokenType.Number, TokenOperation.None, 0)
        ));

        var runtime = new RuntimeEngine(dispatcher, context);
        runtime.Execute(interpreted.ToList());
#endif
    }
}
