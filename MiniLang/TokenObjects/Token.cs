﻿using CacheLily;

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

    private dynamic CloneValue(object value)
    {
        if (value is List<Token> tokenList)
        {
            return tokenList.Select(t => t.New()).ToList();
        }

        return value;
    }
}
