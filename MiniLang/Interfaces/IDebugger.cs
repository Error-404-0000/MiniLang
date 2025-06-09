using MiniLang.GrammarInterpreter;
using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Interfaces
{
    public interface IDebugger
    {
      
        public string ViewSelf(Token Token, GrammarValidator grammarValidator = null, int indentLevel=0);

    }

}
