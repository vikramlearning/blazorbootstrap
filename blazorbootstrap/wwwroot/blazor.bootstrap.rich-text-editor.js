const instances = new Map();
const draftKey = "advanced-rich-text-editor-document";
const allowedTags = new Set(["P", "BR", "H1", "H2", "H3", "FIGCAPTION", "STRONG", "EM", "U", "S", "SPAN", "UL", "OL", "LI", "BLOCKQUOTE", "PRE", "CODE", "HR", "A", "IMG", "FIGURE", "TABLE", "CAPTION", "THEAD", "TBODY", "TR", "TH", "TD"]);
const blockedTags = new Set(["SCRIPT", "STYLE", "TEMPLATE", "IFRAME", "OBJECT", "EMBED", "FORM", "INPUT", "BUTTON", "SVG", "MATH", "META", "LINK"]);
const allowedClasses = new Set(["text-start", "text-center", "text-end", "img-fluid", "table", "table-bordered"]);

function isSafeUrl(value, image = false) {
    if (!value) return false;
    const trimmed = value.trim();
    if (trimmed.startsWith("/") || trimmed.startsWith("#")) return !image;
    try {
        const url = new URL(trimmed, window.location.origin);
        return ["https:", "mailto:", "tel:"].includes(url.protocol) && (!image || url.protocol === "https:");
    } catch { return false; }
}

function sanitizeStyle(value) { const style = document.createElement("span").style; style.cssText = value; const allowed = new Map([["color", /^#[0-9a-f]{6}$/i], ["background-color", /^#[0-9a-f]{6}$/i], ["font-family", /^(Inter|Arial|Georgia|Courier New)$/], ["font-size", /^(12|14|16|18|24)px$/], ["text-align", /^(left|center|right)$/], ["margin-left", /^(0|24|48|72|96)px$/]]); const safe = []; for (const [name, pattern] of allowed) { const value = style.getPropertyValue(name).trim(); if (value && pattern.test(value)) safe.push(`${name}: ${value}`); } return safe.join("; "); }\n\nfunction sanitize(html) {
    const documentFragment = new DOMParser().parseFromString(html || "", "text/html");
    for (const node of [...documentFragment.body.querySelectorAll("*")]) {
        if (blockedTags.has(node.tagName)) { node.remove(); continue; }
        if (!allowedTags.has(node.tagName)) { node.replaceWith(...node.childNodes); continue; }
        for (const attribute of [...node.attributes]) {
            const name = attribute.name.toLowerCase();
            const allowed = (node.tagName === "A" && ["href", "target", "rel"].includes(name))
                || (node.tagName === "IMG" && ["src", "alt"].includes(name))
                || (node.tagName === "TH" && name === "scope")
                || (node.tagName === "SPAN" && ["data-bb-rte-color", "style"].includes(name))
                || (name === "class" && [...attribute.value.split(/\s+/)].every(value => allowedClasses.has(value)));
            if (name === "style" && !/^(color|background-color|font-family|font-size|text-align|margin-left):\s*[-#(),.%\w\s]+;?$/i.test(attribute.value)) node.removeAttribute(attribute.name); else if (name.startsWith("on") || !allowed) node.removeAttribute(attribute.name);
        }
        if (node.tagName === "A" && !isSafeUrl(node.getAttribute("href"))) node.remove();
        if (node.tagName === "A" && node.target === "_blank") node.rel = "noopener noreferrer";
        if (node.tagName === "IMG" && !isSafeUrl(node.getAttribute("src"), true)) node.remove();
    }
    return documentFragment.body.innerHTML;
}

function currentRange(instance) {
    const selection = window.getSelection();
    return selection?.rangeCount && instance.editor.contains(selection.anchorNode) ? selection.getRangeAt(0) : instance.range;
}
function saveRange(instance) { const range = currentRange(instance); if (range) instance.range = range.cloneRange(); }
function restoreRange(instance) { if (!instance.range) return false; const selection = window.getSelection(); selection.removeAllRanges(); selection.addRange(instance.range); return true; }
function textMetrics(instance) { const text = instance.editor.innerText.trim(); return { text, characters: text.length, words: text ? text.split(/\s+/).length : 0 }; }
function updateFooter(instance) { const metrics = textMetrics(instance); instance.root.querySelector("[data-bb-rte-character-count]").textContent = `${metrics.characters} characters`; instance.root.querySelector("[data-bb-rte-word-count]").textContent = `${metrics.words} words`; const range = currentRange(instance); let node = range?.commonAncestorContainer; if (node?.nodeType === Node.TEXT_NODE) node = node.parentElement; const block = node?.closest?.("p,h1,h2,h3,figcaption,blockquote,pre,li"); const context = block?.tagName === "P" || !block ? "Paragraph" : block.tagName.replace("H", "Heading "); instance.root.querySelector("[data-bb-rte-context]").textContent = context; instance.root.querySelector("[data-bb-rte-alignment-context]").textContent = block?.style.textAlign ? `${block.style.textAlign[0].toUpperCase()}${block.style.textAlign.slice(1)} aligned` : "Left aligned"; }
function pushHistory(instance) { const html = sanitize(instance.editor.innerHTML); if (instance.history[instance.historyIndex] === html) return; instance.history.splice(instance.historyIndex + 1); instance.history.push(html); if (instance.history.length > 100) instance.history.shift(); instance.historyIndex = instance.history.length - 1; }
function commit(instance, status = "") { normalizeTables(instance); const html = sanitize(instance.editor.innerHTML); if (instance.editor.innerHTML !== html) instance.editor.innerHTML = html; pushHistory(instance); updateFooter(instance); try { sessionStorage.setItem(draftKey, html); } catch { } instance.dotNetRef.invokeMethodAsync("OnEditorValueChangedAsync", html); if (status) instance.dotNetRef.invokeMethodAsync("OnEditorStatusChangedAsync", status); }
function insertNode(instance, node) { restoreRange(instance); const range = currentRange(instance) || document.createRange(); if (!range.startContainer.isConnected) range.selectNodeContents(instance.editor); range.collapse(false); range.deleteContents(); range.insertNode(node); range.setStartAfter(node); range.collapse(true); const selection = window.getSelection(); selection.removeAllRanges(); selection.addRange(range); saveRange(instance); }
function wrapSelection(instance, tagName, attributes = {}) { restoreRange(instance); const range = currentRange(instance); if (!range || range.collapsed) return false; const element = document.createElement(tagName); Object.entries(attributes).forEach(([name, value]) => element.setAttribute(name, value)); try { range.surroundContents(element); } catch { const fragment = range.extractContents(); element.append(fragment); range.insertNode(element); } saveRange(instance); return true; }
function blockFormat(instance, tagName) { restoreRange(instance); const range = currentRange(instance); let node = range?.commonAncestorContainer; if (node?.nodeType === Node.TEXT_NODE) node = node.parentElement; const block = node?.closest("p,h1,h2,h3,figcaption,blockquote,pre,li") || null; if (!block) return false; const replacement = document.createElement(tagName); replacement.append(...block.childNodes); block.replaceWith(replacement); const selection = window.getSelection(); const nextRange = document.createRange(); nextRange.selectNodeContents(replacement); nextRange.collapse(false); selection.removeAllRanges(); selection.addRange(nextRange); saveRange(instance); return true; }
function selectedElement(instance, selector) {
    const range = currentRange(instance); let node = range?.commonAncestorContainer;
    if (node?.nodeType === Node.TEXT_NODE) node = node.parentElement;
    return node?.closest?.(selector) || null;
}
function normalizeTables(instance) {
    for (const table of instance.editor.querySelectorAll("table")) {
        table.classList.add("table", "table-bordered");
        let rows = [...table.querySelectorAll(":scope > tr, :scope > thead > tr, :scope > tbody > tr")];
        if (!rows.length) continue;
        let thead = table.querySelector(":scope > thead");
        let tbody = table.querySelector(":scope > tbody");
        if (!thead) { thead = document.createElement("thead"); table.prepend(thead); }
        if (!tbody) { tbody = document.createElement("tbody"); table.append(tbody); }
        const header = rows.shift();
        for (const cell of [...header.children]) { const th = document.createElement("th"); th.scope = "col"; th.append(...cell.childNodes); cell.replaceWith(th); }
        thead.replaceChildren(header);
        for (const row of rows) { for (const cell of [...row.children]) { const td = document.createElement("td"); td.append(...cell.childNodes); cell.replaceWith(td); } tbody.append(row); }
    }
}
function showModal(instance, name) {
    saveRange(instance);
    const element = document.getElementById(`${instance.id}-${name}-modal`);
    if (!element || !window.bootstrap?.Modal) { instance.dotNetRef.invokeMethodAsync("OnEditorStatusChangedAsync", "This dialog is unavailable."); return; }
    window.bootstrap.Modal.getOrCreateInstance(element).show();
}
function insertLink(instance) {
    const modal = document.getElementById(`${instance.id}-link-modal`);
    const url = modal?.querySelector("[data-bb-rte-link-url]")?.value?.trim();
    const text = modal?.querySelector("[data-bb-rte-link-text]")?.value?.trim();
    const newTab = modal?.querySelector("[data-bb-rte-link-new-tab]")?.checked;
    if (!isSafeUrl(url)) { instance.dotNetRef.invokeMethodAsync("OnEditorStatusChangedAsync", "Enter an HTTPS, relative, mailto, or telephone URL."); return; }
    const link = instance.editingLink || document.createElement("a"); link.href = url; link.textContent = text || currentRange(instance)?.toString() || url;
    if (newTab) { link.target = "_blank"; link.rel = "noopener noreferrer"; }
    insertNode(instance, link); window.bootstrap.Modal.getInstance(modal)?.hide(); commit(instance);
}
function insertImageFromModal(instance) {
    const modal = document.getElementById(`${instance.id}-image-modal`);
    const url = modal?.querySelector("[data-bb-rte-image-url]")?.value?.trim();
    const alt = modal?.querySelector("[data-bb-rte-image-alt]")?.value?.trim();
    if (!isSafeUrl(url, true) || !alt) { instance.dotNetRef.invokeMethodAsync("OnEditorStatusChangedAsync", "Provide an HTTPS image URL and alternative text."); return; }
    const image = document.createElement("img"); image.src = url; image.alt = alt; image.className = "img-fluid";
    insertNode(instance, image); window.bootstrap.Modal.getInstance(modal)?.hide(); commit(instance);
}
function toggleFullscreen(instance) {
    if (document.fullscreenElement === instance.root) document.exitFullscreen?.();
    else instance.root.requestFullscreen?.().catch(() => instance.dotNetRef.invokeMethodAsync("OnEditorStatusChangedAsync", "Fullscreen is unavailable."));
}
function printEditor(instance) {
    const previousTitle = document.title; document.title = instance.root.getAttribute("aria-label") || "Rich text document";
    window.addEventListener("afterprint", () => { document.title = previousTitle; }, { once: true }); window.print();
}
function insertTable(instance) {
    const modal = document.getElementById(`${instance.id}-table-modal`);
    const rows = Number(modal?.querySelector("[data-bb-rte-table-rows]")?.value);
    const columns = Number(modal?.querySelector("[data-bb-rte-table-columns]")?.value);
    if (!Number.isInteger(rows) || !Number.isInteger(columns) || rows < 1 || rows > 20 || columns < 1 || columns > 10) { instance.dotNetRef.invokeMethodAsync("OnEditorStatusChangedAsync", "Choose between 1–20 rows and 1–10 columns."); return; }
    const table = document.createElement("table"); table.className = "table table-bordered";
    const thead = document.createElement("thead"); const header = document.createElement("tr");
    for (let column = 0; column < columns; column++) { const cell = document.createElement("th"); cell.scope = "col"; cell.append(document.createElement("br")); header.append(cell); }
    thead.append(header); table.append(thead); const body = document.createElement("tbody");
    for (let row = 1; row < rows; row++) { const tr = document.createElement("tr"); for (let column = 0; column < columns; column++) { const cell = document.createElement("td"); cell.append(document.createElement("br")); tr.append(cell); } body.append(tr); }
    table.append(body); insertNode(instance, table); window.bootstrap.Modal.getInstance(modal)?.hide(); commit(instance);
}
function toggleList(instance, tagName) { restoreRange(instance); const range = currentRange(instance); let node = range?.commonAncestorContainer; if (node?.nodeType === Node.TEXT_NODE) node = node.parentElement; const block = node?.closest?.("p,h1,h2,h3,figcaption,blockquote,pre,li"); if (!block) return false; const list = document.createElement(tagName); const item = document.createElement("li"); item.append(...block.childNodes); list.append(item); block.replaceWith(list); const selection = window.getSelection(); const nextRange = document.createRange(); nextRange.selectNodeContents(item); nextRange.collapse(false); selection.removeAllRanges(); selection.addRange(nextRange); saveRange(instance); return true; }\n\nfunction execute(instance, command, value) {
    if (instance.disabled || instance.readOnly) return;
    instance.editor.focus(); restoreRange(instance);
    let changed = false;
    if (command === "Link") { showModal(instance, "link"); return; } else if (command === "Image") { showModal(instance, "image"); return; } else if (command === "Table") { showModal(instance, "table"); return; } else if (command === "Fullscreen") { toggleFullscreen(instance); return; } else if (command === "Print") { printEditor(instance); return; } else if (["Bold", "Italic", "Underline", "Strikethrough"].includes(command)) changed = wrapSelection(instance, { Bold: "strong", Italic: "em", Underline: "u", Strikethrough: "s" }[command]);
    else if (command === "BlockFormat") changed = blockFormat(instance, value || "p");\n    else if (command === "OrderedList") changed = toggleList(instance, "ol");\n    else if (command === "UnorderedList") changed = toggleList(instance, "ul");\n    else if (command === "Blockquote") changed = blockFormat(instance, "blockquote");\n    else if (command === "CodeBlock") changed = blockFormat(instance, "pre");
    else if (command === "HorizontalRule") { insertNode(instance, document.createElement("hr")); changed = true; }
    else if (command === "Undo" || command === "Redo") { const next = instance.historyIndex + (command === "Undo" ? -1 : 1); if (next >= 0 && next < instance.history.length) { instance.historyIndex = next; instance.editor.innerHTML = instance.history[next]; changed = true; } }
    else if (command === "ClearFormatting") { restoreRange(instance); const range = currentRange(instance); if (range && !range.collapsed) { const text = range.toString(); range.deleteContents(); range.insertNode(document.createTextNode(text)); changed = true; } }
    else if (["TextColor", "HighlightColor", "FontFamily", "FontSize"].includes(command)) { changed = wrapSelection(instance, "span", { "data-bb-rte-color": value || "" }); const range = currentRange(instance); const span = range?.commonAncestorContainer?.parentElement?.closest?.("span"); if (span) { if (command === "TextColor") span.style.color = value; if (command === "HighlightColor") span.style.backgroundColor = value; if (command === "FontFamily") span.style.fontFamily = value; if (command === "FontSize") span.style.fontSize = `${value}px`; } } else if (["AlignStart", "AlignCenter", "AlignEnd", "Indent", "Outdent"].includes(command)) {
        const range = currentRange(instance); let node = range?.commonAncestorContainer; if (node?.nodeType === Node.TEXT_NODE) node = node.parentElement; const block = node?.closest?.("p,h1,h2,h3,figcaption,blockquote,pre,li"); if (block) { if (command.startsWith("Align")) block.style.textAlign = { AlignStart: "left", AlignCenter: "center", AlignEnd: "right" }[command]; else { const margin = Number.parseInt(block.style.marginLeft || "0", 10) || 0; block.style.marginLeft = `${Math.max(0, margin + (command === "Indent" ? 24 : -24))}px`; } changed = true; }
        if (changed) commit(instance);
        else if (!["Link", "Image", "Table", "Fullscreen", "Print"].includes(command)) instance.dotNetRef.invokeMethodAsync("OnEditorStatusChangedAsync", "Select content before applying this command.");
    }

    export function initialize(id, dotNetRef, debounceInterval, maxLength, readOnly, disabled) {
        dispose(id); const root = document.getElementById(id); const editor = document.getElementById(`${id}-editor`); if (!root || !editor) return;
        const instance = { id, root, editor, dotNetRef, debounceInterval: Math.max(0, debounceInterval || 300), maxLength, readOnly, disabled, range: null, timer: 0, history: [], historyIndex: -1 };
        instances.set(id, instance); let stored = null; try { stored = sessionStorage.getItem(draftKey); } catch { } editor.innerHTML = sanitize(stored || editor.innerHTML); pushHistory(instance); updateFooter(instance);
        editor.addEventListener("input", () => { if (instance.maxLength && editor.innerText.length > instance.maxLength) editor.innerText = editor.innerText.slice(0, instance.maxLength); saveRange(instance); clearTimeout(instance.timer); instance.timer = setTimeout(() => commit(instance), instance.debounceInterval); });
        ["keyup", "mouseup", "focusout"].forEach(eventName => editor.addEventListener(eventName, () => saveRange(instance)));
        editor.addEventListener("paste", event => { event.preventDefault(); const text = event.clipboardData.getData("text/plain"); insertNode(instance, document.createTextNode(text)); commit(instance); });
        root.addEventListener("click", event => { const button = event.target.closest("[data-bb-rte-command]"); if (button) { saveRange(instance); execute(instance, button.dataset.bbRteCommand, button.dataset.bbRteValue); return; } if (event.target.closest("[data-bb-rte-save-link]")) insertLink(instance); if (event.target.closest("[data-bb-rte-save-image]")) insertImageFromModal(instance); if (event.target.closest("[data-bb-rte-save-table]")) insertTable(instance); });
    }
    export function clear(id) { const instance = instances.get(id); if (!instance) return; instance.editor.innerHTML = ""; commit(instance); }
    export function dispose(id) { const instance = instances.get(id); if (instance) clearTimeout(instance.timer); instances.delete(id); }
    export function focus(id) { instances.get(id)?.editor.focus(); }
    export function insertImage(id, url, altText) { const instance = instances.get(id); if (!instance || !isSafeUrl(url, true)) return; const image = document.createElement("img"); image.src = url; image.alt = altText || ""; image.className = "img-fluid"; insertNode(instance, image); commit(instance); }
    export function setValue(id, value) { const instance = instances.get(id); if (!instance) return; instance.editor.innerHTML = sanitize(value); pushHistory(instance); updateFooter(instance); }