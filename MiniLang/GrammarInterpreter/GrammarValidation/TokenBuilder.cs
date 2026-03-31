using MiniLang.Functions;
using MiniLang.SyntaxObjects.Csharp;
using MiniLang.SyntaxObjects.Collections;
using MiniLang.TokenObjects;

namespace MiniLang.GrammarInterpreter
{
    public static class TokenBuilder
    {
        public static List<Token> BuildStructuredTokens(List<Token> tokens)
        {
            List<Token> groupedTokens = new();
            int i = 0;

            while (i < tokens.Count)
            {
                Token current = tokens[i];

                if (current.TokenType == TokenType.CSharp
                    && i + 3 < tokens.Count
                    && tokens[i + 1].TokenType == TokenType.Identifier
                    && tokens[i + 2].TokenType == TokenType.Identifier
                    && tokens[i + 3].TokenTree == TokenTree.Group
                    && tokens[i + 3].Value is List<Token> interopGroup)
                {
                    var resolvedArgs = new List<FunctionArgments>();
                    var args = SplitArguments(interopGroup);

                    foreach (var (arg, index) in args.Select((arg, idx) => (arg, idx)))
                    {
                        resolvedArgs.Add(new FunctionArgments(BuildStructuredTokens(arg), index));
                    }

                    var namespaceToken = tokens[i + 1].Value.ToString()!;
                    if (current.TokenOperation == TokenOperation.Win && !namespaceToken.StartsWith("win.", StringComparison.Ordinal))
                    {
                        namespaceToken = $"win.{namespaceToken}";
                    }

                    groupedTokens.Add(new Token(
                        TokenType.CSharp,
                        current.TokenOperation,
                        TokenTree.Single,
                        new CSharpCallSyntaxObject(
                            namespaceToken,
                            new FunctionCallTokenObject(
                                tokens[i + 2].Value.ToString()!,
                                resolvedArgs.Count,
                                resolvedArgs),
                            current.TokenOperation)));

                    i += 4;
                    continue;
                }

                if (current.TokenType == TokenType.Identifier && i + 1 < tokens.Count && tokens[i + 1].TokenType == TokenType.Group && tokens[i + 1].TokenTree == TokenTree.Group && tokens[i + 1].Value is List<Token> group)
                {
                    var resolvedArgs = new List<FunctionArgments>();
                    var args = SplitArguments(group);

                    foreach (var (arg, index) in args.Select((arg, idx) => (arg, idx)))
                    {
                        var deep = BuildStructuredTokens(arg);
                        resolvedArgs.Add(new FunctionArgments(deep, index));
                    }

                    var functionToken = new FunctionCallTokenObject(
                        current.Value.ToString()!,
                        resolvedArgs.Count,
                        resolvedArgs
                    );

                    Token builtToken = new(TokenType.FunctionCall, TokenOperation.None, TokenTree.Single, functionToken);
                    if (i + 2 < tokens.Count && tokens[i + 2].TokenType == TokenType.Array && tokens[i + 2].TokenTree == TokenTree.Group && tokens[i + 2].Value is List<Token> functionIndexGroup)
                    {
                        var indexExpressions = SplitArguments(functionIndexGroup);
                        if (indexExpressions.Count != 1)
                        {
                            throw new Exception("Array index access requires exactly one index expression.");
                        }

                        builtToken = new Token(
                            TokenType.Array,
                            TokenOperation.IndexAccess,
                            TokenTree.Single,
                            new ArrayAccessSyntaxObject(builtToken, BuildStructuredTokens(indexExpressions[0])));

                        i += 3;
                        groupedTokens.Add(builtToken);
                        continue;
                    }

                    groupedTokens.Add(builtToken);
                    i += 2;
                    continue;
                }

                if (IsIndexableToken(current) && i + 1 < tokens.Count && tokens[i + 1].TokenType == TokenType.Array && tokens[i + 1].TokenTree == TokenTree.Group && tokens[i + 1].Value is List<Token> indexGroup)
                {
                    var indexExpressions = SplitArguments(indexGroup);
                    if (indexExpressions.Count != 1)
                    {
                        throw new Exception("Array index access requires exactly one index expression.");
                    }

                    groupedTokens.Add(new Token(
                        TokenType.Array,
                        TokenOperation.IndexAccess,
                        TokenTree.Single,
                        new ArrayAccessSyntaxObject(current, BuildStructuredTokens(indexExpressions[0]))));

                    i += 2;
                    continue;
                }

                if (current.TokenType == TokenType.Array && current.TokenTree == TokenTree.Group && current.Value is List<Token> arrayGroup)
                {
                    var elements = SplitArguments(arrayGroup)
                        .Select(BuildStructuredTokens)
                        .Select(static item => (IReadOnlyList<Token>)item)
                        .ToArray();

                    groupedTokens.Add(new Token(
                        TokenType.Array,
                        TokenOperation.None,
                        TokenTree.Single,
                        new ArrayLiteralSyntaxObject(elements)));

                    i++;
                    continue;
                }

                if (current.TokenType == TokenType.CurlybracketStart)
                {
                    int end = FindCurlyScopeEnd(tokens, i);
                    if (end == -1)
                        throw new Exception("Unclosed scope block");

                    var inner = tokens.GetRange(i + 1, end - i - 1);
                    var scoped = BuildStructuredTokens(inner);
                    groupedTokens.Add(new Token(TokenType.Scope, TokenOperation.None, TokenTree.Group, scoped));
                    i = end + 1;
                    continue;
                }
                else if (current.TokenType == TokenType.Then)
                {
                    int j = i + 1;
                    int depth = 1;

                    for (; j < tokens.Count; j++)
                    {
                        if (tokens[j].TokenType == TokenType.Then)
                            depth++;
                        else if (tokens[j].TokenType == TokenType.Done)
                        {
                            depth--;
                            if (depth == 0)
                                break;
                        }
                    }

                    if (j >= tokens.Count || depth != 0)
                        throw new Exception("Missing 'done' to close 'then' block");

                    var scopeTokens = tokens.GetRange(i + 1, j - i - 1);
                    var scoped = BuildStructuredTokens(scopeTokens);
                    groupedTokens.Add(new Token(TokenType.Scope, TokenOperation.None, TokenTree.Group, scoped));
                    i = j + 1;
                    continue;
                }

                groupedTokens.Add(current);
                i++;
            }

            return groupedTokens;
        }

        private static int FindCurlyScopeEnd(List<Token> tokens, int start)
        {
            int depth = 0;
            for (int i = start; i < tokens.Count; i++)
            {
                if (tokens[i].TokenType == TokenType.CurlybracketStart) depth++;
                else if (tokens[i].TokenType == TokenType.CurlybracketEnds) depth--;
                if (depth == 0) return i;
            }
            return -1;
        }

        private static List<List<Token>> SplitArguments(List<Token> tokens)
        {
            List<List<Token>> args = new();
            List<Token> current = new();
            int parenDepth = 0;

            foreach (var token in tokens)
            {
                if (token.TokenType == TokenType.Comma && parenDepth == 0)
                {
                    args.Add(new List<Token>(current));
                    current.Clear();
                    continue;
                }
                if (token.TokenType == TokenType.ParenthesisOpen) parenDepth++;
                if (token.TokenType == TokenType.ParenthesisClose) parenDepth--;
                current.Add(token);
            }

            if (current.Count > 0)
                args.Add(current);

            return args;
        }

        private static bool IsIndexableToken(Token token) =>
            token.TokenType is TokenType.Identifier or TokenType.FunctionCall ||
            (token.TokenType == TokenType.Array && token.Value is ArrayAccessSyntaxObject);
    }
}
