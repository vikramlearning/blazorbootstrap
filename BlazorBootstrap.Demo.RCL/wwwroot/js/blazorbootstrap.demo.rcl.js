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