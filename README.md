# MiniLang

MiniLang currently has two codepaths in the repository:

- the legacy English-like runtime in `MiniLang\MiniLang`
- the newer compiler-platform experiments under `src\*`

The active language/runtime for this phase is the legacy project in `MiniLang\MiniLang`. That is the path used by the CLI host, the guide samples, and MiniLang Studio.

## Active legacy runtime

The legacy runtime keeps the existing syntax style and now includes:

- `fn`, `struct`, `give`, `use`, `say`, `done`
- named `enum` declarations and enum member access
- safe `win` and `cscall` interop through approved managed wrappers
- a CLI host that can check, inspect, and run a file passed on the command line
- a Studio shell that is being stabilized around the legacy runtime rather than the newer compiler stack

## Legacy file and sample locations

- language runtime: `MiniLang\`
- CLI host: `MiniLangTest\`
- guide samples: `MiniLangGuide\MiniLang_Syntax_Guide\`
- reusable libraries: `MiniLangLibraries\`
- reusable project workspace samples: `MiniLangProjects\`
- Studio shell: `apps\MiniLang.Studio\`

New legacy samples added for this phase:

- `MiniLangGuide\MiniLang_Syntax_Guide\EnumInterop.mini.c`
- `MiniLangGuide\MiniLang_Syntax_Guide\WinInterop.mini.c`
- `MiniLangProjects\Workspace\App\StartupApp.mini.c`

Reusable legacy library folders:

- `MiniLangLibraries\Core\`
- `MiniLangLibraries\Console\`
- `MiniLangLibraries\IO\`
- `MiniLangLibraries\Windows\`

## Commands

### Legacy runtime CLI

```powershell
dotnet run --project MiniLangTest\MiniLangCLI.csproj -- check MiniLangGuide\MiniLang_Syntax_Guide\EnumInterop.mini.c
dotnet run --project MiniLangTest\MiniLangCLI.csproj -- inspect-json MiniLangGuide\MiniLang_Syntax_Guide\EnumInterop.mini.c
dotnet run --project MiniLangTest\MiniLangCLI.csproj -- run MiniLangGuide\MiniLang_Syntax_Guide\EnumInterop.mini.c
```

### MiniLang Studio

```powershell
dotnet build apps\MiniLang.Studio\MiniLang.Studio.csproj -p:Platform=x64
.\apps\MiniLang.Studio\bin\x64\Debug\net9.0-windows10.0.19041.0\win-x64\MiniLang.Studio.exe
```

### Newer compiler-platform work

The `src\*`, `apps\MiniLang.Cli`, and `site\minilang-docs` projects remain in the repo, but they are not the active runtime path for this stabilization phase.
