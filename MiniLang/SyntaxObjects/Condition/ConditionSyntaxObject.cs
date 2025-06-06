using MiniLang.TokenObjects;

namespace MiniLang.SyntaxObjects.Condition
{
    public class ConditionSyntaxObject
    {
        public ConditionSyntaxObject(IEnumerable<Token> expression, int line, IEnumerable<Token> scope,bool haveElse,bool haveBody, ElseSyntaxObject @else)
        {
            Expression = expression;
            Line = line;
            Scope = scope;
            HasElse = haveElse;
            HasBody = haveBody;
            Else = @else;
        }

        public IEnumerable<Token> Expression { get; }
        public int Line { get; }
        public IEnumerable<Token> Scope { get; }
        public bool HasElse { get; }
        public bool HasBody { get; }
        public ElseSyntaxObject Else { get; }
    }
    public class ElseSyntaxObject
    {
        public ElseSyntaxObject(int line, IEnumerable<Token> scope)
        {
            Line = line;
            Scope = scope;
        }

        public int Line { get; }
        public IEnumerable<Token> Scope { get; }

    }
}
