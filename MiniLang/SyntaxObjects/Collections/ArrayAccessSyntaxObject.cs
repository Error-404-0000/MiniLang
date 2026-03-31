using MiniLang.TokenObjects;

namespace MiniLang.SyntaxObjects.Collections;

public sealed record ArrayAccessSyntaxObject(Token Target, IReadOnlyList<Token> IndexExpression);
