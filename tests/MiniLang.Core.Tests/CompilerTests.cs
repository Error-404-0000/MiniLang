using MiniLang.Core;
using MiniLang.LanguageServices;

namespace MiniLang.Core.Tests;

public sealed class CompilerTests
{
    private const string Sample = """
#[docs(summary: "sample")]
module tests.sample;

use std.core;

#[layout(kind: "sequential")]
struct Point<T> {
    public x: T;
}

#[dll_import(library: "user32.dll", entrypoint: "MessageBoxW", charset: "utf16")]
extern fn message_box(window: handle, text: string, caption: string, kind: int) -> int;

#[export]
fn main() -> string {
    return "ok";
}
""";

    [Fact]
    public void Compilation_Binds_Core_Features_Without_Errors()
    {
        var compilation = Compilation.Create([new SourceDocument("sample.mini", Sample)]);
        Assert.Empty(compilation.Diagnostics);
        Assert.Contains(compilation.Symbols.Values, static x => x.Name == "Point");
        Assert.Contains(compilation.ExternSignatures, static x => x.Name == "message_box");
    }

    [Fact]
    public void Completion_Contains_Attributes_And_Symbols()
    {
        var compilation = Compilation.Create([new SourceDocument("sample.mini", Sample)]);
        var completions = CompletionService.GetCompletions(compilation, "sample.mini", 0);
        Assert.Contains(completions, static x => x.Label == "export");
        Assert.Contains(completions, static x => x.Label == "Point");
    }

    [Fact]
    public void Hover_Returns_Builtin_Type_Documentation()
    {
        const string text = """
fn main() -> string {
    return "ok";
}
""";
        var document = new SourceDocument("hover.mini", text);
        var compilation = Compilation.Create([document]);
        var position = text.IndexOf("string", StringComparison.Ordinal);
        var hover = HoverService.GetHover(compilation, document.Path, position);
        Assert.NotNull(hover);
        Assert.Equal("string", hover!.Title);
    }
}
