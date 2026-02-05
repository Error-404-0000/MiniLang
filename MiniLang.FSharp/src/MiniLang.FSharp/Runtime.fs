namespace MiniLang.FSharp

open System
open System.Collections.Generic

module Runtime =
    type private Scope(parent: Scope option) =
        let values = Dictionary<string, Value>()
        let functions = Dictionary<string, FunctionValue>()

        member _.TryGetValue(name: string) =
            match values.TryGetValue(name) with
            | true, value -> Some value
            | _ ->
                parent
                |> Option.bind (fun p -> p.TryGetValue(name))

        member _.SetValue(name: string, value: Value) =
            values[name] <- value

        member _.SetFunction(name: string, fn: FunctionValue) =
            functions[name] <- fn

        member _.TryGetFunction(name: string) =
            match functions.TryGetValue(name) with
            | true, value -> Some value
            | _ ->
                parent
                |> Option.bind (fun p -> p.TryGetFunction(name))

        member this.CreateChild() = Scope(Some this)

    let private asNumber = function
        | Value.Number n -> n
        | other -> raise (RuntimeException($"Expected number but got '{other}'"))

    let private asBool = function
        | Value.Bool b -> b
        | Value.Number n -> n <> 0.0
        | Value.Text t -> not (String.IsNullOrWhiteSpace(t))
        | Value.Nothing -> false
        | Value.Future _ -> true

    let rec private evalExpr (scope: Scope) (expr: Expr) : Async<Value> =
        async {
            match expr with
            | Expr.Literal value -> return value
            | Expr.Identifier name ->
                match scope.TryGetValue name with
                | Some value -> return value
                | None -> raise (RuntimeException($"Variable '{name}' not found"))
            | Expr.Await nested ->
                let! value = evalExpr scope nested
                match value with
                | Value.Future job -> return! job
                | _ -> raise (RuntimeException("await can only be used with future values"))
            | Expr.Binary(left, op, right) ->
                let! leftValue = evalExpr scope left
                let! rightValue = evalExpr scope right
                return
                    match op with
                    | "+" -> Value.Number(asNumber leftValue + asNumber rightValue)
                    | "-" -> Value.Number(asNumber leftValue - asNumber rightValue)
                    | "*" -> Value.Number(asNumber leftValue * asNumber rightValue)
                    | "/" -> Value.Number(asNumber leftValue / asNumber rightValue)
                    | "%" -> Value.Number(asNumber leftValue % asNumber rightValue)
                    | "==" -> Value.Bool(leftValue = rightValue)
                    | "!=" -> Value.Bool(leftValue <> rightValue)
                    | ">" -> Value.Bool(asNumber leftValue > asNumber rightValue)
                    | "<" -> Value.Bool(asNumber leftValue < asNumber rightValue)
                    | ">=" -> Value.Bool(asNumber leftValue >= asNumber rightValue)
                    | "<=" -> Value.Bool(asNumber leftValue <= asNumber rightValue)
                    | _ -> raise (RuntimeException($"Operator '{op}' is not supported"))
            | Expr.Call(name, args) ->
                match name with
                | "future" ->
                    if args.Length <> 1 then
                        raise (RuntimeException("future() expects exactly one expression"))
                    let delayed = evalExpr scope args[0]
                    return Value.Future delayed
                | "sleep" ->
                    if args.Length <> 1 then
                        raise (RuntimeException("sleep(ms) expects exactly one argument"))
                    let! msVal = evalExpr scope args[0]
                    do! Async.Sleep(int (asNumber msVal))
                    return Value.Nothing
                | "str" ->
                    if args.Length <> 1 then
                        raise (RuntimeException("str(x) expects exactly one argument"))
                    let! value = evalExpr scope args[0]
                    return Value.Text(value.ToString())
                | _ ->
                    match scope.TryGetFunction name with
                    | None -> raise (RuntimeException($"Function '{name}' not found"))
                    | Some fn ->
                        if fn.Parameters.Length <> args.Length then
                            raise (RuntimeException($"Function '{name}' expects {fn.Parameters.Length} arguments"))
                        let local = scope.CreateChild()
                        for (parameter, argExpr) in List.zip fn.Parameters args do
                            let! argVal = evalExpr scope argExpr
                            local.SetValue(parameter, argVal)
                        let! result = execBlock local fn.Body
                        return result |> Option.defaultValue Value.Nothing
        }

    and private execStatement (scope: Scope) (statement: Statement) : Async<Value option> =
        async {
            match statement with
            | Statement.Make(name, expr) ->
                let! value = evalExpr scope expr
                scope.SetValue(name, value)
                return None
            | Statement.Say expr ->
                let! value = evalExpr scope expr
                printfn "%A" value
                return None
            | Statement.Give expr ->
                let! value = evalExpr scope expr
                return Some value
            | Statement.ExprStatement expr ->
                let! _ = evalExpr scope expr
                return None
            | Statement.Fn(name, parameters, body) ->
                scope.SetFunction(name, { Parameters = parameters; Body = body })
                return None
            | Statement.While(condition, body) ->
                let mutable keepRunning = true
                let mutable shortCircuit: Value option = None
                while keepRunning && shortCircuit.IsNone do
                    let! shouldRun = evalExpr scope condition
                    if asBool shouldRun then
                        let! value = execBlock (scope.CreateChild()) body
                        shortCircuit <- value
                    else
                        keepRunning <- false
                return shortCircuit
            | Statement.If(condition, yesBody, noBody) ->
                let! test = evalExpr scope condition
                if asBool test then
                    return! execBlock (scope.CreateChild()) yesBody
                else
                    return! execBlock (scope.CreateChild()) noBody
        }

    and private execBlock (scope: Scope) (statements: Statement list) : Async<Value option> =
        async {
            let mutable result = None
            let mutable index = 0
            while index < statements.Length && result.IsNone do
                let! current = execStatement scope statements[index]
                result <- current
                index <- index + 1
            return result
        }

    let execute (program: Program) =
        async {
            let globalScope = Scope(None)
            let! result = execBlock globalScope program.Statements
            return result |> Option.defaultValue Value.Nothing
        }
