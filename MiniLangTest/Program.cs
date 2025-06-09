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
using MiniLang.Interpreter.RuntimeExecutors.Builtins;

class MiniLangRuntime
{
    public static void Main(string[] args)
    {
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
        // Tokenize
        var tokens = Tokenizer.Tokenize(code);

        // Parse
        var parsedTokens = Parser.Parse(tokens);

        // Build grammar validators
        var grammarValidator = new GrammarValidator([
            new MakeGrammar(), new ConditionGrammar(), new SayGrammar(), new TypeofGrammar(), new UseGrammar(),
            new SetterGrammar(), new FunctionDeclarationGrammar(), new FunctionCallsGrammar(),
            new StandaloneExpressionGrammar(), new ScopeGrammar(), new GiveGrammar(), new WhileGrammar()
        ]);

        // Interpret Grammar <- this does not execute the actual code, it builds an AST-like structure and validates  by building a dummy scope trees
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
            new SetterExecutable()
        ]);

        //  Context
        var context = new RuntimeContext(dispatcher);
        context.PushScope();
        context.PushFunctionTable();

        // 7. Load default variables
        context.RuntimeScopeFrame.Declare(new MiniLang.Runtime.StackObjects.StackFrame.RuntimeVariable(
            "true", TokenType.Number, new MiniLang.Runtime.StackObjects.StackFrame.RuntimeValue(TokenType.Number, TokenOperation.None, 1)
        ));

        context.RuntimeScopeFrame.Declare(new MiniLang.Runtime.StackObjects.StackFrame.RuntimeVariable(
            "false", TokenType.Number, new MiniLang.Runtime.StackObjects.StackFrame.RuntimeValue(TokenType.Number, TokenOperation.None, 0)
        ));

        // 8. Execute
        var runtime = new RuntimeEngine(dispatcher, context);
        runtime.Execute(interpreted.ToList());
    }
}
