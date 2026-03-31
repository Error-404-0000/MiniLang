using System.Runtime.InteropServices;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;

namespace MiniLang.Interop;

public enum BridgeValueKind
{
    Number,
    String,
    Boolean,
    Opaque
}

public sealed record BridgeMethodDescriptor(
    string Namespace,
    string Name,
    IReadOnlyList<BridgeValueKind> ArgumentTypes,
    BridgeValueKind? ReturnType,
    Func<IReadOnlyList<RuntimeValue>, object?> Invoke);

public static class InteropBridgeRegistry
{
    private static readonly IReadOnlyDictionary<string, BridgeMethodDescriptor> Methods =
        new[]
        {
            Create("win.user", "MessageBox", [BridgeValueKind.String, BridgeValueKind.String], BridgeValueKind.Number,
                args => Win32Bridge.MessageBox(args[0].Value?.ToString() ?? string.Empty, args[1].Value?.ToString() ?? string.Empty)),
            Create("win.process", "GetCurrentProcessId", [], BridgeValueKind.Number,
                _ => Environment.ProcessId),
            Create("win.time", "Sleep", [BridgeValueKind.Number], null,
                args =>
                {
                    Thread.Sleep(Convert.ToInt32(args[0].Value));
                    return null;
                }),
            Create("win.time", "GetTickCount", [], BridgeValueKind.Number,
                _ => unchecked((long)Win32Bridge.GetTickCount64())),
            Create("win.console", "SetTitle", [BridgeValueKind.String], null,
                args =>
                {
                    Console.Title = args[0].Value?.ToString() ?? string.Empty;
                    return null;
                }),
            Create("win.console", "GetTitle", [], BridgeValueKind.String,
                _ => Console.Title),
            Create("win.io", "FileExists", [BridgeValueKind.String], BridgeValueKind.Boolean,
                args => File.Exists(args[0].Value?.ToString() ?? string.Empty)),
            Create("win.io", "ReadText", [BridgeValueKind.String], BridgeValueKind.String,
                args => File.ReadAllText(args[0].Value?.ToString() ?? string.Empty)),
            Create("win.io", "WriteText", [BridgeValueKind.String, BridgeValueKind.String], null,
                args =>
                {
                    File.WriteAllText(args[0].Value?.ToString() ?? string.Empty, args[1].Value?.ToString() ?? string.Empty);
                    return null;
                }),
            Create("win.io", "EnsureDirectory", [BridgeValueKind.String], null,
                args =>
                {
                    Directory.CreateDirectory(args[0].Value?.ToString() ?? string.Empty);
                    return null;
                })
        }.ToDictionary(static x => $"{x.Namespace}.{x.Name}", StringComparer.Ordinal);

    public static IEnumerable<string> GetNamespaces() =>
        Methods.Values.Select(static x => x.Namespace).Distinct(StringComparer.Ordinal).OrderBy(static x => x, StringComparer.Ordinal);

    public static IEnumerable<string> GetFunctions(string @namespace) =>
        Methods.Values.Where(x => string.Equals(x.Namespace, @namespace, StringComparison.Ordinal))
            .Select(static x => x.Name)
            .OrderBy(static x => x, StringComparer.Ordinal);

    public static bool TryResolve(string @namespace, string name, out BridgeMethodDescriptor method, out string? errorMessage)
    {
        if (Methods.TryGetValue($"{@namespace}.{name}", out method!))
        {
            errorMessage = null;
            return true;
        }

        errorMessage = $"Interop target '{@namespace}.{name}' is not supported. Use an approved win/cscall bridge target.";
        return false;
    }

    public static RuntimeValue Invoke(string @namespace, string name, IReadOnlyList<RuntimeValue> arguments)
    {
        if (!TryResolve(@namespace, name, out var method, out var error))
        {
            throw new InvalidOperationException(error);
        }

        if (arguments.Count != method.ArgumentTypes.Count)
        {
            throw new InvalidOperationException($"Interop target '{@namespace}.{name}' expects {method.ArgumentTypes.Count} arguments but got {arguments.Count}.");
        }

        for (var index = 0; index < method.ArgumentTypes.Count; index++)
        {
            ValidateArgumentType(method, index, arguments[index]);
        }

        var result = method.Invoke(arguments);
        return method.ReturnType switch
        {
            null => new RuntimeValue(TokenType.ReturnType, TokenOperation.ReturnsNothing, null),
            BridgeValueKind.Number => new RuntimeValue(TokenType.Number, TokenOperation.None, Convert.ToDouble(result ?? 0)),
            BridgeValueKind.String => new RuntimeValue(TokenType.StringLiteralExpression, TokenOperation.None, result?.ToString() ?? string.Empty),
            BridgeValueKind.Boolean => new RuntimeValue(TokenType.Number, TokenOperation.None, Convert.ToBoolean(result) ? 1d : 0d),
            _ => new RuntimeValue(TokenType.Object, TokenOperation.None, result)
        };
    }

    private static void ValidateArgumentType(BridgeMethodDescriptor method, int index, RuntimeValue argument)
    {
        var expected = method.ArgumentTypes[index];
        var matches = expected switch
        {
            BridgeValueKind.Number => argument.Type == TokenType.Number,
            BridgeValueKind.String => argument.Type == TokenType.StringLiteralExpression,
            BridgeValueKind.Boolean => argument.Type == TokenType.Number,
            BridgeValueKind.Opaque => argument.Type is TokenType.Object or TokenType.Struct or TokenType.Enum,
            _ => false
        };

        if (!matches)
        {
            throw new InvalidOperationException($"Interop target '{method.Namespace}.{method.Name}' argument {index + 1} expected {expected} but got {argument.Type}.");
        }
    }

    private static BridgeMethodDescriptor Create(
        string @namespace,
        string name,
        IReadOnlyList<BridgeValueKind> arguments,
        BridgeValueKind? returnType,
        Func<IReadOnlyList<RuntimeValue>, object?> invoke) =>
        new(@namespace, name, arguments, returnType, invoke);

    private static class Win32Bridge
    {
        [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
        public static extern int MessageBoxW(nint hWnd, string text, string caption, uint type);

        [DllImport("kernel32.dll", EntryPoint = "GetTickCount64")]
        public static extern ulong GetTickCount64();

        public static int MessageBox(string title, string message) => MessageBoxW(nint.Zero, message, title, 0);
    }
}
