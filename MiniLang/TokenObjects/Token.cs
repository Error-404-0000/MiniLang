using CacheLily;

namespace MiniLang.TokenObjects;
public record Token(TokenType TokenType,TokenOperation TokenOperation,TokenTree TokenTree,object Value):ICacheable
{
    public int CacheCode { get ; set ; }

    public Token New()
    {
        return new Token(
            TokenType,
            TokenOperation,
            TokenTree,
            CloneValue(Value)
        );
    }

    public static Token DefaultToken() => new(TokenType.None, TokenOperation.None, TokenTree.Single, null);
    private dynamic CloneValue(object value)
    {
        if (value is List<Token> tokenList)
        {
            return tokenList.Select(t => t.New()).ToList();
        }

        return value;
    }



    //
    public  void Print()
    {
        PrintToken(this, "", true);
    }

    private static void PrintToken(Token token, string indent, bool last)
    {
        // Token header
        Console.Write(indent);
        Console.Write(last ? "└──" : "├──");

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("Token");
        Console.ResetColor();

        string childIndent = indent + (last ? "    " : "│   ");

        // TokenType
        PrintRow("TokenType", token.TokenType, childIndent, false);

        // TokenOperation
        PrintRow("TokenOperation", token.TokenOperation, childIndent, false);

        // TokenTree (just prints null or the object ref)
        PrintRow("TokenTree", token.TokenTree, childIndent, false);

        // Value
        bool valueIsToken = token.Value is Token;

        if (!valueIsToken)
        {
            // Print normally
            PrintRow("Value", token.Value, childIndent, true);
        }
        else
        {
            // Print header for nested token
            Console.Write(childIndent);
            Console.Write("└──");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Value (Token)");
            Console.ResetColor();

            // Recursively print nested token
            PrintToken((Token)token.Value, childIndent + "    ", true);
        }
    }

    private static void PrintRow(string name, object value, string indent, bool last)
    {
        Console.Write(indent);
        Console.Write(last ? "└──" : "├──");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"{name}: ");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(value ?? "null");
        Console.ResetColor();
    }
}
