const pending = new Map();
const monacoBasePath = "./vendor/vs";
let editor = null;
let currentModel = null;
let suppressChange = false;
let analyzeTimer = null;
let dragState = null;
let currentInspectorSelection = null;
let validationDecorations = [];

const paneCatalog = {
  explorer: { title: "Explorer", region: "left", kind: "explorer" },
  outline: { title: "Symbols", region: "left", kind: "outline" },
  search: { title: "Search", region: "left", kind: "search" },
  syntaxTree: { title: "Syntax Tree", region: "right", kind: "tree" },
  boundTree: { title: "Bound Tree", region: "right", kind: "tree" },
  semantic: { title: "Semantic", region: "right", kind: "tree" },
  lowered: { title: "Lowered", region: "right", kind: "tree" },
  symbolInspector: { title: "Symbols", region: "right", kind: "tree" },
  interop: { title: "Interop", region: "right", kind: "tree" },
  diagnostics: { title: "Diagnostics", region: "bottom", kind: "diagnostics" },
  buildOutput: { title: "Build Output", region: "bottom", kind: "console" },
  terminal: { title: "Terminal", region: "bottom", kind: "console" },
  debugConsole: { title: "Debug Console", region: "bottom", kind: "console" }
};

const defaultLayout = {
  sizes: {
    left: 290,
    right: 360,
    bottom: 240
  },
  regions: {
    left: { panes: ["explorer", "outline", "search"], active: "explorer" },
    right: { panes: ["syntaxTree", "boundTree", "semantic", "lowered", "symbolInspector", "interop"], active: "syntaxTree" },
    bottom: { panes: ["diagnostics", "buildOutput", "terminal", "debugConsole"], active: "diagnostics" }
  },
  hiddenPanes: [],
  openDocuments: [],
  activeDocumentPath: null,
  startupFilePath: null
};

const state = {
  explorer: [],
  diagnostics: [],
  output: {
    buildOutput: "Build output will appear here.",
    terminal: "Release console runs will be launched here.",
    debugConsole: "Debug runtime output will appear here."
  },
  inspectors: {
    syntaxTree: [],
    boundTree: [],
    semantic: [],
    lowered: [],
    symbolInspector: [],
    interop: [],
    diagnostics: []
  },
  searchResults: [],
  hoverIndex: {},
  definitions: {},
  outline: [],
  completions: [],
  startupFilePath: null,
  documents: new Map(),
  openDocuments: [],
  activeDocumentPath: null,
  layout: structuredClone(defaultLayout),
  status: {
    cursor: { line: 1, column: 1 },
    dirty: false,
    buildState: "Debug | Idle",
    analysisState: "Waiting for analysis",
    configuration: "Debug"
  }
};

function $(id) {
  return document.getElementById(id);
}

function post(message) {
  window.chrome?.webview?.postMessage(message);
}

function request(method, payload = {}) {
  const requestId = crypto.randomUUID();
  post({ type: "request", requestId, method, ...payload });
  return new Promise((resolve) => pending.set(requestId, resolve));
}

function log(message) {
  post({ type: "editorLog", message });
}

function parseIncomingMessage(raw) {
  if (typeof raw === "string") {
    try {
      return JSON.parse(raw);
    } catch {
      return { type: "unknown" };
    }
  }

  return raw || { type: "unknown" };
}

function applyCssVariables() {
  document.documentElement.style.setProperty("--left-width", `${state.layout.sizes.left}px`);
  document.documentElement.style.setProperty("--right-width", `${state.layout.sizes.right}px`);
  document.documentElement.style.setProperty("--bottom-height", `${state.layout.sizes.bottom}px`);
}

function normalizeLayout(layout) {
  if (!layout || typeof layout !== "object") {
    return structuredClone(defaultLayout);
  }

  const merged = structuredClone(defaultLayout);
  merged.sizes = { ...merged.sizes, ...(layout.sizes || {}) };
  merged.hiddenPanes = Array.isArray(layout.hiddenPanes) ? layout.hiddenPanes.filter((pane) => paneCatalog[pane]) : [];
  merged.openDocuments = Array.isArray(layout.openDocuments) ? layout.openDocuments.filter(Boolean) : [];
  merged.activeDocumentPath = layout.activeDocumentPath || null;
  merged.startupFilePath = layout.startupFilePath || null;

  for (const region of ["left", "right", "bottom"]) {
    const nextRegion = layout.regions?.[region];
    if (!nextRegion) {
      continue;
    }

    merged.regions[region] = {
      panes: Array.isArray(nextRegion.panes) ? nextRegion.panes.filter((pane) => paneCatalog[pane]) : merged.regions[region].panes,
      active: paneCatalog[nextRegion.active] ? nextRegion.active : merged.regions[region].active
    };

    if (!merged.regions[region].panes.includes(merged.regions[region].active)) {
      merged.regions[region].active = merged.regions[region].panes[0] || null;
    }
  }

  return merged;
}

function saveLayout() {
  const payload = {
    ...state.layout,
    openDocuments: [...state.openDocuments],
    activeDocumentPath: state.activeDocumentPath,
    startupFilePath: state.startupFilePath
  };
  request("persistLayout", { layout: payload }).catch(() => {});
}

function setStatusPill(text) {
  $("statusPill").textContent = text;
}

function updateStatusBar() {
  $("cursorStatus").textContent = `Ln ${state.status.cursor.line}, Col ${state.status.cursor.column}`;
  $("analysisStatus").textContent = state.status.analysisState;
  $("buildStatus").textContent = state.status.buildState;
  const doc = state.documents.get(state.activeDocumentPath);
  const docName = doc ? doc.name : "No document";
  $("activeDocumentStatus").textContent = doc && doc.dirty ? `${docName} • Dirty` : docName;
  $("configSelector").value = state.status.configuration || "Debug";
  const caption = state.activeDocumentPath || "No document open";
  $("windowCaption").textContent = caption;
  $("startupFilePill").textContent = state.startupFilePath
    ? `Startup: ${state.startupFilePath.split(/[\\/]/).pop()}`
    : "Startup: none";
  setStatusPill(`${state.status.buildState} | ${state.status.analysisState}`);
}

function setActivePane(region, paneId) {
  state.layout.regions[region].active = paneId;
  renderRegion(region);
  saveLayout();
}

function ensurePaneVisible(paneId) {
  const meta = paneCatalog[paneId];
  if (!meta) {
    return;
  }

  state.layout.hiddenPanes = state.layout.hiddenPanes.filter((value) => value !== paneId);
  for (const regionName of ["left", "right", "bottom"]) {
    const region = state.layout.regions[regionName];
    region.panes = region.panes.filter((value) => value !== paneId);
  }

  const targetRegion = state.layout.regions[meta.region];
  targetRegion.panes.push(paneId);
  targetRegion.active = paneId;
  renderShell();
  saveLayout();
}

function hidePane(paneId) {
  state.layout.hiddenPanes.push(paneId);
  for (const regionName of ["left", "right", "bottom"]) {
    const region = state.layout.regions[regionName];
    region.panes = region.panes.filter((value) => value !== paneId);
    if (region.active === paneId) {
      region.active = region.panes[0] || null;
    }
  }
  renderShell();
  saveLayout();
}

function movePaneToRegion(paneId, targetRegionName) {
  if (!paneCatalog[paneId]) {
    return;
  }

  state.layout.hiddenPanes = state.layout.hiddenPanes.filter((value) => value !== paneId);
  for (const regionName of ["left", "right", "bottom"]) {
    const region = state.layout.regions[regionName];
    region.panes = region.panes.filter((value) => value !== paneId);
    if (region.active === paneId) {
      region.active = region.panes[0] || null;
    }
  }

  const region = state.layout.regions[targetRegionName];
  if (!region.panes.includes(paneId)) {
    region.panes.push(paneId);
  }
  region.active = paneId;
  renderShell();
  saveLayout();
}

function renderShell() {
  applyCssVariables();
  renderRegion("left");
  renderRegion("right");
  renderRegion("bottom");
  renderEditorTabs();
  renderPanelModal();
  updateStatusBar();
}

function renderRegion(regionName) {
  const region = state.layout.regions[regionName];
  const host = $(`${regionName}Region`);
  host.innerHTML = "";

  const tabbar = document.createElement("div");
  tabbar.className = "dock-tabbar";
  tabbar.dataset.region = regionName;
  wireDropTarget(tabbar, regionName);

  for (const paneId of region.panes) {
    if (state.layout.hiddenPanes.includes(paneId)) {
      continue;
    }

    const tab = document.createElement("div");
    tab.className = `dock-tab${region.active === paneId ? " active" : ""}`;
    tab.draggable = true;
    tab.dataset.paneId = paneId;
    tab.addEventListener("click", () => setActivePane(regionName, paneId));
    tab.addEventListener("dragstart", () => {
      dragState = { paneId };
      tab.classList.add("dragging");
    });
    tab.addEventListener("dragend", () => {
      dragState = null;
      tab.classList.remove("dragging");
    });

    const title = document.createElement("span");
    title.textContent = paneCatalog[paneId].title;
    tab.appendChild(title);

    const close = document.createElement("button");
    close.className = "tab-close";
    close.textContent = "×";
    close.title = `Hide ${paneCatalog[paneId].title}`;
    close.addEventListener("click", (event) => {
      event.stopPropagation();
      hidePane(paneId);
    });
    tab.appendChild(close);
    tabbar.appendChild(tab);
  }

  const content = document.createElement("div");
  content.className = "dock-content";
  const pane = region.active && !state.layout.hiddenPanes.includes(region.active)
    ? createPaneContent(region.active)
    : createEmptyState("All panes in this region are hidden.");
  content.appendChild(pane);

  wireDropTarget(host, regionName);
  host.appendChild(tabbar);
  host.appendChild(content);
}

function createEmptyState(message) {
  const node = document.createElement("div");
  node.className = "empty-state";
  node.textContent = message;
  return node;
}

function createPaneContent(paneId) {
  switch (paneId) {
    case "explorer":
      return renderExplorerPane();
    case "outline":
      return renderOutlinePane();
    case "search":
      return renderSearchPane();
    case "diagnostics":
      return renderDiagnosticsPane();
    case "buildOutput":
      return renderConsolePane("buildOutput");
    case "terminal":
      return renderConsolePane("terminal");
    case "debugConsole":
      return renderConsolePane("debugConsole");
    case "syntaxTree":
      return renderTreePane(state.inspectors.syntaxTree);
    case "boundTree":
      return renderTreePane(state.inspectors.boundTree);
    case "semantic":
      return renderTreePane(state.inspectors.semantic);
    case "lowered":
      return renderTreePane(state.inspectors.lowered);
    case "symbolInspector":
      return renderTreePane(state.inspectors.symbolInspector);
    case "interop":
      return renderTreePane(state.inspectors.interop);
    default:
      return createEmptyState("This pane is not wired yet.");
  }
}

function renderExplorerPane() {
  const wrapper = document.createElement("div");
  wrapper.className = "pane-scroll";
  const tree = document.createElement("div");
  tree.className = "tree";
  const nodes = buildExplorerTree(state.explorer);
  if (!nodes.length) {
    wrapper.appendChild(createEmptyState("No MiniLang files were discovered in the workspace."));
    return wrapper;
  }

  for (const node of nodes) {
    tree.appendChild(createExplorerNode(node));
  }

  wrapper.appendChild(tree);
  return wrapper;
}

function buildExplorerTree(files) {
  const roots = new Map();

  for (const item of files) {
    const normalized = (item.name || item.path || "").replaceAll("\\", "/");
    const segments = normalized.split("/").filter(Boolean);
    let currentMap = roots;
    let currentPath = "";

    segments.forEach((segment, index) => {
      currentPath = currentPath ? `${currentPath}/${segment}` : segment;
      const isLeaf = index === segments.length - 1;
      if (!currentMap.has(segment)) {
        currentMap.set(segment, {
          id: currentPath,
          label: segment,
          kind: isLeaf ? "file" : "folder",
          path: isLeaf ? item.path : null,
          childrenMap: new Map()
        });
      }

      const node = currentMap.get(segment);
      if (isLeaf) {
        node.kind = "file";
        node.path = item.path;
      }

      currentMap = node.childrenMap;
    });
  }

  function finalize(map) {
    return [...map.values()]
      .sort((left, right) => {
        if (left.kind !== right.kind) {
          return left.kind === "folder" ? -1 : 1;
        }
        return left.label.localeCompare(right.label);
      })
      .map((node) => ({
        id: node.id,
        label: node.label,
        kind: node.kind,
        path: node.path,
        children: finalize(node.childrenMap)
      }));
  }

  return finalize(roots);
}

function createExplorerNode(node) {
  const hasChildren = node.children && node.children.length > 0;
  const item = document.createElement("div");
  item.className = "tree-node";

  const row = document.createElement("div");
  const isActive = node.path && node.path === state.activeDocumentPath;
  row.className = `tree-row${isActive ? " selected" : ""}`;
  row.addEventListener("click", (event) => {
    event.stopPropagation();
    if (node.path) {
      openDocument(node.path);
    } else if (hasChildren) {
      item.classList.toggle("collapsed");
      expander.textContent = item.classList.contains("collapsed") ? "▸" : "▾";
    }
  });
  row.addEventListener("contextmenu", (event) => {
    event.preventDefault();
    event.stopPropagation();
    if (node.path) {
      openExplorerContextMenu(event.clientX, event.clientY, node);
    }
  });

  const expander = document.createElement("div");
  expander.className = "expander";
  expander.textContent = hasChildren ? "▾" : "";
  expander.addEventListener("click", (event) => {
    event.stopPropagation();
    if (!hasChildren) {
      return;
    }
    item.classList.toggle("collapsed");
    expander.textContent = item.classList.contains("collapsed") ? "▸" : "▾";
  });
  row.appendChild(expander);

  const label = document.createElement("div");
  label.className = "tree-label";
  const details = node.path
    ? (node.path === state.startupFilePath ? "Startup file" : "MiniLang source")
    : `${node.children.length} item${node.children.length === 1 ? "" : "s"}`;
  label.innerHTML = `<div class="tree-title">${escapeHtml(node.label)}</div><div class="tree-details">${escapeHtml(details)}</div>`;
  row.appendChild(label);

  const kind = document.createElement("div");
  kind.className = "kind-pill";
  const doc = node.path ? state.documents.get(node.path) : null;
  kind.textContent = node.path
    ? (node.path === state.startupFilePath ? "startup" : (doc?.dirty ? "dirty" : "file"))
    : "folder";
  row.appendChild(kind);
  item.appendChild(row);

  if (hasChildren) {
    const children = document.createElement("div");
    children.className = "tree-children";
    for (const child of node.children) {
      children.appendChild(createExplorerNode(child));
    }
    item.appendChild(children);
  }

  return item;
}

function renderOutlinePane() {
  const items = state.outline.map((item) => ({
    id: `outline-${item.kind}-${item.label}-${item.start}`,
    label: item.label,
    kind: item.kind,
    start: item.start,
    length: Math.max(1, item.label.length),
    details: `${item.kind} declaration`
  }));
  return renderTreePane(items);
}

function renderSearchPane() {
  const query = ($("toolbarSearchBox").value || $("commandBox").value || "").trim().toLowerCase();
  const results = query.length === 0
    ? state.completions.slice(0, 24)
    : state.completions.filter((item) => `${item.label} ${item.detail}`.toLowerCase().includes(query)).slice(0, 40);

  const wrapper = document.createElement("div");
  wrapper.className = "pane-scroll";
  const list = document.createElement("div");
  list.className = "list-panel";

  for (const item of results) {
    const row = document.createElement("div");
    row.className = "list-row";
    row.addEventListener("click", () => triggerCompletionInsert(item.label));

    const text = document.createElement("div");
    text.textContent = item.label;
    row.appendChild(text);

    const badge = document.createElement("div");
    badge.className = "badge";
    badge.textContent = item.detail || "symbol";
    row.appendChild(badge);
    list.appendChild(row);
  }

  wrapper.appendChild(results.length === 0 ? createEmptyState("No symbol matches the current filter.") : list);
  return wrapper;
}

function renderDiagnosticsPane() {
  const wrapper = document.createElement("div");
  wrapper.className = "pane-scroll";
  const list = document.createElement("div");
  list.className = "list-panel";

  if (!state.diagnostics.length) {
    wrapper.appendChild(createEmptyState("No diagnostics. Build or run the active document to populate this pane."));
    return wrapper;
  }

  for (const diagnostic of state.diagnostics) {
    const row = document.createElement("div");
    row.className = "list-row";
    row.addEventListener("click", () => revealOffset(diagnostic.start));

    const left = document.createElement("div");
    left.innerHTML = `<div>${escapeHtml(diagnostic.message)}</div><div class="tree-details">${escapeHtml(`${diagnostic.id} • Ln ${diagnostic.line}, Col ${diagnostic.column}`)}</div>`;
    row.appendChild(left);

    const badge = document.createElement("div");
    badge.className = `badge ${severityClass(diagnostic.severity)}`;
    badge.textContent = diagnostic.severity;
    row.appendChild(badge);
    list.appendChild(row);
  }

  wrapper.appendChild(list);
  return wrapper;
}

function renderConsolePane(key) {
  const wrapper = document.createElement("div");
  wrapper.className = "pane-scroll";
  const pre = document.createElement("pre");
  pre.className = "console-output";
  pre.textContent = state.output[key] || "";
  wrapper.appendChild(pre);
  return wrapper;
}

function renderTreePane(nodes) {
  const wrapper = document.createElement("div");
  wrapper.className = "pane-scroll";
  if (!nodes || !nodes.length) {
    wrapper.appendChild(createEmptyState("This pane has no data yet."));
    return wrapper;
  }

  const tree = document.createElement("div");
  tree.className = "tree";
  for (const node of nodes) {
    tree.appendChild(createTreeNode(node));
  }
  wrapper.appendChild(tree);
  return wrapper;
}

function createTreeNode(node) {
  const hasChildren = Array.isArray(node.children) && node.children.length > 0;
  const item = document.createElement("div");
  item.className = "tree-node";
  item.dataset.nodeId = node.id;
  if (hasChildren) {
    item.classList.add("expanded");
  }

  const row = document.createElement("div");
  row.className = `tree-row${currentInspectorSelection === node.id ? " selected" : ""}`;
  row.addEventListener("click", (event) => {
    event.stopPropagation();
    currentInspectorSelection = node.id;
    if (typeof node.start === "number") {
      revealOffset(node.start);
    }
    renderShell();
  });

  const expander = document.createElement("div");
  expander.className = "expander";
  expander.textContent = hasChildren ? "▾" : "";
  expander.addEventListener("click", (event) => {
    event.stopPropagation();
    if (!hasChildren) {
      return;
    }
    item.classList.toggle("collapsed");
    expander.textContent = item.classList.contains("collapsed") ? "▸" : "▾";
  });
  row.appendChild(expander);

  const label = document.createElement("div");
  label.className = "tree-label";
  label.innerHTML = `<div class="tree-title">${escapeHtml(node.label)}</div>${node.details ? `<div class="tree-details">${escapeHtml(node.details)}</div>` : ""}`;
  row.appendChild(label);

  const kind = document.createElement("div");
  kind.className = "kind-pill";
  kind.textContent = node.kind || "node";
  row.appendChild(kind);
  item.appendChild(row);

  if (hasChildren) {
    const children = document.createElement("div");
    children.className = "tree-children";
    for (const child of node.children) {
      children.appendChild(createTreeNode(child));
    }
    item.appendChild(children);
  }

  return item;
}

function renderEditorTabs() {
  const host = $("editorTabs");
  host.innerHTML = "";

  for (const path of state.openDocuments) {
    const doc = state.documents.get(path);
    if (!doc) {
      continue;
    }

    const tab = document.createElement("div");
    tab.className = `editor-tab${path === state.activeDocumentPath ? " active" : ""}`;
    tab.addEventListener("click", () => activateDocument(path));

    const title = document.createElement("span");
    title.textContent = doc.dirty ? `${doc.name} •` : doc.name;
    tab.appendChild(title);

    const close = document.createElement("button");
    close.className = "tab-close";
    close.textContent = "×";
    close.addEventListener("click", (event) => {
      event.stopPropagation();
      closeDocument(path);
    });
    tab.appendChild(close);

    host.appendChild(tab);
  }
}

function openDocumentPayload(documentPayload, activate = true) {
  if (!documentPayload || !documentPayload.path) {
    return;
  }

  const existing = state.documents.get(documentPayload.path) || {};
  const doc = {
    ...existing,
    path: documentPayload.path,
    name: documentPayload.name || documentPayload.path.split(/[\\/]/).pop(),
    text: documentPayload.text || "",
    dirty: Boolean(documentPayload.dirty),
    diagnostics: existing.diagnostics || [],
    inspectorData: existing.inspectorData || {}
  };

  state.documents.set(doc.path, doc);
  if (!state.openDocuments.includes(doc.path)) {
    state.openDocuments.push(doc.path);
  }

  if (activate) {
    state.activeDocumentPath = doc.path;
    state.layout.activeDocumentPath = doc.path;
    ensureModelForDocument(doc);
    if (editor && currentModel !== doc.model) {
      editor.setModel(doc.model);
      currentModel = doc.model;
    }
  }

  renderShell();
}

async function openDocument(path, activate = true) {
  const response = await request("openDocument", { path });
  if (!response?.ok) {
    setStatusPill(response?.error || "Unable to open document");
    return;
  }

  openDocumentPayload(response.document, activate);
  saveLayout();
}

function activateDocument(path) {
  const doc = state.documents.get(path);
  if (!doc) {
    openDocument(path, true);
    return;
  }

  state.activeDocumentPath = path;
  state.layout.activeDocumentPath = path;
  ensureModelForDocument(doc);
  if (editor) {
    editor.setModel(doc.model);
    currentModel = doc.model;
    editor.focus();
  }

  request("openDocument", { path }).catch(() => {});
  renderShell();
  saveLayout();
}

function closeDocument(path) {
  const index = state.openDocuments.indexOf(path);
  if (index === -1) {
    return;
  }

  state.openDocuments.splice(index, 1);
  if (state.activeDocumentPath === path) {
    state.activeDocumentPath = state.openDocuments[index] || state.openDocuments[index - 1] || null;
    if (state.activeDocumentPath) {
      activateDocument(state.activeDocumentPath);
      return;
    }
  }

  renderShell();
  saveLayout();
}

function ensureModelForDocument(doc) {
  if (!window.monaco) {
    return;
  }

  if (!doc.model) {
    doc.model = monaco.editor.createModel(doc.text, "minilang", monaco.Uri.parse(`file:///${doc.path.replace(/\\/g, "/")}`));
    doc.model.onDidChangeContent(() => {
      if (suppressChange || state.activeDocumentPath !== doc.path) {
        doc.text = doc.model.getValue();
        doc.dirty = true;
        return;
      }

      doc.text = doc.model.getValue();
      doc.dirty = true;
      queueDocumentSync();
      renderEditorTabs();
      updateStatusBar();
    });
  } else if (doc.model.getValue() !== doc.text) {
    suppressChange = true;
    doc.model.setValue(doc.text);
    suppressChange = false;
  }
}

function queueDocumentSync() {
  clearTimeout(analyzeTimer);
  analyzeTimer = setTimeout(() => {
    const doc = state.documents.get(state.activeDocumentPath);
    if (!doc) {
      return;
    }

    post({
      type: "documentChanged",
      path: doc.path,
      text: doc.model ? doc.model.getValue() : doc.text
    });
  }, 160);
}

async function saveCurrentDocument() {
  const doc = state.documents.get(state.activeDocumentPath);
  if (!doc) {
    return;
  }

  const response = await request("saveDocument", {
    path: doc.path,
    text: doc.model ? doc.model.getValue() : doc.text
  });

  if (response?.ok && response.document) {
    openDocumentPayload(response.document, true);
    doc.dirty = false;
    renderEditorTabs();
    updateStatusBar();
  }
}

async function performBuild() {
  const targetPath = state.startupFilePath || state.activeDocumentPath;
  const doc = state.documents.get(targetPath);
  if (!targetPath) {
    return;
  }

  const configuration = $("configSelector").value || "Debug";
  state.status.configuration = configuration;
  updateStatusBar();
  await request("build", {
    path: targetPath,
    text: doc ? (doc.model ? doc.model.getValue() : doc.text) : undefined,
    configuration
  });
}

async function performRun(forceConsole = false) {
  const targetPath = state.startupFilePath || state.activeDocumentPath;
  const doc = state.documents.get(targetPath);
  if (!targetPath) {
    return;
  }

  const configuration = forceConsole ? "Release" : ($("configSelector").value || "Debug");
  state.status.configuration = configuration;
  updateStatusBar();
  await request("run", {
    path: targetPath,
    text: doc ? (doc.model ? doc.model.getValue() : doc.text) : undefined,
    configuration
  });
}

function setStartupFile(path) {
  state.startupFilePath = path;
  state.layout.startupFilePath = path;
  updateStatusBar();
  renderRegion("left");
  saveLayout();
}

function openExplorerContextMenu(x, y, node) {
  const menu = $("explorerContextMenu");
  menu.innerHTML = "";

  const openItem = document.createElement("div");
  openItem.className = "context-item";
  openItem.innerHTML = `<span>Open</span><span class="badge">Enter</span>`;
  openItem.addEventListener("click", () => {
    closeExplorerContextMenu();
    openDocument(node.path);
  });
  menu.appendChild(openItem);

  const startupItem = document.createElement("div");
  startupItem.className = "context-item";
  startupItem.innerHTML = `<span>Set as Startup File</span><span class="badge">${node.path === state.startupFilePath ? "Current" : "Run"}</span>`;
  startupItem.addEventListener("click", () => {
    closeExplorerContextMenu();
    setStartupFile(node.path);
  });
  menu.appendChild(startupItem);

  menu.style.left = `${Math.min(x, window.innerWidth - 240)}px`;
  menu.style.top = `${Math.min(y, window.innerHeight - 120)}px`;
  menu.classList.remove("hidden");
}

function closeExplorerContextMenu() {
  $("explorerContextMenu").classList.add("hidden");
}

function severityClass(severity) {
  const normalized = String(severity || "").toLowerCase();
  if (normalized.includes("error")) {
    return "error";
  }

  if (normalized.includes("warn")) {
    return "warning";
  }

  return "";
}

function revealOffset(offset) {
  const doc = state.documents.get(state.activeDocumentPath);
  if (!doc || !doc.model || !editor) {
    return;
  }

  const position = doc.model.getPositionAt(Math.max(0, offset || 0));
  editor.setPosition(position);
  editor.revealPositionInCenter(position);
  editor.focus();
}

function triggerCompletionInsert(label) {
  if (!editor || !currentModel) {
    return;
  }

  const position = editor.getPosition();
  const word = currentModel.getWordUntilPosition(position);
  const range = {
    startLineNumber: position.lineNumber,
    startColumn: word.startColumn,
    endLineNumber: position.lineNumber,
    endColumn: word.endColumn
  };
  editor.executeEdits("minilang-search", [{ range, text: label }]);
  editor.focus();
}

function wireDropTarget(element, regionName) {
  element.addEventListener("dragover", (event) => {
    if (!dragState?.paneId) {
      return;
    }
    event.preventDefault();
    element.classList.add("over");
  });
  element.addEventListener("dragleave", () => element.classList.remove("over"));
  element.addEventListener("drop", (event) => {
    if (!dragState?.paneId) {
      return;
    }
    event.preventDefault();
    element.classList.remove("over");
    movePaneToRegion(dragState.paneId, regionName);
    dragState = null;
  });
}

function wireSplitters() {
  for (const splitter of document.querySelectorAll(".splitter")) {
    splitter.addEventListener("mousedown", (event) => {
      const type = splitter.dataset.splitter;
      const startX = event.clientX;
      const startY = event.clientY;
      const starting = { ...state.layout.sizes };

      function onMove(moveEvent) {
        if (type === "left") {
          state.layout.sizes.left = Math.max(220, Math.min(window.innerWidth - 640, starting.left + (moveEvent.clientX - startX)));
        } else if (type === "right") {
          state.layout.sizes.right = Math.max(280, Math.min(window.innerWidth - 500, starting.right - (moveEvent.clientX - startX)));
        } else if (type === "bottom") {
          state.layout.sizes.bottom = Math.max(160, Math.min(window.innerHeight - 240, starting.bottom - (moveEvent.clientY - startY)));
        }

        applyCssVariables();
      }

      function onUp() {
        window.removeEventListener("mousemove", onMove);
        window.removeEventListener("mouseup", onUp);
        saveLayout();
      }

      window.addEventListener("mousemove", onMove);
      window.addEventListener("mouseup", onUp);
    });
  }
}

function renderPanelModal() {
  const host = $("paneToggleList");
  host.innerHTML = "";

  for (const paneId of Object.keys(paneCatalog)) {
    const row = document.createElement("label");
    row.className = "pane-toggle";

    const checkbox = document.createElement("input");
    checkbox.type = "checkbox";
    checkbox.checked = !state.layout.hiddenPanes.includes(paneId);
    checkbox.addEventListener("change", () => {
      if (checkbox.checked) {
        ensurePaneVisible(paneId);
      } else {
        hidePane(paneId);
      }
    });
    row.appendChild(checkbox);

    const title = document.createElement("div");
    title.textContent = paneCatalog[paneId].title;
    row.appendChild(title);

    const region = document.createElement("div");
    region.className = "badge";
    region.textContent = paneCatalog[paneId].region;
    row.appendChild(region);
    host.appendChild(row);
  }
}

function openPanelModal() {
  $("paneModal").classList.remove("hidden");
}

function closePanelModal() {
  $("paneModal").classList.add("hidden");
}

function escapeHtml(text) {
  return String(text ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll("\"", "&quot;");
}

function attachHostListener() {
  window.chrome?.webview?.addEventListener("message", (event) => {
    const message = parseIncomingMessage(event.data);

    if (message.type === "response") {
      const resolver = pending.get(message.requestId);
      if (resolver) {
        pending.delete(message.requestId);
        resolver(message.payload);
      }
      return;
    }

    if (message.type === "initialize") {
      initializeFromHost(message);
      return;
    }

    if (message.type === "analysis") {
      applyAnalysis(message);
      return;
    }

    if (message.type === "buildResult") {
      applyBuildResult(message);
      return;
    }

    if (message.type === "runResult") {
      applyRunResult(message);
      return;
    }

    if (message.type === "externalRunStarted") {
      state.output.terminal = `${message.commandLine}\n\nWorking directory: ${message.workingDirectory}`;
      setActivePane("bottom", "terminal");
      return;
    }

    if (message.type === "status") {
      state.status = { ...state.status, ...message };
      updateStatusBar();
      return;
    }

    if (message.type === "hostError") {
      state.output.debugConsole = `${state.output.debugConsole}\n${message.message}`;
      setActivePane("bottom", "debugConsole");
    }
  });
}

function initializeFromHost(message) {
  state.explorer = message.explorer || [];
  state.layout = normalizeLayout(message.layout || {});
  state.startupFilePath = state.layout.startupFilePath;
  state.status = { ...state.status, ...(message.state || {}) };
  openDocumentPayload(message.document, true);
  state.layout.openDocuments = state.layout.openDocuments || [];

  for (const path of state.layout.openDocuments) {
    if (path && path !== message.document.path) {
      openDocument(path, false);
    }
  }

  if (state.layout.activeDocumentPath && state.layout.activeDocumentPath !== message.document.path) {
    activateDocument(state.layout.activeDocumentPath);
  }

  if (!state.startupFilePath) {
    state.startupFilePath = message.document.path;
    state.layout.startupFilePath = message.document.path;
  }

  renderShell();
}

function applyAnalysis(message) {
  state.diagnostics = message.diagnostics || [];
  state.outline = message.outline || [];
  state.completions = message.completions || [];
  state.hoverIndex = message.hoverIndex || {};
  state.inspectors.syntaxTree = message.inspectors?.syntaxTree || [];
  state.inspectors.semantic = message.inspectors?.semantic || [];
  state.inspectors.boundTree = message.inspectors?.boundTree || [];
  state.inspectors.lowered = message.inspectors?.lowered || [];
  state.inspectors.symbolInspector = message.inspectors?.symbols || [];
  state.inspectors.interop = message.inspectors?.interop || [];
  state.inspectors.diagnostics = message.inspectors?.diagnostics || [];

  const doc = state.documents.get(message.documentPath || state.activeDocumentPath);
  if (doc) {
    doc.diagnostics = state.diagnostics;
  }

  applyMarkers();
  renderShell();
}

function applyBuildResult(message) {
  state.diagnostics = message.diagnostics || [];
  state.inspectors.syntaxTree = message.syntaxTree || [];
  state.inspectors.symbolInspector = message.symbols || [];
  state.inspectors.semantic = message.semanticModel || [];
  state.inspectors.boundTree = message.boundTree || [];
  state.inspectors.lowered = message.lowered || [];
  state.inspectors.interop = message.interop || [];
  state.output.buildOutput = message.output || "";
  state.status.configuration = message.configuration || state.status.configuration;
  applyMarkers();
  setActivePane("bottom", state.diagnostics.length ? "diagnostics" : "buildOutput");
}

function applyRunResult(message) {
  state.diagnostics = message.diagnostics || [];
  if (message.mode === "external") {
    state.output.terminal = message.output || "";
    setActivePane("bottom", "terminal");
  } else {
    state.output.debugConsole = message.output || "";
    setActivePane("bottom", state.diagnostics.length ? "diagnostics" : "debugConsole");
  }
  applyMarkers();
  renderShell();
}

function applyMarkers() {
  const doc = state.documents.get(state.activeDocumentPath);
  if (!window.monaco || !doc?.model) {
    return;
  }

  const markers = state.diagnostics.map((diagnostic) => {
    const start = doc.model.getPositionAt(diagnostic.start || 0);
    const end = doc.model.getPositionAt((diagnostic.start || 0) + Math.max(1, diagnostic.length || 1));
    const severityText = String(diagnostic.severity || "").toLowerCase();
    const severity = severityText.includes("error")
      ? monaco.MarkerSeverity.Error
      : severityText.includes("warn")
        ? monaco.MarkerSeverity.Warning
        : monaco.MarkerSeverity.Info;
    return {
      severity,
      message: `${diagnostic.id}: ${diagnostic.message}`,
      startLineNumber: start.lineNumber,
      startColumn: start.column,
      endLineNumber: end.lineNumber,
      endColumn: end.column
    };
  });

  monaco.editor.setModelMarkers(doc.model, "minilang", markers);

  if (editor && currentModel === doc.model) {
    const decorations = state.diagnostics.map((diagnostic) => {
      const start = doc.model.getPositionAt(diagnostic.start || 0);
      const end = doc.model.getPositionAt((diagnostic.start || 0) + Math.max(1, diagnostic.length || 1));
      const severityText = String(diagnostic.severity || "").toLowerCase();
      return {
        range: new monaco.Range(start.lineNumber, start.column, end.lineNumber, end.column),
        options: {
          inlineClassName: severityText.includes("warn") ? "mini-warning-squiggle" : "mini-error-squiggle",
          stickiness: monaco.editor.TrackedRangeStickiness.NeverGrowsWhenTypingAtEdges
        }
      };
    });

    validationDecorations = editor.deltaDecorations(validationDecorations, decorations);
  }
}

function registerLanguage() {
  monaco.languages.register({ id: "minilang" });

  monaco.editor.defineTheme("minilang-charcoal", {
    base: "vs-dark",
    inherit: true,
    rules: [
      { token: "keyword", foreground: "78C3FF", fontStyle: "bold" },
      { token: "type.identifier", foreground: "9ABEFF", fontStyle: "bold" },
      { token: "string", foreground: "A6D98B" },
      { token: "number", foreground: "FFCC7C" },
      { token: "identifier", foreground: "E6EDF9" },
      { token: "annotation", foreground: "D8A7FF" }
    ],
    colors: {
      "editor.background": "#0E141C",
      "editorLineNumber.foreground": "#586679",
      "editorLineNumber.activeForeground": "#D8E7FF",
      "editorCursor.foreground": "#7CC5FF",
      "editor.selectionBackground": "#1E3956",
      "editor.inactiveSelectionBackground": "#1B2D42",
      "editor.lineHighlightBackground": "#121D2A",
      "editorIndentGuide.background": "#223044",
      "editorIndentGuide.activeBackground": "#3A5577"
    }
  });

  monaco.languages.setMonarchTokensProvider("minilang", {
    tokenizer: {
      root: [
        [/\b(fn|struct|enum|give|use|say|done|make|if|else|while|win|cscall|public|private|new)\b/, "keyword"],
        [/\b(number|string|object|nothing)\b/, "type.identifier"],
        [/"[^"]*"/, "string"],
        [/\d+(\.\d+)?/, "number"],
        [/[A-Za-z_][\w.]*/, "identifier"]
      ]
    }
  });

  monaco.languages.registerCompletionItemProvider("minilang", {
    triggerCharacters: [".", "_"],
    async provideCompletionItems() {
      const items = await request("completion", { offset: getCurrentOffset() });
      return {
        suggestions: (items || []).map((item) => ({
          label: item.label,
          kind: monaco.languages.CompletionItemKind.Text,
          insertText: item.label,
          detail: item.detail
        }))
      };
    }
  });

  monaco.languages.registerHoverProvider("minilang", {
    async provideHover(model, position) {
      const hover = await request("hover", { offset: model.getOffsetAt(position) });
      if (!hover || !hover.title) {
        return null;
      }

      return {
        contents: [
          { value: `**${hover.title}**` },
          { value: hover.detail || "" },
          hover.docsPath ? { value: `Docs: ${hover.docsPath}` } : null
        ].filter(Boolean)
      };
    }
  });

  monaco.languages.registerDefinitionProvider("minilang", {
    async provideDefinition(model, position) {
      const definition = await request("definition", { offset: model.getOffsetAt(position) });
      if (!definition) {
        return null;
      }

      if (definition.docsPath && !definition.documentPath) {
        post({ type: "openDocs", docsPath: definition.docsPath });
        return null;
      }

      if (!definition.documentPath || definition.start === undefined) {
        return null;
      }

      if (definition.documentPath !== state.activeDocumentPath) {
        await openDocument(definition.documentPath, true);
      }

      const doc = state.documents.get(definition.documentPath);
      if (!doc?.model) {
        return null;
      }

      const start = doc.model.getPositionAt(definition.start);
      const end = doc.model.getPositionAt(definition.start + Math.max(1, definition.length || 1));
      return {
        uri: doc.model.uri,
        range: new monaco.Range(start.lineNumber, start.column, end.lineNumber, end.column)
      };
    }
  });
}

function getCurrentOffset() {
  if (!editor || !currentModel) {
    return 0;
  }

  return currentModel.getOffsetAt(editor.getPosition());
}

function startMonaco() {
  registerLanguage();
  editor = monaco.editor.create($("editorHost"), {
    language: "minilang",
    theme: "minilang-charcoal",
    automaticLayout: true,
    fontFamily: '"JetBrains Mono", "Cascadia Code", Consolas, monospace',
    fontSize: 13.5,
    lineNumbers: "on",
    smoothScrolling: true,
    quickSuggestions: { other: true, comments: false, strings: false },
    suggestOnTriggerCharacters: true,
    minimap: { enabled: false },
    scrollBeyondLastLine: false,
    renderLineHighlight: "all",
    renderValidationDecorations: "on",
    glyphMargin: true,
    padding: { top: 16, bottom: 16 },
    bracketPairColorization: { enabled: true }
  });

  editor.onDidChangeCursorPosition((event) => {
    state.status.cursor = { line: event.position.lineNumber, column: event.position.column };
    updateStatusBar();
    post({ type: "cursorChanged", line: event.position.lineNumber, column: event.position.column });
  });

  editor.onDidChangeModelContent((event) => {
    if (suppressChange || !editor.hasTextFocus()) {
      return;
    }

    const doc = state.documents.get(state.activeDocumentPath);
    if (!doc) {
      return;
    }

    doc.text = doc.model.getValue();
    doc.dirty = true;
    queueDocumentSync();
    renderEditorTabs();
    updateStatusBar();

    const change = event.changes[event.changes.length - 1];
    const last = change?.text?.slice(-1);
    if (last && /[A-Za-z_.]/.test(last)) {
      window.clearTimeout(editor._suggestTimer);
      editor._suggestTimer = window.setTimeout(() => editor.trigger("minilang", "editor.action.triggerSuggest", {}), 70);
    }
  });

  attachUiHandlers();
  wireSplitters();
  renderShell();
  setStatusPill("Monaco ready");
  log("monaco:ready");
  post({ type: "ready" });
}

function attachUiHandlers() {
  $("saveButton").addEventListener("click", () => saveCurrentDocument());
  $("buildButton").addEventListener("click", () => performBuild());
  $("runButton").addEventListener("click", () => performRun(false));
  $("consoleRunButton").addEventListener("click", () => performRun(true));
  $("panelsButton").addEventListener("click", openPanelModal);
  $("closePaneModalButton").addEventListener("click", closePanelModal);
  $("resetLayoutButton").addEventListener("click", () => {
    state.layout = structuredClone(defaultLayout);
    state.startupFilePath = state.activeDocumentPath;
    state.layout.startupFilePath = state.activeDocumentPath;
    renderShell();
    saveLayout();
  });
  $("openConsoleTabButton").addEventListener("click", () => setActivePane("bottom", "terminal"));
  $("toolbarSearchBox").addEventListener("input", () => renderRegion("left"));
  $("commandBox").addEventListener("input", () => renderRegion("left"));
  $("configSelector").addEventListener("change", () => {
    state.status.configuration = $("configSelector").value;
    updateStatusBar();
  });
  $("paneModal").addEventListener("click", (event) => {
    if (event.target === $("paneModal")) {
      closePanelModal();
    }
  });
  document.addEventListener("click", () => closeExplorerContextMenu());
  window.addEventListener("contextmenu", (event) => {
    if (!event.target.closest(".tree-row")) {
      closeExplorerContextMenu();
    }
  });
  window.addEventListener("resize", applyCssVariables);
}

function beginMonacoLoad() {
  const loaderScript = document.createElement("script");
  loaderScript.src = `${monacoBasePath}/loader.js`;
  loaderScript.async = true;
  loaderScript.onload = () => {
    log("loader:loaded");
    require.config({ paths: { vs: monacoBasePath } });
    require(["vs/editor/editor.main"], startMonaco, (error) => {
      console.error(error);
      setStatusPill("Monaco failed to load");
    });
  };
  loaderScript.onerror = () => setStatusPill("Monaco loader failed");
  document.head.appendChild(loaderScript);
}

attachHostListener();
beginMonacoLoad();
