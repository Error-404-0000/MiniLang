using System.Text;
using System.Text.Json;
using MiniLang.Core;

namespace MiniLang.Interop;

public static class InteropBindingGenerator
{
    public static string GenerateManifest(Compilation compilation)
    {
        var payload = new
        {
            exports = compilation.Symbols.Values
                .OfType<FunctionSymbol>()
                .Where(static x => x.IsExported)
                .Select(static x => new
                {
                    x.Name,
                    parameters = x.Parameters.Select(static p => new { p.Name, type = p.Type.Name }),
                    returnType = x.ReturnType.Name
                }),
            externs = compilation.ExternSignatures,
            marshalling = compilation.MarshallingRules
        };
        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    public static string GenerateCSharpStubs(Compilation compilation, string @namespace = "MiniLang.Generated")
    {
        var builder = new StringBuilder();
        builder.AppendLine("using System;");
        builder.AppendLine();
        builder.Append("namespace ").Append(@namespace).AppendLine(";");
        builder.AppendLine();
        builder.AppendLine("public static class MiniLangExports");
        builder.AppendLine("{");

        foreach (var export in compilation.Symbols.Values.OfType<FunctionSymbol>().Where(static x => x.IsExported))
        {
            var parameterList = string.Join(", ", export.Parameters.Select(static p => $"{MapManagedType(p.Type.Name)} {p.Name}"));
            builder.Append("    public static ").Append(MapManagedType(export.ReturnType.Name)).Append(' ').Append(export.Name).Append('(').Append(parameterList).AppendLine(")");
            builder.AppendLine("    {");
            builder.Append("        throw new NotImplementedException(\"Wire MiniLang runtime invocation for export '").Append(export.Name).AppendLine("'.\");");
            builder.AppendLine("    }");
            builder.AppendLine();
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string MapManagedType(string miniType) => miniType switch
    {
        "int" => "int",
        "bool" => "bool",
        "string" => "string",
        "void" => "void",
        "handle" => "nint",
        _ => "object"
    };
}
