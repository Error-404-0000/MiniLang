using MiniLang.TokenObjects;

namespace MiniLang.SyntaxObjects.Collections;

public sealed record ArrayLiteralSyntaxObject(IReadOnlyList<IReadOnlyList<Token>> Elements);
