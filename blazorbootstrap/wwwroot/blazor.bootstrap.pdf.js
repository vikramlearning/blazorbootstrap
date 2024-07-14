import * as pdfJS from './pdfjs-4.0.379.min.js';
import * as pdfWorker from './pdfjs-4.0.379.worker.min.js';

if (pdfJS != null && pdfWorker != null) {
    pdfJS.GlobalWorkerOptions.workerSrc = pdfWorker;
}

function getCanvas(item) {
    if (isDomSupported() && typeof item === 'string') {
        item = document.getElementById(item);
    } else if (item && item.length) {
        // support for array based queries
        item = item[0];
    }

    if (item && item.canvas !== undefined && item.canvas) {
        // support for any object associated to a canvas (including a context2d)
        item = item.canvas;
    }

    return item;
}

const getPdf = (key) => {
    const canvas = getCanvas(key);
    return Object.values(pdfInstances).filter((c) => c.canvas === canvas).pop();
};

const pdfInstances = {};

class Pdf {
    static instances = pdfInstances;
    static getPdf = getPdf;

    constructor(item) {
        const canvas = getCanvas(item);
        //const existingPdf = getPdf(canvas);

        this.id = canvas.id;
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        this.pdfDoc = null;
        this.pageNum = 1;
        this.pagesCount = 0;
        this.pageRendering = false;
        this.pageNumPending = null;
        this.scale = 1;
        this.rotation = 0;

        pdfInstances[this.id] = this;
    }
}

export function firstPage(dotNetHelper, elementId) {
    const pdf = getPdf(elementId);

    if (pdf == null || pdf.pageNum === 1)
        return;

    if (pdf.pageNum > 1)
        pdf.pageNum = 1;

    queueRenderPage(pdf, pdf.pageNum);

    setPdfViewerMetaData(dotNetHelper, pdf);
}

export function gotoPage(dotNetHelper, elementId, gotoPageNum) {
    const pdf = getPdf(elementId);

    if (pdf == null || gotoPageNum < 1 || gotoPageNum > pdf.pagesCount)
        return;

    pdf.pageNum = gotoPageNum;

    queueRenderPage(pdf, pdf.pageNum);

    setPdfViewerMetaData(dotNetHelper, pdf);
}

export function lastPage(dotNetHelper, elementId) {
    const pdf = getPdf(elementId);

    if (pdf == null || (pdf.pageNum === 1 && pdf.pageNum === pdf.pagesCount))
        return;

    if (pdf.pageNum < pdf.pagesCount)
        pdf.pageNum = pdf.pagesCount;

    queueRenderPage(pdf, pdf.pageNum);

    setPdfViewerMetaData(dotNetHelper, pdf);
}

export function nextPage(dotNetHelper, elementId) {
    const pdf = getPdf(elementId);

    if (pdf == null || pdf.pageNum === pdf.pagesCount)
        return;

    if (pdf.pageNum < pdf.pagesCount)
        pdf.pageNum += 1;

    queueRenderPage(pdf, pdf.pageNum);

    setPdfViewerMetaData(dotNetHelper, pdf);
}

export function previousPage(dotNetHelper, elementId) {
    const pdf = getPdf(elementId);

    if (pdf == null || pdf.pageNum === 0 || pdf.pageNum === 1)
        return;

    if (pdf.pageNum > 0)
        pdf.pageNum -= 1;

    queueRenderPage(pdf, pdf.pageNum);

    setPdfViewerMetaData(dotNetHelper, pdf);
}

export async function print(dotNetHelper, elementId, url) {
    const pdfDoc = await pdfJS.getDocument(url).promise;
    const pageRange = [1, 2, 3, 4]; // TODO: update this

    const iframeEl = document.createElement('iframe');
    iframeEl.style = 'display:none';
    document.body.appendChild(iframeEl);

    for (const pageNumber of pageRange) {
        const page = await pdfDoc.getPage(pageNumber);

        const viewport = page.getViewport({ scale: 1.5 });
        const canvas = document.createElement("canvas");
        canvas.height = viewport.height;
        canvas.width = viewport.width;

        //canvas.style.height = `${viewport.height * 2}px`;
        //canvas.style.width = `${viewport.width * 2}px`;

        const ctx = canvas.getContext('2d');

        const renderContext = {
            //intent: 'print',
            canvasContext: ctx,
            viewport: viewport
        };
        await page.render(renderContext).promise;

        const iframeDoc = iframeEl.contentWindow.document;
        iframeDoc.body.appendChild(canvas);
    }

    setTimeout(() => {
        iframeEl.contentWindow.print();
        iframeEl.remove();
    },
        1000);
}

export function rotate(dotNetHelper, elementId, rotation) {
    const pdf = getPdf(elementId);

    if (pdf == null || Number.isNaN(rotation) || rotation % 90 !== 0)
        return;

    pdf.rotation = rotation;

    queueRenderPage(pdf, pdf.pageNum);
}

export function zoomInOut(dotNetHelper, elementId, scale) {
    const pdf = getPdf(elementId);

    if (pdf == null)
        return;

    if (!Number.isNaN(scale))
        pdf.scale = scale;

    queueRenderPage(pdf, pdf.pageNum);
}

// resize
// print
// download
// zoomreset

/*
firstPageButton.disabled = this.pageNumber <= 1;
lastPageButton.disabled = this.pageNumber >= this.pagesCount;
pageRotateCwButton.disabled = this.pagesCount === 0;
pageRotateCcwButton.disabled = this.pagesCount === 0;
*/

export function initialize(dotNetHelper, elementId, scale, rotation, url) {
    const pdf = new Pdf(elementId);
    pdf.scale = scale;
    pdf.rotation = rotation;

    pdfJS.getDocument(url).promise.then(function (doc) {
        pdf.pdfDoc = doc;
        pdf.pagesCount = doc.numPages;
        renderPage(pdf, pdf.pageNum);
        dotNetHelper.invokeMethodAsync('DocumentLoaded', { pagesCount: pdf.pagesCount, pageNumber: pdf.pageNum });
    });
}

function isDomSupported() {
    return typeof window !== 'undefined' && typeof document !== 'undefined';
}

/**
 * If another page rendering in progress, waits until the rendering is
 * finished. Otherwise, executes rendering immediately.
 */
function queueRenderPage(pdf, num) {
    if (pdf.pageRendering) {
        pdf.pageNumPending = num;
    } else {
        renderPage(pdf, num);
    }
}

/**
 * Get page info from document, resize canvas accordingly, and render page.
 * @param num Page number.
 */
function renderPage(pdf, num) {
    pdf.pageRendering = true;

    // Using promise to fetch the page
    pdf.pdfDoc.getPage(num).then((page) => {
        const viewport = page.getViewport({ scale: pdf.scale, rotation: pdf.rotation });
        pdf.canvas.height = viewport.height;
        pdf.canvas.width = viewport.width;

        // Render PDF page into canvas context
        const renderContext = {
            canvasContext: pdf.ctx,
            viewport: viewport
        };

        const renderTask = page.render(renderContext);

        // Wait for rendering to finish
        renderTask.promise.then(() => {
            pdf.pageRendering = false;
            if (pdf.pageNumPending !== null) {
                // New page rendering is pending
                renderPage(pdf, pdf.pageNumPending);
                pdf.pageNumPending = null;
            }
        })
            .catch((error) => {
                // TODO: track exception
            });
    });
}

function setPdfViewerMetaData(dotNetHelper, pdf) {
    if (dotNetHelper == null)
        return;

    dotNetHelper.invokeMethodAsync('SetPdfViewerMetaData', { pagesCount: pdf.pagesCount, pageNumber: pdf.pageNum });
}
