(globalThis["TURBOPACK"] || (globalThis["TURBOPACK"] = [])).push([typeof document === "object" ? document.currentScript : undefined,
"[project]/src/lib/docs.js [app-client] (ecmascript)", ((__turbopack_context__) => {
"use strict";

__turbopack_context__.s([
    "docsPages",
    ()=>docsPages,
    "getDocNeighbors",
    ()=>getDocNeighbors,
    "getDocPage",
    ()=>getDocPage
]);
const docsPages = [
    {
        slug: "getting-started",
        title: "Getting Started",
        section: "Learn",
        topic: "getting-started",
        summary: "Set up the legacy MiniLang runtime, open MiniLang Studio, and run your first real `.mini.c` program.",
        whatItIs: "Getting Started is the shortest path from an empty repo clone to a working MiniLang file you can check, run, and edit in MiniLang Studio.",
        whyItExists: "MiniLang is being developed as a real language environment, so the first step has to cover the runtime, CLI, Studio, and the actual file shape used by the legacy language today.",
        syntax: "MiniLang source files currently use the `.mini.c` extension. Entry points are regular functions such as `fn number Main(){ ... }`.",
        pitfalls: "Do not use the newer `.mini`, `.miniproj`, or `.miniws` examples from older experimental work when you are targeting the active legacy runtime in this repository.",
        bestPractices: "Start with one runnable file, keep `Main` simple, and use the CLI `check` command before adding interop or reusable libraries.",
        compilerView: "The legacy host tokenizes the file, groups scopes and expressions, validates grammar, interprets `use` imports, then executes the resulting token stream through the runtime engine.",
        examples: [
            {
                topic: "getting-started",
                title: "Smallest runnable program",
                code: `fn number Main(){\n    say "MiniLang ready";\n    give 0;\n}`,
                result: `MiniLang ready`
            },
            {
                topic: "getting-started",
                title: "Check a source file from the command line",
                code: `dotnet run --project MiniLangTest/MiniLangCLI.csproj -- check MiniLangGuide/MiniLang_Syntax_Guide/ArraysAndForeach.mini.c`
            }
        ]
    },
    {
        slug: "variables-make",
        title: "Variables and make",
        section: "Learn",
        topic: "variables-make",
        summary: "Use `make` to declare mutable variables and assign numbers, strings, arrays, and reusable function results.",
        whatItIs: "The `make` keyword introduces a variable into the current scope and optionally assigns a value immediately.",
        whyItExists: "MiniLang keeps local state explicit so scopes, assignment, and diagnostics stay easy to understand while the language grows.",
        syntax: "Use `make name = expression;` to declare a variable and `name = expression;` to update it later.",
        pitfalls: "Do not read a variable that was never declared, and remember that every `make` statement still needs a terminating semicolon.",
        bestPractices: "Initialize variables as close as possible to where they are used, and give loop accumulators simple names like `count`, `sum`, or `current`.",
        compilerView: "The `MakeGrammar` validates the declaration, records the symbol in the local scope model, and stores the runtime type inferred from the assigned value.",
        examples: [
            {
                topic: "variables-make",
                title: "Declare a number and update it",
                code: `fn number Main(){\n    make total = 10;\n    total = total + 5;\n    say total;\n    give 0;\n}`,
                result: `15`
            },
            {
                topic: "variables-make",
                title: "Store an array in a variable",
                code: `fn number Main(){\n    make values = [1, 2, 3];\n    say Length(values);\n    give 0;\n}`,
                result: `3`
            }
        ]
    },
    {
        slug: "functions",
        title: "Functions",
        section: "Learn",
        topic: "functions",
        summary: "Declare functions with `fn`, explicit return types, typed parameters, and `give` return values.",
        whatItIs: "Functions are MiniLang's reusable executable units. They can accept typed parameters, return typed values, and be imported through `use`.",
        whyItExists: "A real language needs stable reusable behavior, not only top-level statements in one file.",
        syntax: "Use `fn <return-type> Name(<params>){ ... }`. Return with `give expression;` or `give 0;` for numeric entry points.",
        pitfalls: "Do not omit a return value for a function that declares `number`, `string`, `array`, or another non-`nothing` type.",
        bestPractices: "Keep each function focused on one job and use explicit parameter types like `array values` when the expected input matters.",
        compilerView: "Function declarations are recorded in the declaration scope first, then their bodies are interpreted with parameter symbols injected into a child scope.",
        examples: [
            {
                topic: "functions",
                title: "Numeric return value",
                code: `fn number Add(number left, number right){\n    give left + right;\n}\n\nfn number Main(){\n    say Add(3, 4);\n    give 0;\n}`,
                result: `7`
            },
            {
                topic: "functions",
                title: "Array return value",
                code: `fn array BuildValues(){\n    give [10, 20, 30];\n}\n\nfn number Main(){\n    say Length(BuildValues());\n    give 0;\n}`,
                result: `3`
            }
        ]
    },
    {
        slug: "arrays",
        title: "Arrays",
        section: "Reference",
        topic: "arrays",
        summary: "MiniLang arrays are mutable, zero-based, dynamic collections that work with literals, indexing, and builtin helpers.",
        whatItIs: "The builtin `array` type stores ordered runtime values and supports literals, indexing, mutation, and array-specific helpers.",
        whyItExists: "Collections are necessary for real data work, reusable libraries, and iteration features like `foreach`.",
        syntax: "Use `[value1, value2]` for literals, `values[index]` to read, and `values[index] = expression;` to write.",
        pitfalls: "Indexes must be numbers, indexing a non-array value is an error, and out-of-range indexes fail at runtime.",
        bestPractices: "Use arrays for ordered data, keep indexes zero-based in your mental model, and prefer `Length(values)` over hard-coded limits.",
        compilerView: "Array literals and array index expressions are converted into dedicated syntax objects, validated by the expression analyser, and executed through `RuntimeArrayValue` at runtime.",
        examples: [
            {
                topic: "arrays",
                title: "Create and read an array",
                code: `fn number Main(){\n    make values = [5, 10, 15];\n    say values[0];\n    say values[2];\n    give 0;\n}`,
                result: `5\n15`
            },
            {
                topic: "arrays",
                title: "Mutate an array through indexing",
                code: `fn number Main(){\n    make values = [1, 2, 3];\n    values[1] = 42;\n    say values[1];\n    give 0;\n}`,
                result: `42`
            },
            {
                topic: "arrays",
                title: "Use the builtin array helpers",
                code: `fn number Main(){\n    make values = [1, 2];\n    Push(values, 3);\n    say Length(values);\n    say Contains(values, 2);\n    say Pop(values);\n    give 0;\n}`,
                result: `3\n1\n3`
            }
        ]
    },
    {
        slug: "foreach",
        title: "foreach",
        section: "Reference",
        topic: "foreach",
        summary: "Use legacy-style `foreach item in values:` loops to iterate through array values without manual indexing.",
        whatItIs: "The `foreach` statement binds one loop variable at a time and runs its body once for each element in an array expression.",
        whyItExists: "It gives MiniLang a practical iteration construct for collection code without forcing users to write manual `while` index loops for everything.",
        syntax: "Use `foreach item in values:` followed by one or more body statements and end the block with `done`.",
        pitfalls: "The loop target must be an array, and the current v1 form is value-only, so you do not get the index automatically.",
        bestPractices: "Use `foreach` when you care about each item more than the numeric position, and keep the loop variable short and descriptive.",
        compilerView: "The `ForeachGrammar` validates the loop header and emits a dedicated syntax object. The runtime snapshots the array and executes the loop body inside a fresh scope for each item.",
        examples: [
            {
                topic: "foreach",
                title: "Print every array item",
                code: `fn number Main(){\n    make values = [1, 2, 3];\n    foreach item in values:\n        say item;\n    done\n    give 0;\n}`,
                result: `1\n2\n3`
            },
            {
                topic: "foreach",
                title: "Accumulate a total",
                code: `fn number Main(){\n    make values = [10, 20, 30];\n    make total = 0;\n    foreach item in values:\n        total = total + item;\n    done\n    say total;\n    give 0;\n}`,
                result: `60`
            }
        ]
    },
    {
        slug: "operators-expressions",
        title: "Operators and expressions",
        section: "Reference",
        topic: "operators-expressions",
        summary: "Numbers, strings, comparisons, function calls, interop calls, and array expressions all participate in MiniLang expressions.",
        whatItIs: "Expressions are the values MiniLang can evaluate inside `make`, `say`, `if`, `while`, `give`, and array operations.",
        whyItExists: "A usable language needs a consistent value model across arithmetic, branching, array access, and function calls.",
        syntax: "MiniLang supports `+`, `-`, `*`, `/`, `%`, `^`, `==`, `!=`, `<`, `>`, `<=`, and `>=` with grouped expressions in parentheses.",
        pitfalls: "Do not mix invalid operand types, and remember that array indexes are expressions too, so invalid index types surface as expression diagnostics.",
        bestPractices: "Keep nested expressions readable with parentheses and split very long calculations into named helper variables.",
        compilerView: "The runtime expression evaluator parses precedence, resolves identifiers and function calls, and applies operators over numbers, strings, enums, and arrays where supported.",
        examples: [
            {
                topic: "operators-expressions",
                title: "Arithmetic and precedence",
                code: `fn number Main(){\n    say 2 + 3 * 4;\n    say (2 + 3) * 4;\n    give 0;\n}`,
                result: `14\n20`
            },
            {
                topic: "operators-expressions",
                title: "Use an indexed array value in an expression",
                code: `fn number Main(){\n    make values = [5, 6, 7];\n    say values[0] + values[2];\n    give 0;\n}`,
                result: `12`
            }
        ]
    },
    {
        slug: "enums",
        title: "Enums",
        section: "Reference",
        topic: "enums",
        summary: "Enums provide named symbolic values with member access like `Tone.Warm` and equality comparisons.",
        whatItIs: "An enum declares a closed set of named values that MiniLang can resolve, compare, and surface in completion and hover.",
        whyItExists: "Enums make state and modes easier to read than raw numbers or strings, especially once functions and interop wrappers grow.",
        syntax: "Declare an enum with `enum Name { ... }` and access members with `Name.Member`.",
        pitfalls: "Do not declare duplicate members, and do not compare enum values using arithmetic operators.",
        bestPractices: "Use enums for modes, states, and named options that should stay closed and documented.",
        compilerView: "Enum members are registered into the enum frame so expressions can resolve `Name.Member` as real runtime values instead of plain dead identifiers.",
        examples: [
            {
                topic: "enums",
                title: "Declare and compare enum members",
                code: `enum Tone {\n    Warm;\n    Cool;\n}\n\nfn number Main(){\n    if(Tone.Warm == Tone.Warm):\n        say "same";\n    else\n        say "different";\n    done\n    give 0;\n}`,
                result: `same`
            },
            {
                topic: "enums",
                title: "Pass an enum to a function",
                code: `enum BuildMode {\n    Debug;\n    Release;\n}\n\nfn nothing ShowMode(BuildMode mode){\n    say mode;\n}\n\nfn number Main(){\n    ShowMode(BuildMode.Release);\n    give 0;\n}`,
                result: `BuildMode.Release`
            }
        ]
    },
    {
        slug: "structs",
        title: "Structs",
        section: "Reference",
        topic: "structs",
        summary: "Structs group related fields and constructor-style initialization behind MiniLang's existing object model.",
        whatItIs: "Structs are MiniLang's current grouped data declaration form for named fields and reusable data shapes.",
        whyItExists: "They let the language express richer data than separate loose variables while staying readable in the legacy syntax model.",
        syntax: "Declare a struct with `struct Name { ... }` and create one with `make item = new Name;`.",
        pitfalls: "Struct support in the legacy runtime is older than arrays and enums, so keep field usage straightforward and test imported struct helpers carefully.",
        bestPractices: "Use structs for bundled data, keep field names simple, and keep logic in functions rather than overloading the struct declaration itself.",
        compilerView: "Struct declarations are registered in the struct table and resolved during `new` object creation at runtime.",
        examples: [
            {
                topic: "structs",
                title: "Declare a simple struct",
                code: `struct AppWindow {\n    public Title;\n    public Width;\n}`
            },
            {
                topic: "structs",
                title: "Construct a struct value",
                code: `fn number Main(){\n    make window = new AppWindow;\n    give 0;\n}`
            }
        ]
    },
    {
        slug: "use-libraries",
        title: "use and reusable libraries",
        section: "Tooling",
        topic: "use-libraries",
        summary: "Import reusable `.mini.c` files with `use` and organize shared code under `MiniLangLibraries`.",
        whatItIs: "The `use` statement imports another MiniLang source file and injects its declarations into the current compilation flow.",
        whyItExists: "It gives the language a real reusable code path instead of forcing every app to live in one file.",
        syntax: "Use `use \"MiniLangLibraries/Console/Console.mini.c\";` or a relative path like `use \"../libs/Greeter.mini.c\";`.",
        pitfalls: "Imported file paths must point to real files, and confusing relative paths can still create fragile code even with the smarter path resolver.",
        bestPractices: "Put shared helpers under `MiniLangLibraries`, runnable entry files under `MiniLangProjects`, and keep imports explicit and readable.",
        compilerView: "The use-path context tracks the importing file, resolves the referenced file, parses it, then injects the imported tokens into the active interpreted token stream.",
        examples: [
            {
                topic: "use-libraries",
                title: "Import reusable console helpers",
                code: `use "MiniLangLibraries/Console/Console.mini.c";\n\nfn number Main(){\n    ConsoleWriteLine("ready");\n    give 0;\n}`,
                result: `ready`
            },
            {
                topic: "use-libraries",
                title: "Import the reusable array helpers",
                code: `use "MiniLangLibraries/Collections/ArrayTools.mini.c";\n\nfn number Main(){\n    make values = BuildReleaseNumbers();\n    say SumValues(values);\n    give 0;\n}`,
                result: `12`
            }
        ]
    },
    {
        slug: "windows-interop",
        title: "Windows interop",
        section: "Interop",
        topic: "windows-interop",
        summary: "Use the safe `win` bridge to call approved user-mode Windows helpers from MiniLang code.",
        whatItIs: "Windows interop in the active legacy runtime is a curated bridge over managed wrappers for approved user-mode APIs and system helpers.",
        whyItExists: "It gives MiniLang a serious platform direction without opening the door to unsafe kernel, bypass, or exploit-oriented behavior.",
        syntax: "Use `win <namespace> <Function>(...)` such as `win process GetCurrentProcessId()` or `win console SetTitle(\"MiniLang\")`.",
        pitfalls: "Do not expect arbitrary namespaces or functions to work; only registered bridge namespaces and functions are valid.",
        bestPractices: "Keep interop behind small wrapper functions, prefer user-mode console, process, time, IO, and user APIs, and validate behavior through the CLI before wiring it into larger apps.",
        compilerView: "Interop calls are lowered into `CSharpCallSyntaxObject` values and executed through the managed interop bridge registry at runtime.",
        examples: [
            {
                topic: "windows-interop",
                title: "Read process information",
                code: `fn number Main(){\n    say win process GetCurrentProcessId();\n    give 0;\n}`
            },
            {
                topic: "windows-interop",
                title: "Change the console title",
                code: `fn number Main(){\n    say win console SetTitle("MiniLang Legacy Runtime");\n    give 0;\n}`
            }
        ]
    },
    {
        slug: "cli",
        title: "CLI",
        section: "Tooling",
        topic: "cli",
        summary: "The legacy MiniLang CLI checks, runs, and inspects `.mini.c` files from the command line.",
        whatItIs: "The CLI is the scripted interface to the active legacy runtime and analysis host.",
        whyItExists: "A serious language project needs a repeatable non-IDE workflow for checking programs, running samples, and inspecting diagnostics as JSON.",
        syntax: "Current commands are `check`, `check-json`, `run`, `run-json`, and `inspect-json`.",
        pitfalls: "Point the CLI at a real source file path. If you run commands from the wrong folder, relative imports can still confuse you even though `use` resolution is smarter now.",
        bestPractices: "Use `check` during editing, `run` for quick runtime smoke tests, and `inspect-json` when working on Studio integration or tooling.",
        compilerView: "The CLI forwards to `LegacyMiniLangHost`, which builds the same analysis and runtime data used by Studio.",
        examples: [
            {
                topic: "cli",
                title: "Check a source file",
                code: `dotnet run --project MiniLangTest/MiniLangCLI.csproj -- check MiniLangGuide/MiniLang_Syntax_Guide/ArraysAndForeach.mini.c`
            },
            {
                topic: "cli",
                title: "Run a source file as JSON",
                code: `dotnet run --project MiniLangTest/MiniLangCLI.csproj -- run-json MiniLangProjects/Workspace/App/CollectionApp.mini.c`
            }
        ]
    },
    {
        slug: "studio",
        title: "MiniLang Studio",
        section: "Tooling",
        topic: "studio",
        summary: "MiniLang Studio is the WinUI host plus Monaco-based IDE shell for editing, diagnostics, startup files, and build/run workflows.",
        whatItIs: "Studio is the desktop IDE for the current MiniLang runtime: explorer, Monaco editor, diagnostics, inspector panes, startup-file workflow, and Debug/Release runs.",
        whyItExists: "A serious language ecosystem needs a first-party editing environment that understands the language rather than a blank text box.",
        syntax: "Open a `.mini.c` file, edit in Monaco, use the explorer to set a startup file, and run in Debug or Release from the toolbar.",
        pitfalls: "Do not assume the active editor tab is always the startup file; if you have set a startup file in Explorer, Build and Run use that file.",
        bestPractices: "Set one startup file per runnable workspace area, keep reusable library files non-startup, and use the diagnostics list to jump directly to source problems.",
        compilerView: "Studio asks the host for analysis, completions, hover, definitions, build results, and runtime output, then renders those through Monaco and structured tool panes.",
        examples: [
            {
                topic: "studio",
                title: "Launch Studio from PowerShell",
                code: `& "C:\\Users\\Demon\\source\\repos\\MiniLang\\run-studio.ps1"`
            },
            {
                topic: "studio",
                title: "Release run behavior",
                code: `Config: Release\nRun -> save startup file -> build MiniLangCLI Release -> launch external console window`
            }
        ]
    },
    {
        slug: "diagnostics",
        title: "Diagnostics",
        section: "Tooling",
        topic: "diagnostics",
        summary: "MiniLang diagnostics explain syntax, scope, interop, and collection mistakes in both the CLI and Studio.",
        whatItIs: "Diagnostics are the structured errors and warnings returned by the analysis and runtime hosts when MiniLang code is invalid.",
        whyItExists: "Once a language grows past toy examples, users need source-linked feedback rather than vague exceptions.",
        syntax: "Diagnostics currently surface with an id, severity, message, line, column, start offset, and length.",
        pitfalls: "Do not ignore analysis failures just because the runtime still builds; syntax and scope diagnostics usually point at real broken behavior.",
        bestPractices: "Fix the first syntax or scope error before chasing follow-up diagnostics, and use `inspect-json` when you want the raw diagnostic payload.",
        compilerView: "The legacy host catches interpreter and runtime exceptions, maps them to source offsets, then sends the normalized diagnostics to the CLI and Studio.",
        examples: [
            {
                topic: "diagnostics",
                title: "Indexing a non-array value",
                code: `fn number Main(){\n    make value = 5;\n    say value[0];\n    give 0;\n}`,
                result: `Error ML0001: Cannot index non-array target 'value'.`
            },
            {
                topic: "diagnostics",
                title: "Foreach over a non-array value",
                code: `fn number Main(){\n    make value = 5;\n    foreach item in value:\n        say item;\n    done\n    give 0;\n}`,
                result: `Error ML0001: Cannot foreach over non-array target 'value'.`
            }
        ]
    },
    {
        slug: "compiler-internals",
        title: "Compiler internals",
        section: "Internals",
        topic: "compiler-internals",
        summary: "How the active legacy MiniLang pipeline tokenizes, groups, validates, interprets, and executes source files today.",
        whatItIs: "Compiler Internals documents the actual architecture of the legacy runtime that powers the current language, CLI, and Studio.",
        whyItExists: "The codebase is growing into a larger ecosystem, so the real pipeline needs to be understandable and documented instead of hidden behind vague marketing language.",
        syntax: "The current pipeline is: raw source -> tokenizer -> parser grouping -> structured token builder -> grammar validation/interpreter -> runtime engine.",
        pitfalls: "Do not confuse the legacy runtime path with the separate experimental newer compiler stack elsewhere in the repository.",
        bestPractices: "When adding a language feature, update tokenization, grouping, validation, runtime execution, tests, and docs together.",
        compilerView: "Arrays and foreach follow the same rule: they start as tokens, become structured syntax objects, are validated in grammar/expression analysis, then execute through runtime-specific handlers.",
        examples: [
            {
                topic: "compiler-internals",
                title: "Legacy analysis flow",
                code: `Source text -> Tokenizer -> Parser -> TokenBuilder -> GrammarInterpreter -> LegacyMiniLangHost.AnalyzeSource`
            },
            {
                topic: "compiler-internals",
                title: "Legacy runtime flow",
                code: `Interpreted tokens -> RuntimeEngine -> RuntimeExpressionEvaluator -> builtin executables / user functions / interop bridge`
            }
        ]
    }
];
function getDocPage(slug) {
    return docsPages.find((page)=>page.slug === slug);
}
function getDocNeighbors(slug) {
    const index = docsPages.findIndex((page)=>page.slug === slug);
    return {
        previous: index > 0 ? docsPages[index - 1] : null,
        next: index >= 0 && index < docsPages.length - 1 ? docsPages[index + 1] : null
    };
}
if (typeof globalThis.$RefreshHelpers$ === 'object' && globalThis.$RefreshHelpers !== null) {
    __turbopack_context__.k.registerExports(__turbopack_context__.m, globalThis.$RefreshHelpers$);
}
}),
"[project]/src/components/SearchPanel.tsx [app-client] (ecmascript)", ((__turbopack_context__) => {
"use strict";

__turbopack_context__.s([
    "SearchPanel",
    ()=>SearchPanel
]);
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$compiled$2f$react$2f$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/node_modules/next/dist/compiled/react/jsx-dev-runtime.js [app-client] (ecmascript)");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$client$2f$app$2d$dir$2f$link$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/node_modules/next/dist/client/app-dir/link.js [app-client] (ecmascript)");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$compiled$2f$react$2f$index$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/node_modules/next/dist/compiled/react/index.js [app-client] (ecmascript)");
var __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$docs$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/src/lib/docs.js [app-client] (ecmascript)");
;
var _s = __turbopack_context__.k.signature();
"use client";
;
;
;
function SearchPanel() {
    _s();
    const [query, setQuery] = (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$compiled$2f$react$2f$index$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["useState"])("");
    const results = (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$compiled$2f$react$2f$index$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["useMemo"])({
        "SearchPanel.useMemo[results]": ()=>{
            const lowered = query.trim().toLowerCase();
            if (!lowered) {
                return __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$docs$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["docsPages"].slice(0, 6);
            }
            return __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$docs$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["docsPages"].filter({
                "SearchPanel.useMemo[results]": (page)=>`${page.title} ${page.summary} ${page.section} ${page.topic}`.toLowerCase().includes(lowered)
            }["SearchPanel.useMemo[results]"]);
        }
    }["SearchPanel.useMemo[results]"], [
        query
    ]);
    return /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$compiled$2f$react$2f$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["jsxDEV"])("section", {
        className: "search-panel",
        children: [
            /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$compiled$2f$react$2f$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["jsxDEV"])("label", {
                className: "search-label",
                htmlFor: "docs-search",
                children: "Search the official docs"
            }, void 0, false, {
                fileName: "[project]/src/components/SearchPanel.tsx",
                lineNumber: 23,
                columnNumber: 7
            }, this),
            /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$compiled$2f$react$2f$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["jsxDEV"])("input", {
                id: "docs-search",
                className: "search-input",
                value: query,
                onChange: (event)=>setQuery(event.target.value),
                placeholder: "Search modules, attributes, interop, IDE, CLI..."
            }, void 0, false, {
                fileName: "[project]/src/components/SearchPanel.tsx",
                lineNumber: 26,
                columnNumber: 7
            }, this),
            /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$compiled$2f$react$2f$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["jsxDEV"])("div", {
                className: "search-results",
                children: results.map((page)=>/*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$compiled$2f$react$2f$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["jsxDEV"])(__TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$client$2f$app$2d$dir$2f$link$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["default"], {
                        href: `/docs/${page.slug}`,
                        className: "search-result",
                        children: [
                            /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$compiled$2f$react$2f$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["jsxDEV"])("span", {
                                children: page.title
                            }, void 0, false, {
                                fileName: "[project]/src/components/SearchPanel.tsx",
                                lineNumber: 36,
                                columnNumber: 13
                            }, this),
                            /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$compiled$2f$react$2f$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__["jsxDEV"])("small", {
                                children: page.section
                            }, void 0, false, {
                                fileName: "[project]/src/components/SearchPanel.tsx",
                                lineNumber: 37,
                                columnNumber: 13
                            }, this)
                        ]
                    }, page.slug, true, {
                        fileName: "[project]/src/components/SearchPanel.tsx",
                        lineNumber: 35,
                        columnNumber: 11
                    }, this))
            }, void 0, false, {
                fileName: "[project]/src/components/SearchPanel.tsx",
                lineNumber: 33,
                columnNumber: 7
            }, this)
        ]
    }, void 0, true, {
        fileName: "[project]/src/components/SearchPanel.tsx",
        lineNumber: 22,
        columnNumber: 5
    }, this);
}
_s(SearchPanel, "yY/BQ2gaUJJqUyGV4dDwTIt5KVc=");
_c = SearchPanel;
var _c;
__turbopack_context__.k.register(_c, "SearchPanel");
if (typeof globalThis.$RefreshHelpers$ === 'object' && globalThis.$RefreshHelpers !== null) {
    __turbopack_context__.k.registerExports(__turbopack_context__.m, globalThis.$RefreshHelpers$);
}
}),
"[project]/node_modules/next/dist/compiled/react/cjs/react-jsx-dev-runtime.development.js [app-client] (ecmascript)", ((__turbopack_context__, module, exports) => {
"use strict";

var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$build$2f$polyfills$2f$process$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__ = /*#__PURE__*/ __turbopack_context__.i("[project]/node_modules/next/dist/build/polyfills/process.js [app-client] (ecmascript)");
/**
 * @license React
 * react-jsx-dev-runtime.development.js
 *
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */ "use strict";
"production" !== ("TURBOPACK compile-time value", "development") && function() {
    function getComponentNameFromType(type) {
        if (null == type) return null;
        if ("function" === typeof type) return type.$$typeof === REACT_CLIENT_REFERENCE ? null : type.displayName || type.name || null;
        if ("string" === typeof type) return type;
        switch(type){
            case REACT_FRAGMENT_TYPE:
                return "Fragment";
            case REACT_PROFILER_TYPE:
                return "Profiler";
            case REACT_STRICT_MODE_TYPE:
                return "StrictMode";
            case REACT_SUSPENSE_TYPE:
                return "Suspense";
            case REACT_SUSPENSE_LIST_TYPE:
                return "SuspenseList";
            case REACT_ACTIVITY_TYPE:
                return "Activity";
            case REACT_VIEW_TRANSITION_TYPE:
                return "ViewTransition";
        }
        if ("object" === typeof type) switch("number" === typeof type.tag && console.error("Received an unexpected object in getComponentNameFromType(). This is likely a bug in React. Please file an issue."), type.$$typeof){
            case REACT_PORTAL_TYPE:
                return "Portal";
            case REACT_CONTEXT_TYPE:
                return type.displayName || "Context";
            case REACT_CONSUMER_TYPE:
                return (type._context.displayName || "Context") + ".Consumer";
            case REACT_FORWARD_REF_TYPE:
                var innerType = type.render;
                type = type.displayName;
                type || (type = innerType.displayName || innerType.name || "", type = "" !== type ? "ForwardRef(" + type + ")" : "ForwardRef");
                return type;
            case REACT_MEMO_TYPE:
                return innerType = type.displayName || null, null !== innerType ? innerType : getComponentNameFromType(type.type) || "Memo";
            case REACT_LAZY_TYPE:
                innerType = type._payload;
                type = type._init;
                try {
                    return getComponentNameFromType(type(innerType));
                } catch (x) {}
        }
        return null;
    }
    function testStringCoercion(value) {
        return "" + value;
    }
    function checkKeyStringCoercion(value) {
        try {
            testStringCoercion(value);
            var JSCompiler_inline_result = !1;
        } catch (e) {
            JSCompiler_inline_result = !0;
        }
        if (JSCompiler_inline_result) {
            JSCompiler_inline_result = console;
            var JSCompiler_temp_const = JSCompiler_inline_result.error;
            var JSCompiler_inline_result$jscomp$0 = "function" === typeof Symbol && Symbol.toStringTag && value[Symbol.toStringTag] || value.constructor.name || "Object";
            JSCompiler_temp_const.call(JSCompiler_inline_result, "The provided key is an unsupported type %s. This value must be coerced to a string before using it here.", JSCompiler_inline_result$jscomp$0);
            return testStringCoercion(value);
        }
    }
    function getTaskName(type) {
        if (type === REACT_FRAGMENT_TYPE) return "<>";
        if ("object" === typeof type && null !== type && type.$$typeof === REACT_LAZY_TYPE) return "<...>";
        try {
            var name = getComponentNameFromType(type);
            return name ? "<" + name + ">" : "<...>";
        } catch (x) {
            return "<...>";
        }
    }
    function getOwner() {
        var dispatcher = ReactSharedInternals.A;
        return null === dispatcher ? null : dispatcher.getOwner();
    }
    function UnknownOwner() {
        return Error("react-stack-top-frame");
    }
    function hasValidKey(config) {
        if (hasOwnProperty.call(config, "key")) {
            var getter = Object.getOwnPropertyDescriptor(config, "key").get;
            if (getter && getter.isReactWarning) return !1;
        }
        return void 0 !== config.key;
    }
    function defineKeyPropWarningGetter(props, displayName) {
        function warnAboutAccessingKey() {
            specialPropKeyWarningShown || (specialPropKeyWarningShown = !0, console.error("%s: `key` is not a prop. Trying to access it will result in `undefined` being returned. If you need to access the same value within the child component, you should pass it as a different prop. (https://react.dev/link/special-props)", displayName));
        }
        warnAboutAccessingKey.isReactWarning = !0;
        Object.defineProperty(props, "key", {
            get: warnAboutAccessingKey,
            configurable: !0
        });
    }
    function elementRefGetterWithDeprecationWarning() {
        var componentName = getComponentNameFromType(this.type);
        didWarnAboutElementRef[componentName] || (didWarnAboutElementRef[componentName] = !0, console.error("Accessing element.ref was removed in React 19. ref is now a regular prop. It will be removed from the JSX Element type in a future release."));
        componentName = this.props.ref;
        return void 0 !== componentName ? componentName : null;
    }
    function ReactElement(type, key, props, owner, debugStack, debugTask) {
        var refProp = props.ref;
        type = {
            $$typeof: REACT_ELEMENT_TYPE,
            type: type,
            key: key,
            props: props,
            _owner: owner
        };
        null !== (void 0 !== refProp ? refProp : null) ? Object.defineProperty(type, "ref", {
            enumerable: !1,
            get: elementRefGetterWithDeprecationWarning
        }) : Object.defineProperty(type, "ref", {
            enumerable: !1,
            value: null
        });
        type._store = {};
        Object.defineProperty(type._store, "validated", {
            configurable: !1,
            enumerable: !1,
            writable: !0,
            value: 0
        });
        Object.defineProperty(type, "_debugInfo", {
            configurable: !1,
            enumerable: !1,
            writable: !0,
            value: null
        });
        Object.defineProperty(type, "_debugStack", {
            configurable: !1,
            enumerable: !1,
            writable: !0,
            value: debugStack
        });
        Object.defineProperty(type, "_debugTask", {
            configurable: !1,
            enumerable: !1,
            writable: !0,
            value: debugTask
        });
        Object.freeze && (Object.freeze(type.props), Object.freeze(type));
        return type;
    }
    function jsxDEVImpl(type, config, maybeKey, isStaticChildren, debugStack, debugTask) {
        var children = config.children;
        if (void 0 !== children) if (isStaticChildren) if (isArrayImpl(children)) {
            for(isStaticChildren = 0; isStaticChildren < children.length; isStaticChildren++)validateChildKeys(children[isStaticChildren]);
            Object.freeze && Object.freeze(children);
        } else console.error("React.jsx: Static children should always be an array. You are likely explicitly calling React.jsxs or React.jsxDEV. Use the Babel transform instead.");
        else validateChildKeys(children);
        if (hasOwnProperty.call(config, "key")) {
            children = getComponentNameFromType(type);
            var keys = Object.keys(config).filter(function(k) {
                return "key" !== k;
            });
            isStaticChildren = 0 < keys.length ? "{key: someKey, " + keys.join(": ..., ") + ": ...}" : "{key: someKey}";
            didWarnAboutKeySpread[children + isStaticChildren] || (keys = 0 < keys.length ? "{" + keys.join(": ..., ") + ": ...}" : "{}", console.error('A props object containing a "key" prop is being spread into JSX:\n  let props = %s;\n  <%s {...props} />\nReact keys must be passed directly to JSX without using spread:\n  let props = %s;\n  <%s key={someKey} {...props} />', isStaticChildren, children, keys, children), didWarnAboutKeySpread[children + isStaticChildren] = !0);
        }
        children = null;
        void 0 !== maybeKey && (checkKeyStringCoercion(maybeKey), children = "" + maybeKey);
        hasValidKey(config) && (checkKeyStringCoercion(config.key), children = "" + config.key);
        if ("key" in config) {
            maybeKey = {};
            for(var propName in config)"key" !== propName && (maybeKey[propName] = config[propName]);
        } else maybeKey = config;
        children && defineKeyPropWarningGetter(maybeKey, "function" === typeof type ? type.displayName || type.name || "Unknown" : type);
        return ReactElement(type, children, maybeKey, getOwner(), debugStack, debugTask);
    }
    function validateChildKeys(node) {
        isValidElement(node) ? node._store && (node._store.validated = 1) : "object" === typeof node && null !== node && node.$$typeof === REACT_LAZY_TYPE && ("fulfilled" === node._payload.status ? isValidElement(node._payload.value) && node._payload.value._store && (node._payload.value._store.validated = 1) : node._store && (node._store.validated = 1));
    }
    function isValidElement(object) {
        return "object" === typeof object && null !== object && object.$$typeof === REACT_ELEMENT_TYPE;
    }
    var React = __turbopack_context__.r("[project]/node_modules/next/dist/compiled/react/index.js [app-client] (ecmascript)"), REACT_ELEMENT_TYPE = Symbol.for("react.transitional.element"), REACT_PORTAL_TYPE = Symbol.for("react.portal"), REACT_FRAGMENT_TYPE = Symbol.for("react.fragment"), REACT_STRICT_MODE_TYPE = Symbol.for("react.strict_mode"), REACT_PROFILER_TYPE = Symbol.for("react.profiler"), REACT_CONSUMER_TYPE = Symbol.for("react.consumer"), REACT_CONTEXT_TYPE = Symbol.for("react.context"), REACT_FORWARD_REF_TYPE = Symbol.for("react.forward_ref"), REACT_SUSPENSE_TYPE = Symbol.for("react.suspense"), REACT_SUSPENSE_LIST_TYPE = Symbol.for("react.suspense_list"), REACT_MEMO_TYPE = Symbol.for("react.memo"), REACT_LAZY_TYPE = Symbol.for("react.lazy"), REACT_ACTIVITY_TYPE = Symbol.for("react.activity"), REACT_VIEW_TRANSITION_TYPE = Symbol.for("react.view_transition"), REACT_CLIENT_REFERENCE = Symbol.for("react.client.reference"), ReactSharedInternals = React.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE, hasOwnProperty = Object.prototype.hasOwnProperty, isArrayImpl = Array.isArray, createTask = console.createTask ? console.createTask : function() {
        return null;
    };
    React = {
        react_stack_bottom_frame: function(callStackForError) {
            return callStackForError();
        }
    };
    var specialPropKeyWarningShown;
    var didWarnAboutElementRef = {};
    var unknownOwnerDebugStack = React.react_stack_bottom_frame.bind(React, UnknownOwner)();
    var unknownOwnerDebugTask = createTask(getTaskName(UnknownOwner));
    var didWarnAboutKeySpread = {};
    exports.Fragment = REACT_FRAGMENT_TYPE;
    exports.jsxDEV = function(type, config, maybeKey, isStaticChildren) {
        var trackActualOwner = 1e4 > ReactSharedInternals.recentlyCreatedOwnerStacks++;
        if (trackActualOwner) {
            var previousStackTraceLimit = Error.stackTraceLimit;
            Error.stackTraceLimit = 10;
            var debugStackDEV = Error("react-stack-top-frame");
            Error.stackTraceLimit = previousStackTraceLimit;
        } else debugStackDEV = unknownOwnerDebugStack;
        return jsxDEVImpl(type, config, maybeKey, isStaticChildren, debugStackDEV, trackActualOwner ? createTask(getTaskName(type)) : unknownOwnerDebugTask);
    };
}();
}),
"[project]/node_modules/next/dist/compiled/react/jsx-dev-runtime.js [app-client] (ecmascript)", ((__turbopack_context__, module, exports) => {
"use strict";

var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$build$2f$polyfills$2f$process$2e$js__$5b$app$2d$client$5d$__$28$ecmascript$29$__ = /*#__PURE__*/ __turbopack_context__.i("[project]/node_modules/next/dist/build/polyfills/process.js [app-client] (ecmascript)");
'use strict';
if ("TURBOPACK compile-time falsy", 0) //TURBOPACK unreachable
;
else {
    module.exports = __turbopack_context__.r("[project]/node_modules/next/dist/compiled/react/cjs/react-jsx-dev-runtime.development.js [app-client] (ecmascript)");
}
}),
]);

//# sourceMappingURL=_0pyh_q.._.js.map