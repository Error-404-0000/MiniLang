using MiniLang.Functions;
using MiniLang.Runtime.Collections;
using MiniLang.Runtime.StackObjects.StackFrame;
using MiniLang.TokenObjects;

namespace MiniLang.Collections;

public sealed record CollectionBuiltinDescriptor(string Name, string Detail, string DocumentationPath, int ArgumentCount, TokenType ReturnType, bool ReturnsNothing = false);

public static class CollectionBuiltins
{
    private static readonly IReadOnlyDictionary<string, CollectionBuiltinDescriptor> Builtins = new Dictionary<string, CollectionBuiltinDescriptor>(StringComparer.Ordinal)
    {
        ["Length"] = new("Length", "Returns the current item count of an array.", "builtin://array/length", 1, TokenType.Number),
        ["Push"] = new("Push", "Appends a value to an array and returns the new length.", "builtin://array/push", 2, TokenType.Number),
        ["Pop"] = new("Pop", "Removes and returns the last item from an array.", "builtin://array/pop", 1, TokenType.Object),
        ["Clear"] = new("Clear", "Removes all items from an array.", "builtin://array/clear", 1, TokenType.ReturnType, true),
        ["Contains"] = new("Contains", "Returns 1 when the array contains the supplied value, otherwise 0.", "builtin://array/contains", 2, TokenType.Number),
        [nameof(ToArray)] = new(nameof(ToArray), "Converts a string or array value to an array. Strings are converted to arrays of single-character strings.", "builtin://array/toarray", 1, TokenType.Array)
    };
    private static readonly IReadOnlyList<string> _non_array_r = ["ToArray"];
    public static IEnumerable<CollectionBuiltinDescriptor> All => Builtins.Values;

    public static bool Exists(string name) => Builtins.ContainsKey(name);

    public static bool TryGet(string name, out CollectionBuiltinDescriptor descriptor) =>
        Builtins.TryGetValue(name, out descriptor!);

    public static bool TryGetReturnType(string name, out TokenType returnType, out bool returnsNothing)
    {
        if (Builtins.TryGetValue(name, out var descriptor))
        {
            returnType = descriptor.ReturnType;
            returnsNothing = descriptor.ReturnsNothing;
            return true;
        }

        returnType = TokenType.None;
        returnsNothing = false;
        return false;
    }

    public static RuntimeValue Invoke(FunctionCallTokenObject call, RuntimeValue[] arguments)
    {
        if (!Builtins.TryGetValue(call.FunctionName, out var descriptor))
        {
            throw new InvalidOperationException($"Unknown collection builtin '{call.FunctionName}'.");
        }

        if (arguments.Length != descriptor.ArgumentCount)
        {
            throw new InvalidOperationException($"Collection builtin '{call.FunctionName}' expected {descriptor.ArgumentCount} arguments but got {arguments.Length}.");
        }
        if (NonArrayReturnType(call.FunctionName))
        {
            return call.FunctionName switch
            {
                nameof(ToArray) => ToArray(arguments[0]),
                _ => throw new InvalidOperationException($"Unknown collection builtin '{call.FunctionName}'.")
            };
        }
        var array = RequireArray(arguments[0], call.FunctionName);
        return call.FunctionName switch
        {
            "Length" => new RuntimeValue(TokenType.Number, TokenOperation.None, (double)array.Count),
            "Push" => Push(array, arguments[1]),
            "Pop" => array.RemoveLast(),
            "Clear" => Clear(array),
            "Contains" => new RuntimeValue(TokenType.Number, TokenOperation.None, array.Contains(arguments[1]) ? 1d : 0d),
            _ => throw new InvalidOperationException($"Unknown collection builtin '{call.FunctionName}'.")
        };
    }

    private static RuntimeValue Push(RuntimeArrayValue array, RuntimeValue value)
    {
        array.Add(value);
        return new RuntimeValue(TokenType.Number, TokenOperation.None, (double)array.Count);
    }

    private static RuntimeValue ToArray(RuntimeValue value)
    {
        if (value.Type == TokenType.Array && value.Value is RuntimeArrayValue array)
        {
            return new RuntimeValue(TokenType.Array, TokenOperation.None, array);
        }
        if(value.Type is TokenType.StringLiteralExpression or TokenType.CharLiteralExpression && value.Value is string str)
        {
            var Stringarray = new RuntimeArrayValue();
            foreach (var ch in str)
            {
                Stringarray.Add(new RuntimeValue(TokenType.CharLiteralExpression, TokenOperation.None, ch.ToString()));
            }
            return new RuntimeValue(TokenType.Array, TokenOperation.None, Stringarray);
        }
        //cant convert to array
        return new RuntimeValue(TokenType.Array, TokenOperation.None, new RuntimeArrayValue([value]));
    }
    private static RuntimeValue Clear(RuntimeArrayValue array)
    {
        array.Clear();
        return new RuntimeValue(TokenType.ReturnType, TokenOperation.ReturnsNothing, null!);
    }

    public static RuntimeArrayValue RequireArray(RuntimeValue value, string operationName)
    {
        if (value.Type != TokenType.Array || value.Value is not RuntimeArrayValue array)
        {
            throw new InvalidOperationException($"{operationName} requires an array value.");
        }

        return array;
    }
    private static bool NonArrayReturnType(string functionName) => _non_array_r.Contains(functionName, StringComparer.Ordinal);
}
