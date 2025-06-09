using MiniLang.Attributes;

namespace MiniLang.TokenObjects;
public enum TokenType
{
    None,
    Group,
    [ValueContainer(true, "=","-=","+=")]
    SETTERS,
    
    [ValueContainer(true, "say", "show", "typeof")]
    Function,
    FunctionCall,
    Number,
    Scope,
    [ValueContainer(true, "+", "-", "*", "/", "%", "^","<=","==",">=",">","<","!=")]
    Operation,

    [ValueContainer(false, "(")]
    ParenthesisOpen,
    [ValueContainer(false, ")")]
    ParenthesisClose,

    [ValueContainer(true, "if", "else","while")]
    Conditions,


    [ValueContainer(true, "use", "make", "give")]
    Keyword,
    Expression,

    [ValueContainer(false, "{")]
    CurlybracketStart,
    [ValueContainer(false, "}")]
    CurlybracketEnds,
    [ValueContainer(false, ":")]
    Then,
    [ValueContainer(false, "done")]
    Done,
    [ValueContainer(false,";")]
    Semicolon,
    [ValueContainer(false, ".")]
    Dot,

    StringLiteralExpression,
    [ValueContainer(false, ",")]
    Comma,
    CharLiteralExpression,
    Identifier,

    [ValueContainer(false, "give")]
    Return,

    [ValueContainer(false,"fn")]
    NewFunction,
    [ValueContainer(true,"number", "string","object","nothing")]
    ReturnType

}
