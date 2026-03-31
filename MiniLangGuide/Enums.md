# Enums

## What it is

MiniLang enums define a closed named set of values. In the legacy runtime, v1 enums are named enums with member access through `Type.Member`.

## Why it exists

Enums give the language a safer alternative to passing around raw strings or numbers for fixed choices.

## Syntax

```mini
enum Tone {
    Warm;
    Cool;
    Accent;
}
```

## Example: declaration and comparison

```mini
enum Tone {
    Warm;
    Cool;
    Accent;
}

fn nothing ShowTone(Tone selected){
    if(selected == Tone.Warm):
        say "Warm tone selected";
    else
        say "Other tone selected";
    done
}
```

## Example: passing enum values into functions

```mini
enum Mode {
    Debug;
    Release;
}

fn nothing Report(Mode active){
    if(active == Mode.Debug):
        say "Debug mode";
    else
        say "Release mode";
    done
}

fn nothing Main(){
    Report(Mode.Debug);
}
```

## Variations

- Use enum members in equality checks such as `selected == Tone.Warm`.
- Use enums as typed function parameters and return types.
- Reference members with the fully qualified `EnumName.Member` form.

## Pitfalls

- Duplicate enum members are rejected.
- Empty enums are rejected.
- Enum members must be named entries; data-carrying variants are not part of v1.
- Use the qualified member name like `Tone.Warm`, not just `Warm`.

## Best practices

- Keep enum names singular and descriptive.
- Use enums for closed choices that would otherwise become fragile string comparisons.
- Prefer enum parameters over free-form string flags when the accepted values are known ahead of time.

## Runnable sample

See `MiniLang_Syntax_Guide\EnumInterop.mini.c`.
