window.blazorBootstrap = window.blazorBootstrap || {};
window.blazorBootstrap.richTextEditor = window.blazorBootstrap.richTextEditor || {};

// Font size label map shared across all editor instances
const _fontSizeLabels = { 1: '10 px', 2: '12 px', 3: '14 px', 4: '16 px', 5: '18 px', 6: '24 px', 7: '32 px' };

// PER-EDITOR STATE

function getEditorState(editorId) {
    return window.blazorBootstrap.richTextEditor[editorId];
}

function createEditorState(editorId, dotNetHelper, allowedLinkDomains, allowedImageDomains) {
    const editor = document.getElementById(editorId + '-editor');
    if (!editor) return null;

    // The toolbar is expected to be a sibling/ancestor element with role="toolbar"
    // inside the nearest section or data-rte-root container.
    const container = editor.closest('[data-rte-root]') || editor.closest('section') || editor.parentElement;
    const toolbar = container && container.querySelector('[role="toolbar"]');

    const state = {
        editorId,
        editor,
        toolbar,
        container,
        dotNetHelper,
        savedRange: null,
        activeTableCell: null,
        activeEditorImage: null,
        selectedTextColor: '#212529',
        selectedHighlightColor: '#ffc107',
        allowedLinkDomains: normalizeAllowedLinkDomains(allowedLinkDomains),
        allowedImageDomains: normalizeAllowedLinkDomains(allowedImageDomains),
        hasImageDomainAllowList: Array.from(allowedImageDomains || []).some((domain) => String(domain).trim()),
        linkBeingEdited: null,
        preparedImage: null,
        imageBeingEdited: null,
        editorUndoStates: [],
        editorRedoStates: [],
        // Stored handlers for cleanup
        _toolbarPointerHandler: null,
        _toolbarClickHandler: null,
        _editorBeforeInputHandler: null,
        _editorInputHandler: null,
        _editorSelectionHandler: null,
        _imageClickHandler: null,
    };

    window.blazorBootstrap.richTextEditor[editorId] = state;
    return state;
}

// DOM LOOKUP HELPERS

// Finds an element with ID = editorId + '-' + suffix.
function el(state, suffix) {
    return document.getElementById(state.editorId + '-' + suffix);
}

// Returns a Bootstrap Modal instance for a modal whose ID is editorId + '-' + suffix.
function getModal(state, suffix) {
    const modalEl = el(state, suffix);
    if (!modalEl || typeof bootstrap === 'undefined') return null;
    return { instance: bootstrap.Modal.getOrCreateInstance(modalEl), element: modalEl };
}

// SELECTION HELPERS

function getRangeElement(container) {
    return container.nodeType === Node.TEXT_NODE ? container.parentElement : container;
}

function getSelectionElement(state) {
    const selection = window.getSelection();
    if (!selection || !selection.rangeCount) return null;
    let node = selection.anchorNode;
    if (node && node.nodeType === Node.TEXT_NODE) node = node.parentElement;
    return node instanceof Element && state.editor.contains(node) ? node : null;
}

// Stores the editor selection before a toolbar interaction can remove it.
function rememberSelection(state) {
    const selection = window.getSelection();
    if (selection && selection.rangeCount && state.editor.contains(selection.anchorNode)) {
        state.savedRange = selection.getRangeAt(0).cloneRange();
    }
    updateFooterContext(state);
}

// Returns focus and the saved selection to the editable area.
function restoreSelection(state) {
    state.editor.focus();
    if (state.savedRange) {
        const selection = window.getSelection();
        selection.removeAllRanges();
        selection.addRange(state.savedRange);
    }
}

// Returns the saved range only when it still belongs to this editor.
function getSavedEditorRange(state) {
    if (!state.savedRange || !state.editor.contains(state.savedRange.commonAncestorContainer)) {
        return null;
    }
    return state.savedRange.cloneRange();
}

// Moves the browser selection to a range and keeps the editor cache in sync.
function setEditorRange(state, range) {
    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
    state.savedRange = range.cloneRange();
}

// Finds all editable blocks touched by a range, or the current block for a collapsed range.
function getRangeBlocks(state, range) {
    const blockSelector = 'p, div, h1, h2, h3, h4, h5, h6, li, td, th, blockquote, pre';
    if (range.collapsed) {
        const element = getRangeElement(range.startContainer);
        const block = element && element.closest(blockSelector);
        return block && state.editor.contains(block) ? [block] : [];
    }
    return Array.from(state.editor.querySelectorAll(blockSelector)).filter((block) => {
        try {
            return range.intersectsNode(block) && !block.querySelector(blockSelector);
        } catch (e) {
            return false;
        }
    });
}

// UNDO / REDO

// Saves the current document state for Undo before a user-visible edit.
function recordEditorState(state) {
    if (!state.editorUndoStates.length || state.editorUndoStates[state.editorUndoStates.length - 1] !== state.editor.innerHTML) {
        state.editorUndoStates.push(state.editor.innerHTML);
        if (state.editorUndoStates.length > 100) state.editorUndoStates.shift();
    }
    state.editorRedoStates = [];
}

// Restores one saved state and moves the current state to the opposite stack.
function restoreEditorHistory(state, fromStates, toStates) {
    if (!fromStates.length) return false;
    toStates.push(state.editor.innerHTML);
    state.editor.innerHTML = fromStates.pop();
    state.activeTableCell = null;
    const range = document.createRange();
    range.selectNodeContents(state.editor);
    range.collapse(true);
    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
    state.savedRange = range.cloneRange();
    notifyEditorChange(state);
    return true;
}

// CHANGE NOTIFICATION

// Notifies the .NET side whenever the editor document changes.
function notifyEditorChange(state) {
    if (state.dotNetHelper) {
        const html = state.editor.innerHTML.replace(/\u200B/g, '');
        state.dotNetHelper.invokeMethodAsync('OnEditorValueChangedAsync', html);
    }
    updateFooterCounts(state);
}

// FOOTER UPDATE

// Updates the character/word counts and the block/font/alignment context in the footer.
function updateFooter(state) {
    updateFooterCounts(state);
    updateFooterContext(state);
}

// Recalculates the character and word counts shown in the footer.
function updateFooterCounts(state) {
    const characterCountEl = document.getElementById(state.editorId + '-footer-character-count');
    const wordCountEl = document.getElementById(state.editorId + '-footer-word-count');
    if (!characterCountEl && !wordCountEl) return;
    const text = (state.editor.innerText || '').replace(/\u200B/g, '').trim();
    const characters = text.length;
    const words = text ? text.split(/\s+/).length : 0;
    if (characterCountEl) characterCountEl.textContent = characters.toLocaleString() + ' characters';
    if (wordCountEl) wordCountEl.textContent = words.toLocaleString() + ' words';
}

// Updates the block type, font/size, and alignment labels in the footer.
function updateFooterContext(state) {
    const blockEl = document.getElementById(state.editorId + '-footer-block');
    const fontEl = document.getElementById(state.editorId + '-footer-font');
    const alignmentEl = document.getElementById(state.editorId + '-footer-alignment');
    if (!blockEl && !fontEl && !alignmentEl) return;

    const element = getSelectionElement(state);

    // Block label
    if (blockEl) {
        const block = element && element.closest('h1, h2, h3, h4, h5, h6, p, div, blockquote, pre, li, td, th');
        const blockLabels = { H1: 'Heading 1', H2: 'Heading 2', H3: 'Heading 3', H4: 'Heading 4', H5: 'Heading 5', H6: 'Heading 6', BLOCKQUOTE: 'Block quote', PRE: 'Code block', LI: 'List item', TD: 'Table cell', TH: 'Table header' };
        const label = block ? (blockLabels[block.tagName] || 'Paragraph') : 'Paragraph';
        blockEl.innerHTML = '<i class="bi bi-file-earmark-text me-1" aria-hidden="true"></i>' + label;
    }

    // Font and size label
    if (fontEl) {
        const fontElement = element && element.closest('[data-rte-font], font[face]');
        const selectedFont = fontElement ? (fontElement.dataset.rteFont || fontElement.getAttribute('face') || 'Inter') : 'Inter';
        const sizeElement = element && element.closest('[data-rte-size], font[size]');
        const selectedSize = sizeElement ? (sizeElement.dataset.rteSize || _fontSizeLabels[sizeElement.getAttribute('size')] || '14 px') : '14 px';
        fontEl.innerHTML = '<i class="bi bi-type me-1" aria-hidden="true"></i>' + selectedFont + ', ' + selectedSize;
    }

    // Alignment label
    if (alignmentEl) {
        const block = element && element.closest('h1, h2, h3, h4, h5, h6, p, li, td, th, blockquote, pre');
        const alignment = block ? getComputedStyle(block).textAlign : 'left';
        const alignLabel = alignment === 'center' ? 'Centered' : alignment === 'right' ? 'Right aligned' : alignment === 'justify' ? 'Justified' : 'Left aligned';
        alignmentEl.innerHTML = '<i class="bi bi-text-left me-1" aria-hidden="true"></i>' + alignLabel;
    }
}

// INLINE FORMATTING

function unwrapElement(element) {
    const fragment = document.createDocumentFragment();
    while (element.firstChild) fragment.appendChild(element.firstChild);
    element.replaceWith(fragment);
}

// Tests whether the selection is fully inside one matching inline wrapper.
function getMatchingInlineWrapper(state, range, matcher) {
    const startElement = getRangeElement(range.startContainer);
    const endElement = getRangeElement(range.endContainer);
    const startWrapper = startElement && startElement.closest('strong, em, span, s, strike, b, i, u');
    const endWrapper = endElement && endElement.closest('strong, em, span, s, strike, b, i, u');
    return startWrapper && startWrapper === endWrapper && state.editor.contains(startWrapper) && matcher(startWrapper)
        ? startWrapper
        : null;
}

// Inserts an inline wrapper using Selection/Range APIs.
function applyInlineFormat(state, createWrapper, matcher) {
    restoreSelection(state);
    const range = getSavedEditorRange(state);
    if (!range) return;
    const matchingWrapper = getMatchingInlineWrapper(state, range, matcher);
    recordEditorState(state);
    if (matchingWrapper) {
        const afterWrapper = document.createRange();
        afterWrapper.setStartAfter(matchingWrapper);
        afterWrapper.collapse(true);
        unwrapElement(matchingWrapper);
        setEditorRange(state, afterWrapper);
    } else {
        const wrapper = createWrapper();
        if (range.collapsed) {
            const placeholder = document.createTextNode('\u200B');
            wrapper.dataset.rtePending = 'true';
            wrapper.appendChild(placeholder);
            range.insertNode(wrapper);
            const caret = document.createRange();
            caret.setStart(placeholder, 1);
            caret.collapse(true);
            setEditorRange(state, caret);
        } else {
            const contents = range.extractContents();
            wrapper.appendChild(contents);
            range.insertNode(wrapper);
            const selectedWrapper = document.createRange();
            selectedWrapper.selectNodeContents(wrapper);
            setEditorRange(state, selectedWrapper);
        }
    }
    rememberSelection(state);
    notifyEditorChange(state);
}

// Converts a hex color to its DOM-style rgb() serialization for state checks.
function rgbFromHex(value) {
    if (!/^#[0-9a-f]{6}$/i.test(value || '')) return value;
    const n = Number.parseInt(value.slice(1), 16);
    return 'rgb(' + ((n >> 16) & 255) + ', ' + ((n >> 8) & 255) + ', ' + (n & 255) + ')';
}

// Applies a semantic tag or standard inline style to the selected text.
function applyInlineCommand(state, command, value) {
    const semanticCommands = {
        bold: { tag: 'strong', match: (e) => e.tagName === 'STRONG' || e.tagName === 'B' },
        italic: { tag: 'em', match: (e) => e.tagName === 'EM' || e.tagName === 'I' }
    };
    if (semanticCommands[command]) {
        const def = semanticCommands[command];
        applyInlineFormat(state, () => document.createElement(def.tag), def.match);
        return;
    }
    const styles = {
        underline: {
            property: 'textDecoration', value: 'underline',
            match: (e) => e.style.textDecoration.includes('underline')
        },
        strikeThrough: {
            property: 'textDecoration', value: 'line-through',
            match: (e) => e.style.textDecoration.includes('line-through')
        },
        fontName: {
            property: 'fontFamily', value,
            attribute: 'data-rte-font', attributeValue: value,
            match: (e) => e.dataset.rteFont === value
        },
        fontSize: {
            property: 'fontSize', value: _fontSizeLabels[value] || '14 px',
            attribute: 'data-rte-size', attributeValue: _fontSizeLabels[value] || '14 px',
            match: (e) => e.dataset.rteSize === (_fontSizeLabels[value] || '14 px')
        },
        foreColor: {
            property: 'color', value,
            match: (e) => e.style.color === value || e.style.color === rgbFromHex(value)
        },
        hiliteColor: {
            property: 'backgroundColor', value,
            match: (e) => e.style.backgroundColor === value || e.style.backgroundColor === rgbFromHex(value)
        }
    };
    const def = styles[command];
    if (!def) return;
    applyInlineFormat(state, () => {
        const span = document.createElement('span');
        span.style[def.property] = def.value;
        if (def.attribute) span.setAttribute(def.attribute, def.attributeValue);
        return span;
    }, def.match);
}

// Removes known inline formatting within the selection.
function clearInlineFormatting(state) {
    restoreSelection(state);
    const range = getSavedEditorRange(state);
    if (!range) return;
    const formattingSelector = 'strong, b, em, i, u, s, strike, font, span, mark';
    if (range.collapsed) {
        const wrapper = getRangeElement(range.startContainer).closest(formattingSelector);
        if (!wrapper || !state.editor.contains(wrapper)) return;
        recordEditorState(state);
        unwrapElement(wrapper);
    } else {
        const boundaryBlocks = getRangeBlocks(state, range);
        const wrappers = Array.from(state.editor.querySelectorAll(formattingSelector)).filter((wrapper) => {
            try {
                return range.intersectsNode(wrapper);
            } catch (e) {
                return false;
            }
        });
        if (!wrappers.length) return;
        if (wrappers.some((wrapper) => wrapper.closest('table'))) {
            const selectedCells = Array.from(state.editor.querySelectorAll('td, th')).filter((cell) => {
                try {
                    return range.intersectsNode(cell) && rangeContainsTextContent(range, cell);
                } catch (e) {
                    return false;
                }
            });
            const tableWrappers = wrappers.filter((wrapper) =>
                rangeContainsNode(range, wrapper)
                || rangeContainsTextContent(range, wrapper)
                || selectedCells.includes(wrapper.closest('td, th')));
            if (!tableWrappers.length) return;
            recordEditorState(state);
            tableWrappers.reverse().forEach(unwrapElement);
            rememberSelection(state);
            notifyEditorChange(state);
            return;
        }
        recordEditorState(state);
        const contents = range.extractContents();
        Array.from(contents.querySelectorAll(formattingSelector)).reverse().forEach(unwrapElement);
        range.insertNode(contents);
        boundaryBlocks.filter((block, index, blocks) => blocks.indexOf(block) === index)
            .forEach((block) => removeEmptyBoundaryAncestors(state.editor, block));
    }
    rememberSelection(state);
    notifyEditorChange(state);
}

// Tests whether a range fully contains an element, including its opening and closing tags.
function rangeContainsNode(range, node) {
    const nodeRange = document.createRange();
    nodeRange.selectNode(node);
    return range.compareBoundaryPoints(Range.START_TO_START, nodeRange) <= 0
        && range.compareBoundaryPoints(Range.END_TO_END, nodeRange) >= 0;
}

// Tests whether every text node in an element is fully within a range.
function rangeContainsTextContent(range, element) {
    const walker = document.createTreeWalker(element, NodeFilter.SHOW_TEXT);
    let hasText = false;
    let textNode;
    while ((textNode = walker.nextNode())) {
        hasText = true;
        const textRange = document.createRange();
        textRange.selectNodeContents(textNode);
        if (range.compareBoundaryPoints(Range.START_TO_START, textRange) > 0
            || range.compareBoundaryPoints(Range.END_TO_END, textRange) < 0) return false;
    }
    return hasText;
}

// Removes only the empty nodes left outside an extracted selection boundary.
function removeEmptyBoundaryAncestors(editor, element) {
    let current = element;
    while (current && current !== editor && !current.innerHTML.trim()) {
        const parent = current.parentElement;
        current.remove();
        current = parent;
    }
}

// BLOCK-LEVEL COMMANDS

// Applies paragraph alignment to all blocks in the selection.
function applyAlignment(state, value) {
    restoreSelection(state);
    const range = getSavedEditorRange(state);
    const blocks = range && getRangeBlocks(state, range);
    if (!blocks || !blocks.length) return;
    recordEditorState(state);
    blocks.forEach((block) => { block.style.textAlign = value; });
    rememberSelection(state);
    notifyEditorChange(state);
}

// Adjusts block indentation through its standard inline style.
function changeIndent(state, direction) {
    restoreSelection(state);
    const range = getSavedEditorRange(state);
    const blocks = range && getRangeBlocks(state, range);
    if (!blocks || !blocks.length) return;
    recordEditorState(state);
    blocks.forEach((block) => {
        const current = Number.parseFloat(block.style.marginInlineStart || '0') || 0;
        block.style.marginInlineStart = Math.max(0, current + direction * 2) + 'rem';
        if (block.style.marginInlineStart === '0rem') block.style.removeProperty('margin-inline-start');
    });
    rememberSelection(state);
    notifyEditorChange(state);
}

// Turns the current block into a list item, or removes the existing list type.
function toggleList(state, listTag) {
    restoreSelection(state);
    const range = getSavedEditorRange(state);
    const blocks = range && getRangeBlocks(state, range).filter((b) => !b.closest('pre, code'));
    if (!blocks || !blocks.length) return;
    const block = blocks[0];
    const existingList = block.tagName === 'LI' && block.parentElement;
    recordEditorState(state);
    if (block.tagName === 'TD' || block.tagName === 'TH') {
        const list = document.createElement(listTag);
        const item = document.createElement('li');
        item.innerHTML = block.innerHTML || '<br>';
        list.appendChild(item);
        block.replaceChildren(list);
        const caret = document.createRange();
        caret.selectNodeContents(item);
        caret.collapse(true);
        setEditorRange(state, caret);
    } else if (existingList && existingList.tagName === listTag.toUpperCase()) {
        const paragraph = document.createElement('p');
        paragraph.innerHTML = block.innerHTML;
        const itemIndex = Array.from(existingList.children).indexOf(block);
        const beforeList = existingList.cloneNode(false);
        const afterList = existingList.cloneNode(false);
        Array.from(existingList.children).forEach((item, index) => {
            if (index < itemIndex) beforeList.appendChild(item);
            else if (index > itemIndex) afterList.appendChild(item);
        });
        const replacement = document.createDocumentFragment();
        if (beforeList.children.length) replacement.appendChild(beforeList);
        replacement.appendChild(paragraph);
        if (afterList.children.length) replacement.appendChild(afterList);
        existingList.replaceWith(replacement);
        const caret = document.createRange();
        caret.selectNodeContents(paragraph);
        caret.collapse(true);
        setEditorRange(state, caret);
    } else if (existingList) {
        const replacementList = document.createElement(listTag);
        while (existingList.firstChild) replacementList.appendChild(existingList.firstChild);
        existingList.replaceWith(replacementList);
        const caret = document.createRange();
        caret.selectNodeContents(block);
        caret.collapse(true);
        setEditorRange(state, caret);
    } else {
        const list = document.createElement(listTag);
        const item = document.createElement('li');
        item.innerHTML = block.innerHTML;
        list.appendChild(item);
        block.replaceWith(list);
        const caret = document.createRange();
        caret.selectNodeContents(item);
        caret.collapse(true);
        setEditorRange(state, caret);
    }
    rememberSelection(state);
    notifyEditorChange(state);
}

// Inserts a horizontal rule and a following paragraph at the selection.
function insertHorizontalRule(state) {
    restoreSelection(state);
    const range = getSavedEditorRange(state);
    if (!range) return;
    recordEditorState(state);
    range.deleteContents();
    const fragment = document.createDocumentFragment();
    const rule = document.createElement('hr');
    const paragraph = document.createElement('p');
    paragraph.appendChild(document.createElement('br'));
    fragment.append(rule, paragraph);
    range.insertNode(fragment);
    const caret = document.createRange();
    caret.selectNodeContents(paragraph);
    caret.collapse(true);
    setEditorRange(state, caret);
    rememberSelection(state);
    notifyEditorChange(state);
}

// Toggles blockquote wrapping on the current paragraph or heading.
function toggleBlockQuote(state) {
    if (!state.savedRange || !state.editor.contains(state.savedRange.commonAncestorContainer)) return;
    const startElement = getRangeElement(state.savedRange.startContainer);
    const endElement = getRangeElement(state.savedRange.endContainer);
    if (state.activeTableCell
        || startElement.closest('table, pre, code, ul, ol, li')
        || endElement.closest('table, pre, code, ul, ol, li')) return;
    const quote = startElement.closest('blockquote');
    const endQuote = endElement.closest('blockquote');
    if (quote && quote === endQuote && state.editor.contains(quote)) {
        recordEditorState(state);
        const quoteContents = document.createDocumentFragment();
        while (quote.firstChild) quoteContents.appendChild(quote.firstChild);
        quote.replaceWith(quoteContents);
        state.activeTableCell = null;
        rememberSelection(state);
        notifyEditorChange(state);
        return;
    }
    const startBlock = startElement.closest('p, h1, h2, h3, h4, h5, h6');
    const endBlock = endElement.closest('p, h1, h2, h3, h4, h5, h6');
    if (!startBlock || startBlock !== endBlock || !state.editor.contains(startBlock)) return;
    recordEditorState(state);
    const newQuote = document.createElement('blockquote');
    newQuote.className = 'blockquote border-start border-4 border-primary ps-3 my-4';
    startBlock.replaceWith(newQuote);
    newQuote.appendChild(startBlock);
    state.activeTableCell = null;
    rememberSelection(state);
    notifyEditorChange(state);
}

// Replaces the current text block with a semantic block element.
function selectBlock(state, block) {
    if (block === 'blockquote') {
        toggleBlockQuote(state);
        return;
    }
    restoreSelection(state);
    const range = getSavedEditorRange(state);
    const blocks = range && getRangeBlocks(state, range).filter((item) => !item.closest('table, ul, ol'));
    if (!blocks || blocks.length !== 1) return;
    const currentBlock = blocks[0];
    const replacement = document.createElement(block === 'small' ? 'p' : block);
    replacement.innerHTML = currentBlock.innerHTML;
    replacement.className = currentBlock.className;
    if (block === 'small') {
        replacement.classList.add('small');
    } else {
        replacement.classList.remove('small');
    }
    recordEditorState(state);
    currentBlock.replaceWith(replacement);
    const updatedRange = document.createRange();
    updatedRange.selectNodeContents(replacement);
    updatedRange.collapse(true);
    setEditorRange(state, updatedRange);
    rememberSelection(state);
    notifyEditorChange(state);
}

// COMMAND ROUTER

// Routes toolbar commands to the appropriate inline or block implementation.
function executeCommand(state, command, value = null) {
    if (command === 'undo') {
        restoreEditorHistory(state, state.editorUndoStates, state.editorRedoStates);
    } else if (command === 'redo') {
        restoreEditorHistory(state, state.editorRedoStates, state.editorUndoStates);
    } else if (command === 'print') {
        printEditorDocument(state);
    } else if (['bold', 'italic', 'underline', 'strikeThrough', 'fontName', 'fontSize', 'foreColor', 'hiliteColor'].includes(command)) {
        applyInlineCommand(state, command, value);
    } else if (command === 'paragraph') {
        selectBlock(state, 'p');
    } else if (command === 'heading1') {
        selectBlock(state, 'h1');
    } else if (command === 'heading2') {
        selectBlock(state, 'h2');
    } else if (command === 'heading3') {
        selectBlock(state, 'h3');
    } else if (command === 'heading4') {
        selectBlock(state, 'h4');
    } else if (command === 'heading5') {
        selectBlock(state, 'h5');
    } else if (command === 'heading6') {
        selectBlock(state, 'h6');    } else if (command === 'caption') {
        selectBlock(state, 'small');    } else if (command === 'justifyLeft') {
        applyAlignment(state, 'left');
    } else if (command === 'justifyCenter') {
        applyAlignment(state, 'center');
    } else if (command === 'justifyRight') {
        applyAlignment(state, 'right');
    } else if (command === 'justifyFull') {
        applyAlignment(state, 'justify');
    } else if (command === 'indent') {
        changeIndent(state, 1);
    } else if (command === 'outdent') {
        changeIndent(state, -1);
    } else if (command === 'insertOrderedList') {
        toggleList(state, 'ol');
    } else if (command === 'insertUnorderedList') {
        toggleList(state, 'ul');
    } else if (command === 'insertHorizontalRule') {
        insertHorizontalRule(state);
    } else if (command === 'link') {
        openInsertLinkModal(state);
    } else if (command === 'image') {
        openInsertImageModal(state);
    } else if (command === 'insertTable') {
        openInsertTableModal(state);
    } else if (command === 'blockquote') {
        toggleBlockQuote(state);
    } else if (command === 'codeBlock') {
        selectBlock(state, 'pre');
    } else if (command === 'fullscreen') {
        const surface = state.editor.closest('section');
        if (document.fullscreenElement) {
            document.exitFullscreen();
        } else if (surface && surface.requestFullscreen) {
            surface.requestFullscreen();
        }
    } else if (command === 'removeFormat') {
        clearInlineFormatting(state);
    }
}

// LINK HELPERS

function normalizeAllowedLinkDomains(domains) {
    return Array.from(domains || [])
        .map((domain) => String(domain).trim().replace(/^\*\./, '').replace(/\.$/, ''))
        .map((domain) => {
            try {
                return /^[a-z][a-z0-9+.-]*:\/\//i.test(domain) ? new URL(domain).hostname : domain;
            } catch (e) {
                return '';
            }
        })
        .map((domain) => domain.toLowerCase())
        .filter((domain) => /^[a-z0-9](?:[a-z0-9-]*[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)*$/i.test(domain));
}
function isAllowedLinkUrl(url, allowedLinkDomains) {
    if (!allowedLinkDomains.length || !/^https?:/i.test(url)) return true;
    const hostname = new URL(url).hostname.toLowerCase();
    return allowedLinkDomains.some((domain) => hostname === domain || hostname.endsWith('.' + domain));
}

// Normalizes allowed link formats and rejects unsafe or malformed URLs.
function normalizeLinkUrl(value) {
    const rawValue = value.trim();
    if (!rawValue) return null;
    if (rawValue.startsWith('#')) return rawValue.length > 1 && !/\s/.test(rawValue) ? rawValue : null;
    if (/^mailto:/i.test(rawValue)) return /^mailto:[^\s@]+@[^\s@]+\.[^\s@]+$/i.test(rawValue) ? rawValue : null;
    if (/^tel:/i.test(rawValue)) return /^tel:\+?[0-9(). -]+$/i.test(rawValue) ? rawValue : null;
    if (/\s/.test(rawValue)) return null;
    const candidate = /^[a-z][a-z0-9+.-]*:/i.test(rawValue) ? rawValue : 'https://' + rawValue;
    try {
        const parsedUrl = new URL(candidate);
        return parsedUrl.protocol === 'http:' || parsedUrl.protocol === 'https:' ? parsedUrl.href : null;
    } catch (e) {
        return null;
    }
}

// Confirms that a link selection stays within one editable text block.
function isLinkSelectionSafe(state, range) {
    const startElement = getRangeElement(range.startContainer);
    const endElement = getRangeElement(range.endContainer);
    const startBlock = startElement.closest('p, h1, h2, h3, h4, h5, h6, li, td, th, blockquote');
    const endBlock = endElement.closest('p, h1, h2, h3, h4, h5, h6, li, td, th, blockquote');
    return startBlock && startBlock === endBlock
        && !startElement.closest('pre, code')
        && !endElement.closest('pre, code');
}

// Finds one existing editor link when the stored selection is inside it.
function getLinkAtSelection(state) {
    if (!state.savedRange || !state.editor.contains(state.savedRange.commonAncestorContainer)) return null;
    const startLink = getRangeElement(state.savedRange.startContainer).closest('a');
    const endLink = getRangeElement(state.savedRange.endContainer).closest('a');
    return startLink && startLink === endLink && state.editor.contains(startLink) ? startLink : null;
}

// Inserts a safe link at the saved selection.
function insertLink(state, url, text, openInNewTab) {
    restoreSelection(state);
    const range = getSavedEditorRange(state);
    if (!range) return;
    recordEditorState(state);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.textContent = text;
    if (openInNewTab) {
        link.target = '_blank';
        link.rel = 'noopener noreferrer';
    }
    range.deleteContents();
    range.insertNode(link);
    const selection = window.getSelection();
    const linkRange = document.createRange();
    linkRange.setStartAfter(link);
    linkRange.collapse(true);
    selection.removeAllRanges();
    selection.addRange(linkRange);
    state.savedRange = linkRange.cloneRange();
    rememberSelection(state);
    notifyEditorChange(state);
}

// Saves edits to an existing link in the editor.
function updateLink(state, link, url, text, openInNewTab) {
    recordEditorState(state);
    link.setAttribute('href', url);
    link.textContent = text;
    if (openInNewTab) {
        link.target = '_blank';
        link.rel = 'noopener noreferrer';
    } else {
        link.removeAttribute('target');
        link.removeAttribute('rel');
    }
    const selection = window.getSelection();
    const linkRange = document.createRange();
    linkRange.setStartAfter(link);
    linkRange.collapse(true);
    selection.removeAllRanges();
    selection.addRange(linkRange);
    state.savedRange = linkRange.cloneRange();
    rememberSelection(state);
    notifyEditorChange(state);
}

// LINK MODAL

// Opens the link modal for inserting or editing a link.
// Modal ID: {editorId}-insert-link-modal
// Fields:   {editorId}-insert-link-url, -insert-link-text, -insert-link-new-tab
function openInsertLinkModal(state) {
    if (!state.savedRange || !state.editor.contains(state.savedRange.commonAncestorContainer)) return;
    if (!isLinkSelectionSafe(state, state.savedRange)) return;

    const modal = getModal(state, 'insert-link-modal');
    if (!modal) return;

    const urlInput = el(state, 'insert-link-url');
    const textInput = el(state, 'insert-link-text');
    const newTabInput = el(state, 'insert-link-new-tab');
    const title = el(state, 'insert-link-title');
    const submit = el(state, 'insert-link-submit');
    const form = modal.element.querySelector('form');
    if (!form || !urlInput || !textInput) return;

    form.reset();
    form.classList.remove('was-validated');
    urlInput.setCustomValidity('');
    textInput.setCustomValidity('');

    state.linkBeingEdited = getLinkAtSelection(state);
    if (state.linkBeingEdited) {
        textInput.value = state.linkBeingEdited.textContent;
        urlInput.value = state.linkBeingEdited.getAttribute('href') || '';
        if (newTabInput) newTabInput.checked = state.linkBeingEdited.target === '_blank';
        if (title) title.textContent = 'Edit link';
        if (submit) submit.textContent = 'Save link';
    } else {
        textInput.value = state.savedRange.toString().trim();
        if (title) title.textContent = 'Insert link';
        if (submit) submit.textContent = 'Insert link';
    }

    if (!modal.element._rteHandlersAttached) {
        modal.element._rteHandlersAttached = true;

        form.addEventListener('submit', (event) => {
            event.preventDefault();
            const text = textInput.value.trim();
            const url = normalizeLinkUrl(urlInput.value);
            textInput.setCustomValidity(text ? '' : 'Link text is required.');
            urlInput.setCustomValidity(url && isAllowedLinkUrl(url, state.allowedLinkDomains) ? '' : 'Enter a valid URL from an allowed domain.');
            form.classList.add('was-validated');
            if (!form.checkValidity()) return;
            urlInput.value = url;
            if (state.linkBeingEdited && state.editor.contains(state.linkBeingEdited)) {
                updateLink(state, state.linkBeingEdited, url, text, newTabInput ? newTabInput.checked : false);
            } else {
                insertLink(state, url, text, newTabInput ? newTabInput.checked : false);
            }
            state.linkBeingEdited = null;
            modal.instance.hide();
        });

        urlInput.addEventListener('input', () => urlInput.setCustomValidity(''));
        textInput.addEventListener('input', () => textInput.setCustomValidity(''));
        modal.element.addEventListener('hidden.bs.modal', () => { state.linkBeingEdited = null; });
    }

    modal.instance.show();
}

// TABLE HELPERS

// Resolves the active table cell from the live selection or the preserved cell.
function getTableContext(state) {
    restoreSelection(state);
    const element = getSelectionElement(state);
    const selectedCell = element && element.closest('td, th');
    const preservedCell = state.activeTableCell && state.editor.contains(state.activeTableCell) ? state.activeTableCell : null;
    const cell = selectedCell || preservedCell;
    if (!cell) return null;
    const row = cell.parentElement;
    const table = cell.closest('table');
    if (!row || !table || !state.editor.contains(table)) return null;
    state.activeTableCell = cell;
    return { table, row, cell };
}

// Moves the selection into a specific table cell.
function selectTableCell(state, cell) {
    if (!cell || !state.editor.contains(cell)) {
        state.activeTableCell = null;
        return;
    }
    const range = document.createRange();
    range.selectNodeContents(cell);
    range.collapse(true);
    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
    state.savedRange = range.cloneRange();
    state.activeTableCell = cell;
}

// Repairs empty or incomplete rows so table actions always work with a rectangular grid.
function normalizeTableRows(table) {
    const rows = Array.from(table.rows);
    const columnCount = Math.max(0, ...rows.map((r) => r.cells.length));
    if (!columnCount) return;
    rows.forEach((tableRow) => {
        while (tableRow.cells.length < columnCount) {
            const newCell = document.createElement(tableRow.parentElement.tagName === 'THEAD' ? 'th' : 'td');
            if (newCell.tagName === 'TH') newCell.scope = 'col';
            newCell.innerHTML = '&nbsp;';
            tableRow.appendChild(newCell);
        }
    });
}

function applyTableAction(state, action) {
    const context = getTableContext(state);
    if (!context) return;
    recordEditorState(state);
    const { table, row, cell } = context;
    normalizeTableRows(table);
    const column = cell.cellIndex;

    if (action === 'add-column-right' || action === 'add-column-left') {
        const insertionIndex = action === 'add-column-right' ? column + 1 : column;
        Array.from(table.rows).forEach((tableRow) => {
            const newCell = document.createElement(tableRow.parentElement.tagName === 'THEAD' ? 'th' : 'td');
            if (newCell.tagName === 'TH') newCell.scope = 'col';
            newCell.innerHTML = '&nbsp;';
            tableRow.insertBefore(newCell, tableRow.cells[Math.min(insertionIndex, tableRow.cells.length)] || null);
        });
    } else if (action === 'add-row-below' || action === 'add-row-above') {
        const rowSection = row.parentElement;
        const body = table.tBodies[0] || table.createTBody();
        const targetSection = rowSection.tagName === 'TBODY' ? rowSection : body;
        const newRow = document.createElement('tr');
        const insertionPoint = rowSection.tagName === 'TBODY'
            ? (action === 'add-row-below' ? row.nextSibling : row)
            : body.firstChild;
        const columns = Math.max(1, row.cells.length);
        for (let index = 0; index < columns; index++) {
            const newCell = document.createElement('td');
            newCell.innerHTML = '&nbsp;';
            newRow.appendChild(newCell);
        }
        targetSection.insertBefore(newRow, insertionPoint);
        selectTableCell(state, newRow.cells[Math.min(column, newRow.cells.length - 1)]);
    } else if (action === 'delete-column') {
        Array.from(table.rows).forEach((tableRow) => {
            if (tableRow.cells.length > column) tableRow.deleteCell(column);
        });
    } else if (action === 'delete-row') {
        table.deleteRow(row.rowIndex);
    } else if (action === 'delete-table') {
        table.remove();
    }

    if (action !== 'add-row-below' && action !== 'add-row-above') {
        selectTableCell(state, action === 'delete-table'
            ? null
            : (state.editor.contains(cell) ? cell : row.cells[Math.min(column, row.cells.length - 1)]));
    }

    notifyEditorChange(state);
    rememberSelection(state);
}

function applyTableStyle(state, style) {
    const context = getTableContext(state);
    if (!context) return;
    recordEditorState(state);
    const { table, cell } = context;
    if (style === 'header-row') {
        const headerSection = table.tHead;
        const firstRow = (headerSection && headerSection.rows[0]) || table.rows[0];
        const hasHeaderCells = Boolean(headerSection && firstRow && Array.from(firstRow.cells).some((c) => c.tagName === 'TH'));
        const useHeaders = firstRow && !hasHeaderCells;
        const selectionWasInFirstRow = firstRow && firstRow.contains(cell);
        const selectedColumn = cell.cellIndex;
        if (firstRow) {
            Array.from(firstRow.cells).forEach((tableCell) => {
                const replacement = document.createElement(useHeaders ? 'th' : 'td');
                replacement.innerHTML = tableCell.innerHTML;
                replacement.className = tableCell.className;
                if (useHeaders) replacement.scope = 'col';
                tableCell.replaceWith(replacement);
            });
            if (useHeaders) {
                let dest = headerSection;
                if (!dest) {
                    const newHead = document.createElement('thead');
                    table.insertBefore(newHead, table.firstChild);
                    dest = newHead;
                }
                if (!dest.contains(firstRow)) dest.appendChild(firstRow);
            }
            selectTableCell(state, selectionWasInFirstRow
                ? firstRow.cells[Math.min(selectedColumn, firstRow.cells.length - 1)]
                : cell);
        }
    } else {
        table.classList.toggle(style);
        selectTableCell(state, cell);
    }
    notifyEditorChange(state);
    rememberSelection(state);
}


// Inserts a bordered data table with a semantic header row.
function insertTable(state, rows, columns) {
    restoreSelection(state);
    const range = getSavedEditorRange(state);
    if (!range) return;
    recordEditorState(state);
    const table = document.createElement('table');
    table.className = 'table table-sm table-bordered align-middle';
    const header = table.createTHead();
    const headerRow = header.insertRow();
    for (let i = 0; i < columns; i++) {
        const th = document.createElement('th');
        th.scope = 'col';
        th.textContent = 'Header ' + (i + 1);
        headerRow.appendChild(th);
    }
    if (rows > 1) {
        const body = table.createTBody();
        for (let r = 1; r < rows; r++) {
            const bodyRow = body.insertRow();
            for (let c = 0; c < columns; c++) {
                const bodyCell = bodyRow.insertCell();
                bodyCell.textContent = 'Cell ' + (c + 1);
            }
        }
    }
    const paragraph = document.createElement('p');
    paragraph.appendChild(document.createElement('br'));
    range.deleteContents();
    const fragment = document.createDocumentFragment();
    fragment.append(table, paragraph);
    range.insertNode(fragment);
    const caret = document.createRange();
    caret.selectNodeContents(headerRow.cells[0]);
    caret.collapse(true);
    setEditorRange(state, caret);
    state.activeTableCell = headerRow.cells[0];
    rememberSelection(state);
    notifyEditorChange(state);
}

// TABLE MODAL

// Opens the table-dimensions modal.
// Modal ID: {editorId}-insert-table-modal
// Fields:   {editorId}-insert-table-rows, -insert-table-columns
function openInsertTableModal(state) {
    const modal = getModal(state, 'insert-table-modal');
    if (!modal) return;

    const rowsInput = el(state, 'insert-table-rows');
    const columnsInput = el(state, 'insert-table-columns');
    const form = modal.element.querySelector('form');
    if (!form || !rowsInput || !columnsInput) return;

    form.reset();
    rowsInput.value = '5';
    columnsInput.value = '5';
    form.classList.remove('was-validated');
    rowsInput.setCustomValidity('');
    columnsInput.setCustomValidity('');

    if (!modal.element._rteHandlersAttached) {
        modal.element._rteHandlersAttached = true;

        form.addEventListener('submit', (event) => {
            event.preventDefault();
            const rows = Number(rowsInput.value);
            const columns = Number(columnsInput.value);
            const rowsValid = Number.isInteger(rows) && rows >= 1 && rows <= 20;
            const columnsValid = Number.isInteger(columns) && columns >= 1 && columns <= 20;
            rowsInput.setCustomValidity(rowsValid ? '' : 'Enter a whole number from 1 to 20.');
            columnsInput.setCustomValidity(columnsValid ? '' : 'Enter a whole number from 1 to 20.');
            form.classList.add('was-validated');
            if (!form.checkValidity()) return;
            insertTable(state, rows, columns);
            modal.instance.hide();
        });

        rowsInput.addEventListener('input', () => rowsInput.setCustomValidity(''));
        columnsInput.addEventListener('input', () => columnsInput.setCustomValidity(''));
    }

    modal.instance.show();
}

// IMAGE HELPERS

function getImageUrlExtension(url) {
    try {
        const filename = new URL(url).pathname.split('/').pop() || '';
        const match = filename.match(/\.([a-z0-9]+)$/i);
        return match ? match[1].toLowerCase() : '';
    } catch (e) {
        return '';
    }
}

function getAllowedImageExtensions(state) {
    const input = el(state, 'image-extension-whitelist');
    return (input ? input.value : '').toLowerCase().split(',').map((ext) => ext.trim().replace(/^\./, '')).filter(Boolean);
}

function isAllowedImageUrl(url, state) {
    if (!state.hasImageDomainAllowList) return true;
    const hostname = new URL(url).hostname.toLowerCase();
    return state.allowedImageDomains.some((domain) => hostname === domain || hostname.endsWith('.' + domain));
}

function normalizeImageUrl(state, value) {
    const rawValue = value.trim();
    if (!rawValue || /\s/.test(rawValue)) return null;
    try {
        const parsedUrl = new URL(rawValue);
        if (parsedUrl.protocol !== 'https:' || parsedUrl.username || parsedUrl.password || !isAllowedImageUrl(parsedUrl.href, state)) return null;
        const extension = getImageUrlExtension(parsedUrl.href);
        const allowedExtensions = getAllowedImageExtensions(state);
        return (!extension || allowedExtensions.includes(extension)) ? parsedUrl.href : null;
    } catch (e) {
        return null;
    }
}
function loadImageDetails(url) {
    return new Promise((resolve, reject) => {
        const probe = new Image();
        probe.onload = () => resolve({ url, width: probe.naturalWidth, height: probe.naturalHeight });
        probe.onerror = () => reject(new Error('The image could not be loaded from this URL.'));
        probe.src = url;
    });
}

function showImageFeedback(state, message) {
    const feedback = el(state, 'image-feedback');
    if (!feedback) return;
    feedback.textContent = message;
    feedback.classList.remove('d-none');
}

function clearImageFeedback(state) {
    const feedback = el(state, 'image-feedback');
    if (!feedback) return;
    feedback.textContent = '';
    feedback.classList.add('d-none');
}

async function prepareImagePreview(state, value) {
    const url = normalizeImageUrl(state, value);
    if (!url) throw new Error('Enter a valid permitted HTTPS image URL with an allowed extension.');
    const details = await loadImageDetails(url);
    state.preparedImage = details;
    const preview = el(state, 'image-preview');
    const widthInput = el(state, 'image-width');
    const heightInput = el(state, 'image-height');
    const imageOptions = el(state, 'image-options');
    const submitBtn = el(state, 'insert-image-submit');
    if (preview) preview.src = details.url;
    if (widthInput) widthInput.value = details.width;
    if (heightInput) heightInput.value = details.height;
    if (imageOptions) imageOptions.classList.remove('d-none');
    if (submitBtn) submitBtn.disabled = false;
    clearImageFeedback(state);
}

function updateImageAspectRatio(state, changedDimension) {
    if (!state.preparedImage) return;
    const aspectLock = el(state, 'image-aspect-lock');
    if (!aspectLock || !aspectLock.checked) return;
    const widthInput = el(state, 'image-width');
    const heightInput = el(state, 'image-height');
    const width = Number(widthInput && widthInput.value);
    const height = Number(heightInput && heightInput.value);
    if (changedDimension === 'width' && Number.isFinite(width) && width > 0) {
        if (heightInput) heightInput.value = Math.max(1, Math.round(width * state.preparedImage.height / state.preparedImage.width));
    } else if (changedDimension === 'height' && Number.isFinite(height) && height > 0) {
        if (widthInput) widthInput.value = Math.max(1, Math.round(height * state.preparedImage.width / state.preparedImage.height));
    }
}

function resetImageModal(state) {
    const form = el(state, 'insert-image-form');
    if (form) form.reset();
    state.preparedImage = null;
    state.imageBeingEdited = null;
    const imageOptions = el(state, 'image-options');
    const preview = el(state, 'image-preview');
    const submitBtn = el(state, 'insert-image-submit');
    const progressEl = el(state, 'image-upload-progress');
    const barEl = el(state, 'image-upload-progress-bar');
    const renderType = el(state, 'image-render-type');
    const alignment = el(state, 'image-alignment');
    const aspectLock = el(state, 'image-aspect-lock');
    const responsive = el(state, 'image-responsive');
    const captionGroup = el(state, 'image-caption-group');
    const altText = el(state, 'image-alt-text');
    const titleEl = el(state, 'insert-image-title');
    if (imageOptions) imageOptions.classList.add('d-none');
    if (preview) preview.removeAttribute('src');
    if (submitBtn) { submitBtn.disabled = true; submitBtn.textContent = 'Insert image'; }
    if (progressEl) progressEl.classList.add('d-none');
    if (barEl) { barEl.style.width = '0%'; barEl.textContent = '0%'; }
    if (renderType) renderType.value = 'figure';
    if (alignment) alignment.value = 'center';
    if (aspectLock) aspectLock.checked = true;
    if (responsive) responsive.checked = true;
    if (captionGroup) captionGroup.classList.remove('d-none');
    if (altText) altText.disabled = false;
    if (titleEl) titleEl.textContent = 'Insert image';
    clearImageFeedback(state);
}

// Finds an editor image from a click or a saved selection, when available.
function getImageForEditing(state) {
    if (state.activeEditorImage && state.editor.contains(state.activeEditorImage)) return state.activeEditorImage;
    if (!state.savedRange || !state.editor.contains(state.savedRange.commonAncestorContainer)) return null;
    const startElement = getRangeElement(state.savedRange.startContainer);
    if (startElement && startElement.closest('img')) return startElement.closest('img');
    if (state.savedRange.startContainer.nodeType === Node.ELEMENT_NODE) {
        const adjacentNode = state.savedRange.startContainer.childNodes[state.savedRange.startOffset]
            || state.savedRange.startContainer.childNodes[state.savedRange.startOffset - 1];
        if (adjacentNode instanceof Element) {
            return adjacentNode.matches('img') ? adjacentNode : adjacentNode.querySelector('img');
        }
    }
    return null;
}

// Pre-fills the image dialog from an existing editor image without changing its source.
function loadImageForEditing(state, image) {
    const url = normalizeImageUrl(state, image.currentSrc || image.src);
    if (!url) {
        showImageFeedback(state, 'This image URL is not permitted by the current security policy.');
        return false;
    }
    const figure = image.closest('figure');
    const imageWidthValue = Number(image.getAttribute('width')) || image.naturalWidth || image.width || 1;
    const imageHeightValue = Number(image.getAttribute('height')) || image.naturalHeight || image.height || 1;
    state.preparedImage = {
        url,
        width: image.naturalWidth || imageWidthValue,
        height: image.naturalHeight || imageHeightValue
    };
    const directUrl = el(state, 'image-direct-url');
    const preview = el(state, 'image-preview');
    const imageOptions = el(state, 'image-options');
    const renderType = el(state, 'image-render-type');
    const captionGroup = el(state, 'image-caption-group');
    const alignmentEl = el(state, 'image-alignment');
    const altText = el(state, 'image-alt-text');
    const decorativeEl = el(state, 'image-decorative');
    const captionEl = el(state, 'image-caption');
    const widthInput = el(state, 'image-width');
    const heightInput = el(state, 'image-height');
    const aspectLock = el(state, 'image-aspect-lock');
    const titleText = el(state, 'image-title-text');
    const responsive = el(state, 'image-responsive');
    const submitBtn = el(state, 'insert-image-submit');
    if (directUrl) directUrl.value = state.preparedImage.url;
    if (preview) preview.src = state.preparedImage.url;
    if (imageOptions) imageOptions.classList.remove('d-none');
    if (renderType) renderType.value = figure ? 'figure' : 'img';
    if (captionGroup) captionGroup.classList.toggle('d-none', !figure);
    if (alignmentEl) {
        alignmentEl.value = figure
            ? (figure.classList.contains('text-end') ? 'end' : figure.classList.contains('text-start') ? 'start' : figure.classList.contains('d-inline-block') ? 'inline' : 'center')
            : (image.classList.contains('ms-auto') ? 'end' : image.classList.contains('mx-auto') ? 'center' : image.classList.contains('d-inline-block') ? 'inline' : 'start');
    }
    if (altText) altText.value = image.alt;
    if (decorativeEl) decorativeEl.checked = !image.alt;
    if (altText) altText.disabled = !image.alt;
    if (captionEl) captionEl.value = figure && figure.querySelector('figcaption') ? figure.querySelector('figcaption').textContent : '';
    if (widthInput) widthInput.value = imageWidthValue;
    if (heightInput) heightInput.value = imageHeightValue;
    if (aspectLock) aspectLock.checked = true;
    if (titleText) titleText.value = image.title || '';
    if (responsive) responsive.checked = image.classList.contains('img-fluid');
    if (submitBtn) submitBtn.disabled = false;
    return true;
}

// Creates the requested image or figure element and inserts it at the saved editor range.
function insertPreparedImage(state) {
    const widthInput = el(state, 'image-width');
    const heightInput = el(state, 'image-height');
    const width = Number(widthInput && widthInput.value);
    const height = Number(heightInput && heightInput.value);
    const decorativeEl = el(state, 'image-decorative');
    const decorative = decorativeEl ? decorativeEl.checked : false;
    const altText = el(state, 'image-alt-text');
    const renderType = el(state, 'image-render-type');
    const alignmentEl = el(state, 'image-alignment');
    const captionEl = el(state, 'image-caption');
    const titleText = el(state, 'image-title-text');
    const responsive = el(state, 'image-responsive');

    if (!state.preparedImage) {
        showImageFeedback(state, 'Preview an image before inserting it.');
        return;
    }
    if ((!state.imageBeingEdited || !state.editor.contains(state.imageBeingEdited))
        && (!state.savedRange || !state.editor.contains(state.savedRange.commonAncestorContainer))) {
        showImageFeedback(state, 'Close this dialog, place the cursor where the image belongs, then open Image again.');
        return;
    }
    if (!decorative && altText && !altText.value.trim()) {
        altText.classList.add('is-invalid');
        return;
    }
    if (!Number.isInteger(width) || width < 1 || !Number.isInteger(height) || height < 1) {
        showImageFeedback(state, 'Enter whole-number width and height values greater than zero.');
        return;
    }

    const image = document.createElement('img');
    image.src = state.preparedImage.url;
    image.alt = decorative ? '' : (altText ? altText.value.trim() : '');
    image.width = width;
    image.height = height;
    if (titleText && titleText.value.trim()) image.title = titleText.value.trim();
    if (responsive && responsive.checked) image.classList.add('img-fluid');

    let content = image;
    const renderTypeValue = renderType ? renderType.value : 'figure';
    const alignmentValue = alignmentEl ? alignmentEl.value : 'center';

    if (renderTypeValue === 'figure') {
        const figure = document.createElement('figure');
        figure.className = 'figure ' + (alignmentValue === 'center' ? 'd-block text-center'
            : alignmentValue === 'start' ? 'd-block text-start'
            : alignmentValue === 'end' ? 'd-block text-end'
            : 'd-inline-block');
        image.classList.add('figure-img', 'mb-2');
        figure.appendChild(image);
        if (captionEl && captionEl.value.trim()) {
            const caption = document.createElement('figcaption');
            caption.className = 'figure-caption';
            caption.textContent = captionEl.value.trim();
            figure.appendChild(caption);
        }
        content = figure;
    } else if (alignmentValue === 'center') {
        image.classList.add('d-block', 'mx-auto');
    } else if (alignmentValue === 'start') {
        image.classList.add('d-block');
    } else if (alignmentValue === 'end') {
        image.classList.add('d-block', 'ms-auto');
    } else {
        image.classList.add('d-inline-block');
    }

    recordEditorState(state);
    const selection = window.getSelection();
    const afterRange = document.createRange();

    if (state.imageBeingEdited && state.editor.contains(state.imageBeingEdited)) {
        const replacedContent = state.imageBeingEdited.closest('figure') || state.imageBeingEdited;
        replacedContent.replaceWith(content);
    } else {
        restoreSelection(state);
        const range = state.savedRange.cloneRange();
        range.deleteContents();
        range.insertNode(content);
    }

    afterRange.setStartAfter(content);
    afterRange.collapse(true);
    selection.removeAllRanges();
    selection.addRange(afterRange);
    state.savedRange = afterRange.cloneRange();
    rememberSelection(state);
    notifyEditorChange(state);

    const modal = getModal(state, 'insert-image-modal');
    if (modal) modal.instance.hide();
}

// IMAGE MODAL

// Opens the image modal in insert or edit mode.
// Modal ID: {editorId}-insert-image-modal
// The host application owns file uploads through ImageUploadHandler; this modal does not expose browser upload endpoints or headers.
function openInsertImageModal(state) {
    const modal = getModal(state, 'insert-image-modal');
    if (!modal) return;

    const imageToEdit = getImageForEditing(state);
    resetImageModal(state);

    if (imageToEdit) {
        state.imageBeingEdited = imageToEdit;
        const titleEl = el(state, 'insert-image-title');
        const submitBtn = el(state, 'insert-image-submit');
        if (titleEl) titleEl.textContent = 'Edit image';
        if (submitBtn) submitBtn.textContent = 'Save image';
        loadImageForEditing(state, imageToEdit);
        const imageUrlTab = el(state, 'image-url-tab');
        if (imageUrlTab && typeof bootstrap !== 'undefined') {
            bootstrap.Tab.getOrCreateInstance(imageUrlTab).show();
        }
    }

    if (!modal.element._rteHandlersAttached) {
        modal.element._rteHandlersAttached = true;

        const validateUrlBtn = el(state, 'validate-image-url');
        const decorativeEl = el(state, 'image-decorative');
        const altTextEl = el(state, 'image-alt-text');
        const renderTypeEl = el(state, 'image-render-type');
        const captionGroupEl = el(state, 'image-caption-group');
        const widthEl = el(state, 'image-width');
        const heightEl = el(state, 'image-height');
        const form = modal.element.querySelector('form');

        if (validateUrlBtn) {
            validateUrlBtn.addEventListener('click', async () => {
                try {
                    const directUrl = el(state, 'image-direct-url');
                    await prepareImagePreview(state, directUrl ? directUrl.value : '');
                } catch (err) {
                    showImageFeedback(state, err.message);
                }
            });
        }
        if (decorativeEl && altTextEl) {
            decorativeEl.addEventListener('change', () => {
                altTextEl.disabled = decorativeEl.checked;
                altTextEl.classList.remove('is-invalid');
            });
        }
        if (altTextEl) {
            altTextEl.addEventListener('input', () => altTextEl.classList.remove('is-invalid'));
        }
        if (renderTypeEl && captionGroupEl) {
            renderTypeEl.addEventListener('change', () => {
                captionGroupEl.classList.toggle('d-none', renderTypeEl.value !== 'figure');
            });
        }
        if (widthEl) widthEl.addEventListener('input', () => updateImageAspectRatio(state, 'width'));
        if (heightEl) heightEl.addEventListener('input', () => updateImageAspectRatio(state, 'height'));

        if (form) {
            form.addEventListener('submit', (event) => {
                event.preventDefault();
                insertPreparedImage(state);
            });
        }

        modal.element.addEventListener('hidden.bs.modal', () => {
            state.imageBeingEdited = null;
            state.activeEditorImage = null;
        });
    }

    modal.instance.show();
}

// PRINT

// Uses the native print dialog while temporarily showing only the editor content.
function printEditorDocument(state) {
    const editor = state.editor;
    const printScopeClass = 'bb-rte-printing';
    const printDocumentClass = 'bb-rte-print-document';
    let printDocument;
    const printStyle = document.createElement('style');
    printStyle.textContent = `@media print {
        body.${printScopeClass} * { visibility: hidden !important; }
        body.${printScopeClass} .${printDocumentClass}, body.${printScopeClass} .${printDocumentClass} * { visibility: visible !important; }
        body.${printScopeClass} .${printDocumentClass} { position: absolute !important; inset: 0 !important; width: auto !important; }
        body.${printScopeClass} .${printDocumentClass} table { width: 100% !important; }
    }`;
    const editorSurface = editor.closest('section');

    const toolbar = editorSurface && editorSurface.querySelector('[role="toolbar"]');
    const footer = editorSurface && editorSurface.querySelector('.card-footer');
    const hiddenElements = [toolbar, footer].filter(Boolean).map((element) => ({ element, wasHidden: element.hidden }));
    const removedClasses = editorSurface ? ['card', 'shadow-sm'].filter((cls) => editorSurface.classList.contains(cls)) : [];
    const classChanges = [];
    const colorAdjustmentChanges = [];
    const editorAttributes = {
        contenteditable: editor.getAttribute('contenteditable'),
        role: editor.getAttribute('role'),
        ariaMultiline: editor.getAttribute('aria-multiline')
    };
    const adjustPrintClasses = (element, addClasses, removeClasses = []) => {
        if (!element) return;
        const managedClasses = [...new Set([...addClasses, ...removeClasses])];
        classChanges.push({ element, managedClasses, originalClasses: managedClasses.filter((cls) => element.classList.contains(cls)) });
        element.classList.remove(...removeClasses);
        element.classList.add(...addClasses);
    };

    adjustPrintClasses(editor, ['p-2'], ['p-4']);
    editor.querySelectorAll('.table-responsive').forEach((e) => adjustPrintClasses(e, [], ['table-responsive']));
    editor.querySelectorAll('table').forEach((e) => adjustPrintClasses(e, ['table-sm', 'small', 'text-break']));
    editor.querySelectorAll('pre').forEach((e) => adjustPrintClasses(e, ['text-wrap', 'text-break']));
    editor.querySelectorAll('img').forEach((e) => adjustPrintClasses(e, ['img-fluid']));
    editor.removeAttribute('contenteditable');
    editor.removeAttribute('role');
    editor.removeAttribute('aria-multiline');
    [editor, ...editor.querySelectorAll('*')].forEach((element) => {
        colorAdjustmentChanges.push({
            element,
            printColorAdjust: element.style.getPropertyValue('print-color-adjust'),
            printColorAdjustPriority: element.style.getPropertyPriority('print-color-adjust'),
            webkitPrintColorAdjust: element.style.getPropertyValue('-webkit-print-color-adjust'),
            webkitPrintColorAdjustPriority: element.style.getPropertyPriority('-webkit-print-color-adjust')
        });
        element.style.setProperty('print-color-adjust', 'exact', 'important');
        element.style.setProperty('-webkit-print-color-adjust', 'exact', 'important');
    });
    let restored = false;
    const restoreEditorView = () => {
        if (restored) return;
        restored = true;
        hiddenElements.forEach(({ element, wasHidden }) => { element.hidden = wasHidden; });
        if (editorSurface) editorSurface.classList.add(...removedClasses);
        classChanges.forEach(({ element, managedClasses, originalClasses }) => {
            element.classList.remove(...managedClasses);
            element.classList.add(...originalClasses);
        });
        colorAdjustmentChanges.forEach(({ element, printColorAdjust, printColorAdjustPriority, webkitPrintColorAdjust, webkitPrintColorAdjustPriority }) => {
            element.style.setProperty('print-color-adjust', printColorAdjust, printColorAdjustPriority);
            element.style.setProperty('-webkit-print-color-adjust', webkitPrintColorAdjust, webkitPrintColorAdjustPriority);
        });
        if (editorAttributes.contenteditable !== null) editor.setAttribute('contenteditable', editorAttributes.contenteditable);
        if (editorAttributes.role !== null) editor.setAttribute('role', editorAttributes.role);
        if (editorAttributes.ariaMultiline !== null) editor.setAttribute('aria-multiline', editorAttributes.ariaMultiline);
        document.body.classList.remove(printScopeClass);
        printDocument?.remove();
        printStyle.remove();
    };
    hiddenElements.forEach(({ element }) => { element.hidden = true; });
    if (editorSurface) editorSurface.classList.remove(...removedClasses);
    printDocument = editor.cloneNode(true);
    printDocument.removeAttribute('id');
    printDocument.classList.add(printDocumentClass);
    document.body.appendChild(printDocument);
    document.head.appendChild(printStyle);
    document.body.classList.add(printScopeClass);
    window.addEventListener('afterprint', restoreEditorView, { once: true });
    try {
        window.focus();
        window.print();
        window.setTimeout(restoreEditorView, 1000);
    } catch (err) {
        restoreEditorView();
    }
}

// TOOLBAR CLICK HANDLER

function handleToolbarClick(state, event) {
    const button = event.target.closest('button');
    if (!button) return;

    if (button.dataset.editorCommand) {
        executeCommand(state, button.dataset.editorCommand);
    } else if (button.dataset.editorBlock) {
        selectBlock(state, button.dataset.editorBlock);
    } else if (button.dataset.editorFont) {
        executeCommand(state, 'fontName', button.dataset.editorFont);
    } else if (button.dataset.editorSize) {
        executeCommand(state, 'fontSize', button.dataset.editorSize);
    } else if (button.dataset.editorColor) {
        state.selectedTextColor = button.dataset.editorColor;
        const indicator = state.container && state.container.querySelector('[data-rte-text-color-indicator]');
        if (indicator && button.dataset.editorIndicator) {
            indicator.className = 'position-absolute bottom-0 start-0 end-0 border-bottom border-3 ' + button.dataset.editorIndicator;
        }
        executeCommand(state, 'foreColor', button.dataset.editorColor);
    } else if (button.dataset.editorHighlight) {
        state.selectedHighlightColor = button.dataset.editorHighlight;
        const indicator = state.container && state.container.querySelector('[data-rte-highlight-color-indicator]');
        if (indicator && button.dataset.editorIndicator) {
            indicator.className = 'position-absolute bottom-0 start-0 end-0 border-bottom border-3 ' + button.dataset.editorIndicator;
        }
        executeCommand(state, 'hiliteColor', button.dataset.editorHighlight);
    } else if (button.dataset.editorTable) {
        const [rows, columns] = button.dataset.editorTable.split('x').map(Number);
        insertTable(state, rows, columns);
    } else if (button.dataset.editorTableAction) {
        applyTableAction(state, button.dataset.editorTableAction);
    } else if (button.dataset.editorTableStyle) {
        applyTableStyle(state, button.dataset.editorTableStyle);
    } else if (button.dataset.editorAction === 'apply-text-color') {
        executeCommand(state, 'foreColor', state.selectedTextColor);
    } else if (button.dataset.editorAction === 'apply-highlight-color') {
        executeCommand(state, 'hiliteColor', state.selectedHighlightColor);
    } else if (button.dataset.editorAction === 'print') {
        printEditorDocument(state);
    } else if (button.dataset.editorAction === 'link') {
        openInsertLinkModal(state);
    } else if (button.dataset.editorAction === 'image') {
        openInsertImageModal(state);
    } else if (button.dataset.editorAction === 'insert-table') {
        openInsertTableModal(state);
    } else if (button.dataset.editorAction === 'fullscreen') {
        const surface = state.editor.closest('section');
        if (document.fullscreenElement) {
            document.exitFullscreen();
        } else if (surface && surface.requestFullscreen) {
            surface.requestFullscreen();
        }
    }
}

// EXPORTED MODULE API

export function dispose(dotNetHelper, editorId) {
    const state = getEditorState(editorId);
    if (!state) return;

    if (state.toolbar) {
        if (state._toolbarPointerHandler) state.toolbar.removeEventListener('pointerdown', state._toolbarPointerHandler);
        if (state._toolbarClickHandler) state.toolbar.removeEventListener('click', state._toolbarClickHandler);
    }
    if (state.editor) {
        if (state._editorBeforeInputHandler) state.editor.removeEventListener('beforeinput', state._editorBeforeInputHandler);
        if (state._editorInputHandler) state.editor.removeEventListener('input', state._editorInputHandler);
        if (state._editorSelectionHandler) {
            state.editor.removeEventListener('mouseup', state._editorSelectionHandler);
            state.editor.removeEventListener('keyup', state._editorSelectionHandler);
        }
        if (state._imageClickHandler) state.editor.removeEventListener('click', state._imageClickHandler);
    }

    delete window.blazorBootstrap.richTextEditor[editorId];
}

export function execute(dotNetHelper, editorId, elementId, command, value) {
    const state = getEditorState(editorId);
    if (!state) return;
    executeCommand(state, command, value || null);
}

export function focus(dotNetHelper, editorId) {
    const state = getEditorState(editorId);
    if (!state) return;
    state.editor.focus();
}

export async function prepareUploadedImage(editorId, imageUrl) {
    const state = getEditorState(editorId);
    if (!state) return;
    try {
        await prepareImagePreview(state, imageUrl);
    } catch (err) {
        showImageFeedback(state, 'The uploaded image could not be loaded.');
    }
}

export function showImageUploadError(editorId, message) {
    const state = getEditorState(editorId);
    if (state) showImageFeedback(state, message);
}
export function initialize(dotNetHelper, editorId, allowedLinkDomains, allowedImageDomains) {
    const state = createEditorState(editorId, dotNetHelper, allowedLinkDomains, allowedImageDomains);
    if (!state || !state.editor) {
        dotNetHelper.invokeMethodAsync('OnEditorValueChangedAsync', '');
        return;
    }

    // Toolbar listeners
    if (state.toolbar) {
        state._toolbarPointerHandler = () => rememberSelection(state);
        state._toolbarClickHandler = (event) => handleToolbarClick(state, event);
        state.toolbar.addEventListener('pointerdown', state._toolbarPointerHandler);
        state.toolbar.addEventListener('click', state._toolbarClickHandler);
    }

    // Editor content listeners
    state._editorBeforeInputHandler = () => recordEditorState(state);
    state._editorInputHandler = () => {
        rememberSelection(state);
        notifyEditorChange(state);
    };
    state._editorSelectionHandler = () => rememberSelection(state);
    state.editor.addEventListener('beforeinput', state._editorBeforeInputHandler);
    state.editor.addEventListener('input', state._editorInputHandler);
    state.editor.addEventListener('mouseup', state._editorSelectionHandler);
    state.editor.addEventListener('keyup', state._editorSelectionHandler);

    // Track the active image for the image modal (edit mode)
    state._imageClickHandler = (event) => {
        const img = event.target.closest('img');
        state.activeEditorImage = img && state.editor.contains(img) ? img : null;
    };
    state.editor.addEventListener('click', state._imageClickHandler);

    // Send the initial editor value to .NET
    dotNetHelper.invokeMethodAsync('OnEditorValueChangedAsync', state.editor.innerHTML.replace(/\u200B/g, ''));

    // Populate the footer with the initial document state
    updateFooter(state);
}