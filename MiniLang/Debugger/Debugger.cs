using MiniLang.Functions;
using MiniLang.TokenObjects;
using System.Linq;
using System.Text;

namespace MiniLang.Debugger;

public static class Debugger
{
    public static string WriteTree(IEnumerable<Token> tokens, string indent = "", bool isLast = true)
    {
        StringBuilder stringBuilder = new StringBuilder();
        var tokenList = tokens?.ToList() ?? new List<Token>();
        for (int i = 0; i < tokenList.Count; i++)
        {
            var token = tokenList[i];
            bool last = i == tokenList.Count - 1;

            string branch = indent + (last ? "└── " : "├── ");
            string nextIndent = indent + (last ? "    " : "│   ");

            // Function token display
            if (token.TokenType == TokenType.Function && token.Value is FunctionTokenObject funcObject)
            {
                stringBuilder.AppendLine($"{branch}[Function: {funcObject.FunctionName} (Args: {funcObject.FunctionArgmentsCount})]");

                // Print each argument as a subtree
                foreach (var arg in funcObject.FunctionArgments)
                {
                    stringBuilder.AppendLine($"{nextIndent}└── [Arg]");
                    stringBuilder.AppendLine(WriteTree(arg.Argment, nextIndent + "    ", true));
                }
            }
            // Single tokens (e.g., identifiers, literals, operators)
            else if (token.TokenTree == TokenTree.Single)
            {
                stringBuilder.AppendLine($"{branch}[{token.TokenType} -> {token.TokenOperation}] {token.Value}");
            }
            // Grouped tokens or scopes
            else if (token.TokenTree == TokenTree.Group || token.TokenType == TokenType.Scope)
            {
                stringBuilder.AppendLine($"{branch}[Group: {token.TokenType}]");
                stringBuilder.AppendLine(WriteTree(token.Value as IEnumerable<Token>, nextIndent, last));
            }
            // Fallback for unknown structures
            else
            {
                stringBuilder.AppendLine($"{branch}[Unknown Token Type] {token.Value}");
            }
        }
        return stringBuilder.ToString();
    }

}


