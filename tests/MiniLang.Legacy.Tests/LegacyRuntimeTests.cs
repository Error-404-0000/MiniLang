using MiniLang.Hosting;

namespace MiniLang.Legacy.Tests;

public sealed class LegacyRuntimeTests
{
    [Fact]
    public void Analyze_EnumAndInteropSample_Succeeds()
    {
        var source = """
            enum Tone {
                Warm;
                Cool;
            }

            fn nothing ShowTone(Tone selected){
                if(selected == Tone.Warm):
                    say "Warm";
                else
                    say "Cool";
                done
            }

            fn nothing Main(){
                ShowTone(Tone.Warm);
                say win process GetCurrentProcessId();
            }
            """;

        var result = LegacyMiniLangHost.AnalyzeSource(source);

        Assert.True(result.Success);
        Assert.Contains(result.Outline, item => item.Kind == "enum" && item.Label == "Tone");
        Assert.Contains(result.Completions, item => item.Label == "Tone.Warm");
        Assert.Contains("GetCurrentProcessId", result.SyntaxTree);
    }

    [Fact]
    public void Run_EnumParameterAndInterop_ProducesOutput()
    {
        var source = """
            enum Tone {
                Warm;
                Cool;
            }

            fn nothing ShowTone(Tone selected){
                if(selected == Tone.Warm):
                    say "Warm tone selected";
                else
                    say "Cool tone selected";
                done
            }

            fn nothing Main(){
                ShowTone(Tone.Warm);
                say win process GetCurrentProcessId();
            }
            """;

        var result = LegacyMiniLangHost.RunSource(source);

        Assert.True(result.Success);
        Assert.Contains("Warm tone selected", result.Output);
    }

    [Fact]
    public void Analyze_DuplicateEnumMember_Fails()
    {
        var source = """
            enum Tone {
                Warm;
                Warm;
            }
            """;

        var result = LegacyMiniLangHost.AnalyzeSource(source);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Message.Contains("already contains member 'Warm'", StringComparison.Ordinal));
    }

    [Fact]
    public void Analyze_UnsupportedInteropTarget_Fails()
    {
        var source = """
            fn nothing Main(){
                win kernel OpenThing();
            }
            """;

        var result = LegacyMiniLangHost.AnalyzeSource(source);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Message.Contains("not supported", StringComparison.Ordinal));
    }

    [Fact]
    public void Analyze_UseResolvesRelativeToImportingFile()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "MiniLangTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            var libDirectory = Path.Combine(tempRoot, "libs");
            var appDirectory = Path.Combine(tempRoot, "apps");
            Directory.CreateDirectory(libDirectory);
            Directory.CreateDirectory(appDirectory);

            var libraryPath = Path.Combine(libDirectory, "Greeter.mini.c");
            File.WriteAllText(libraryPath, """
                fn nothing Greet(){
                    say "Hello from lib";
                }
                """);

            var appPath = Path.Combine(appDirectory, "Main.mini.c");
            File.WriteAllText(appPath, """
                use "../libs/Greeter.mini.c";

                fn nothing Main(){
                    Greet();
                }
                """);

            var result = LegacyMiniLangHost.AnalyzeSource(File.ReadAllText(appPath), appPath);

            Assert.True(result.Success);
            Assert.Contains(result.Completions, item => item.Label == "Greet");
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void Run_ArraysAndForeach_ProduceExpectedOutput()
    {
        var source = """
            fn array BuildValues(){
                give [1, 2, 3];
            }

            fn number Main(){
                make values = BuildValues();
                Push(values, 4);
                values[1] = 42;
                foreach item in values:
                    say item;
                done
                say Length(values);
                say Contains(values, 42);
                give 0;
            }
            """;

        var result = LegacyMiniLangHost.RunSource(source);

        Assert.True(result.Success);
        var lines = result.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(["1", "42", "3", "4", "4", "1"], lines);
    }

    [Fact]
    public void Run_ArrayParametersAndReturnValues_Work()
    {
        var source = """
            fn array BuildValues(){
                give [10, 20, 30];
            }

            fn number Sum(array values){
                make total = 0;
                foreach item in values:
                    total = total + item;
                done
                give total;
            }

            fn number Main(){
                say Sum(BuildValues());
                give 0;
            }
            """;

        var result = LegacyMiniLangHost.RunSource(source);

        Assert.True(result.Success);
        Assert.Contains("60", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_ForeachOverNonArray_Fails()
    {
        var source = """
            fn number Main(){
                make value = 5;
                foreach item in value:
                    say item;
                done
                give 0;
            }
            """;

        var result = LegacyMiniLangHost.AnalyzeSource(source);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Message.Contains("non-array", StringComparison.Ordinal));
    }

    [Fact]
    public void Analyze_IndexingNonArray_Fails()
    {
        var source = """
            fn number Main(){
                make value = 5;
                say value[0];
                give 0;
            }
            """;

        var result = LegacyMiniLangHost.AnalyzeSource(source);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Message.Contains("Cannot index non-array target", StringComparison.Ordinal));
    }
}
