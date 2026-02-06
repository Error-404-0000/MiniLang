open MiniLang.FSharp

[<EntryPoint>]
let main  none:int=
    let argv =
        [ @"C:\Users\Demon\source\repos\MiniLang\MiniLangGuide\MiniLang_Syntax_Guide\ClassCreation.mini.c" ]
    if argv.Length = 0 then
        eprintfn "Usage: minilang-fsharp <file.mini>"
        1
    else
        try
            MiniLangEngine.runFile argv[0] |> ignore
            0
        with
        | ParseException(message, line, col) ->
            eprintfn "Parse error at %d:%d - %s" line col message
            2
        | RuntimeException(message) ->
            eprintfn "Runtime error: %s" message
            3
