/**
 * Copies the provided text to the clipboard and invokes .NET methods based on the success or failure of the operation.
 * @param {string} text - The text to be copied to the clipboard.
 * @param {object} dotNetHelper - The .NET helper object to invoke methods on.
 */
async function copyToClipboard(text, dotNetHelper) {
    let isCopied = true;

    try {
        await navigator.clipboard.writeText(text);
        dotNetHelper.invokeMethodAsync('OnCopySuccessJS');
    } catch (err) {
        isCopied = false;
        dotNetHelper.invokeMethodAsync('OnCopyFailJS', err);
    }

    setTimeout(
        () => { dotNetHelper.invokeMethodAsync('ResetCopyStatusJS'); },
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