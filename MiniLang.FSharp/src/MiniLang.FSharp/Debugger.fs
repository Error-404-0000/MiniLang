namespace MiniLang.FSharp

open System.Globalization
open System.Text

module Debugger =
    let private formatValue value =
        match value with
        | Value.Number n -> $"Number {n.ToString(CultureInfo.InvariantCulture)}"
        | Value.Text t -> $"String \"{t}\""
        | Value.Bool b -> $"Bool {b.ToString().ToLowerInvariant()}"
        | Value.Nothing -> "Nothing"
        | Value.Future _ -> "Future"

    let rec private writeExpr (expr: Expr) (indent: string) (isLast: bool) (builder: StringBuilder) =
        let branch = indent + (if isLast then "└── " else "├── ")
        let nextIndent = indent + (if isLast then "    " else "│   ")
        match expr with
        | Expr.Literal value ->
            builder.AppendLine($"{branch}[Literal] {formatValue value}") |> ignore
        | Expr.Identifier name ->
            builder.AppendLine($"{branch}[Identifier] {name}") |> ignore
        | Expr.TypeOf name ->
            builder.AppendLine($"{branch}[TypeOf] {name}") |> ignore
        | Expr.Await inner ->
            builder.AppendLine($"{branch}[Await]") |> ignore
            writeExpr inner nextIndent true builder
        | Expr.Binary(left, op, right) ->
            builder.AppendLine($"{branch}[Binary {op}]") |> ignore
            writeExpr left nextIndent false builder
            writeExpr right nextIndent true builder
        | Expr.Call(name, args) ->
            builder.AppendLine($"{branch}[Call {name}]") |> ignore
            for i in 0 .. args.Length - 1 do
                writeExpr args[i] nextIndent (i = args.Length - 1) builder

    let rec private writeStatement (statement: Statement) (indent: string) (isLast: bool) (builder: StringBuilder) =
        let branch = indent + (if isLast then "└── " else "├── ")
        let nextIndent = indent + (if isLast then "    " else "│   ")
        match statement with
        | Statement.Make(name, expr) ->
            builder.AppendLine($"{branch}[Make {name}]") |> ignore
            writeExpr expr nextIndent true builder
        | Statement.Set(name, op, expr) ->
            builder.AppendLine($"{branch}[Set {name} {op}]") |> ignore
            writeExpr expr nextIndent true builder
        | Statement.Shorten(name, op) ->
            builder.AppendLine($"{branch}[Shorten {name} {op}]") |> ignore
        | Statement.Use path ->
            builder.AppendLine($"{branch}[Use {path}]") |> ignore
        | Statement.Say expr ->
            builder.AppendLine($"{branch}[Say]") |> ignore
            writeExpr expr nextIndent true builder
        | Statement.Give expr ->
            builder.AppendLine($"{branch}[Give]") |> ignore
            writeExpr expr nextIndent true builder
        | Statement.While(condition, body) ->
            builder.AppendLine($"{branch}[While]") |> ignore
            writeExpr condition nextIndent false builder
            writeBlock body nextIndent true builder
        | Statement.If(condition, yesBody, noBody) ->
            builder.AppendLine($"{branch}[If]") |> ignore
            writeExpr condition nextIndent false builder
            writeBlock yesBody nextIndent (noBody.IsEmpty) builder
            if not noBody.IsEmpty then
                builder.AppendLine($"{nextIndent}└── [Else]") |> ignore
                writeBlock noBody (nextIndent + "    ") true builder
        | Statement.Fn(name, parameters, body) ->
            let signature = $"{name}({System.String.Join(',', parameters)})"
            builder.AppendLine($"{branch}[Fn {signature}]") |> ignore
            writeBlock body nextIndent true builder
        | Statement.ExprStatement expr ->
            builder.AppendLine($"{branch}[Expr]") |> ignore
            writeExpr expr nextIndent true builder

    and private writeBlock (statements: Statement list) (indent: string) (isLast: bool) (builder: StringBuilder) =
        let branch = indent + (if isLast then "└── " else "├── ")
        let nextIndent = indent + (if isLast then "    " else "│   ")
        builder.AppendLine($"{branch}[Block]") |> ignore
        for i in 0 .. statements.Length - 1 do
            writeStatement statements[i] nextIndent (i = statements.Length - 1) builder

    let writeProgram (program: Program) =
        let builder = StringBuilder()
        builder.AppendLine("└── [Program]") |> ignore
        for i in 0 .. program.Statements.Length - 1 do
            writeStatement program.Statements[i] "    " (i = program.Statements.Length - 1) builder
        builder.ToString()
