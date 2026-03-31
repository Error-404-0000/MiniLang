using System.Text;
using MiniLang.Runtime.StackObjects.StackFrame;

namespace MiniLang.Runtime.Collections;

public sealed class RuntimeArrayValue
{
    private readonly List<RuntimeValue> _items;

    public RuntimeArrayValue()
        : this([])
    {
    }

    public RuntimeArrayValue(IEnumerable<RuntimeValue> items)
    {
        _items = items.ToList();
    }

    public IReadOnlyList<RuntimeValue> Items => _items;

    public int Count => _items.Count;

    public RuntimeValue this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public void Add(RuntimeValue value) => _items.Add(value);

    public RuntimeValue RemoveLast()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Pop requires a non-empty array.");
        }

        var value = _items[^1];
        _items.RemoveAt(_items.Count - 1);
        return value;
    }

    public void Clear() => _items.Clear();

    public bool Contains(RuntimeValue value) =>
        _items.Any(item => Equals(item.Value, value.Value));

    public RuntimeArrayValue Snapshot() => new(_items.Select(static item => item));

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append('[');
        for (var index = 0; index < _items.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(", ");
            }

            builder.Append(_items[index].Value);
        }

        builder.Append(']');
        return builder.ToString();
    }
}
