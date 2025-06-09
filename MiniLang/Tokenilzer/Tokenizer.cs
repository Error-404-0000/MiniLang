using System;
using System.Collections.Generic;
using System.Text;
namespace MiniLang.Tokenilzer;
public static class Tokenizer
{
    static HashSet<string>  multiCharOps = new HashSet<string>
    {
        "+=", "-=", "*=","<=",">=","==","<",">","!="
    };
    public static string[] Tokenize(string input)
    {
        List<string> tokens = new();
        StringBuilder currentToken = new();
        bool isString = false;
        bool isCharLiteral = false;

       

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '\n' && !isString && !isCharLiteral)
            {
                if (currentToken.Length > 0)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
                continue;
            }

            if (c == '"')
            {
                currentToken.Append(c);
                if (isString)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
                isString = !isString;
                continue;
            }

            if (c == '\'' && !isString)
            {
                currentToken.Append(c);
                if (isCharLiteral)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
                isCharLiteral = !isCharLiteral;
                continue;
            }

            if (isString || isCharLiteral)
            {
                currentToken.Append(c);
                continue;
            }

            if (char.IsDigit(c) || (c == '.' && currentToken.Length > 0 && char.IsDigit(currentToken[^1])))
            {
                currentToken.Append(c);
                continue;
            }

            if (char.IsLetter(c))
            {
                currentToken.Append(c);
                continue;
            }

            // Finish current token if separator
            if (currentToken.Length > 0)
            {
                tokens.Add(currentToken.ToString());
                currentToken.Clear();
            }

            // Handle multi-character operators
            if (!char.IsWhiteSpace(c))
            {
                string twoChar = i + 1 < input.Length ? $"{c}{input[i + 1]}" : c.ToString();

                if (multiCharOps.Contains(twoChar))
                {
                    tokens.Add(twoChar);
                    i++; // skip next character
                }
                else
                {
                    tokens.Add(c.ToString());
                }
            }
        }

        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        return tokens.ToArray();
    }

    private static bool IsSeparator(char c)
    {
        return char.IsPunctuation(c) || char.IsSymbol(c);
    }
    public static bool IsFunctionName(string str)
    {
      
        return str.Length>0 && isChar(str[0]);
    }
    public static bool isChar(char ch)
    {

        return ch is >= 'a' and <= 'z' || ch is >= 'A' and <= 'Z';
    }
    public static bool IsNumber(string numstr)
    {
        bool hitdot = false;
        for (int i = 0; i < numstr.Length; i++)
        {
            if (numstr[i] >= '0' && numstr[i] <= '9')
                continue;
            if (hitdot is false && numstr[i] == '.')
            {
                if (i + 1 < numstr.Length && IsNumber(numstr[i + 1]))
                {
                    hitdot = true;
                    continue;
                }
            }
            return false;
        }
        return true;
    }
    private static bool IsNumber(char num)
    {

        return num >= '0' && num <= '9';
    }
}