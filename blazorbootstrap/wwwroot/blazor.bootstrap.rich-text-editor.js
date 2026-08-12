const instances = new Map();
const allowedTags = new Set(["P", "BR", "H1", "H2", "H3", "STRONG", "EM", "U", "S", "UL", "OL", "LI", "BLOCKQUOTE", "PRE", "CODE", "A", "IMG", "TABLE", "THEAD", "TBODY", "TR", "TH", "TD", "SPAN"]);
const blockedTags = new Set(["SCRIPT", "STYLE", "TEMPLATE", "IFRAME", "OBJECT", "EMBED", "FORM", "INPUT", "BUTTON", "SVG", "MATH", "META", "LINK"]);
const allowedClasses = new Set(["text-start", "text-center", "text-end", "text-primary", "text-secondary", "text-success", "text-danger", "text-warning", "text-info", "text-dark", "img-fluid", "table", "table-bordered"]);

function isSafeLink(value) {
    if (!value) return false;
    const trimmed = value.trim();
    return trimmed.startsWith("/") || trimmed.startsWith("#") || /^https:\/\//i.test(trimmed);
}

function isSafeImage(value) {
    return !!value && /^https:\/\//i.test(value.trim());
}

function sanitize(html) {
    const documentFragment = new DOMParser().parseFromString(html || "", "text/html");
    const nodes = Array.from(documentFragment.body.querySelectorAll("*"));

    for (const node of nodes) {
        if (blockedTags.has(node.tagName)) {
            node.remove();
            continue;
        }

        if (!allowedTags.has(node.tagName)) {
            node.replaceWith(...Array.from(node.childNodes));
            continue;
        }

        for (const attribute of Array.from(node.attributes)) {
            const name = attribute.name.toLowerCase();
            if (name.startsWith("on") || name === "style" || name === "id") {
                node.removeAttribute(attribute.name);
                continue;
            }

            if (name === "class") {
                const safeClasses = attribute.value.split(/\s+/).filter(value => allowedClasses.has(value));
                if (safeClasses.length) node.setAttribute("class", safeClasses.join(" "));
                else node.removeAttribute("class");
                continue;
            }

            const allowed = (node.tagName === "A" && name === "href")
                || (node.tagName === "IMG" && (name === "src" || name === "alt"))
                || (node.tagName === "TH" && name === "scope");
            if (!allowed) node.removeAttribute(attribute.name);
        }

        if (node.tagName === "A" && !isSafeLink(node.getAttribute("href"))) node.removeAttribute("href");
        if (node.tagName === "IMG" && !isSafeImage(node.getAttribute("src"))) node.remove();
    }

    return documentFragment.body.innerHTML;
}

function getRange(instance) {
    const selection = window.getSelection();
    if (selection && selection.rangeCount && instance.editor.contains(selection.anchorNode)) return selection.getRangeAt(0);
    return instance.range;
}

function saveRange(instance) {
    const range = getRange(instance);
    if (range) instance.range = range.cloneRange();
}

function restoreRange(instance) {
    if (!instance.range) return;
    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(instance.range);
}

function notify(instance, immediate = false) {
    window.clearTimeout(instance.timer);
    const raise = () => {
        const html = sanitize(instance.editor.innerHTML);
        if (instance.editor.innerHTML !== html) instance.editor.innerHTML = html;
        instance.dotNetRef.invokeMethodAsync("OnEditorValueChangedAsync", html);
    };
    if (immediate) raise();
    else instance.timer = window.setTimeout(raise, instance.debounceInterval);
}

function enforceLength(instance) {
    if (!instance.maxLength || instance.editor.innerText.length <= instance.maxLength) return;
    instance.editor.innerText = instance.editor.innerText.substring(0, instance.maxLength);
}

function insertNode(instance, node) {
    restoreRange(instance);
    const range = getRange(instance) || document.createRange();
    if (!range.startContainer.isConnected) range.selectNodeContents(instance.editor);
    range.collapse(false);
    range.deleteContents();
    range.insertNode(node);
    range.setStartAfter(node);
    range.collapse(true);
    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
    saveRange(instance);
}

function applyBlockClass(instance, className) {
    restoreRange(instance);
    const range = getRange(instance);
    let node = range?.commonAncestorContainer;
    if (node?.nodeType === Node.TEXT_NODE) node = node.parentElement;
    const block = node?.closest?.("p,h1,h2,h3,blockquote,pre,li") || instance.editor;
    [...block.classList].filter(value => value.startsWith("text-")).forEach(value => block.classList.remove(value));
    if (className) block.classList.add(className);
}

function execute(instance, command, value) {
    if (instance.disabled || instance.readOnly) return;
    restoreRange(instance);
    instance.editor.focus();

    const commands = {
        Undo: ["undo"], Redo: ["redo"], Bold: ["bold"], Italic: ["italic"], Underline: ["underline"], Strikethrough: ["strikeThrough"],
        OrderedList: ["insertOrderedList"], UnorderedList: ["insertUnorderedList"], Blockquote: ["formatBlock", "blockquote"], CodeBlock: ["formatBlock", "pre"],
        AlignStart: ["justifyLeft"], AlignCenter: ["justifyCenter"], AlignEnd: ["justifyRight"], ClearFormatting: ["removeFormat"]
    };

    if (command === "BlockFormat") document.execCommand("formatBlock", false, value || "p");
    else if (command === "TextColor") applyBlockClass(instance, value);
    else if (commands[command]) document.execCommand(commands[command][0], false, commands[command][1]);

    enforceLength(instance);
    saveRange(instance);
    notify(instance, true);
}

function openDialog(instance, type) {
    saveRange(instance);
    instance.dialogMode = type;
    const dialog = document.getElementById(`${instance.id}-${type === "table" ? "table" : type === "link" ? "link" : "image"}-dialog`);
    if (dialog?.showModal) dialog.showModal();
}

function insertTable(instance, rows, columns) {
    const table = document.createElement("table");
    table.className = "table table-bordered";
    const body = document.createElement("tbody");
    for (let row = 0; row < rows; row++) {
        const tr = document.createElement("tr");
        for (let column = 0; column < columns; column++) {
            const cell = document.createElement(row === 0 ? "th" : "td");
            if (row === 0) cell.scope = "col";
            cell.appendChild(document.createElement("br"));
            tr.appendChild(cell);
        }
        body.appendChild(tr);
    }
    table.appendChild(body);
    insertNode(instance, table);
}

function wireDialog(instance, type) {
    const dialog = document.getElementById(`${instance.id}-${type}-dialog`);
    if (!dialog) return;
    dialog.addEventListener("close", () => {
        if (dialog.returnValue !== "save") return;
        if (type === "link") {
            const url = dialog.querySelector("[data-bb-rte-link-url]").value;
            if (!isSafeLink(url)) return;
            const anchor = document.createElement("a");
            anchor.href = url;
            anchor.textContent = window.getSelection()?.toString() || url;
            insertNode(instance, anchor);
        } else if (type === "image") {
            const url = dialog.querySelector("[data-bb-rte-image-url]").value;
            const alt = dialog.querySelector("[data-bb-rte-image-alt]").value.trim();
            if (!isSafeImage(url) || !alt) return;
            const image = document.createElement("img");
            image.src = url;
            image.alt = alt;
            image.className = "img-fluid";
            insertNode(instance, image);
        } else {
            const rows = Number(dialog.querySelector("[data-bb-rte-table-rows]").value);
            const columns = Number(dialog.querySelector("[data-bb-rte-table-columns]").value);
            if (Number.isInteger(rows) && Number.isInteger(columns) && rows > 0 && rows <= 20 && columns > 0 && columns <= 10) insertTable(instance, rows, columns);
        }
        notify(instance, true);
    });
}

export function initialize(id, dotNetRef, debounceInterval, maxLength, readOnly, disabled) {
    dispose(id);
    const root = document.getElementById(id);
    const editor = document.getElementById(`${id}-editor`);
    if (!root || !editor) return;
    const instance = { id, root, editor, dotNetRef, debounceInterval: Math.max(0, debounceInterval || 300), maxLength, readOnly, disabled, range: null, timer: 0 };
    instances.set(id, instance);
    editor.innerHTML = sanitize(editor.innerHTML);
    editor.addEventListener("input", () => { enforceLength(instance); saveRange(instance); notify(instance); });
    editor.addEventListener("keyup", () => saveRange(instance));
    editor.addEventListener("mouseup", () => saveRange(instance));
    editor.addEventListener("paste", event => {
        event.preventDefault();
        const html = event.clipboardData.getData("text/html");
        const text = event.clipboardData.getData("text/plain");
        const wrapper = document.createElement("div");
        wrapper.innerHTML = sanitize(html || text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/\n/g, "<br>"));
        insertNode(instance, wrapper);
        enforceLength(instance);
        notify(instance, true);
    });
    root.addEventListener("click", event => {
        const button = event.target.closest("[data-bb-rte-command]");
        if (!button) return;
        const command = button.dataset.bbRteCommand;
        if (command === "Link") openDialog(instance, "link");
        else if (command === "Image") openDialog(instance, "image");
        else if (command === "UploadImage") document.getElementById(`${id}-image-input`)?.click();
        else if (command === "Table") openDialog(instance, "table");
        else execute(instance, command);
    });
    root.addEventListener("change", event => {
        const select = event.target.closest("[data-bb-rte-command]");
        if (select) execute(instance, select.dataset.bbRteCommand, select.value);
    });
    wireDialog(instance, "link");
    wireDialog(instance, "image");
    wireDialog(instance, "table");
}

export function clear(id) {
    const instance = instances.get(id);
    if (!instance) return;
    instance.editor.innerHTML = "";
    notify(instance, true);
}

export function dispose(id) {
    const instance = instances.get(id);
    if (instance) window.clearTimeout(instance.timer);
    instances.delete(id);
}

export function focus(id) { instances.get(id)?.editor.focus(); }

export function insertImage(id, url, altText) {
    const instance = instances.get(id);
    if (!instance || !isSafeImage(url)) return;
    const image = document.createElement("img");
    image.src = url;
    image.alt = altText || "";
    image.className = "img-fluid";
    insertNode(instance, image);
    notify(instance, true);
}

export function setValue(id, value) {
    const instance = instances.get(id);
    if (!instance) return;
    const html = sanitize(value);
    if (instance.editor.innerHTML !== html) instance.editor.innerHTML = html;
}
