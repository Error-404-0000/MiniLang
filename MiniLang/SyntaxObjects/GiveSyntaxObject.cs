using MiniLang.TokenObjects;

namespace MiniLang.SyntaxObjects
{
    public record GiveSyntaxObject(IEnumerable<Token> expression);
}