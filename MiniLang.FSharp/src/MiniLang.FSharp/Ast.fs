namespace MiniLang.FSharp

open System

[<RequireQualifiedAccess>]
type TokenKind =
    | Keyword
    | Identifier
    | Number
    | String
    | Operator
    | LParen
    | RParen
    | Colon
    | Comma
    | NewLine
    | Eof

[<CLIMutable>]
type Token =
    { Kind: TokenKind
      Lexeme: string
      Line: int
      Column: int }

[<RequireQualifiedAccess>]
type Value =
    | Number of float
    | Text of string
    | Bool of bool
    | Nothing
    | Future of Async<Value>

[<RequireQualifiedAccess>]
type Expr =
    | Literal of Value
    | Identifier of string
    | Binary of Expr * string * Expr
    | Call of string * Expr list
    | Await of Expr

[<RequireQualifiedAccess>]
type Statement =
    | Make of name: string * value: Expr
    | Say of Expr
    | Give of Expr
    | While of condition: Expr * body: Statement list
    | If of condition: Expr * yesBody: Statement list * noBody: Statement list
    | Fn of name: string * parameters: string list * body: Statement list
    | ExprStatement of Expr

[<CLIMutable>]
type Program =
    { Statements: Statement list }

[<CLIMutable>]
type FunctionValue =
    { Parameters: string list
      Body: Statement list }

exception ParseException of string * int * int
exception RuntimeException of string
