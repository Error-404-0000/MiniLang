using MiniLang.Functions;
using MiniLang.TokenObjects;

namespace MiniLang.Interpreter
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

                // Function call detection: Identifier followed by Group (i.e., (...))
                if (current.TokenType == TokenType.Identifier && i + 1 < tokens.Count && tokens[i + 1].TokenTree == TokenTree.Group && tokens[i + 1].Value is List<Token> group)
                {
                    var resolvedArgs = new List<FunctionArgments>();
                    var args = SplitArguments(group);

                    foreach (var (arg, index) in args.Select((arg, idx) => (arg, idx)))
                    {
                        var deep = BuildStructuredTokens(arg);
                        resolvedArgs.Add(new FunctionArgments(deep, index));
                    }

                    var functionToken = new FunctionTokenObject(
                        current.Value.ToString()!,
                        resolvedArgs.Count,
                        resolvedArgs
                    );

                    groupedTokens.Add(new Token(TokenType.Function, TokenOperation.None, TokenTree.Single, functionToken));
                    i += 2; // Skip identifier and group
                    continue;
                }

                // Group { ... } scope blocks
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

                // 'then' starts a scope, everything until the matching 'done' ends the scope
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
    }
}
