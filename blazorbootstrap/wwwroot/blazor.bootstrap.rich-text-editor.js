window.blzorBootstrap = window.blazorBootstrap || {};
window.blazorBootstrap.richTextEditor = window.blazorBootstrap.richTextEditor || {};

function getEditor(editorId) {
    return window.blazorBootstrap.richTextEditor[editorId];
}

function getOrCreate(editorId) {
    let editorEl = window.blazorBootstrap.richTextEditor[editorId];
    if (!editorEl)
        editorEl = create(editorId);
    return editorEl;

    function create(_editorId) {
        let editorEl = document.getElementById(_editorId);
        window.blazorBootstrap.richTextEditor[_editorId] = {
            editor: editorEl
        };
        return window.blazorBootstrap.richTextEditor[_editorId];
    }
}

export function dispose(dotNetHelper, editorId) {
    console.log("blazor.bootstrap.rich-text-editor.js disposed");
}

export function execute(dotNetHelper, editorId, elementId, command, value) {
    console.log("blazor.bootstrap.rich-text-editor.js executed");
}

export function focus(dotNetHelper, editorId) {
    console.log("blazor.bootstrap.rich-text-editor.js focused");
}

export function initialize(dotNetHelper, editorId) {
    let editorEl = getOrCreate(editorId);
    if (!editorEl && !editorEl.editor) {
        dotNetHelper.invokeMethodAsync('OnEditorValueChangedAsync', "<TODO>"); // TODO: Send the editor's value to the .NET side
        return;
    }
}