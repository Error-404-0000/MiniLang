namespace MiniLang.SyntaxObjects.Enum;

public sealed record EnumMemberSyntax(string Name, int Ordinal);

public sealed class EnumSyntaxObject
{
    public required string EnumName { get; init; }
    public required IReadOnlyList<EnumMemberSyntax> Members { get; init; }
}
