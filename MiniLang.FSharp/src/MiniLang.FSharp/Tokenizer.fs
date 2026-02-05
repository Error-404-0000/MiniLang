namespace MiniLang.FSharp

open System
open System.Globalization

module Tokenizer =
    let private keywords =
        set [ "make"; "say"; "give"; "if"; "else"; "done"; "while"; "fn"; "await" ]

    let private singleCharTokens =
        dict [ '(', TokenKind.LParen; ')', TokenKind.RParen; ':', TokenKind.Colon; ',', TokenKind.Comma ]

    let private isOperatorChar c =
        "+-*/%^=!<>".Contains(c)

    let private emit kind lexeme line col =
        { Kind = kind; Lexeme = lexeme; Line = line; Column = col }

    let tokenize (source: string) =
        let rec loop idx line col acc =
            if idx >= source.Length then
                List.rev (emit TokenKind.Eof "<eof>" line col :: acc)
            else
                let ch = source[idx]
                match ch with
                | ' ' | '\t' | '\r' -> loop (idx + 1) line (col + 1) acc
                | '\n' -> loop (idx + 1) (line + 1) 1 (emit TokenKind.NewLine "\\n" line col :: acc)
                | _ when singleCharTokens.ContainsKey(ch) ->
                    loop (idx + 1) line (col + 1) (emit singleCharTokens[ch] (string ch) line col :: acc)
                | '"' ->
                    let mutable i = idx + 1
                    let mutable buffer = ""
                    let mutable terminated = false
                    while i < source.Length && not terminated do
                        if source[i] = '"' then
                            terminated <- true
                        else
                            buffer <- buffer + string source[i]
                        i <- i + 1
                    if not terminated then
                        raise (ParseException("Unterminated string", line, col))
                    loop i line (col + (i - idx)) (emit TokenKind.String buffer line col :: acc)
                | _ when Char.IsDigit(ch) ->
                    let mutable i = idx
                    while i < source.Length && (Char.IsDigit(source[i]) || source[i] = '.') do
                        i <- i + 1
                    let lexeme = source.Substring(idx, i - idx)
                    let _ = Double.Parse(lexeme, CultureInfo.InvariantCulture)
                    loop i line (col + (i - idx)) (emit TokenKind.Number lexeme line col :: acc)
                | _ when Char.IsLetter(ch) || ch = '_' ->
                    let mutable i = idx
                    while i < source.Length && (Char.IsLetterOrDigit(source[i]) || source[i] = '_') do
                        i <- i + 1
                    let lexeme = source.Substring(idx, i - idx)
                    let kind = if keywords.Contains lexeme then TokenKind.Keyword else TokenKind.Identifier
                    loop i line (col + (i - idx)) (emit kind lexeme line col :: acc)
                | _ when isOperatorChar ch ->
                    let mutable i = idx + 1
                    if i < source.Length && isOperatorChar source[i] then
                        i <- i + 1
                    let lexeme = source.Substring(idx, i - idx)
                    loop i line (col + (i - idx)) (emit TokenKind.Operator lexeme line col :: acc)
                | _ ->
                    raise (ParseException($"Unexpected character '{ch}'", line, col))

        loop 0 1 1 []
