using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using MiniLang.Studio.Legacy;

namespace MiniLang.Studio.Views;

public sealed partial class MainPage : Page
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly Dictionary<string, DocumentSession> _documents = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _explorerPaths = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _refreshCts;
    private string? _repoRoot;
    private string? _layoutFilePath;
    private string? _currentDocumentPath;
    private string _currentText = string.Empty;
    private string _buildState = "Debug | Idle";
    private string _analysisState = "Loading workspace";
    private string _currentConfiguration = "Debug";
    private string _lastBuildOutput = "Studio is loading the Monaco shell.";
    private string _lastDebugOutput = "Debug output will appear here.";
    private string _lastTerminalOutput = "External console runs launch through MiniLangCLI.exe.";
    private int _cursorLine = 1;
    private int _cursorColumn = 1;
    private bool _pageInitialized;
    private bool _shellReady;
    private LegacyCliClient? _cliClient;
    private LegacyEditorAnalysis _analysis = LegacyEditorAnalysisService.Analyze(string.Empty);
    private LegacyAnalysisResultDto? _lastBuildResult;
    private List<LegacyDiagnosticDto> _currentDiagnostics = [];

    public MainPage()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (_pageInitialized)
        {
            return;
        }

        StartupStatusText.Text = "Preparing legacy runtime workspace...";
        _repoRoot = ResolveRepoRoot();
        _layoutFilePath = GetLayoutFilePath();
        _cliClient = new LegacyCliClient(_repoRoot);
        await LoadWorkspaceAsync();
        await InitializeShellAsync();
        _pageInitialized = true;
    }

    private async Task LoadWorkspaceAsync()
    {
        ArgumentNullException.ThrowIfNull(_repoRoot);
        var files = GetWorkspaceFiles();

        _explorerPaths.Clear();
        foreach (var file in files)
        {
            var relative = Path.GetRelativePath(_repoRoot, file);
            _explorerPaths[relative] = file;
        }

        var preferredPath = LoadPreferredDocumentPath(files);
        await OpenDocumentInternalAsync(preferredPath, setCurrent: true);
        _analysisState = "Workspace ready";
    }

    private async Task InitializeShellAsync()
    {
        ArgumentNullException.ThrowIfNull(_repoRoot);

        StartupStatusText.Text = "Booting Monaco shell...";
        await ShellView.EnsureCoreWebView2Async();
        ShellView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        ShellView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;
        ShellView.CoreWebView2.Settings.AreDevToolsEnabled = true;
        ShellView.CoreWebView2.WebMessageReceived += ShellView_WebMessageReceived;

        var indexPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Editor", "index.html");
        ShellView.Source = new Uri(indexPath);
    }

    private async void ShellView_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        try
        {
            using var document = JsonDocument.Parse(args.WebMessageAsJson);
            var root = document.RootElement;
            var type = GetString(root, "type");
            switch (type)
            {
                case "ready":
                    _shellReady = true;
                    StartupStatusText.Text = "Loading workspace...";
                    await SendInitializeAsync();
                    StartupOverlay.Visibility = Visibility.Collapsed;
                    break;
                case "editorLog":
                    AppendDebugLog($"[shell] {GetString(root, "message")}");
                    break;
                case "documentChanged":
                    await HandleDocumentChangedAsync(root);
                    break;
                case "cursorChanged":
                    HandleCursorChanged(root);
                    break;
                case "request":
                    await HandleRequestAsync(root);
                    break;
                case "openDocs":
                    _analysisState = $"Docs target: {GetString(root, "docsPath")}";
                    await SendStatusAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            AppendDebugLog($"[host-error] {ex.Message}");
            await PostMessageAsync(new { type = "hostError", message = ex.Message });
        }
    }

    private async Task HandleRequestAsync(JsonElement request)
    {
        var requestId = GetString(request, "requestId") ?? string.Empty;
        var method = GetString(request, "method");
        try
        {
            switch (method)
            {
                case "initializeShell":
                    await SendInitializeAsync();
                    await RespondAsync(requestId, new { ok = true });
                    break;
                case "openDocument":
                    await HandleOpenDocumentRequestAsync(requestId, request);
                    break;
                case "saveDocument":
                    await HandleSaveDocumentRequestAsync(requestId, request);
                    break;
                case "completion":
                    await RespondAsync(requestId, BuildCompletionPayload());
                    break;
                case "hover":
                    await RespondAsync(requestId, BuildHoverPayload(GetInt(request, "offset")));
                    break;
                case "definition":
                    await RespondAsync(requestId, BuildDefinitionPayload(GetInt(request, "offset")));
                    break;
                case "outline":
                    await RespondAsync(requestId, BuildOutlinePayload());
                    break;
                case "build":
                    await HandleBuildRequestAsync(requestId, request);
                    break;
                case "run":
                    await HandleRunRequestAsync(requestId, request);
                    break;
                case "requestInspectorData":
                    await RespondAsync(requestId, BuildInspectorPayload(GetString(request, "kind")));
                    break;
                case "persistLayout":
                    await HandlePersistLayoutAsync(requestId, request);
                    break;
                default:
                    await RespondAsync(requestId, new { ok = false, error = $"Unknown request '{method}'." });
                    break;
            }
        }
        catch (Exception ex)
        {
            AppendDebugLog($"[{method}] {ex.Message}");
            await RespondAsync(requestId, new { ok = false, error = ex.Message });
            await PostMessageAsync(new { type = "hostError", message = ex.Message });
        }
    }

    private async Task HandleOpenDocumentRequestAsync(string requestId, JsonElement request)
    {
        var path = GetString(request, "path");
        if (string.IsNullOrWhiteSpace(path))
        {
            await RespondAsync(requestId, new { ok = false, error = "Document path is required." });
            return;
        }

        var session = await OpenDocumentInternalAsync(path, setCurrent: true);
        await RespondAsync(requestId, new
        {
            ok = true,
            document = BuildDocumentPayload(session)
        });
        await SendAnalysisAsync();
    }

    private async Task HandleSaveDocumentRequestAsync(string requestId, JsonElement request)
    {
        var path = GetString(request, "path") ?? _currentDocumentPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            await RespondAsync(requestId, new { ok = false, error = "No active document." });
            return;
        }

        var session = await UpdateSessionFromRequestAsync(request, path);
        await SaveDocumentAsync(session.Path);
        await RespondAsync(requestId, new
        {
            ok = true,
            document = BuildDocumentPayload(session)
        });
        await SendStatusAsync();
    }

    private async Task HandleBuildRequestAsync(string requestId, JsonElement request)
    {
        ArgumentNullException.ThrowIfNull(_cliClient);
        var configuration = NormalizeConfiguration(GetString(request, "configuration"));
        var session = await UpdateSessionFromRequestAsync(request, _currentDocumentPath);
        _currentConfiguration = configuration;
        _buildState = $"{configuration} | Building";
        _analysisState = "Inspecting legacy runtime";
        await SendStatusAsync();

        try
        {
            var result = await _cliClient.InspectAsync(session.Path, session.Text, configuration);
            _lastBuildResult = result;
            _currentDiagnostics = result.Diagnostics;
            _buildState = result.Success ? $"{configuration} | Ready" : $"{configuration} | Diagnostics";
            _analysisState = result.Success ? "Legacy runtime ready" : $"{result.Diagnostics.Count} diagnostics";
            _lastBuildOutput = result.Success
                ? $"Build succeeded for {Path.GetFileName(session.Path)}."
                : string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Severity} {diagnostic.Id} ({diagnostic.Line},{diagnostic.Column}): {diagnostic.Message}"));

            await PostMessageAsync(new
            {
                type = "buildResult",
                configuration,
                diagnostics = MapDiagnostics(result.Diagnostics),
                syntaxTree = BuildSyntaxInspectorTree(result),
                symbols = BuildSymbolInspectorTree(result),
                semanticModel = BuildSemanticInspectorTree(result),
                boundTree = BuildBoundInspectorTree(result),
                lowered = BuildLoweredInspectorTree(result),
                interop = BuildInteropInspectorTree(result),
                output = _lastBuildOutput
            });
            await SendAnalysisAsync();
            await RespondAsync(requestId, new { ok = true, success = result.Success });
        }
        catch (Exception ex)
        {
            _buildState = $"{configuration} | Error";
            _analysisState = "Build failed";
            _currentDiagnostics = [CreateHostErrorDiagnostic(ex.Message)];
            _lastBuildOutput = ex.Message;
            await PostMessageAsync(new
            {
                type = "buildResult",
                configuration,
                diagnostics = MapDiagnostics(_currentDiagnostics),
                syntaxTree = BuildSyntaxInspectorTree(_lastBuildResult),
                symbols = BuildSymbolInspectorTree(_lastBuildResult),
                semanticModel = BuildSemanticInspectorTree(_lastBuildResult),
                boundTree = BuildBoundInspectorTree(_lastBuildResult),
                lowered = BuildLoweredInspectorTree(_lastBuildResult),
                interop = BuildInteropInspectorTree(_lastBuildResult),
                output = ex.Message
            });
            await SendStatusAsync();
            await RespondAsync(requestId, new { ok = false, error = ex.Message });
        }
    }

    private async Task HandleRunRequestAsync(string requestId, JsonElement request)
    {
        ArgumentNullException.ThrowIfNull(_cliClient);
        var configuration = NormalizeConfiguration(GetString(request, "configuration"));
        var session = await UpdateSessionFromRequestAsync(request, _currentDocumentPath);
        _currentConfiguration = configuration;

        if (string.Equals(configuration, "Release", StringComparison.Ordinal))
        {
            await SaveDocumentAsync(session.Path);
            _buildState = "Release | Launching console";
            _analysisState = "Starting external console run";
            await SendStatusAsync();

            var launch = await _cliClient.LaunchExternalRunAsync(session.Path, "Release");
            _lastTerminalOutput = $"Launched external console:{Environment.NewLine}{launch.CommandLine}";
            _buildState = "Release | Running externally";

            await PostMessageAsync(new
            {
                type = "externalRunStarted",
                commandLine = launch.CommandLine,
                workingDirectory = launch.WorkingDirectory
            });
            await PostMessageAsync(new
            {
                type = "runResult",
                configuration = "Release",
                mode = "external",
                output = _lastTerminalOutput,
                diagnostics = MapDiagnostics(Array.Empty<LegacyDiagnosticDto>())
            });
            await SendStatusAsync();
            await RespondAsync(requestId, new { ok = true, mode = "external" });
            return;
        }

        _buildState = $"{configuration} | Running";
        _analysisState = "Executing legacy runtime";
        await SendStatusAsync();

        try
        {
            var result = await _cliClient.RunAsync(session.Path, session.Text, configuration);
            _currentDiagnostics = result.Diagnostics;
            _lastDebugOutput = result.Success
                ? result.Output
                : string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message));
            _buildState = result.Success ? $"{configuration} | Ready" : $"{configuration} | Error";
            _analysisState = result.Success ? "Run completed" : "Runtime diagnostics";

            await PostMessageAsync(new
            {
                type = "runResult",
                configuration,
                mode = "embedded",
                output = _lastDebugOutput,
                diagnostics = MapDiagnostics(result.Diagnostics)
            });
            await SendAnalysisAsync();
            await RespondAsync(requestId, new { ok = true, success = result.Success });
        }
        catch (Exception ex)
        {
            _buildState = $"{configuration} | Error";
            _analysisState = "Run failed";
            _currentDiagnostics = [CreateHostErrorDiagnostic(ex.Message)];
            _lastDebugOutput = ex.Message;
            await PostMessageAsync(new
            {
                type = "runResult",
                configuration,
                mode = "embedded",
                output = ex.Message,
                diagnostics = MapDiagnostics(_currentDiagnostics)
            });
            await SendStatusAsync();
            await RespondAsync(requestId, new { ok = false, error = ex.Message });
        }
    }

    private async Task HandlePersistLayoutAsync(string requestId, JsonElement request)
    {
        var layout = request.TryGetProperty("layout", out var layoutElement)
            ? layoutElement.GetRawText()
            : GetString(request, "layoutJson");

        if (string.IsNullOrWhiteSpace(layout))
        {
            await RespondAsync(requestId, new { ok = false, error = "Layout payload was empty." });
            return;
        }

        ArgumentNullException.ThrowIfNull(_layoutFilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(_layoutFilePath)!);
        await File.WriteAllTextAsync(_layoutFilePath, layout);
        await RespondAsync(requestId, new { ok = true });
    }

    private async Task HandleDocumentChangedAsync(JsonElement message)
    {
        var path = GetString(message, "path") ?? _currentDocumentPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var text = GetString(message, "text") ?? string.Empty;
        var session = GetOrCreateSession(path);
        session.Text = text;
        session.Dirty = true;
        _currentDocumentPath = path;
        _currentText = text;
        _analysisState = "Analyzing...";
        _buildState = $"{_currentConfiguration} | Dirty";
        QueueAnalysisRefresh();
        await SendStatusAsync();
    }

    private void HandleCursorChanged(JsonElement message)
    {
        _cursorLine = GetInt(message, "line", 1);
        _cursorColumn = GetInt(message, "column", 1);
        _ = SendStatusAsync();
    }

    private async Task SendInitializeAsync()
    {
        var session = GetCurrentSession();
        _analysis = LegacyEditorAnalysisService.Analyze(session.Text);
        _currentText = session.Text;
        _currentDiagnostics = [];
        await PostMessageAsync(new
        {
            type = "initialize",
            document = BuildDocumentPayload(session),
            explorer = _explorerPaths
                .OrderBy(static item => item.Key, StringComparer.OrdinalIgnoreCase)
                .Select(item => new
                {
                    path = item.Value,
                    name = item.Key
                }),
            layout = LoadLayoutNode(),
            state = new
            {
                configuration = _currentConfiguration,
                buildState = _buildState,
                analysisState = _analysisState,
                cursor = new { line = _cursorLine, column = _cursorColumn },
                target = "windows-x64"
            }
        });
        await SendAnalysisAsync();
    }

    private async Task SendAnalysisAsync()
    {
        if (!_shellReady)
        {
            return;
        }

        var session = GetCurrentSession();
        _analysis = LegacyEditorAnalysisService.Analyze(session.Text);
        _currentText = session.Text;
        await PostMessageAsync(new
        {
            type = "analysis",
            documentPath = session.Path,
            diagnostics = MapDiagnostics(_currentDiagnostics),
            outline = BuildOutlinePayload(),
            completions = BuildCompletionPayload(),
            hoverIndex = _analysis.HoverMap.ToDictionary(
                static pair => pair.Key,
                pair => new
                {
                    title = pair.Value.Title,
                    detail = pair.Value.Detail,
                    docsPath = pair.Value.DocumentationPath
                }),
            definitions = _analysis.Definitions.Select(static pair => new { symbol = pair.Key, start = pair.Value }),
            semanticTokens = _analysis.Classifications.Select(static classification => new
            {
                start = classification.Start,
                length = classification.Length,
                classification = classification.Classification
            }),
            inspectors = new
            {
                syntaxTree = BuildSyntaxInspectorTree(_lastBuildResult),
                symbols = BuildSymbolInspectorTree(_lastBuildResult),
                semantic = BuildSemanticInspectorTree(_lastBuildResult),
                boundTree = BuildBoundInspectorTree(_lastBuildResult),
                lowered = BuildLoweredInspectorTree(_lastBuildResult),
                interop = BuildInteropInspectorTree(_lastBuildResult),
                diagnostics = BuildDiagnosticsInspectorTree(_currentDiagnostics)
            }
        });
        await SendStatusAsync();
    }

    private async Task SendStatusAsync()
    {
        if (!_shellReady)
        {
            return;
        }

        await PostMessageAsync(new
        {
            type = "status",
            cursor = new { line = _cursorLine, column = _cursorColumn },
            dirty = GetCurrentSession().Dirty,
            buildState = _buildState,
            analysisState = _analysisState,
            activeDocumentPath = _currentDocumentPath,
            configuration = _currentConfiguration
        });
    }

    private async Task SaveDocumentAsync(string path)
    {
        var session = GetOrCreateSession(path);
        await File.WriteAllTextAsync(path, session.Text);
        session.Dirty = false;
        _analysisState = "Saved";
        _buildState = $"{_currentConfiguration} | Saved";
    }

    private async Task<DocumentSession> UpdateSessionFromRequestAsync(JsonElement request, string? fallbackPath)
    {
        var path = GetString(request, "path") ?? fallbackPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("No active document path was available.");
        }

        var session = GetOrCreateSession(path);
        if (request.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
        {
            session.Text = textElement.GetString() ?? string.Empty;
            session.Dirty = true;
        }

        _currentDocumentPath = session.Path;
        _currentText = session.Text;
        _analysis = LegacyEditorAnalysisService.Analyze(_currentText);
        return session;
    }

    private async Task<DocumentSession> OpenDocumentInternalAsync(string path, bool setCurrent)
    {
        var session = GetOrCreateSession(path);
        if (string.IsNullOrWhiteSpace(session.Text))
        {
            session.Text = await File.ReadAllTextAsync(path);
        }

        if (setCurrent)
        {
            _currentDocumentPath = session.Path;
            _currentText = session.Text;
            _analysis = LegacyEditorAnalysisService.Analyze(session.Text);
            _cursorLine = 1;
            _cursorColumn = 1;
            _currentDiagnostics = [];
        }

        return session;
    }

    private DocumentSession GetOrCreateSession(string path)
    {
        if (_documents.TryGetValue(path, out var session))
        {
            return session;
        }

        session = new DocumentSession
        {
            Path = path,
            Name = Path.GetFileName(path),
            Text = File.Exists(path) ? File.ReadAllText(path) : string.Empty,
            Dirty = false
        };
        _documents[path] = session;
        return session;
    }

    private DocumentSession GetCurrentSession()
    {
        if (_currentDocumentPath is null)
        {
            throw new InvalidOperationException("The current MiniLang document was not initialized.");
        }

        return GetOrCreateSession(_currentDocumentPath);
    }

    private void QueueAnalysisRefresh()
    {
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();
        var token = _refreshCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(160, token);
                if (token.IsCancellationRequested)
                {
                    return;
                }

                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    _analysisState = "Analysis ready";
                    await SendAnalysisAsync();
                });
            }
            catch (TaskCanceledException)
            {
            }
        }, token);
    }

    private object BuildDocumentPayload(DocumentSession session) => new
    {
        path = session.Path,
        name = session.Name,
        text = session.Text,
        dirty = session.Dirty
    };

    private IReadOnlyList<object> BuildCompletionPayload() =>
        _analysis.Completions
            .Select(static completion => (object)new
            {
                label = completion.Label,
                detail = completion.Detail
            })
            .ToArray();

    private object? BuildHoverPayload(int offset)
    {
        var word = TryGetWordAt(offset);
        if (word is null || !_analysis.HoverMap.TryGetValue(word, out var hover))
        {
            return null;
        }

        return new
        {
            title = hover.Title,
            detail = hover.Detail,
            docsPath = hover.DocumentationPath
        };
    }

    private object? BuildDefinitionPayload(int offset)
    {
        var word = TryGetWordAt(offset);
        if (word is null)
        {
            return null;
        }

        if (_analysis.Definitions.TryGetValue(word, out var definitionOffset))
        {
            return new
            {
                documentPath = _currentDocumentPath,
                start = definitionOffset,
                length = Math.Max(1, word.Length)
            };
        }

        if (_analysis.HoverMap.TryGetValue(word, out var hover))
        {
            return new
            {
                docsPath = hover.DocumentationPath
            };
        }

        return null;
    }

    private IReadOnlyList<object> BuildOutlinePayload() =>
        _analysis.Outline
            .Select(static item => (object)new
            {
                kind = item.Kind,
                label = item.Label,
                start = item.Start
            })
            .ToArray();

    private object BuildInspectorPayload(string? kind)
    {
        var payload = new Dictionary<string, object?>
        {
            ["syntaxTree"] = BuildSyntaxInspectorTree(_lastBuildResult),
            ["symbols"] = BuildSymbolInspectorTree(_lastBuildResult),
            ["semantic"] = BuildSemanticInspectorTree(_lastBuildResult),
            ["boundTree"] = BuildBoundInspectorTree(_lastBuildResult),
            ["lowered"] = BuildLoweredInspectorTree(_lastBuildResult),
            ["interop"] = BuildInteropInspectorTree(_lastBuildResult),
            ["diagnostics"] = BuildDiagnosticsInspectorTree(_currentDiagnostics)
        };

        if (!string.IsNullOrWhiteSpace(kind) && payload.TryGetValue(kind, out var single))
        {
            return new { kind, data = single };
        }

        return payload;
    }

    private IReadOnlyList<InspectorNodeDto> BuildSyntaxInspectorTree(LegacyAnalysisResultDto? result)
    {
        var root = new InspectorNodeDto("syntax-root", Path.GetFileName(_currentDocumentPath ?? "document"), "document");
        foreach (var declaration in _analysis.Outline)
        {
            var node = new InspectorNodeDto(
                $"syntax-{declaration.Kind}-{declaration.Label}",
                declaration.Label,
                declaration.Kind,
                start: declaration.Start,
                length: Math.Max(1, declaration.Label.Length),
                details: $"{declaration.Kind} declaration");

            if (string.Equals(declaration.Kind, "enum", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var member in _analysis.Definitions
                             .Where(pair => pair.Key.StartsWith($"{declaration.Label}.", StringComparison.Ordinal))
                             .OrderBy(static pair => pair.Key, StringComparer.Ordinal))
                {
                    node.Children.Add(new InspectorNodeDto(
                        $"syntax-enum-member-{member.Key}",
                        member.Key.Split('.').Last(),
                        "enum-member",
                        start: member.Value,
                        length: Math.Max(1, member.Key.Length)));
                }
            }

            root.Children.Add(node);
        }

        if (result is not null)
        {
            root.Children.AddRange(ParseTextTree("inspection", "inspection", result.SyntaxTree));
        }

        return [root];
    }

    private IReadOnlyList<InspectorNodeDto> BuildSymbolInspectorTree(LegacyAnalysisResultDto? result)
    {
        var functions = new InspectorNodeDto("symbols-functions", "Functions", "group");
        var structs = new InspectorNodeDto("symbols-structs", "Structs", "group");
        var enums = new InspectorNodeDto("symbols-enums", "Enums", "group");
        var builtins = new InspectorNodeDto("symbols-builtins", "Builtins and Interop", "group");

        foreach (var item in _analysis.Outline)
        {
            var target = item.Kind switch
            {
                "function" => functions,
                "struct" => structs,
                "enum" => enums,
                _ => builtins
            };

            var node = new InspectorNodeDto(
                $"symbol-{item.Kind}-{item.Label}",
                item.Label,
                item.Kind,
                start: item.Start,
                length: Math.Max(1, item.Label.Length),
                details: _analysis.HoverMap.TryGetValue(item.Label, out var hover) ? hover.Detail : item.Kind);

            if (string.Equals(item.Kind, "enum", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var member in _analysis.Definitions
                             .Where(pair => pair.Key.StartsWith($"{item.Label}.", StringComparison.Ordinal))
                             .OrderBy(static pair => pair.Key, StringComparer.Ordinal))
                {
                    node.Children.Add(new InspectorNodeDto(
                        $"symbol-{member.Key}",
                        member.Key,
                        "enum-member",
                        start: member.Value,
                        length: Math.Max(1, member.Key.Length)));
                }
            }

            target.Children.Add(node);
        }

        foreach (var builtin in _analysis.HoverMap
                     .Where(static pair => pair.Key is "win" or "cscall" or "number" or "string" or "object" or "nothing")
                     .OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            builtins.Children.Add(new InspectorNodeDto(
                $"builtin-{builtin.Key}",
                builtin.Key,
                "builtin",
                details: builtin.Value.Detail));
        }

        if (result is not null)
        {
            builtins.Children.AddRange(ParseTextTree("build-symbols", "Build symbols", result.Symbols));
        }

        return [functions, structs, enums, builtins];
    }

    private IReadOnlyList<InspectorNodeDto> BuildSemanticInspectorTree(LegacyAnalysisResultDto? result)
    {
        var document = new InspectorNodeDto("semantic-document", "Document", "group");
        document.Children.Add(new InspectorNodeDto("semantic-path", Path.GetFileName(_currentDocumentPath ?? "document"), "file", details: _currentDocumentPath));
        document.Children.Add(new InspectorNodeDto("semantic-dirty", GetCurrentSession().Dirty ? "Dirty" : "Saved", "state", details: $"Configuration {_currentConfiguration}"));

        var hover = new InspectorNodeDto("semantic-hover", "Hover targets", "group");
        foreach (var item in _analysis.HoverMap.OrderBy(static pair => pair.Key, StringComparer.Ordinal).Take(18))
        {
            hover.Children.Add(new InspectorNodeDto(
                $"semantic-hover-{item.Key}",
                item.Key,
                "symbol",
                details: item.Value.Detail));
        }

        var completions = new InspectorNodeDto("semantic-completions", "Top completions", "group");
        foreach (var item in _analysis.Completions.Take(24))
        {
            completions.Children.Add(new InspectorNodeDto(
                $"semantic-completion-{item.Label}",
                item.Label,
                "completion",
                details: item.Detail));
        }

        var diagnosticsNode = new InspectorNodeDto("semantic-diagnostics", "Diagnostics", "group");
        diagnosticsNode.Children.AddRange(BuildDiagnosticsInspectorTree(_currentDiagnostics));

        var nodes = new List<InspectorNodeDto> { document, hover, completions, diagnosticsNode };
        if (result is not null)
        {
            nodes.AddRange(ParseTextTree("semantic-build", "Build summary", result.InterpretedTree));
        }

        return nodes;
    }

    private IReadOnlyList<InspectorNodeDto> BuildBoundInspectorTree(LegacyAnalysisResultDto? result)
    {
        if (result is not null && !string.IsNullOrWhiteSpace(result.SyntaxTree))
        {
            return ParseTextTree("bound-tree", "Bound and parsed stages", result.SyntaxTree);
        }

        return
        [
            new InspectorNodeDto("bound-placeholder", "Awaiting build", "state", details: "Build the active document to populate the bound-tree inspector.")
        ];
    }

    private IReadOnlyList<InspectorNodeDto> BuildLoweredInspectorTree(LegacyAnalysisResultDto? result)
    {
        if (result is not null && !string.IsNullOrWhiteSpace(result.InterpretedTree))
        {
            return ParseTextTree("lowered-tree", "Lowered / interpreted", result.InterpretedTree);
        }

        return
        [
            new InspectorNodeDto("lowered-placeholder", "Awaiting runtime inspection", "state", details: "Run Build to populate lowered and interpreter stages.")
        ];
    }

    private IReadOnlyList<InspectorNodeDto> BuildInteropInspectorTree(LegacyAnalysisResultDto? result)
    {
        var namespaces = new InspectorNodeDto("interop-namespaces", "Approved namespaces", "group");
        namespaces.Children.Add(new InspectorNodeDto("interop-process", "win.process", "namespace", details: "Process information bridge"));
        namespaces.Children.Add(new InspectorNodeDto("interop-time", "win.time", "namespace", details: "Timing helpers and sleep"));
        namespaces.Children.Add(new InspectorNodeDto("interop-user", "win.user", "namespace", details: "MessageBox and UI helpers"));
        namespaces.Children.Add(new InspectorNodeDto("interop-console", "win.console", "namespace", details: "Console title and shell helpers"));

        var members = new InspectorNodeDto("interop-members", "Interop members", "group");
        foreach (var completion in _analysis.Completions.Where(static item => item.Label.StartsWith("win.", StringComparison.Ordinal) || item.Detail.Contains("interop", StringComparison.OrdinalIgnoreCase)))
        {
            members.Children.Add(new InspectorNodeDto($"interop-{completion.Label}", completion.Label, "interop", details: completion.Detail));
        }

        var nodes = new List<InspectorNodeDto> { namespaces, members };
        if (result is not null)
        {
            nodes.AddRange(ParseTextTree("interop-build", "Build inspection", result.Symbols));
        }

        return nodes;
    }

    private IReadOnlyList<InspectorNodeDto> BuildDiagnosticsInspectorTree(IEnumerable<LegacyDiagnosticDto> diagnostics) =>
        diagnostics
            .Select(static diagnostic => new InspectorNodeDto(
                $"diagnostic-{diagnostic.Id}-{diagnostic.Start}",
                $"{diagnostic.Severity} {diagnostic.Id}",
                "diagnostic",
                start: diagnostic.Start,
                length: Math.Max(1, diagnostic.Length),
                details: $"{diagnostic.Message} (Ln {diagnostic.Line}, Col {diagnostic.Column})"))
            .ToArray();

    private static IReadOnlyList<object> MapDiagnostics(IEnumerable<LegacyDiagnosticDto> diagnostics) =>
        diagnostics
            .Select(static diagnostic => (object)new
            {
                id = diagnostic.Id,
                severity = diagnostic.Severity,
                message = diagnostic.Message,
                start = diagnostic.Start,
                length = Math.Max(1, diagnostic.Length),
                line = diagnostic.Line,
                column = diagnostic.Column
            })
            .ToArray();

    private List<InspectorNodeDto> ParseTextTree(string idPrefix, string label, string? text)
    {
        var root = new InspectorNodeDto(idPrefix, label, "group");
        if (string.IsNullOrWhiteSpace(text))
        {
            root.Children.Add(new InspectorNodeDto($"{idPrefix}-empty", "No build data available yet.", "state"));
            return [root];
        }

        var lines = text.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();
            root.Children.Add(new InspectorNodeDto($"{idPrefix}-{index}", line, "line"));
        }

        return [root];
    }

    private static LegacyDiagnosticDto CreateHostErrorDiagnostic(string message) => new()
    {
        Id = "STUDIO0009",
        Severity = "Error",
        Message = message,
        Start = 0,
        Length = 1,
        Line = 1,
        Column = 1
    };

    private string? TryGetWordAt(int offset)
    {
        if (string.IsNullOrEmpty(_currentText))
        {
            return null;
        }

        var bounded = Math.Clamp(offset, 0, Math.Max(0, _currentText.Length - 1));
        return LegacyEditorAnalysisService.GetWordAt(_currentText, bounded);
    }

    private async Task PostMessageAsync(object payload)
    {
        if (ShellView.CoreWebView2 is null)
        {
            return;
        }

        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        await DispatcherQueue.EnqueueAsync(() => ShellView.CoreWebView2.PostWebMessageAsJson(json));
    }

    private Task RespondAsync(string requestId, object? payload) =>
        PostMessageAsync(new
        {
            type = "response",
            requestId,
            payload
        });

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static int GetInt(JsonElement element, string propertyName, int fallback = 0) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
            ? value
            : fallback;

    private void AppendDebugLog(string message)
    {
        _lastDebugOutput = $"{_lastDebugOutput}{Environment.NewLine}{message}";
    }

    private JsonNode? LoadLayoutNode()
    {
        if (_layoutFilePath is null || !File.Exists(_layoutFilePath))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(File.ReadAllText(_layoutFilePath));
        }
        catch
        {
            return null;
        }
    }

    private string LoadPreferredDocumentPath(string[] files)
    {
        if (_layoutFilePath is not null && File.Exists(_layoutFilePath))
        {
            try
            {
                var node = JsonNode.Parse(File.ReadAllText(_layoutFilePath));
                var startupPath = node?["startupFilePath"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(startupPath) && File.Exists(startupPath))
                {
                    return startupPath;
                }

                var activePath = node?["activeDocumentPath"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(activePath) && File.Exists(activePath))
                {
                    return activePath;
                }
            }
            catch
            {
            }
        }

        return files.FirstOrDefault()
               ?? throw new FileNotFoundException("No MiniLang guide files were found for Studio startup.");
    }

    private static string NormalizeConfiguration(string? configuration) =>
        string.Equals(configuration, "Release", StringComparison.OrdinalIgnoreCase)
            ? "Release"
            : "Debug";

    private static string ResolveRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "MiniLangGuide")) && Directory.Exists(Path.Combine(current.FullName, "MiniLangTest")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate the MiniLang repository root.");
    }

    private static string GetLayoutFilePath()
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiniLang", "Studio");
        return Path.Combine(root, "layout.json");
    }

    private string[] GetWorkspaceFiles()
    {
        ArgumentNullException.ThrowIfNull(_repoRoot);
        var roots = new[]
        {
            Path.Combine(_repoRoot, "MiniLangGuide", "MiniLang_Syntax_Guide"),
            Path.Combine(_repoRoot, "MiniLangLibraries"),
            Path.Combine(_repoRoot, "MiniLangProjects")
        };

        return roots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.GetFiles(root, "*.mini*", SearchOption.AllDirectories))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

public sealed class DocumentSession
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool Dirty { get; set; }
}

public sealed class InspectorNodeDto
{
    public InspectorNodeDto(string id, string label, string kind, int? start = null, int? length = null, string? details = null, string? icon = null)
    {
        Id = id;
        Label = label;
        Kind = kind;
        Start = start;
        Length = length;
        Details = details;
        Icon = icon;
    }

    public string Id { get; }
    public string Label { get; }
    public string Kind { get; }
    public List<InspectorNodeDto> Children { get; } = [];
    public int? Start { get; }
    public int? Length { get; }
    public string? Details { get; }
    public string? Icon { get; }
}

internal static class DispatcherQueueExtensions
{
    public static Task EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue, Action action)
    {
        var completion = new TaskCompletionSource<bool>();
        if (!dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    action();
                    completion.SetResult(true);
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            }))
        {
            completion.SetException(new InvalidOperationException("Unable to enqueue work on the UI dispatcher."));
        }

        return completion.Task;
    }

    public static Task EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue, Func<Task> action)
    {
        var completion = new TaskCompletionSource<bool>();
        if (!dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    await action();
                    completion.SetResult(true);
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            }))
        {
            completion.SetException(new InvalidOperationException("Unable to enqueue work on the UI dispatcher."));
        }

        return completion.Task;
    }
}
