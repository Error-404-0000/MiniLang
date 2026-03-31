using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MiniLang.GrammarsAnalyers;

internal static class UsePathContext
{
    private static readonly AsyncLocal<Stack<string>> Current = new();

    public static string? CurrentFilePath =>
        Current.Value is { Count: > 0 } stack
            ? stack.Peek()
            : null;

    public static IEnumerable<string> ImporterDirectories
    {
        get
        {
            if (Current.Value is not { Count: > 0 } stack)
            {
                yield break;
            }

            foreach (var path in stack.Where(static value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    yield return directory;
                }
            }
        }
    }

    public static IDisposable Push(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return NoopDisposable.Instance;
        }

        var stack = Current.Value ??= new Stack<string>();
        stack.Push(Path.GetFullPath(filePath));
        return new PopWhenDisposed(stack);
    }

    private sealed class PopWhenDisposed(Stack<string> stack) : IDisposable
    {
        private readonly Stack<string> _stack = stack;
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_stack.Count > 0)
            {
                _stack.Pop();
            }
        }
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}
