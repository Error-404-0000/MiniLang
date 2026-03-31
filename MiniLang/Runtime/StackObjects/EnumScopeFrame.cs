using MiniLang.TokenObjects;

namespace MiniLang.Runtime.StackObjects;

public sealed record RuntimeEnumDefinition(string Name, IReadOnlyDictionary<string, RuntimeEnumValue> Members);

public sealed record RuntimeEnumValue(string EnumName, string MemberName, int Ordinal)
{
    public override string ToString() => $"{EnumName}.{MemberName}";
}

public sealed class RuntimeEnumScopeFrame
{
    private readonly List<RuntimeEnumDefinition> _definitions = [];

    public RuntimeEnumScopeFrame? Parent { get; set; }

    public void Declare(RuntimeEnumDefinition definition)
    {
        if (_definitions.Any(x => string.Equals(x.Name, definition.Name, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Enum '{definition.Name}' is already declared in this scope.");
        }

        _definitions.Add(definition);
    }

    public bool Exists(string name) => TryResolve(name, out _);

    public bool TryResolve(string name, out RuntimeEnumValue? value)
    {
        value = null;
        var dotIndex = name.IndexOf('.');
        if (dotIndex <= 0 || dotIndex >= name.Length - 1)
        {
            return Parent?.TryResolve(name, out value) ?? false;
        }

        var enumName = name[..dotIndex];
        var memberName = name[(dotIndex + 1)..];

        for (var frame = this; frame is not null; frame = frame.Parent)
        {
            var definition = frame._definitions.FirstOrDefault(x => string.Equals(x.Name, enumName, StringComparison.Ordinal));
            if (definition is null)
            {
                continue;
            }

            if (definition.Members.TryGetValue(memberName, out value))
            {
                return true;
            }

            throw new InvalidOperationException($"Enum '{enumName}' does not contain member '{memberName}'.");
        }

        return false;
    }

    public bool TryGetDefinition(string name, out RuntimeEnumDefinition? definition)
    {
        for (var frame = this; frame is not null; frame = frame.Parent)
        {
            definition = frame._definitions.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));
            if (definition is not null)
            {
                return true;
            }
        }

        definition = null;
        return false;
    }
}
