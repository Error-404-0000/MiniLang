using MiniLang.TokenObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.SyntaxObjects
{
    public record UseSyntaxObject(string path, IEnumerable<Token> Tokens);

}
