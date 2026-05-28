open MiniLang.FSharp

[<EntryPoint>]
let main s =
    let argv = [@"C:\Users\Demon\source\repos\MiniLangF#\MiniLangGuide\MiniLangSpaces\console.mini.py"]
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
