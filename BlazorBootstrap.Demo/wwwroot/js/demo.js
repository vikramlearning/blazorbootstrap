function navigateToHeading() {
    if (window.location.hash) {
        // get hashtag in URL
        var hashTagName = window.location.hash.substring(1);
        let el = document.getElementById(hashTagName);
        if (el) {
            // do the scroll
            el.scrollIntoView();
        }
    }
}

window.highlightCode = function () {
    Prism.highlightAll();
};