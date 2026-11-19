/**
 * Copies the provided text to the clipboard and invokes .NET methods based on the success or failure of the operation.
 * @param {string} text - The text to be copied to the clipboard.
 * @param {object} dotNetHelper - The .NET helper object to invoke methods on.
 */
async function copyToClipboard(text, dotNetHelper) {
    let isCopied = true;

    // The component may be disposed before a callback runs (for example, the user leaves the page
    // before the reset timer fires), so a rejected .NET call is ignored instead of surfacing an error.
    const invokeDotNet = (methodName, ...args) =>
        dotNetHelper.invokeMethodAsync(methodName, ...args).catch(() => { });

    try {
        await navigator.clipboard.writeText(text);
        invokeDotNet('OnCopySuccessJS');
    } catch (err) {
        isCopied = false;
        invokeDotNet('OnCopyFailJS', err?.message ?? String(err));
    }

    setTimeout(
        () => { invokeDotNet('ResetCopyStatusJS'); },
        isCopied ? 2000 : 3000
    );
}

/**
 * Highlights all code blocks on the page using the Prism.js library.
 * If the Prism.js custom class plugin is available, it prefixes all classes with 'prism-'.
 */
function highlightCode() {
    if (Prism) {
        Prism.plugins.customClass.prefix('prism-');
        Prism.highlightAll();
    }
};

/**
 * Scrolls the page to the heading element specified by the URL hash.
 * If a hash is present in the URL, it finds the corresponding element by ID and scrolls it into view.
 */
function navigateToHeading() {
    if (window.location.hash) {
        // get hash tag in URL
        let hashTagName = window.location.hash.substring(1);
        let el = document.getElementById(hashTagName);
        if (el) {
            // do the scroll
            el.scrollIntoView();
        }
    }
}

/**
 * Update the theme of the demo code
 * @param {} theme 
 * @returns {} 
 */
function updateDemoCodeThemeCss(theme) {
    if (theme === "dark") {
        let prismThemeLightLinkEl = document.getElementById('prismThemeLightLink');
        if (prismThemeLightLinkEl)
            prismThemeLightLinkEl?.remove();

        let prismThemeDarkLinkEl = document.createElement("link");
        prismThemeDarkLinkEl.setAttribute("rel", "stylesheet");
        prismThemeDarkLinkEl.setAttribute("href", "/_content/BlazorBootstrap.Demo.RCL/css/prism-vsc-dark-plus.min.css");
        prismThemeDarkLinkEl.setAttribute("id", "prismThemeDarkLink");

        document.head.append(prismThemeDarkLinkEl);
    }
    else if (theme === "light") {
        let prismThemeDarkLinkEl = document.getElementById('prismThemeDarkLink');
        if (prismThemeDarkLinkEl)
            prismThemeDarkLinkEl?.remove();

        let prismThemeLightLinkEl = document.createElement("link");
        prismThemeLightLinkEl.setAttribute("rel", "stylesheet");
        prismThemeLightLinkEl.setAttribute("href", "/_content/BlazorBootstrap.Demo.RCL/css/prism-vs.min.css");
        prismThemeLightLinkEl.setAttribute("id", "prismThemeLightLink");

        document.head.append(prismThemeLightLinkEl);
    }
}

/**
 * Per-tab storage for the Showcase sample sites (sessionStorage only, keys under "bb-showcase:").
 * A snapshot is written in one call and read back in slices, because on the Blazor Server host every value returned
 * to .NET travels as one SignalR message, capped at 32 KB by default. Every call fails soft: storage is optional.
 */
window.showcaseStore = {
    prefix: 'bb-showcase:',

    /** Returns the stored value's length, -1 when the key is missing, or -2 when storage is unavailable (blocked, private mode). */
    length(key) {
        if (!key.startsWith(this.prefix)) return -1;
        try {
            const value = sessionStorage.getItem(key);
            return value === null ? -1 : value.length;
        } catch {
            return -2;
        }
    },

    /** Returns one slice of the stored value, or null when the key is missing. */
    slice(key, start, length) {
        if (!key.startsWith(this.prefix)) return null;
        const value = sessionStorage.getItem(key);
        return value === null ? null : value.substring(start, start + length);
    },

    /** Writes a value in one call; returns false when storage refuses it (quota, private mode). */
    set(key, value) {
        if (!key.startsWith(this.prefix)) return false;
        try {
            sessionStorage.setItem(key, value);
            return true;
        } catch {
            return false;
        }
    },

    /** Removes a value. */
    remove(key) {
        if (!key.startsWith(this.prefix)) return;
        try {
            sessionStorage.removeItem(key);
        } catch {
            // Storage is optional.
        }
    }
};

/**
 * Form errors for the Showcase sample sites (WCAG 3.3.1, 4.1.3). Called by ShowcaseFormErrors after each render of a form
 * whose validation state changed: links every visible validation message to its field through aria-describedby, so a
 * screen reader reads it with the field, and after a failed submit moves focus to the first invalid field.
 */
window.showcaseForms = {
    count: 0,

    sync(anchor, focusFirstInvalid) {
        const form = anchor?.closest('form');
        if (!form) return false;
        const fieldSelector = 'input:not([type=hidden]), select, textarea';
        // Forget links to messages that are gone.
        form.querySelectorAll('[data-sc-error-ids]').forEach(field => {
            const own = field.dataset.scErrorIds.split(' ').filter(Boolean);
            const kept = (field.getAttribute('aria-describedby') || '').split(' ').filter(id => id && !(own.includes(id) && !document.getElementById(id)));
            const ownKept = own.filter(id => document.getElementById(id));
            kept.length ? field.setAttribute('aria-describedby', kept.join(' ')) : field.removeAttribute('aria-describedby');
            ownKept.length ? field.dataset.scErrorIds = ownKept.join(' ') : delete field.dataset.scErrorIds;
        });
        // Link each message to the nearest field in its group (the message follows its field in the same wrapper).
        form.querySelectorAll(".validation-message").forEach(message => {
            // The field is the last one before the message in its wrapper, looking at most three wrappers up and at most
            // as far as the form itself (a message can sit directly in the form).
            let group = message.parentElement, field = null;
            for (let depth = 0; group && !field && depth < 3; depth++, group = group.parentElement) {
                field = [...group.querySelectorAll(fieldSelector)]
                    .filter(candidate => candidate.compareDocumentPosition(message) & Node.DOCUMENT_POSITION_FOLLOWING)
                    .pop() || null;
                if (group === form) break;
            }
            if (!field) return;
            // A radio or checkbox group is described as a whole, not by its last option.
            if (field.matches('input[type=radio], input[type=checkbox]')) {
                field = field.closest('fieldset, [role=radiogroup], [role=group]') || field;
            }
            // Skip a field that already points at a wrapper holding this message, so it isn't read twice.
            const current = (field.getAttribute('aria-describedby') || '').split(' ').filter(Boolean);
            if (current.some(id => id !== message.id && document.getElementById(id)?.contains(message))) return;
            if (!message.id) message.id = `sc-form-error-${++this.count}`;
            const ids = (field.getAttribute('aria-describedby') || '').split(' ').filter(Boolean);
            if (!ids.includes(message.id)) field.setAttribute('aria-describedby', [...ids, message.id].join(' '));
            const own = (field.dataset.scErrorIds || '').split(' ').filter(Boolean);
            if (!own.includes(message.id)) field.dataset.scErrorIds = [...own, message.id].join(' ');
        });
        if (!focusFirstInvalid) return false;
        // Blazor's own inputs set aria-invalid; Blazor Bootstrap's inputs only carry the "invalid" class.
        const first = form.querySelector('[aria-invalid=true]:not([type=hidden]), :is(input, select, textarea):is(.invalid, .is-invalid)');
        if (!first) return false;
        first.focus();
        return true;
    }
};
