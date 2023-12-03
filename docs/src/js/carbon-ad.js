import React from 'react';
import ReactDOM from 'react-dom'

export function RenderAd() {
    let scriptLoaderEl = document.getElementById(`divCarbonAd`);
    if (scriptLoaderEl) {
        setTimeout(() => {
            let scriptEl = document.getElementById(`_carbonads_js`);
            if (scriptEl) {
                scriptEl.remove();
            }
            scriptEl = document.createElement('script');
            scriptEl.async = true;
            scriptEl.id = `_carbonads_js`;
            scriptEl.src = `//cdn.carbonads.com/carbon.js?serve=CWYICKJI&placement=docsblazorbootstrapcom`;
            scriptEl.type = `text/javascript`;
            scriptEl.onerror = function () {
                console.error(`An error occurred while loading the script: ${source}`);
            }
            scriptLoaderEl.replaceChildren();
            if (scriptEl) {
                scriptLoaderEl.appendChild(scriptEl);
            }
        }, 5000);
    }
    return null;
}