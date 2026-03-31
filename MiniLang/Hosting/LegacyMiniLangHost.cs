using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using MiniLang.Collections;
using MiniLang.Functions;
using MiniLang.GrammarAnalyzers;
using MiniLang.GrammarsAnalyers;
using MiniLang.GrammarsAnalyers.StructDeclaration;
using MiniLang.Interfaces;
using MiniLang.Runtime.Execution;
using MiniLang.Runtime.Executor;
using MiniLang.Runtime.RuntimeExecutors.Builtins;
using MiniLang.Runtime.RuntimeExecutors.Builtins.Struct;
using MiniLang.Runtime.RuntimeExecutors.Singles;
using MiniLang.Runtime.RuntimeObjectStack;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.SyntaxObjects.Csharp;
using MiniLang.SyntaxObjects.Enum;
using MiniLang.SyntaxObjects.FunctionBuilder;
using MiniLang.SyntaxObjects.Structure;
using MiniLang.Tokenilzer;
using MiniLang.TokenObjects;

namespace MiniLang.Hosting;

public static class LegacyMiniLangHost
{
    public static LegacyAnalysisResult AnalyzeSource(string code)
        => AnalyzeSource(code, filePath: null);

    public static LegacyAnalysisResult AnalyzeSource(string code, string? filePath)
    {
        try
        {
            using var _ = UsePathContext.Push(filePath);
            var cleanedCode = RemoveCommentLines(code);
            var tokens = Tokenizer.Tokenize(cleanedCode);
            var parsedTokens = MiniLang.Parser.Parser.Parse(tokens);
            var validator = CreateValidator();
            var interpreter = new MiniLang.GrammarInterpreter.GrammarInterpreter(validator, parsedTokens);
            var interpreted = interpreter.InjectUse(interpreter.Interpret()).ToList();

            return new LegacyAnalysisResult(
                true,
                RenderTokenTree(parsedTokens),
                RenderSymbols(interpreted),
                RenderTokenTree(interpreted),
                [],
                BuildOutline(cleanedCode, interpreted),
                BuildCompletions(interpreted));
        }
        catch (Exception ex)
        {
            return new LegacyAnalysisResult(
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                [CreateDiagnostic("ML0001", "Error", ex, code)],
                [],
                BuildKeywordCompletions());
        }
    }

    public static LegacyRunResult RunSource(string code)
        => RunSource(code, filePath: null);

    public static LegacyRunResult RunSource(string code, string? filePath)
    {
        try
        {
            using var _ = UsePathContext.Push(filePath);
            var cleanedCode = RemoveCommentLines(code);
            var tokens = Tokenizer.Tokenize(cleanedCode);
            var parsedTokens = MiniLang.Parser.Parser.Parse(tokens);
            var validator = CreateValidator();
            var interpreter = new MiniLang.GrammarInterpreter.GrammarInterpreter(validator, parsedTokens);
            var interpreted = interpreter.InjectUse(interpreter.Interpret()).ToList();
            var dispatcher = CreateDispatcher();
            var context = CreateRuntimeContext(dispatcher);

            using var writer = new StringWriter();
            var previousOut = Console.Out;
            try
            {
                Console.SetOut(writer);
                context.RuntimeEngine.Execute(interpreted);
                InvokeEntryPoint(context);
            }
            finally
            {
                Console.SetOut(previousOut);
            }

            return new LegacyRunResult(true, writer.ToString(), []);
        }
        catch (Exception ex)
        {
            return new LegacyRunResult(false, string.Empty, [CreateDiagnostic("ML0002", "Error", ex, code)]);
        }
    }

    private static MiniLang.GrammarInterpreter.GrammarValidator CreateValidator() =>
        new([
            new MakeGrammar(),
            new ConditionGrammar(),
            new SayGrammar(),
            new TypeofGrammar(),
            new UseGrammar(),
            new SetterGrammar(),
            new FunctionDeclarationGrammar(),
            new FunctionCallsGrammar(),
            new StandaloneExpressionGrammar(),
            new ScopeGrammar(),
            new GiveGrammar(),
            new WhileGrammar(),
            new ForeachGrammar(),
            new StructGrammer(),
            new FieldDeclarationGrammer(),
            new EnumGrammar(),
            new CSharpGrammer(),
            new ShortenOperatorGrammar()
        ]);

    private static ExecutableTokenDispatcher CreateDispatcher() =>
        new([
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
            new ForeachExecutable(),
            new SetterExecutable(),
            new StructExecteable(),
            new EnumExecutable(),
            new CSharpExecutable(),
            new StandaloneExecteable()
        ]);

    private static RuntimeContext CreateRuntimeContext(ExecutableTokenDispatcher dispatcher)
    {
        var context = new RuntimeContext(dispatcher);
        context.PushScope();
        context.PushFunctionTable();
        context.PushStructTable();
        context.PushEnumTable();
        context.RuntimeScopeFrame.Declare(new RuntimeVariable(
            "true",
            TokenType.Number,
            new RuntimeValue(TokenType.Number, TokenOperation.None, 1d)));
        context.RuntimeScopeFrame.Declare(new RuntimeVariable(
            "false",
            TokenType.Number,
            new RuntimeValue(TokenType.Number, TokenOperation.None, 0d)));
        return context;
    }

    private static void InvokeEntryPoint(RuntimeContext context)
    {
        try
        {
            context.FunctionTable.Resolve("Main", 0);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        var entryCall = new Token(
            TokenType.FunctionCall,
            TokenOperation.None,
            TokenTree.Single,
            new FunctionCallTokenObject("Main", 0, []));

        context.RuntimeEngine.Execute([entryCall]);
    }

    private static string RenderSymbols(IEnumerable<Token> interpreted)
    {
        var builder = new StringBuilder();
        foreach (var token in interpreted)
        {
            switch (token.Value)
            {
                case FunctionDeclarationSyntaxObject function:
                    builder.AppendLine($"fn {function.FunctionName}/{function.FunctionArgmentsCount} -> {function.DeclaredTypeName ?? function.ReturnType.ToString()}");
                    break;
                case StructSyntaxObject @struct:
                    builder.AppendLine($"struct {@struct.StructName}");
                    break;
                case EnumSyntaxObject @enum:
                    builder.AppendLine($"enum {@enum.EnumName}: {string.Join(", ", @enum.Members.Select(static x => x.Name))}");
                    break;
                case CSharpCallSyntaxObject csharpCall:
                    builder.AppendLine($"interop {csharpCall.NameSpace}.{csharpCall.FunctionCall.FunctionName}/{csharpCall.FunctionCall.FunctionArgmentsCount}");
                    break;
            }
        }

        return builder.ToString();
    }

    private static string RenderTokenTree(IEnumerable<Token> tokens)
    {
        var builder = new StringBuilder();
        foreach (var token in tokens)
        {
            AppendToken(builder, token, 0);
        }

        return builder.ToString();
    }

    private static void AppendToken(StringBuilder builder, Token token, int indent)
    {
        var prefix = new string(' ', indent * 2);
        builder.Append(prefix);
        builder.Append(token.TokenType);
        builder.Append(" <");
        builder.Append(token.TokenOperation);
        builder.Append("> ");
        builder.AppendLine(token.Value?.ToString() ?? "null");

        if (token.Value is IEnumerable<Token> nested)
        {
            foreach (var child in nested)
            {
                AppendToken(builder, child, indent + 1);
            }
        }
    }

    private static IReadOnlyList<LegacyOutlineItem> BuildOutline(string code, IEnumerable<Token> interpreted)
    {
        var items = new List<LegacyOutlineItem>();
        foreach (var token in interpreted)
        {
            switch (token.Value)
            {
                case FunctionDeclarationSyntaxObject function:
                    items.Add(new LegacyOutlineItem("function", function.FunctionName, FindOffset(code, function.FunctionName)));
                    break;
                case StructSyntaxObject @struct:
                    items.Add(new LegacyOutlineItem("struct", @struct.StructName, FindOffset(code, @struct.StructName)));
                    break;
                case EnumSyntaxObject @enum:
                    items.Add(new LegacyOutlineItem("enum", @enum.EnumName, FindOffset(code, @enum.EnumName)));
                    break;
            }
        }

        return items;
    }

    private static IReadOnlyList<LegacyCompletionItem> BuildCompletions(IEnumerable<Token> interpreted)
    {
        var items = new Dictionary<string, LegacyCompletionItem>(StringComparer.Ordinal)
        {
            ["fn"] = new("fn", "keyword"),
            ["struct"] = new("struct", "keyword"),
            ["enum"] = new("enum", "keyword"),
            ["array"] = new("array", "type"),
            ["make"] = new("make", "keyword"),
            ["give"] = new("give", "keyword"),
            ["say"] = new("say", "keyword"),
            ["use"] = new("use", "keyword"),
            ["if"] = new("if", "keyword"),
            ["else"] = new("else", "keyword"),
            ["while"] = new("while", "keyword"),
            ["foreach"] = new("foreach", "keyword"),
            ["in"] = new("in", "keyword"),
            ["win"] = new("win", "interop"),
            ["cscall"] = new("cscall", "interop")
        };

        foreach (var builtin in CollectionBuiltins.All)
        {
            items[builtin.Name] = new LegacyCompletionItem(builtin.Name, builtin.Detail);
        }

        foreach (var ns in Interop.InteropBridgeRegistry.GetNamespaces())
        {
            items[ns] = new LegacyCompletionItem(ns, "interop namespace");
            foreach (var function in Interop.InteropBridgeRegistry.GetFunctions(ns))
            {
                items[function] = new LegacyCompletionItem(function, $"interop function in {ns}");
            }
        }

        foreach (var token in interpreted)
        {
            switch (token.Value)
            {
                case FunctionDeclarationSyntaxObject function:
                    items[function.FunctionName] = new LegacyCompletionItem(function.FunctionName, "function");
                    break;
                case StructSyntaxObject @struct:
                    items[@struct.StructName] = new LegacyCompletionItem(@struct.StructName, "struct");
                    break;
                case EnumSyntaxObject @enum:
                    items[@enum.EnumName] = new LegacyCompletionItem(@enum.EnumName, "enum");
                    foreach (var member in @enum.Members)
                    {
                        items[$"{@enum.EnumName}.{member.Name}"] = new LegacyCompletionItem($"{@enum.EnumName}.{member.Name}", "enum member");
                    }
                    break;
            }
        }

        return items.Values.OrderBy(static x => x.Label, StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<LegacyCompletionItem> BuildKeywordCompletions() => BuildCompletions(Array.Empty<Token>());

    private static LegacyDiagnostic CreateDiagnostic(string id, string severity, Exception ex, string code)
    {
        var offset = TryFindOffsetFromMessage(ex.Message, code);
        var (line, column) = ComputeLineColumn(code, offset);
        return new LegacyDiagnostic(id, severity, ex.Message, offset, 1, line, column);
    }

    private static int TryFindOffsetFromMessage(string message, string code)
    {
        var quoted = Regex.Match(message, "'([^']+)'");
        if (quoted.Success)
        {
            return FindOffset(code, quoted.Groups[1].Value);
        }

        return 0;
    }

    private static int FindOffset(string code, string needle)
    {
        var offset = code.IndexOf(needle, StringComparison.Ordinal);
        return offset >= 0 ? offset : 0;
    }

    private static (int line, int column) ComputeLineColumn(string code, int offset)
    {
        var line = 1;
        var column = 1;
        for (var index = 0; index < Math.Min(offset, code.Length); index++)
        {
            if (code[index] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        return (line, column);
    }

    private static string RemoveCommentLines(string source) =>
        string.Join(
            Environment.NewLine,
            source.Split(["\r\n", "\n"], StringSplitOptions.None)
                .Where(static line => !line.TrimStart().StartsWith('#')));
}
