module MiniLang.FSharp.Tests

open System
open System.IO
open Expecto
open MiniLang.FSharp

let tokenize source =
    source
    |> Preprocessor.removeCommentLines
    |> Tokenizer.tokenize

let parse source =
    source
    |> tokenize
    |> Parser.parse

let run source =
    MiniLangEngine.run source

let captureOutput f =
    let original = Console.Out
    use writer = new StringWriter()
    Console.SetOut(writer)
    try
        f () |> ignore
        writer.ToString().Trim()
    finally
        Console.SetOut(original)

[<Tests>]
let tokenizerTests =
    testCase "Tokenizer merges dotted identifiers and handles chars" <| fun _ ->
        let source = "@@ comment\nmake user.name = 'a';"
        let tokens = tokenize source
        let lexemes = tokens |> List.map (fun token -> token.Lexeme)
        Expect.sequenceEqual lexemes [ "make"; "user.name"; "="; "a"; ";"; "<eof>" ] "Unexpected lexemes"

[<Tests>]
let parserTests =
    testCase "Parser recognizes setters and shortens" <| fun _ ->
        let program = parse "make count = 1\ncount += 2\ncount++"
        match program.Statements with
        | [ Statement.Make("count", _); Statement.Set("count", "+=", _); Statement.Shorten("count", "++") ] -> ()
        | other -> failtestf "Unexpected statements: %A" other

[<Tests>]
let runtimeTests =
    testCase "Runtime evaluates setters and say" <| fun _ ->
        let output =
            captureOutput (fun () ->
                run "make total = 1\ntotal += 2\ntotal++\nsay total" |> ignore)
        Expect.equal output "4" "Expected say output to match"

[<Tests>]
let debuggerTests =
    testCase "Debugger renders tree output" <| fun _ ->
        let program = parse "make x = 1 + 2"
        let tree = Debugger.writeProgram program
        let expected =
            String.concat "\n"
                [ "└── [Program]"
                  "    └── [Make x]"
                  "        └── [Binary +]"
                  "            ├── [Literal] Number 1"
                  "            └── [Literal] Number 2" ]
        Expect.equal (tree.Trim()) expected "Unexpected tree output"

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv (testList "MiniLang.FSharp" [ tokenizerTests; parserTests; runtimeTests; debuggerTests ])
