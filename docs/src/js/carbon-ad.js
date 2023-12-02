import React from 'react';
import ReactDOM from 'react-dom'

export function RenderAd() {
    let scriptLoaderEl = document.getElementById(`divCarbonAd`);
    let scriptEl = document.getElementById(`_carbonads_js`);
    if (scriptLoaderEl) {
        if (scriptEl) {
            // do nothing
        } else {
            scriptEl = document.createElement('script');
            scriptEl.async = true;
            scriptEl.id = `_carbonads_js`;
            scriptEl.src = `//cdn.carbonads.com/carbon.js?serve=CWYICKJI&placement=docsblazorbootstrapcom`;
            scriptEl.type = `text/javascript`;            
            scriptEl.onerror = function () {
                console.error(`An error occurred while loading the script: ${source}`);
            }
            scriptLoaderEl.appendChild(scriptEl);
        }
    }
    return null;
}
