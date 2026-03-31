using MiniLang.TokenObjects;

namespace MiniLang.SyntaxObjects.Collections;

public sealed record ForeachSyntaxObject(string Identifier, IReadOnlyList<Token> CollectionExpression, IReadOnlyList<Token> Scope);
