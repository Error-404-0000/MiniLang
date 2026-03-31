# Windows Interop

## What it is

MiniLang exposes Windows interop through the legacy `win` bridge. The bridge resolves only approved managed wrappers and user-mode Win32 calls.

## Why it exists

This gives MiniLang access to useful platform features without opening arbitrary native execution from script text.

## Syntax

```mini
win console SetTitle("MiniLang Window");
say win process GetCurrentProcessId();
say win time GetTickCount();
```

## Example: console and process information

```mini
fn nothing ShowIdentity(){
    win console SetTitle("MiniLang Window");
    say "Process id:";
    say win process GetCurrentProcessId();
}
```

## Example: timing and delay

```mini
fn nothing PauseForMoment(){
    say "Sleeping for 250ms";
    win time Sleep(250);
    say "Awake again";
}
```

## Example: dialog preview

```mini
fn nothing PreviewDialog(){
    win user MessageBox("MiniLang", "Interop bridge reached the Windows API safely");
}
```

## Supported bridge targets in this phase

- `win.console.SetTitle`
- `win.console.GetTitle`
- `win.process.GetCurrentProcessId`
- `win.time.Sleep`
- `win.time.GetTickCount`
- `win.user.MessageBox`

## Pitfalls

- Unsupported namespaces or function names are rejected with diagnostics.
- Argument counts and argument types are validated before dispatch.
- This bridge is user-mode only; it is not a general native escape hatch.

## Best practices

- Prefer the approved `win.*` bridge targets over ad hoc platform calls.
- Keep interop code isolated in small functions so the rest of your MiniLang code stays portable.
- Use `MessageBox` for preview/debug scenarios and `console` / `process` / `time` APIs for non-blocking runtime flows.

## Runnable sample

See `MiniLang_Syntax_Guide\WinInterop.mini.c` and `MiniLang_Syntax_Guide\EnumInterop.mini.c`.
