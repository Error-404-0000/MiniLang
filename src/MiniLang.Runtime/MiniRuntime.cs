using MiniLang.Core;

namespace MiniLang.Runtime;

public static class MiniRuntimeHost
{
    public static object? InvokeExport(Compilation compilation, string exportName, IReadOnlyDictionary<string, object?>? arguments = null)
    {
        arguments ??= new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var export = compilation.Symbols.Values.OfType<FunctionSymbol>().FirstOrDefault(x => x.IsExported && string.Equals(x.Name, exportName, StringComparison.OrdinalIgnoreCase));
        if (export is null)
        {
            throw new InvalidOperationException($"No exported function named '{exportName}' exists in the compilation.");
        }

        var documentModel = compilation.SemanticModels.Values.FirstOrDefault(model => model.DeclaredSymbols.Contains(export))
            ?? throw new InvalidOperationException($"Unable to locate the body for export '{exportName}'.");

        var syntax = documentModel.SyntaxTree.Root.Members.OfType<FunctionDeclarationSyntax>().First(member => member.Identifier.Text == export.Name);
        if (syntax.Body is null || syntax.Body.Statements.Count == 0 || syntax.Body.Statements[0] is not ReturnStatementSyntax @return)
        {
            throw new InvalidOperationException($"Export '{exportName}' does not contain a return-based body that the lightweight runtime can execute.");
        }

        return Evaluate(@return.Expression, arguments, compilation);
    }

    private static object? Evaluate(ExpressionSyntax expression, IReadOnlyDictionary<string, object?> arguments, Compilation compilation) => expression switch
    {
        LiteralExpressionSyntax literal => literal.LiteralToken.Value ?? literal.LiteralToken.Text.Trim('"'),
        NameExpressionSyntax name when arguments.TryGetValue(name.Identifier.Text, out var value) => value,
        BinaryExpressionSyntax binary => EvaluateBinary(binary, arguments, compilation),
        ParenthesizedExpressionSyntax parenthesized => Evaluate(parenthesized.Expression, arguments, compilation),
        CallExpressionSyntax call => InvokeExport(compilation, ((NameExpressionSyntax)call.Target).Identifier.Text, call.Arguments.Select((arg, index) => new KeyValuePair<string, object?>($"arg{index}", Evaluate(arg, arguments, compilation))).ToDictionary()),
        _ => throw new InvalidOperationException($"The lightweight runtime cannot evaluate expression kind '{expression.Kind}' yet.")
    };

    private static object? EvaluateBinary(BinaryExpressionSyntax binary, IReadOnlyDictionary<string, object?> arguments, Compilation compilation)
    {
        var left = Evaluate(binary.Left, arguments, compilation);
        var right = Evaluate(binary.Right, arguments, compilation);
        return binary.OperatorToken.Kind switch
        {
            SyntaxKind.PlusToken when left is string || right is string => $"{left}{right}",
            SyntaxKind.PlusToken => Convert.ToInt32(left) + Convert.ToInt32(right),
            SyntaxKind.MinusToken => Convert.ToInt32(left) - Convert.ToInt32(right),
            SyntaxKind.StarToken => Convert.ToInt32(left) * Convert.ToInt32(right),
            SyntaxKind.SlashToken => Convert.ToInt32(left) / Convert.ToInt32(right),
            SyntaxKind.DoubleEqualsToken => Equals(left, right),
            SyntaxKind.BangEqualsToken => !Equals(left, right),
            SyntaxKind.LessToken => Convert.ToInt32(left) < Convert.ToInt32(right),
            SyntaxKind.GreaterToken => Convert.ToInt32(left) > Convert.ToInt32(right),
            SyntaxKind.LessOrEqualsToken => Convert.ToInt32(left) <= Convert.ToInt32(right),
            SyntaxKind.GreaterOrEqualsToken => Convert.ToInt32(left) >= Convert.ToInt32(right),
            _ => throw new InvalidOperationException($"Operator '{binary.OperatorToken.Text}' is not supported by the lightweight runtime.")
        };
    }
}
