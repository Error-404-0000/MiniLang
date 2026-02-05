namespace MiniLang.FSharp

module MiniLangEngine =
    let run source =
        source
        |> Tokenizer.tokenize
        |> Parser.parse
        |> Runtime.execute
        |> Async.RunSynchronously

    let runFile path =
        let source = System.IO.File.ReadAllText(path)
        run source
