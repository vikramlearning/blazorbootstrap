import * as pdfJS from './pdfjs-4.0.379.min.mjs';
import * as pdfWorker from './pdfjs-4.0.379.worker.min.mjs';

if (pdfJS != null && pdfWorker != null) {
    // The workerSrc property shall be specified.
    pdfJS.GlobalWorkerOptions.workerSrc = pdfWorker;
}

function getCanvas(item) {
    if (isDomSupported() && typeof item === 'string') {
        item = document.getElementById(item);
    } else if (item && item.length) {
        // Support for array based queries (such as jQuery)
        item = item[0];
    }

    if (item && item.canvas !== undefined && item.canvas) {
        // Support for any object associated to a canvas (including a context2d)
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
        const existingPdf = getPdf(canvas);

        //if (existingPdf === undefined)
        //    return;

        this.id = canvas.id;
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        //this.width = width;
        //this.height = height;
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
    const pageRange = [1,2,3,4]; // Print pages 1 to 3, inclusive

    const printIframe = document.getElementById(elementId);

    for (const pageNumber of pageRange) {
        const page = await pdfDoc.getPage(pageNumber);

        // Render and print each page sequentially
        const viewport = page.getViewport({ scale: 1.333 });
        const canvas = document.createElement("canvas");
        //canvas.style = 'display:none';
        canvas.height = viewport.height;
        canvas.width = viewport.width;

        const renderContext = {
            canvasContext: canvas.getContext("2d"),
            viewport: viewport
        };
        await page.render(renderContext).promise;

        // Append canvas to iframe for each page
        const iframeDoc = printIframe.contentWindow.document;
        iframeDoc.body.appendChild(canvas);
    }

    setTimeout(() => {
        // Print all pages at once
        printIframe.contentWindow.print();

        // Clean up by removing all canvases
        const canvases = document.querySelectorAll("#printIframe canvas");
        for (const canvas of canvases) {
            iframeDoc.body.removeChild(canvas);
        }
    },
    1000);
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

export function firstPage(dotNetHelper, elementId) {
    const pdf = getPdf(elementId);

    if (pdf == null || pdf.pageNum === 1)
        return;

    if (pdf.pageNum > 1)
        pdf.pageNum = 1;

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

export function gotoPage(dotNetHelper, elementId, gotoPageNum) {
    const pdf = getPdf(elementId);

    if (pdf == null || gotoPageNum < 1 || gotoPageNum > pdf.pagesCount)
        return;

    pdf.pageNum = gotoPageNum;

    queueRenderPage(pdf, pdf.pageNum);

    setPdfViewerMetaData(dotNetHelper, pdf);
}

export function zoomInOut(dotNetHelper, elementId, scale) {
    const pdf = getPdf(elementId);

    if (pdf == null)
        return;

    if (!Number.isNaN(scale))
        pdf.scale = scale;

    queueRenderPage(pdf, pdf.pageNum);
}

export function rotate(dotNetHelper, elementId, rotation) {
    const pdf = getPdf(elementId);

    if (pdf == null || Number.isNaN(rotation) || rotation % 90 !== 0)
        return;

    pdf.rotation = rotation;

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

function setPdfViewerMetaData(dotNetHelper, pdf) {
    if (dotNetHelper == null)
        return;

    dotNetHelper.invokeMethodAsync('SetPdfViewerMetaData', { pagesCount: pdf.pagesCount, pageNumber: pdf.pageNum });
}

function isDomSupported() {
    return typeof window !== 'undefined' && typeof document !== 'undefined';
}