using MiniLang.Debugger;
using MiniLang.GrammarsAnalyers;
using MiniLang.Interfaces;
using MiniLang.Interpreter;
using MiniLang.Parser;
using MiniLang.SyntaxObjects;
using MiniLang.Tokenilzer;
using MiniLang.TokenObjects;

var tokens = Tokenizer.Tokenize(@"
{
    use ""C:\Users\Demon\source\repos\MiniLang\MiniLang\MiniLang_Syntax_Guide\Variables.txt"";
}
make flag = 200;
flag = 200;
flag -= 200;
flag += 200;
");

var TokensParsed = Parser.Parse(tokens);
GrammarValidator grammerValidation = new GrammarValidator([new MakeGrammar(),new ConditionGrammar(),new SayGrammar(),new TypeofGrammar(),new UseGrammar()
    ,new SetterGrammar()]);
GrammarInterpreter grammerInterpreter = new GrammarInterpreter(grammerValidation, TokensParsed);
var nodes = grammerInterpreter.Interpret();
void PrintToken(Token token, int depth = 0)
{
    if (grammerValidation.ResolveAnalyser(token) is IGrammarAnalyser gm && gm is IDebugger db)
    {
        Console.WriteLine(db.ViewSelf(token, grammerValidation, depth));
    }
    if(token.TokenOperation is TokenOperation.use && token.Value is UseSyntaxObject useSyntax)
    {
        Console.WriteLine("[Use:: ");
        
            foreach (var child in useSyntax.Tokens)
            {
                PrintToken(child, depth + 1);
            }
        
        Console.WriteLine(" ]");
    }
    if (token.TokenType is TokenType.Scope && token.Value != null)
    {
        Console.WriteLine("[Scope:: ");
        if(token.Value is IEnumerable<Token> sub_tokens)
        {
            foreach (var child in sub_tokens)
            {
                PrintToken(child, depth + 1);
            }
        }
        Console.WriteLine(" ]");
    }
}

foreach (Token token in nodes)
{
    PrintToken(token);
}

//Debugger.WriteTree(TokenBuilder.BuildStructuredTokens(TokensParsed));