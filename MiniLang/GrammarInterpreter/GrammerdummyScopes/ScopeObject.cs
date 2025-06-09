using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.GrammarInterpreter.GrammerdummyScopes
{
    public class ScopeObjectValue
    {
        public string Identifier { get; set; }
        public bool IsAssigned { get; set; }
        public TokenType TokenType { get; set; }
    }

}
