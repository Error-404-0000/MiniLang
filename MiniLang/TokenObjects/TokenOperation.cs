namespace MiniLang.TokenObjects
{
    using MiniLang.Attributes;

    public enum TokenOperation
    {
        None = 0,

     

        #region Operations
        [ValueContainer(false, "+")]
        AddOperation,

        [ValueContainer(false, "-")]
        SubtractOperation,

        [ValueContainer(false, "*")]
        MultiplyOperation,

        [ValueContainer(false, "/")]
        DivideOperation,

        [ValueContainer(false, "^")]
        PowerOperation,
        #region Conditon
        [ValueContainer(false, "==")]

        EqualOperation,
        [ValueContainer(false, "<")]
        LessThanOperation,

        [ValueContainer(false, ">")]
        GreaterThanOperation,
        [ValueContainer(false, ">=")]
        GreaterThanOrEqual,
        [ValueContainer(false, "<=")]
        LessThanOrEqual,
        [ValueContainer(false, "!=")]
        Not,
        #endregion

        [ValueContainer(false, "|")]
        OrOperation,

        [ValueContainer(false, "&")]
        AndOperation,

        [ValueContainer(false, "%")]
        ModuloOperation,
        [ValueContainer(false,"=")]
        SETTER,
        [ValueContainer(false, "-=")]
        SETTERSubtractOperation,
        [ValueContainer(false, "+=")]
        SETTERAddOperation,
        #endregion

        #region Keywords
        [ValueContainer(false, "use")]
        use,

        [ValueContainer(false, "make")]
        make,

        [ValueContainer(false, "say")]
        SayKeyword,
        [ValueContainer(false, "typeof")]
        @typeof,
        [ValueContainer(false, "give")]
        give,
        #endregion

        #region Conditions
       
        [ValueContainer(false, "if")]
        If,

     
        [ValueContainer(false, "else")]
        @else,
        [ValueContainer(false, "while")]
        While,
        #endregion
        #region Return Types operators
        [ValueContainer(false,"nothing")]
        ReturnsNothing,
        [ValueContainer(false,"number")]
        ReturnsNumber,
        [ValueContainer(false, "string")]
        ReturnsString,
        [ValueContainer(false, "object")]
        ReturnsObject,
        #endregion


        #region FieldAccess
        [ValueContainer(false, "public")]
        Public,
        [ValueContainer(false, "private")]
        Private,
        #endregion

        #region Talks to c# directly
        [ValueContainer(false, "cscall")]
        Cscall,
        #endregion

    }
}
