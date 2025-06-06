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

        [ValueContainer(false, "<")]
        LessThanOperation,

        [ValueContainer(false, ">")]
        GreaterThanOperation,

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
        say,
        [ValueContainer(false, "typeof")]
        @typeof,
        [ValueContainer(false, "give")]
        give,
        #endregion

        #region Conditions
        [ValueContainer(false, "eq")]
        eq,

        [ValueContainer(false, "neq")]
        neq,

        [ValueContainer(false, "lt")]
        lt,

        [ValueContainer(false, "gt")]
        gt,

        [ValueContainer(false, "if")]
        If,

     
        [ValueContainer(false, "else")]
        @else,

        
        #endregion
    }
}
