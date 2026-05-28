namespace MiniLang.FSharp

open System
open System.Globalization

module Parser =
    type private State =
        { Tokens: Token array
          mutable Position: int }

    let private current state = state.Tokens[state.Position]
    let private previous state = state.Tokens[state.Position - 1]
    let private peek state offset =
        let index = state.Position + offset
        if index >= state.Tokens.Length then
            state.Tokens[state.Tokens.Length - 1]
        else
            state.Tokens[index]

    let private advance state =
        if state.Position < state.Tokens.Length - 1 then
            state.Position <- state.Position + 1
        previous state

    let private check kind lexeme (state: State) =
        let token = current state
        token.Kind = kind && (lexeme = None || lexeme = Some token.Lexeme)

    let private matchToken kind lexeme state =
        if check kind lexeme state then
            ignore (advance state)
            true
        else
            false

    let private consume kind lexeme error state =
        if check kind lexeme state then
            advance state
        else
            let token = current state
            raise (ParseException(error, token.Line, token.Column))

    let private skipNewLines state =
        while matchToken TokenKind.NewLine None state || matchToken TokenKind.Semicolon None state do
            ()

    let rec private parsePrimary state =
        if matchToken TokenKind.Number None state then
            Expr.Literal(Value.Number(Double.Parse((previous state).Lexeme, CultureInfo.InvariantCulture)))
        elif matchToken TokenKind.Char None state then
            Expr.Literal(Value.Text((previous state).Lexeme))
        elif matchToken TokenKind.String None state then
            Expr.Literal(Value.Text((previous state).Lexeme))
        elif matchToken TokenKind.Identifier None state then
            let id = (previous state).Lexeme
            if matchToken TokenKind.LParen None state then
                let args = ResizeArray<Expr>()
                if not (check TokenKind.RParen None state) then
                    args.Add(parseExpression state)
                    while matchToken TokenKind.Comma None state do
                        args.Add(parseExpression state)
                ignore (consume TokenKind.RParen None "Expected ')' after call arguments" state)
                Expr.Call(id, List.ofSeq args)
            else
                Expr.Identifier id
        elif matchToken TokenKind.Keyword (Some "typeof") state then
            let nameToken = consume TokenKind.Identifier None "Expected identifier after typeof" state
            Expr.TypeOf nameToken.Lexeme
        elif matchToken TokenKind.Keyword (Some "await") state then
            Expr.Await(parsePrimary state)
        elif matchToken TokenKind.LParen None state then
            let expr = parseExpression state
            ignore (consume TokenKind.RParen None "Expected ')' after expression" state)
            expr
        else
            let token = current state
            raise (ParseException($"Expected expression but got '{token.Lexeme}'", token.Line, token.Column))

    and private parseExponent state =
        let mutable expr = parsePrimary state
        while check TokenKind.Operator None state && List.contains (current state).Lexeme   ["^"] do
            let op = (advance state).Lexeme
            let right = parsePrimary state
            expr <- Expr.Binary(expr, op, right)
        expr

    and private parseFactor state =
        let mutable expr = parseExponent state
        while check TokenKind.Operator None state && List.contains (current state).Lexeme  ["*"; "/"; "%"] do
            let op = (advance state).Lexeme
            let right = parseExponent state
            expr <- Expr.Binary(expr, op, right)
        expr

    and private parseTerm state =
        let mutable expr = parseFactor state
        while check TokenKind.Operator None state && List.contains (current state).Lexeme ["+"; "-"] do
            let op = (advance state).Lexeme
            let right = parseFactor state
            expr <- Expr.Binary(expr, op, right)
        expr

    and private parseComparison state =
        let mutable expr = parseTerm state
        while check TokenKind.Operator None state && List.contains (current state).Lexeme  ["=="; "!="; "<"; ">"; "<="; ">="] do
            let op = (advance state).Lexeme
            let right = parseTerm state
            expr <- Expr.Binary(expr, op, right)
        expr

    and private parseExpression state = parseComparison state

    let rec private parseBlock state =
        let statements = ResizeArray<Statement>()
        skipNewLines state
        while not (check TokenKind.Keyword (Some "done") state)
              && not (check TokenKind.Keyword (Some "else") state)
              && not (check TokenKind.Eof None state) do
            statements.Add(parseStatement state)
            skipNewLines state
        List.ofSeq statements

    and private parseFunction state =
        let name = (consume TokenKind.Identifier None "Expected function name" state).Lexeme
        ignore (consume TokenKind.LParen None "Expected '(' after function name" state)
        let parameters = ResizeArray<string>()
        if not (check TokenKind.RParen None state) then
            parameters.Add((consume TokenKind.Identifier None "Expected parameter name" state).Lexeme)
            while matchToken TokenKind.Comma None state do
                parameters.Add((consume TokenKind.Identifier None "Expected parameter name" state).Lexeme)
        ignore (consume TokenKind.RParen None "Expected ')' after parameters" state)
        ignore (consume TokenKind.Colon None "Expected ':' after function signature" state)
        skipNewLines state
        let body = parseBlock state
        ignore (consume TokenKind.Keyword (Some "done") "Expected 'done' to close function" state)
        Statement.Fn(name, List.ofSeq parameters, body)

    and private parseStatement state =
        if matchToken TokenKind.Keyword (Some "make") state then
            let name = (consume TokenKind.Identifier None "Expected variable name after make" state).Lexeme
            ignore (consume TokenKind.Operator (Some "=") "Expected '=' in make statement" state)
            Statement.Make(name, parseExpression state)
        elif matchToken TokenKind.Keyword (Some "use") state then
            let pathToken = consume TokenKind.String None "Expected string literal after use" state
            Statement.Use pathToken.Lexeme
        elif check TokenKind.Identifier None state
             && (peek state 1).Kind = TokenKind.Operator then
            let name = (advance state).Lexeme
            let op = (advance state).Lexeme
            if op = "++" || op = "--" then
                Statement.Shorten(name, op)
            else
                Statement.Set(name, op, parseExpression state)
        elif matchToken TokenKind.Keyword (Some "say") state then
            Statement.Say(parseExpression state)
        elif matchToken TokenKind.Keyword (Some "show") state then
            Statement.Say(parseExpression state)
        elif matchToken TokenKind.Keyword (Some "give") state then
            Statement.Give(parseExpression state)
        elif matchToken TokenKind.Keyword (Some "fn") state then
            parseFunction state
        elif matchToken TokenKind.Keyword (Some "while") state then
            ignore (consume TokenKind.LParen None "Expected '(' after while" state)
            let condition = parseExpression state
            ignore (consume TokenKind.RParen None "Expected ')' after while condition" state)
            ignore (consume TokenKind.Colon None "Expected ':' after while condition" state)
            skipNewLines state
            let body = parseBlock state
            ignore (consume TokenKind.Keyword (Some "done") "Expected 'done' after while body" state)
            Statement.While(condition, body)
        elif matchToken TokenKind.Keyword (Some "if") state then
            ignore (consume TokenKind.LParen None "Expected '(' after if" state)
            let condition = parseExpression state
            ignore (consume TokenKind.RParen None "Expected ')' after if condition" state)
            ignore (consume TokenKind.Colon None "Expected ':' after if condition" state)
            skipNewLines state
            let yesBody = parseBlock state
            let noBody =
                if matchToken TokenKind.Keyword (Some "else") state then
                    skipNewLines state
                    parseBlock state
                else
                    []
            ignore (consume TokenKind.Keyword (Some "done") "Expected 'done' after if block" state)
            Statement.If(condition, yesBody, noBody)
        else
            Statement.ExprStatement(parseExpression state)

    let parse (tokens: Token list) =
        let state = { Tokens = List.toArray tokens; Position = 0 }
        let statements = ResizeArray<Statement>()
        skipNewLines state
        while not (check TokenKind.Eof None state) do
            statements.Add(parseStatement state)
            skipNewLines state
        { Statements = List.ofSeq statements }
