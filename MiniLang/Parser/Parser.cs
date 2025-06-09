using CacheLily;
using MiniLang.Attributes;
using MiniLang.Tokenilzer;
using MiniLang.TokenObjects;
using System;
using System.Data.Common;
using System.Linq;
namespace MiniLang.Parser;

public static partial class Parser
{
    /// <summary>
    /// returns a flat tokens
    /// </summary>
    /// <param name="StringTokens"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static List<Token> Parse(string[] StringTokens)
    {
        List<(string valueName, double Value)> LocalValues = new();
        List<Token> tokens = new List<Token>();
        for (int i = 0; i < StringTokens.Length; i++)
        {

            if (Tokenizer.IsNumber(StringTokens[i]))
            {
                tokens.Add(new Token(TokenType.Number, TokenOperation.None, TokenTree.Single, StringTokens[i]));
            }
            else
            {
                var GetTokenType = ValueContainerAttribute.GetContainerValue(typeof(TokenType), StringTokens[i]);

                if (AreLetters(StringTokens[i]) && GetTokenType is (_, null))
                {
                    tokens.Add(new Token(TokenType.Identifier, TokenOperation.None, TokenTree.Single, StringTokens[i]));
                    continue;
                }
                else if (StringTokens[i].StartsWith("\""))
                {
                    if (!StringTokens[i].EndsWith("\"")|| StringTokens[i].Length<2)
                    {
                        throw new Exception($"Unterminated string literal: {StringTokens[i]}");
                    }

                    string cleanedToken = StringTokens[i].Substring(1, StringTokens[i].Length - 2);

                    tokens.Add(new Token(TokenType.StringLiteralExpression,
                                         TokenOperation.None,
                                         TokenTree.Single,
                                         cleanedToken));

                    continue;
                }
                else if (StringTokens[i].StartsWith("'"))
                {
                    if (!StringTokens[i].EndsWith("'") || StringTokens[i].Length < 2)
                    {
                        throw new Exception($"Unterminated char literal: {StringTokens[i]}");
                    }

                    string cleanedToken = StringTokens[i].Substring(1, StringTokens[i].Length - 2);

                    tokens.Add(new Token(TokenType.CharLiteralExpression,
                                         TokenOperation.None,
                                         TokenTree.Single,
                                         cleanedToken));

                    continue;
                }


                if (GetTokenType.haveNext)
                {

                    var GetTokenOperation = ValueContainerAttribute.GetContainerValue(typeof(TokenOperation), StringTokens[i]);
                    if (GetTokenOperation is (_, null))
                        throw new Exception($"{string.Join("", StringTokens)}  : Invalid TokenOperation {StringTokens[i]}");


                    tokens.Add(new Token((TokenType)Enum.Parse(typeof(TokenType), GetTokenType.Value),
                         (TokenOperation)Enum.Parse(typeof(TokenOperation), GetTokenOperation.Value),
                          TokenTree.Single, StringTokens[i]));

                }
                else if (GetTokenType is not (_, null))
                {
                    tokens.Add(new Token((TokenType)Enum.Parse(typeof(TokenType), GetTokenType.Value),
                       TokenOperation.None,
                       TokenTree.Single, StringTokens[i]));
                }
               
                else
                {
                    throw new Exception($"Invalid or unrecognized token: '{StringTokens[i]}' at position {i}.");
                }
            }

        }


        return _group_token_object(tokens);
    }
    private static List<Token> _group_token_object(List<Token> tokens)
    {
        int startIndex;

        while ((startIndex = tokens.FindIndex(x => x.TokenType == TokenType.ParenthesisOpen)) != -1)
        {
            int endIndex = FindMatchingClosingParenthesis(tokens, startIndex);

            if (endIndex == -1)
                throw new Exception("Unmatched opening parenthesis.");

            var subTokens = tokens.Skip(startIndex + 1).Take(endIndex - startIndex - 1).ToList();

            // Recursively process inner groups
            var groupedSubTokens = _group_token_object(subTokens);

            // Create the grouped token
            var groupToken = new Token(TokenType.Group, TokenOperation.None, TokenTree.Group, groupedSubTokens);

            // Replace the original tokens with the grouped token
            tokens.RemoveRange(startIndex, endIndex - startIndex + 1);
            tokens.Insert(startIndex, groupToken);
        }

        return tokens;
    }


    private static int FindMatchingClosingParenthesis(List<Token> tokens, int openIndex)
    {
        int depth = 1;
        for (int i = openIndex + 1; i < tokens.Count; i++)
        {
            if (tokens[i].TokenType == TokenType.ParenthesisOpen)
                depth++;
            else if (tokens[i].TokenType == TokenType.ParenthesisClose)
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }

        return -1;  // No matching closing parenthesis found
    }


    private static bool AreLetters(string varName)
   => Tokenizer.IsFunctionName(varName);


  

    private static int FindEndOfExpression(string[] strings)
    {
        for (int i = 0; i < strings.Length; i++)
        {
            if (strings[i] == ";")
                return i - 1;
        }
        return -1;
    }
    public static int FindOpenedParenthesisEnd(string[] StartParenthesis)
    {
        if (StartParenthesis is null || StartParenthesis.Length == 0)
            return -1;
        int startPr = 0;
        int endPr = 0;
        for (int i = 0; i < StartParenthesis.Length; i++)
        {

            if (StartParenthesis[i] == "(")
                startPr++;
            else if (StartParenthesis[i] == ")")
                startPr--;
            if (startPr == 0)
            {
                endPr = i;
                break;
            }

        }
        return startPr != 0 ? -1 : endPr;
    }
    public static int FindOpenedStringEnd(string[] tokens)
    {
        if (tokens == null || tokens.Length == 0)
            return -1;

        bool insideQuotes = false;
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i] == "\"")
            {
                insideQuotes = !insideQuotes;
                if (!insideQuotes)
                    return i; // found matching end
            }
        }

        return -1; // no clos

    }
}