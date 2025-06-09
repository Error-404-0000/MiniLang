# MiniLang

MiniLang is a small, token-based programming language I built completely from scratch. It's simple, but powerful enough to let you create real logic and functions with your own syntax. I wanted something light, clean, and understandable.

It uses scoped blocks, expressions, and is built on custom token parsing and runtime interpretation. It doesn't compile, it runs your code directly.

---

## ✨ Why I Made It

I made MiniLang because I wanted to learn how languages work,like how tokens get parsed, how scopes manage variables, and how code can evaluate itself. It's not built to be fancy or overly smart but just smart enough to make things feel real. And it actually runs.

Also, I love having full control over the syntax and runtime behavior, down to how things like `make x = 5` behave. It's kinda fun seeing something you created understand code.
---

## 🔤 The Syntax

MiniLang uses words and symbols to define actions. It's not based on C-like languages , it's just its own thing:

```ml
make x = 10;
make y = Add(x, 20);

if(x != 5):
    say "X is not 5";
else
    say "X is 5";
done
```

Some keywords:

* `make` — declare variables
* `say` — print values
* `give` — return a value
* `use` — import another file
* `fn` — create functions
* `done` — end blocks
* `typeof` - get the type of something(it's replaced with the actaul type before being executed)
---

## 🧠 How It Works

### Token-Based

The core of MiniLang is a tokenizer that reads your code and breaks it into meaningful pieces called **tokens**. These tokens are grouped into expressions and scopes.

### Interpreter

The interpreter reads those tokens and builds an abstract understanding of what your code is doing, like checking if an `if` block is valid, or what arguments a function has.

### Runtime

Finally, the **runtime engine** actually runs the tokens. It supports functions, scoped variables, math, string interpolation (like `"Hello $(name)! $(<expression>)"`), and logic operations.

---

## 🚀 What It Can Do

* Handle arithmetic and logic expressions
* Run user-defined functions with arguments
* Manage scope with full stack tracing
* Interpolated strings with embedded expressions
* Execute expressions like `Add(2, 3) * 5`
* Parse and check grammar correctness before runtime

---

## 💡 What Makes It Cool

* It has a fully working grammar and validation layer
* There's a runtime stack with actual variable resolution
* You can extend it with new commands by just writing a new `IExecutableToken`
* You can debug it with pretty tree output (via `IDebugger`)

---

## 🧪 Sample Code

```ml
fn number add(number a, number b):
    give a + b;
done

make result = add(5, 10);
say result;
make result1=add(result, ((result != 0)*7>2)*100);
```

---

## 📦 Structure

* `Tokenizer` — breaks source into raw tokens
* `Parser` — handles `()`, `{}`,': done' and block grouping
* `GrammarInterpreter` — turns groups into meaning and also create a dummy scope, for validation and other stuff
* `RuntimeEngine` — actually runs the code

---

## 🛠️ Built-In Features

* Built-in math operations: `+`, `-`, `*`, `/`, `^`, `%`
* Conditions: `==`, `!=`, `<`, `>`, `<=`, `>=`
* `and`, `or`, `not` logic
* Scope isolation for blocks and functions
* Variable type validation (`number`, `string`, `object`, `nothing`)

---

## 📘 How I Made It

I wrote everything , the tokenizer, interpreter, grammar rules, runtime, debugger, expression evaluator, and string interpolation handler. I even made the `fn` builder and `give` return system.

Everything was built with a goal: learn and make something that actually works.

---

## 💭 
## 🛠️ How New Features Are Built (with `say` as an Example)

it's really easy to create new keywords or operators — because everything is **token-driven**. Here's how the `say` keyword was added from start to finish.

---

### 1. Define the Token

Tokens are declared using an enum with a special `[ValueContainer]` attribute:

```csharp
[ValueContainer(true, "say")]
Function, // This allows 'say' to be parsed as a function keyword

[ValueContainer(false, "say")]
SayKeyword // This represents the actual operation to trigger later
```

* `true` in the first one tells the tokenizer "this also have a operator it links to
* `false` means it does not  have a operator,it is just a stand alone

---

### 2. Grammar: Validate Syntax

The `SayGrammar` class checks if `say` is used correctly — for example:

```ml
say "Hello!";
```

You define this rule using `IGrammarAnalyser`:

```csharp
public class SayGrammar : IGrammarAnalyser
{
    public TokenOperation[] TriggerTokensOperator => [TokenOperation.SayKeyword];
    public bool RequiresTermination => true; // must end with ";"

    public bool Analyse(Token[] tokens, out string errorMessage)
    {
        errorMessage = null;

        if (tokens.Length < 2)
        {
            errorMessage = "'say' must be followed by a value (like a string, identifier or an expression).";
            return true;
        }

        if (tokens[1].TokenType != TokenType.StringLiteralExpression &&
            tokens[1].TokenType != TokenType.Identifier &&
            tokens[1].TokenType != TokenType.Expression)
        {
            errorMessage = $"Invalid argument to 'say': {tokens[1].TokenType}";
            return true;
        }

        return false; // no error
    }

    public Token BuildNode(Token[] tokens,
        ScopeObjectValueManager scopeObjectValueManager,
        ExpressionGrammarAnalyser expressionGrammarAnalyser,
        FunctionDeclarationScopeManager FunctionDeclarationManager,
        IGrammarInterpreter grammarInterpreter,
        int line)
    {
        var args = tokens.Skip(1)
                         .TakeWhile(t => t.TokenType != TokenType.Semicolon)
                         .ToList();

        var sayFunction = new SayFunctionSyntaxObject
        {
            FunctionName = "say",
            ArgmentCounts = args.Count,
            Argments = args
        };

        return new Token(TokenType.Function, TokenOperation.SayKeyword, TokenTree.Single, sayFunction);
    }
}
```

---

### 3. Execution: Actually Run It

The `SayExecutable` class is what finally runs your `say` command at runtime:

```csharp
public class SayExecutable : IExecutableToken
{
    public TokenType[] InvokeType => [TokenType.Function];
    public TokenOperation[] InvokeOperation => [TokenOperation.SayKeyword];

    public RuntimeValue Dispatch(Token yourToken, RuntimeContext context)
    {
        if (yourToken.Value is not SayFunctionSyntaxObject say)
            throw new InvalidOperationException("Invalid 'say' payload.");

        var value = context.RuntimeExpressionEvaluator.Evaluate(say.Argments?.ToList() ?? []);
        Console.WriteLine(value.Value);
        return null;
    }
}
```

Now when you write:

```ml
say "hi there!";
say "this is a cool math expression $(2*4*2)";
say "2 * 2 is $(2+2)";
```

MiniLang will tokenize it, validate it using `SayGrammar`, convert it into a function call, and execute it via `SayExecutable`.

---


Want to create a new feature? Just:

1. Add a `[ValueContainer]` entry
2. Write a `Grammar` rule to validate
3. Add an `Executable` to run the logic

It’s like plug and play for custom behavior!

you can also see the syntax [here](https://github.com/Error-404-0000/MiniLang/tree/master/MiniLangGuide)
---

