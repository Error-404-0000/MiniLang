using MiniLang.Attributes;

namespace MiniLang.TokenObjects;

public enum TokenType
{
    None,
    [ValueContainer(true, "cscall","win")]
    CSharp,
    [ValueContainer(false, "enum")]
    Enum,
    Group,
    [ValueContainer(true, "=", "-=", "+=")]
    SETTERS,
    [ValueContainer(true, "--", "++")]
    ShortenOperator,

    [ValueContainer(true, "say", "show", "typeof")]
    Function,
    FunctionCall,
    Number,
    Scope,
    [ValueContainer(true, "+", "-", "*", "/", "%", "^", "<=", "==", ">=", ">", "<", "!=")]
    Operation,

    [ValueContainer(false, "(")]
    ParenthesisOpen,
    [ValueContainer(false, ")")]
    ParenthesisClose,

    [ValueContainer(true, "if", "else", "while", "foreach")]
    Conditions,


    [ValueContainer(true, "use", "make", "give", "in")]
    Keyword,
    Expression,

    [ValueContainer(false, "{")]
    CurlybracketStart,
    [ValueContainer(false, "}")]
    CurlybracketEnds,
    [ValueContainer(false, ":")]
    Then,
    [ValueContainer(false, "->")]
    Director,
    [ValueContainer(false, "done")]
    Done,
    [ValueContainer(false, ";")]
    Semicolon,
    [ValueContainer(false, ".")]
    Dot,
    [ValueContainer(false, "[")]
    SquareBracketOpen,
    [ValueContainer(false, "]")]
    SquareBracketClose,
    StringLiteralExpression,
    [ValueContainer(false, ",")]
    Comma,
    CharLiteralExpression,
    Identifier,

    [ValueContainer(false, "give")]
    Return,

    [ValueContainer(false, "fn")]
    NewFunction,
    [ValueContainer(true, "number", "string", "object", "nothing", "array")]
    ReturnType,
    #region struct
    [ValueContainer(false, "new")]
    New,
    [ValueContainer(false, "struct")]
    Struct,
    [ValueContainer(true, "private", "public")]
    @FieldAccess,
    #endregion

    Object,
    Array

}
