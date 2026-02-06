namespace MiniLang.FSharp

module Preprocessor =
    let removeCommentLines (source: string) =
        source.Split('\n')
        |> Array.filter (fun line -> not (line.TrimStart().StartsWith("@@")))
        |> String.concat "\n"
