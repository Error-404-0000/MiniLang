# MiniLang.FSharp

A fresh F# reimplementation of MiniLang, created as a sub-repo style module inside this project.

## Included language features

- `make`, `say`, `give`, `if/else`, `while`, `fn`, `done`
- Arithmetic and comparison expressions
- Function calls and scoped execution
- `future(expr)` to create async values
- `await futureExpr` to resolve futures
- Built-in `sleep(ms)` and `str(value)`

## "More futures" additions

This implementation adds explicit asynchronous first-class values to the language runtime:

```ml
fn delayedAdd(a, b):
    make task = future(a + b)
    give await task
done

make result = delayedAdd(4, 6)
say result
```

You can also combine with `sleep` to orchestrate async work:

```ml
make waitJob = future(sleep(200))
await waitJob
say "async pipeline complete"
```

## Run

```bash
dotnet run --project src/MiniLang.FSharp.Cli -- examples/futures.mini
```
