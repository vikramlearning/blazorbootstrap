window.blazorBootstrap = window.blazorBootstrap || {};
window.blazorBootstrap.richTextEditor = window.blazorBootstrap.richTextEditor || {};

// Font size label map shared across all editor instances
const _fontSizeLabels = { 1: '10 px', 2: '12 px', 3: '14 px', 4: '16 px', 5: '18 px', 6: '24 px', 7: '32 px' };
const _fontFamilies = new Set(['Inter', 'Arial', 'Georgia', 'Courier New']);

// PER-EDITOR STATE

/**
 * Gets the state object that belongs to one rich-text editor instance.
 *
 * @param {string} editorId Component-generated unique editor identifier.
 * @returns {any} The result of the operation.
 */
function getEditorState(editorId) {
    return window.blazorBootstrap.richTextEditor[editorId];
}

/**
 * Creates and registers isolated state for one rich-text editor instance.
 *
 * @param {string} editorId Component-generated unique editor identifier.
 * @param {any} dotNetHelper .NET object reference used for Blazor interop.
 * @param {string[]} allowedLinkDomains Configured link-domain allow-list.
 * @param {string[]} allowedImageDomains Configured image-domain allow-list.
 * @returns {any} The result of the operation.
 */
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

    // Register by ID so concurrent components never share selections, history, or policies.
    window.blazorBootstrap.richTextEditor[editorId] = state;
    return state;
}

// DOM LOOKUP HELPERS

/**
 * Finds an editor-owned element by its ID suffix.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string} suffix The suffix argument for this operation.
 * @returns {any} The result of the operation.
 */
function el(state, suffix) {
    return document.getElementById(state.editorId + '-' + suffix);
}

/**
 * Returns the Bootstrap modal instance and element for an editor-owned dialog.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string} suffix The suffix argument for this operation.
 * @returns {any} The result of the operation.
 */
function getModal(state, suffix) {
    const modalEl = el(state, suffix);
    if (!modalEl || typeof bootstrap === 'undefined') return null;
    return { instance: bootstrap.Modal.getOrCreateInstance(modalEl), element: modalEl };
}

/**
 * Disposes a closed Bootstrap modal and asks .NET to unmount its markup.
 *
 * @param {object} state Current rich-text editor state.
 * @param {{ instance: bootstrap.Modal, element: HTMLElement }} modal The closed modal.
 * @param {string} modalName The logical modal name sent to .NET.
 * @returns {void} No return value.
 */
function notifyModalClosed(state, modal, modalName) {
    modal.instance.dispose();
    state.dotNetHelper?.invokeMethodAsync('OnEditorModalClosedAsync', modalName);
}

// SELECTION HELPERS

/**
 * Converts a Range boundary node into an element for ancestor lookups.
 *
 * @param {Node} container The container argument for this operation.
 * @returns {any} The result of the operation.
 */
function getRangeElement(container) {
    return container.nodeType === Node.TEXT_NODE ? container.parentElement : container;
}

/**
 * Gets the current selection element only when it belongs to this editor.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {any} The result of the operation.
 */
function getSelectionElement(state) {
    const selection = window.getSelection();
    if (!selection || !selection.rangeCount) return null;
    let node = selection.anchorNode;
    if (node && node.nodeType === Node.TEXT_NODE) node = node.parentElement;
    return node instanceof Element && state.editor.contains(node) ? node : null;
}

/**
 * Stores the editor selection before a toolbar interaction can remove it.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 *
 * @example
 * // Preserve selected text before opening a toolbar modal.
 * rememberSelection(state);
 */
function rememberSelection(state) {
    const selection = window.getSelection();
    if (selection && selection.rangeCount && state.editor.contains(selection.anchorNode)) {
        state.savedRange = selection.getRangeAt(0).cloneRange();
    }
    updateFooterContext(state);
}

/**
 * Returns focus and the saved selection to the editable area.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 *
 * @example
 * // Restore the selection before inserting a link or image.
 * restoreSelection(state);
 */
function restoreSelection(state) {
    state.editor.focus();
    if (state.savedRange) {
        const selection = window.getSelection();
        selection.removeAllRanges();
        selection.addRange(state.savedRange);
    }
}

/**
 * Returns the saved range only when it still belongs to this editor.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {any} The result of the operation.
 */
function getSavedEditorRange(state) {
    if (!state.savedRange || !state.editor.contains(state.savedRange.commonAncestorContainer)) {
        return null;
    }
    return state.savedRange.cloneRange();
}

/**
 * Moves the browser selection to a range and keeps the editor cache in sync.
 *
 * @param {object} state Current rich-text editor state.
 * @param {Range} range Browser selection range to inspect or update.
 * @returns {void} No return value.
 */
function setEditorRange(state, range) {
    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
    state.savedRange = range.cloneRange();
}

/**
 * Finds editable blocks touched by a range, or the current block for a collapsed range.
 *
 * @param {object} state Current rich-text editor state.
 * @param {Range} range Browser selection range to inspect or update.
 * @returns {any} The result of the operation.
 */
function getRangeBlocks(state, range) {
    const blockSelector = 'p, div, h1, h2, h3, h4, h5, h6, li, td, th, blockquote, pre';
    if (range.collapsed) {
        const element = getRangeElement(range.startContainer);
        const block = element && element.closest(blockSelector);
        return block && state.editor.contains(block) ? [block] : [];
    }
    return Array.from(state.editor.querySelectorAll(blockSelector)).filter((block) => {
        try {
            // Return leaf blocks only; otherwise a parent div and its nested paragraph would be formatted twice.
            return range.intersectsNode(block) && !block.querySelector(blockSelector);
        } catch (e) {
            return false;
        }
    });
}

// UNDO / REDO

/**
 * Saves the current document state for Undo before a user-visible edit.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 *
 * @example
 * // Save a snapshot immediately before mutating editor.innerHTML.
 * recordEditorState(state);
 */
function recordEditorState(state) {
    if (!state.editorUndoStates.length || state.editorUndoStates[state.editorUndoStates.length - 1] !== state.editor.innerHTML) {
        state.editorUndoStates.push(state.editor.innerHTML);
        // Cap snapshots so prolonged editing does not retain unbounded HTML.
        if (state.editorUndoStates.length > 100) state.editorUndoStates.shift();
    }
    state.editorRedoStates = [];
}

/**
 * Restores an Undo or Redo snapshot and transfers the current document to the opposite stack.
 *
 * @param {object} state Current rich-text editor state.
 * @param {any} fromStates The fromStates argument for this operation.
 * @param {any} toStates The toStates argument for this operation.
 * @returns {any} The result of the operation.
 *
 * @example
 * // Undo the most recent custom editor mutation.
 * restoreEditorHistory(state, state.editorUndoStates, state.editorRedoStates);
 */
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

/**
 * Notifies the .NET side whenever the editor document changes.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 */
function notifyEditorChange(state) {
    const metrics = getEditorTextMetrics(state);
    if (state.dotNetHelper) {
        const html = state.editor.innerHTML.replace(/\u200B/g, '');
        state.dotNetHelper.invokeMethodAsync('OnEditorValueChangedAsync', html, metrics.text, metrics.characterCount, metrics.wordCount);
    }
    updateFooterCounts(state, metrics);
}

// FOOTER UPDATE

/**
 * Updates the character and word counts and selection context in the footer.
 *
 * @param {object} state Current rich-text editor state.
 * @param {{ text: string, characterCount: number, wordCount: number }} [metrics] Previously calculated editor metrics.
 * @returns {void} No return value.
 */
function updateFooter(state, metrics = getEditorTextMetrics(state)) {
    updateFooterCounts(state, metrics);
    updateFooterContext(state);
}

/**
 * Updates the character and word counts shown in the footer from shared editor metrics.
 *
 * @param {object} state Current rich-text editor state.
 * @param {{ text: string, characterCount: number, wordCount: number }} metrics Normalized editor metrics.
 * @returns {void} No return value.
 */
function updateFooterCounts(state, metrics) {
    const characterCountEl = document.getElementById(state.editorId + '-footer-character-count');
    const wordCountEl = document.getElementById(state.editorId + '-footer-word-count');
    if (!characterCountEl && !wordCountEl) return;
    if (characterCountEl) characterCountEl.textContent = metrics.characterCount.toLocaleString() + ' characters';
    if (wordCountEl) wordCountEl.textContent = metrics.wordCount.toLocaleString() + ' words';
}

/**
 * Produces the text and counts shared by the footer and the .NET change callback.
 *
 * Empty structure and zero-width caret markers are ignored. Non-empty blocks and table
 * rows are separated by newlines; table cells are separated by tabs.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {{ text: string, characterCount: number, wordCount: number }} Normalized editor metrics.
 *
 * @example
 * // A row containing "Draft" and "Editorial" produces "Draft\tEditorial".
 * const metrics = getEditorTextMetrics(state);
 */
function getEditorTextMetrics(state) {
    const getText = (node) => {
        if (node.nodeType === Node.TEXT_NODE) return node.nodeValue.replace(/\u200B/g, '');
        if (node.nodeType !== Node.ELEMENT_NODE) return '';
        if (node.tagName === 'BR') return '\n';
        if (node.tagName === 'TABLE') {
            return Array.from(node.querySelectorAll('tr'))
                .filter((row) => row.closest('table') === node && hasText(row))
                .map((row) => Array.from(row.children)
                    .filter((cell) => cell.tagName === 'TD' || cell.tagName === 'TH')
                    .map((cell) => hasText(cell) ? getText(cell) : '')
                    .join('\t'))
                .join('\n');
        }

        const parts = [];
        let inline = '';
        for (const child of node.childNodes) {
            const isBlock = child.nodeType === Node.ELEMENT_NODE && /^(ADDRESS|ARTICLE|ASIDE|BLOCKQUOTE|DIV|FIGCAPTION|FIGURE|FOOTER|H[1-6]|HEADER|LI|MAIN|NAV|OL|P|PRE|SECTION|TABLE|UL)$/.test(child.tagName);
            if (isBlock) {
                if (inline) parts.push(inline);
                inline = '';
                if (hasText(child)) parts.push(getText(child));
            } else {
                inline += getText(child);
            }
        }
        if (inline) parts.push(inline);
        return parts.join('\n');
    };
    const hasText = (node) => Array.from(node.childNodes).some((child) =>
        child.nodeType === Node.TEXT_NODE
            ? child.nodeValue.replace(/\u200B/g, '').length > 0
            : child.nodeType === Node.ELEMENT_NODE && hasText(child));
    const text = getText(state.editor);
    const graphemeSegmenter = typeof Intl !== 'undefined' && typeof Intl.Segmenter === 'function'
        ? new Intl.Segmenter(undefined, { granularity: 'grapheme' })
        : null;
    const wordSegmenter = typeof Intl !== 'undefined' && typeof Intl.Segmenter === 'function'
        ? new Intl.Segmenter(undefined, { granularity: 'word' })
        : null;

    return {
        text,
        characterCount: graphemeSegmenter ? Array.from(graphemeSegmenter.segment(text)).length : Array.from(text).length,
        wordCount: !text ? 0 : wordSegmenter
            ? Array.from(wordSegmenter.segment(text)).filter((segment) => segment.isWordLike).length
            : text.trim().split(/\s+/).filter(Boolean).length
    };
}
/**
 * Updates the block type, font, size, and alignment labels in the footer.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 */
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
        const sizeElement = element && element.closest('span[style*="font-size"], font[size]');
        const selectedSize = sizeElement ? (
            _fontSizeLabels[sizeElement.getAttribute('size')]
            || Object.values(_fontSizeLabels).find((label) => label.replace(' ', '') === sizeElement.style.fontSize)
            || '14 px') : '14 px';
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

/**
 * Removes an element while retaining all of its child nodes.
 *
 * @param {Element} element The element argument for this operation.
 * @returns {void} No return value.
 */
function unwrapElement(element) {
    const fragment = document.createDocumentFragment();
    while (element.firstChild) fragment.appendChild(element.firstChild);
    element.replaceWith(fragment);
}

/**
 * Tests whether the selection is fully inside one matching inline wrapper.
 *
 * @param {object} state Current rich-text editor state.
 * @param {Range} range Browser selection range to inspect or update.
 * @param {Function} matcher The matcher argument for this operation.
 * @param {string} matchingSelector Selector used to find the active inline wrapper.
 * @returns {any} The result of the operation.
 */
function getMatchingInlineWrapper(state, range, matcher, matchingSelector) {
    const startElement = getRangeElement(range.startContainer);
    const endElement = getRangeElement(range.endContainer);
    const startWrapper = startElement && startElement.closest(matchingSelector);
    const endWrapper = endElement && endElement.closest(matchingSelector);
    return startWrapper && startWrapper === endWrapper && state.editor.contains(startWrapper) && matcher(startWrapper)
        ? startWrapper
        : null;
}

/**
 * Toggles an inline wrapper around the saved selection, including pending caret formatting and whole table-cell selections.
 *
 * @param {object} state Current rich-text editor state.
 * @param {Function} createWrapper The createWrapper argument for this operation.
 * @param {Function} matcher The matcher argument for this operation.
 * @param {string|null} conflictingSelector Selector for formatting that must be removed before applying this wrapper.
 * @param {string} matchingSelector Selector used to find the active wrapper for toggling.
 * @returns {void} No return value.
 */
function applyInlineFormat(state, createWrapper, matcher, conflictingSelector = null, matchingSelector = 'strong, em, span, s, strike, b, i, u') {
    restoreSelection(state);
    const range = getSavedEditorRange(state);
    if (!range) return;
    const tableCells = range.collapsed ? [] : getFullySelectedTableCells(state, range);
    if (tableCells.length) {
        recordEditorState(state);
        tableCells.forEach((cell) => {
            const directWrapper = cell.childElementCount === 1 ? cell.firstElementChild : null;
            if (directWrapper && matcher(directWrapper)) {
                unwrapElement(directWrapper);
                return;
            }

            if (conflictingSelector) Array.from(cell.querySelectorAll(conflictingSelector)).reverse().forEach(unwrapElement);

            // Wrap cell contents, never the td/th itself, to preserve valid table structure.
            const cellRange = document.createRange();
            cellRange.selectNodeContents(cell);
            const wrapper = createWrapper();
            wrapper.appendChild(cellRange.extractContents());
            cellRange.insertNode(wrapper);
        });
        const updatedRange = document.createRange();
        updatedRange.selectNodeContents(tableCells[tableCells.length - 1]);
        setEditorRange(state, updatedRange);
        rememberSelection(state);
        notifyEditorChange(state);
        return;
    }

    const matchingWrapper = getMatchingInlineWrapper(state, range, matcher, matchingSelector);
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
            const rangeElement = getRangeElement(range.startContainer);
            const conflictingWrapper = conflictingSelector && rangeElement && rangeElement.closest(conflictingSelector);
            if (conflictingWrapper && state.editor.contains(conflictingWrapper)) unwrapElement(conflictingWrapper);

            // A zero-width space keeps the wrapper and caret alive until the user types content.
            const placeholder = document.createTextNode('\u200B');
            wrapper.dataset.rtePending = 'true';
            wrapper.appendChild(placeholder);
            range.insertNode(wrapper);
            const caret = document.createRange();
            caret.setStart(placeholder, 1);
            caret.collapse(true);
            setEditorRange(state, caret);
        } else {
            const startElement = getRangeElement(range.startContainer);
            const endElement = getRangeElement(range.endContainer);
            const startConflictingWrapper = conflictingSelector && startElement && startElement.closest(conflictingSelector);
            const endConflictingWrapper = conflictingSelector && endElement && endElement.closest(conflictingSelector);
            if (startConflictingWrapper && startConflictingWrapper === endConflictingWrapper
                && state.editor.contains(startConflictingWrapper)
                && rangeContainsTextContent(range, startConflictingWrapper)) {
                while (startConflictingWrapper.firstChild) wrapper.appendChild(startConflictingWrapper.firstChild);
                startConflictingWrapper.replaceWith(wrapper);
            } else {
                const contents = range.extractContents();
                if (conflictingSelector) Array.from(contents.querySelectorAll(conflictingSelector)).reverse().forEach(unwrapElement);
                wrapper.appendChild(contents);
                range.insertNode(wrapper);
            }
            const selectedWrapper = document.createRange();
            selectedWrapper.selectNodeContents(wrapper);
            setEditorRange(state, selectedWrapper);
        }
    }
    rememberSelection(state);
    notifyEditorChange(state);
}

/**
 * Converts a hex color to its DOM-style rgb() serialization for state checks.
 *
 * @param {string|number} value Optional formatting value.
 * @returns {any} The result of the operation.
 */
function rgbFromHex(value) {
    if (!/^#[0-9a-f]{6}$/i.test(value || '')) return value;
    const n = Number.parseInt(value.slice(1), 16);
    return 'rgb(' + ((n >> 16) & 255) + ', ' + ((n >> 8) & 255) + ', ' + (n & 255) + ')';
}

/**
 * Applies a semantic tag or standard inline style to selected text.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string} command Toolbar command to execute.
 * @param {string|number} value Optional formatting value.
 * @returns {void} No return value.
 *
 * @example
 * // Apply the selected text color.
 * applyInlineCommand(state, 'foreColor', '#dc3545');
 *
 * // Apply the supported 16 px size level.
 * applyInlineCommand(state, 'fontSize', '4');
 */
function applyInlineCommand(state, command, value) {
    // Reject arbitrary font-family values supplied through stale UI or direct JS interop.
    if (command === 'fontName' && !_fontFamilies.has(value)) return;
    // Reject unsupported size levels instead of silently falling back to the default size.
    if (command === 'fontSize' && !Object.hasOwn(_fontSizeLabels, value)) return;
    const semanticCommands = {
        bold: { tag: 'strong', match: (e) => e.tagName === 'STRONG' || e.tagName === 'B' },
        italic: { tag: 'em', match: (e) => e.tagName === 'EM' || e.tagName === 'I' },
        subscript: { tag: 'sub', match: (e) => e.tagName === 'SUB', conflictingSelector: 'sup', matchingSelector: 'sub' },
        superscript: { tag: 'sup', match: (e) => e.tagName === 'SUP', conflictingSelector: 'sub', matchingSelector: 'sup' }
    };
    if (semanticCommands[command]) {
        const def = semanticCommands[command];
        applyInlineFormat(state, () => document.createElement(def.tag), def.match, def.conflictingSelector, def.matchingSelector);
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
            // Labels include a space for readability; CSS length values must not include that space.
            property: 'fontSize', value: (_fontSizeLabels[value] || '14 px').replace(' ', ''),
            // Match the serialized CSS style so no editor-only data attribute is emitted in saved HTML.
            match: (e) => e.style.fontSize === (_fontSizeLabels[value] || '14 px').replace(' ', '')
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

/**
 * Removes browser-inherited inline formatting from a newly created empty paragraph.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 */
function normalizeEmptyParagraphAfterEnter(state) {
    const selection = window.getSelection();
    if (!selection || !selection.rangeCount) return;
    const element = getRangeElement(selection.anchorNode);
    const paragraph = element && element.closest('p');
    if (!paragraph
        || !state.editor.contains(paragraph)
        || paragraph.textContent.replace(/\u200B/g, '').trim()
        || paragraph.querySelector('img, table, hr')) return;
    paragraph.replaceChildren(document.createElement('br'));
    const range = document.createRange();
    range.selectNodeContents(paragraph);
    range.collapse(true);
    setEditorRange(state, range);
}
/**
 * Removes inline formatting while retaining the selected document structure.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 *
 * @example
 * // Remove bold, color, and other inline styling while preserving paragraphs and lists.
 * clearInlineFormatting(state);
 */
function clearInlineFormatting(state) {
    restoreSelection(state);
    const range = getSavedEditorRange(state);
    if (!range) return;
    const formattingSelector = 'strong, b, em, i, u, s, strike, sub, sup, font, span, mark';
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
        // Extracting a partial table range clones table structure; unwrap selected formatting in place instead.
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

/**
 * Tests whether a range fully contains an element, including its opening and closing tags.
 *
 * @param {Range} range Browser selection range to inspect or update.
 * @param {Node} node The node argument for this operation.
 * @returns {any} The result of the operation.
 */
function rangeContainsNode(range, node) {
    const nodeRange = document.createRange();
    nodeRange.selectNode(node);
    return range.compareBoundaryPoints(Range.START_TO_START, nodeRange) <= 0
        && range.compareBoundaryPoints(Range.END_TO_END, nodeRange) >= 0;
}

/**
 * Tests whether every text node in an element is fully within a range.
 *
 * @param {Range} range Browser selection range to inspect or update.
 * @param {Element} element The element argument for this operation.
 * @returns {any} The result of the operation.
 */
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

/**
 * Gets table cells whose complete content is selected by a range.
 *
 * @param {object} state Current rich-text editor state.
 * @param {Range} range Selection to inspect.
 * @returns {HTMLTableCellElement[]} Fully selected table cells.
 *
 * @example
 * // A range that selects one cell returns that cell, not its containing row.
 * const cells = getFullySelectedTableCells(state, range);
 */
function getFullySelectedTableCells(state, range) {
    return Array.from(state.editor.querySelectorAll('td, th')).filter((cell) => {
        try {
            return range.intersectsNode(cell)
                && (rangeContainsNode(range, cell) || rangeContainsTextContent(range, cell));
        } catch (e) {
            return false;
        }
    });
}
/**
 * Removes only the empty nodes left outside an extracted selection boundary.
 *
 * @param {HTMLElement} editor The editor argument for this operation.
 * @param {Element} element The element argument for this operation.
 * @returns {void} No return value.
 */
function removeEmptyBoundaryAncestors(editor, element) {
    let current = element;
    while (current && current !== editor && !current.innerHTML.trim()) {
        const parent = current.parentElement;
        current.remove();
        current = parent;
    }
}

// BLOCK-LEVEL COMMANDS

/**
 * Applies paragraph alignment to all blocks in the selection.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string|number} value Optional formatting value.
 * @returns {void} No return value.
 *
 * @example
 * // Center every block touched by the saved selection.
 * applyAlignment(state, 'center');
 */
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

/**
 * Adjusts block indentation through its standard inline style.
 *
 * @param {object} state Current rich-text editor state.
 * @param {number} direction The direction argument for this operation.
 * @returns {void} No return value.
 *
 * @example
 * // Increase indentation by one editor step.
 * changeIndent(state, 1);
 */
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

/**
 * Creates, changes, or removes an ordered or unordered list at the saved selection.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string} listTag The listTag argument for this operation.
 * @returns {void} No return value.
 *
 * @example
 * // Convert the current paragraph into an ordered list item.
 * toggleList(state, 'ol');
 */
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

/**
 * Inserts a horizontal rule and a following paragraph at the selection.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 *
 * @example
 * // Insert a rule and leave a writable paragraph after it.
 * insertHorizontalRule(state);
 */
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

/**
 * Toggles blockquote wrapping on the current paragraph or heading.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 *
 * @example
 * // Wrap the current eligible paragraph in a Bootstrap-styled quote.
 * toggleBlockQuote(state);
 */
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

/**
 * Replaces the current text block with a semantic block element.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string} block The block argument for this operation.
 * @returns {void} No return value.
 *
 * @example
 * // Convert the current paragraph into a level-two heading.
 * selectBlock(state, 'h2');
 */
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

/**
 * Routes toolbar commands to the appropriate inline or block implementation.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string} command Toolbar command to execute.
 * @param {string|number} value Optional formatting value.
 * @returns {void} No return value.
 */
function executeCommand(state, command, value = null) {
    if (command === 'undo') {
        restoreEditorHistory(state, state.editorUndoStates, state.editorRedoStates);
    } else if (command === 'redo') {
        restoreEditorHistory(state, state.editorRedoStates, state.editorUndoStates);
    } else if (command === 'print') {
        printEditorDocument(state);
    } else if (['bold', 'italic', 'underline', 'strikeThrough', 'subscript', 'superscript', 'fontName', 'fontSize', 'foreColor', 'hiliteColor'].includes(command)) {
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

/**
 * Normalizes configured link domains for hostname comparisons.
 *
 * @param {string[]} domains The domains argument for this operation.
 * @returns {any} The result of the operation.
 */
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
/**
 * Checks whether an HTTP(S) link URL matches the configured domain allow-list.
 *
 * @param {string} url The url argument for this operation.
 * @param {string[]} allowedLinkDomains Configured link-domain allow-list.
 * @returns {any} The result of the operation.
 *
 * @example
 * // Allow an exact configured host or one of its subdomains.
 * isAllowedLinkUrl('https://docs.example.com/guide', ['example.com']);
 */
function isAllowedLinkUrl(url, allowedLinkDomains) {
    if (!allowedLinkDomains.length || !/^https?:/i.test(url)) return true;
    const hostname = new URL(url).hostname.toLowerCase();
    return allowedLinkDomains.some((domain) => hostname === domain || hostname.endsWith('.' + domain));
}

/**
 * Validates and normalizes a link URL without permitting unsafe schemes.
 *
 * @param {string|number} value Optional formatting value.
 * @returns {any} The result of the operation.
 *
 * @example
 * // Accept an address without a scheme and normalize it for href.
 * normalizeLinkUrl('example.com/docs'); // https://example.com/docs
 */
function normalizeLinkUrl(value) {
    const rawValue = value.trim();
    if (!rawValue) return null;
    // Permit only explicit fragment, mail, tel, and HTTP(S) formats; all other schemes fail closed.
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

/**
 * Confirms that a link selection stays within one editable text block.
 *
 * @param {object} state Current rich-text editor state.
 * @param {Range} range Browser selection range to inspect or update.
 * @returns {any} The result of the operation.
 */
function isLinkSelectionSafe(state, range) {
    const startElement = getRangeElement(range.startContainer);
    const endElement = getRangeElement(range.endContainer);
    const startBlock = startElement.closest('p, h1, h2, h3, h4, h5, h6, li, td, th, blockquote');
    const endBlock = endElement.closest('p, h1, h2, h3, h4, h5, h6, li, td, th, blockquote');
    return startBlock && startBlock === endBlock
        && !startElement.closest('pre, code')
        && !endElement.closest('pre, code');
}

/**
 * Finds an existing editor link when the saved selection is inside it.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {any} The result of the operation.
 */
function getLinkAtSelection(state) {
    if (!state.savedRange || !state.editor.contains(state.savedRange.commonAncestorContainer)) return null;
    const startLink = getRangeElement(state.savedRange.startContainer).closest('a');
    const endLink = getRangeElement(state.savedRange.endContainer).closest('a');
    return startLink && startLink === endLink && state.editor.contains(startLink) ? startLink : null;
}

/**
 * Inserts a validated link at the saved selection.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string} url The url argument for this operation.
 * @param {string} text The text argument for this operation.
 * @param {boolean} openInNewTab The openInNewTab argument for this operation.
 * @returns {void} No return value.
 *
 * @example
 * // Replace the saved selection with a secure new-tab link.
 * insertLink(state, 'https://example.com', 'Read more', true);
 */
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

/**
 * Saves edits to an existing link in the editor.
 *
 * @param {object} state Current rich-text editor state.
 * @param {HTMLAnchorElement} link The link argument for this operation.
 * @param {string} url The url argument for this operation.
 * @param {string} text The text argument for this operation.
 * @param {boolean} openInNewTab The openInNewTab argument for this operation.
 * @returns {void} No return value.
 *
 * @example
 * // Update an existing anchor without rebuilding the surrounding paragraph.
 * updateLink(state, link, 'https://example.com/new', 'New destination', false);
 */
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
/**
 * Opens the link modal in insert or edit mode and attaches one-time validation handlers.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 *
 * @example
 * // Open the modal after the selection has been saved.
 * openInsertLinkModal(state);
 */
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
        modal.element.addEventListener('hidden.bs.modal', () => {
            state.linkBeingEdited = null;
            notifyModalClosed(state, modal, 'link');
        });
    }

    modal.instance.show();
}

// TABLE HELPERS

/**
 * Resolves the active table cell from the live selection or preserved cell.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {any} The result of the operation.
 */
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

/**
 * Moves the selection into a specific table cell.
 *
 * @param {object} state Current rich-text editor state.
 * @param {HTMLTableCellElement|null} cell The cell argument for this operation.
 * @returns {void} No return value.
 */
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

/**
 * Repairs empty or incomplete rows so table actions work with a rectangular grid.
 *
 * @param {HTMLTableElement} table The table argument for this operation.
 * @returns {void} No return value.
 */
function normalizeTableRows(table) {
    const rows = Array.from(table.rows);
    // Use the widest row as the schema so later column operations preserve table geometry.
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

/**
 * Performs a structural add or delete action relative to the active table cell.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string} action The action argument for this operation.
 * @returns {void} No return value.
 *
 * @example
 * // Insert a column to the right of the active table cell.
 * applyTableAction(state, 'add-column-right');
 */
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

/**
 * Applies a style to the active table or toggles its header row.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string} style The style argument for this operation.
 * @returns {void} No return value.
 *
 * @example
 * // Convert the first table row into semantic header cells.
 * applyTableStyle(state, 'header-row');
 */
function applyTableStyle(state, style) {
    const context = getTableContext(state);
    if (!context) return;
    recordEditorState(state);
    const { table, cell } = context;
    // Header-row conversion changes element semantics and section placement, not only CSS classes.
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


/**
 * Inserts a table with a semantic header row and a following writable paragraph.
 *
 * @param {object} state Current rich-text editor state.
 * @param {number} rows The rows argument for this operation.
 * @param {number} columns The columns argument for this operation.
 * @returns {void} No return value.
 *
 * @example
 * // Insert a five-by-five table at the saved selection.
 * insertTable(state, 5, 5);
 */
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
    const fragment = document.createDocumentFragment();
    fragment.append(table, paragraph);
    const insertionParagraph = getRangeElement(range.startContainer).closest('p');
    if (range.collapsed
        && insertionParagraph
        && state.editor.contains(insertionParagraph)
        && !insertionParagraph.textContent.trim()
        && !insertionParagraph.querySelector('img, table')) {
        insertionParagraph.replaceWith(fragment);
    } else {
        range.deleteContents();
        range.insertNode(fragment);
    }
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
/**
 * Opens the table-dimensions modal and validates row and column input.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 *
 * @example
 * // Open the validated Rows and Columns dialog.
 * openInsertTableModal(state);
 */
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
        modal.element.addEventListener('hidden.bs.modal', () => notifyModalClosed(state, modal, 'table'));
    }

    modal.instance.show();
}

// IMAGE HELPERS

/**
 * Gets the filename extension from an image URL.
 *
 * @param {string} url The url argument for this operation.
 * @returns {any} The result of the operation.
 */
function getImageUrlExtension(url) {
    try {
        const filename = new URL(url).pathname.split('/').pop() || '';
        const match = filename.match(/\.([a-z0-9]+)$/i);
        return match ? match[1].toLowerCase() : '';
    } catch (e) {
        return '';
    }
}

/**
 * Reads allowed image extensions from the image modal allow-list.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {any} The result of the operation.
 */
function getAllowedImageExtensions(state) {
    const input = el(state, 'image-extension-whitelist');
    return (input ? input.value : '').toLowerCase().split(',').map((ext) => ext.trim().replace(/^\./, '')).filter(Boolean);
}

/**
 * Checks whether an image URL matches the image-domain allow-list.
 *
 * @param {string} url The url argument for this operation.
 * @param {object} state Current rich-text editor state.
 * @returns {any} The result of the operation.
 */
function isAllowedImageUrl(url, state) {
    if (!state.hasImageDomainAllowList) return true;
    const hostname = new URL(url).hostname.toLowerCase();
    return state.allowedImageDomains.some((domain) => hostname === domain || hostname.endsWith('.' + domain));
}

/**
 * Validates a direct image URL against HTTPS, credentials, domain, and extension policies.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string|number} value Optional formatting value.
 * @returns {any} The result of the operation.
 *
 * @example
 * // Accept only policy-compliant HTTPS image URLs.
 * normalizeImageUrl(state, 'https://cdn.example.com/photo.png');
 */
function normalizeImageUrl(state, value) {
    const rawValue = value.trim();
    if (!rawValue || /\s/.test(rawValue)) return null;
    try {
        const parsedUrl = new URL(rawValue);
        // Reject non-HTTPS, credential-bearing, and policy-disallowed sources before any network request.
        if (parsedUrl.protocol !== 'https:' || parsedUrl.username || parsedUrl.password || !isAllowedImageUrl(parsedUrl.href, state)) return null;
        const extension = getImageUrlExtension(parsedUrl.href);
        const allowedExtensions = getAllowedImageExtensions(state);
        return (!extension || allowedExtensions.includes(extension)) ? parsedUrl.href : null;
    } catch (e) {
        return null;
    }
}
/**
 * Loads a validated image and returns its intrinsic dimensions.
 *
 * @param {string} url The url argument for this operation.
 * @returns {any} The result of the operation.
 */
function loadImageDetails(url) {
    return new Promise((resolve, reject) => {
        const probe = new Image();
        probe.onload = () => resolve({ url, width: probe.naturalWidth, height: probe.naturalHeight });
        probe.onerror = () => reject(new Error('The image could not be loaded from this URL.'));
        probe.src = url;
    });
}

/**
 * Displays a validation or loading message in the image modal.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string} message The message argument for this operation.
 * @returns {void} No return value.
 */
function showImageFeedback(state, message) {
    const feedback = el(state, 'image-feedback');
    if (!feedback) return;
    feedback.textContent = message;
    feedback.classList.remove('d-none');
}

/**
 * Clears the validation or loading message in the image modal.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 */
function clearImageFeedback(state) {
    const feedback = el(state, 'image-feedback');
    if (!feedback) return;
    feedback.textContent = '';
    feedback.classList.add('d-none');
}

/**
 * Validates and previews an image before enabling insertion.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string|number} value Optional formatting value.
 * @returns {Promise<any>} The result of the operation.
 *
 * @example
 * // Validate and load an image before enabling insertion.
 * await prepareImagePreview(state, 'https://cdn.example.com/photo.png');
 */
async function prepareImagePreview(state, value) {
    const url = normalizeImageUrl(state, value);
    if (!url) throw new Error('Enter a valid permitted HTTPS image URL with an allowed extension.');
    // Load first so invalid or unreachable images cannot be inserted into the document.
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

/**
 * Updates the other image dimension when aspect-ratio locking is enabled.
 *
 * @param {object} state Current rich-text editor state.
 * @param {string} changedDimension The changedDimension argument for this operation.
 * @returns {void} No return value.
 *
 * @example
 * // Update height after the user changes width with aspect lock enabled.
 * updateImageAspectRatio(state, 'width');
 */
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

/**
 * Resets image modal fields and transient state to insertion defaults.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 */
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

/**
 * Finds an editor image from a click or saved selection, when available.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {any} The result of the operation.
 */
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

/**
 * Pre-fills the image dialog from an existing editor image without changing its source.
 *
 * @param {object} state Current rich-text editor state.
 * @param {HTMLImageElement} image The image argument for this operation.
 * @returns {any} The result of the operation.
 */
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

/**
 * Inserts or replaces an image or figure after validating all modal options.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 *
 * @example
 * // Insert the image prepared by the modal preview action.
 * insertPreparedImage(state);
 */
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
    // Non-decorative images require alternative text for accessibility.
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
/**
 * Opens the image modal in insert or edit mode.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 *
 * @example
 * // Open the dialog in insert mode, or edit mode when an image is selected.
 * openInsertImageModal(state);
 */
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
            notifyModalClosed(state, modal, 'image');
        });
    }

    modal.instance.show();
}

// PRINT

/**
 * Opens the native print dialog with a temporary print-only copy of editor content.
 *
 * @param {object} state Current rich-text editor state.
 * @returns {void} No return value.
 *
 * @example
 * // Print only this editor's document, excluding its toolbar and footer.
 * printEditorDocument(state);
 */
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
    /**
     * Records and applies temporary print-only class changes for one element.
     *
     * @param {Element} element Element to adjust.
     * @param {string[]} addClasses Classes needed for printed output.
     * @param {string[]} removeClasses Classes that interfere with printing.
     * @returns {void} No return value.
     */
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
    /**
     * Restores every attribute, class, inline print style, and temporary node changed for printing.
     * This is idempotent because browsers may raise afterprint after the fallback timeout fires.
     *
     * @returns {void} No return value.
     */
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
    // Print a clone so the live editable DOM can be restored exactly after the native dialog closes.
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

/**
 * Routes legacy toolbar data attributes to the corresponding editor operation.
 *
 * @param {object} state Current rich-text editor state.
 * @param {MouseEvent} event Browser event raised by the toolbar.
 * @returns {void} No return value.
 */
function handleToolbarClick(state, event) {
    const button = event.target.closest('button');
    if (!button) return;

    if (state.editor.getAttribute('aria-disabled') === 'true') {
        event.preventDefault();
        event.stopImmediatePropagation();
        return;
    }

    if (button.dataset.editorCommand) {
        executeCommand(state, button.dataset.editorCommand);
    } else if (button.dataset.editorBlock) {
        selectBlock(state, button.dataset.editorBlock);
    } else if (button.dataset.editorFont) {
        executeCommand(state, 'fontName', button.dataset.editorFont);
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

/**
 * Releases DOM listeners and registered state for a disposed Blazor editor.
 *
 * @param {any} dotNetHelper .NET object reference used for Blazor interop.
 * @param {string} editorId Component-generated unique editor identifier.
 * @returns {void} No return value.
 *
 * @example
 * // Remove event handlers when the component is disposed.
 * dispose(dotNetHelper, 'editor-1');
 */
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

/**
 * Closes an editor modal or disposes an unshown modal that must be unmounted.
 *
 * @param {string} editorId Component-generated unique editor identifier.
 * @param {string} modalId The editor-scoped modal identifier.
 * @returns {void} No return value.
 */
export function hideModal(editorId, modalId) {
    const state = getEditorState(editorId);
    if (!state) return;

    const modal = getModal(state, modalId);
    if (!modal) return;

    if (modal.element.classList.contains('show'))
    {
        modal.instance.hide();
        return;
    }

    notifyModalClosed(state, modal, modalId.replace('insert-', '').replace('-modal', ''));
}

/**
 * Executes a toolbar command requested by the Blazor onclick handler.
 *
 * @param {any} dotNetHelper .NET object reference used for Blazor interop.
 * @param {string} editorId Component-generated unique editor identifier.
 * @param {string} elementId The elementId argument for this operation.
 * @param {string} command Toolbar command to execute.
 * @param {string|number} value Optional formatting value.
 * @returns {void} No return value.
 *
 * @example
 * // Apply bold using the selection saved before the toolbar click.
 * execute(dotNetHelper, 'editor-1', 'editor-1-bold', 'bold');
 */
export function execute(dotNetHelper, editorId, elementId, command, value) {
    const state = getEditorState(editorId);
    if (!state) return;
    executeCommand(state, command, value || null);
}

/**
 * Moves focus to the editor surface.
 *
 * @param {any} dotNetHelper .NET object reference used for Blazor interop.
 * @param {string} editorId Component-generated unique editor identifier.
 * @returns {void} No return value.
 *
 * @example
 * // Return keyboard focus to an initialized editor.
 * focus(dotNetHelper, 'editor-1');
 */
export function focus(dotNetHelper, editorId) {
    const state = getEditorState(editorId);
    if (!state) return;
    state.editor.focus();
}

/**
 * Prepares a host-uploaded image URL for preview in the image modal.
 *
 * @param {string} editorId Component-generated unique editor identifier.
 * @param {string} imageUrl The imageUrl argument for this operation.
 * @returns {Promise<any>} The result of the operation.
 *
 * @example
 * // Preview a host-uploaded image in the open image modal.
 * await prepareUploadedImage('editor-1', 'https://cdn.example.com/image.png');
 */
export async function prepareUploadedImage(editorId, imageUrl) {
    const state = getEditorState(editorId);
    if (!state) return false;
    try {
        await prepareImagePreview(state, imageUrl);
        return true;
    } catch (err) {
        showImageFeedback(state, 'The uploaded image could not be loaded.');
        return false;
    }
}

/**
 * Displays an uploaded-image error message in the image modal.
 *
 * @param {string} editorId Component-generated unique editor identifier.
 * @param {string} message The message argument for this operation.
 * @returns {void} No return value.
 *
 * @example
 * // Show a host-upload failure to the modal user.
 * showImageUploadError('editor-1', 'The upload could not be completed.');
 */
export function showImageUploadError(editorId, message) {
    const state = getEditorState(editorId);
    if (state) showImageFeedback(state, message);
}
/**
 * Initializes an editor instance, registers event listeners, and sends initial HTML to Blazor.
 *
 * @param {any} dotNetHelper .NET object reference used for Blazor interop.
 * @param {string} editorId Component-generated unique editor identifier.
 * @param {string[]} allowedLinkDomains Configured link-domain allow-list.
 * @param {string[]} allowedImageDomains Configured image-domain allow-list.
 * @returns {void} No return value.
 *
 * @example
 * // Initialize one Blazor editor with link and image host policies.
 * initialize(dotNetHelper, 'editor-1', ['example.com'], ['cdn.example.com']);
 */
export function initialize(dotNetHelper, editorId, allowedLinkDomains, allowedImageDomains) {
    const state = createEditorState(editorId, dotNetHelper, allowedLinkDomains, allowedImageDomains);
    if (!state || !state.editor) {
        dotNetHelper.invokeMethodAsync('OnEditorValueChangedAsync', '', '', 0, 0);
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
    // Capture the pre-edit DOM so native typing and deletion participate in Undo and Redo.
    state._editorBeforeInputHandler = () => recordEditorState(state);
    state._editorInputHandler = (event) => {
        if (event.inputType === 'insertParagraph') normalizeEmptyParagraphAfterEnter(state);
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

    // Reuse initial metrics for both .NET and footer output.
    const metrics = getEditorTextMetrics(state);
    dotNetHelper.invokeMethodAsync('OnEditorValueChangedAsync', state.editor.innerHTML.replace(/\u200B/g, ''), metrics.text, metrics.characterCount, metrics.wordCount);
    updateFooter(state, metrics);
}