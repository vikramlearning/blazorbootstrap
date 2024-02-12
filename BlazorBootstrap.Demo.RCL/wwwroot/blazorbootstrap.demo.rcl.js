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

function highlightCode() {
    if (Prism) {
        Prism.highlightAll();
    }
};

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

