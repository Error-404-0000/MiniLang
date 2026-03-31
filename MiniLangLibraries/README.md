# MiniLang Reusable Libraries

Reusable legacy MiniLang building blocks live here.

Folders:

- `Core` for app and shared helpers
- `Console` for console-oriented helpers
- `IO` for safe file and directory wrappers
- `Windows` for window/process helpers built on the approved `win` bridge

These files are intended to be used from legacy `.mini.c` programs with:

```text
use "MiniLangLibraries/Console/Console.mini.c";
```
