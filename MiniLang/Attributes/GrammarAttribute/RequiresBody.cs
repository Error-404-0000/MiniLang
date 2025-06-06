using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Attributes.GrammarAttribute
{
    /// <summary>
    /// Tells the engine to treat this grammar as requiring a body block if no terminator (e.g. semicolon) is present.
    /// Used for things like `if`, `fn`, `loop`, etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresBody : Attribute
    {
    }
}
