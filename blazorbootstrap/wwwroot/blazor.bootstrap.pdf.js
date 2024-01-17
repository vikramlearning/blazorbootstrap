import * as pdfJS from "./pdfjs-4.0.379.min.mjs";
import * as pdfWorker from "./pdfjs-4.0.379.worker.min.mjs";

if (pdfJS != null && pdfWorker != null) {
    // The workerSrc property shall be specified.
    pdfJS.GlobalWorkerOptions.workerSrc = pdfWorker;
}

function getCanvas(item) {
    if (_isDomSupported() && typeof item === 'string') {
        item = document.getElementById(item);
    } else if (item && item.length) {
        // Support for array based queries (such as jQuery)
        item = item[0];
    }

    if (item && item.canvas) {
        // Support for any object associated to a canvas (including a context2d)
        item = item.canvas;
    }

    return item;
}

const getPdf = (key) => {
    const canvas = getCanvas(key);
    return Object.values(instances).filter((c) => c.canvas === canvas).pop();
};

const instances = {};

class Pdf {
    static instances = instances;
    static getPdf = getPdf;

    constructor(item) {
        const canvas = getCanvas(item);
        const existingPdf = getPdf(canvas);
        if (existingPdf != null) {
            throw new Error(
                'Canvas is already in use. Canvas with ID \'' + existingPdf.id + '\'' +
                ' must be destroyed before the canvas with ID \'' + existingPdf.canvas.id + '\' can be reused.'
            );
        }

        this.id = canvas.id;
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        //this.width = width;
        //this.height = height;
        this.pdfDoc = null;
        this.pageNum = 1;
        this.numPages = 0;
        this.pageRendering = false;
        this.pageNumPending = null;
        this.scale = 1.3333;

        instances[this.id] = this;
    }
}

/**
 * Get page info from document, resize canvas accordingly, and render page.
 * @param num Page number.
 */
function renderPage(pdf, num) {
    pdf.pageRendering = true;

    // Using promise to fetch the page
    pdf.pdfDoc.getPage(num).then(function (page) {
        var viewport = page.getViewport({ scale: pdf.scale });
        pdf.canvas.height = viewport.height;
        pdf.canvas.width = viewport.width;

        // Render PDF page into canvas context
        var renderContext = {
            canvasContext: pdf.ctx,
            viewport: viewport
        };

        var renderTask = page.render(renderContext);

        // Wait for rendering to finish
        renderTask.promise.then(function () {
            pdf.pageRendering = false;
            if (pdf.pageNumPending !== null) {
                // New page rendering is pending
                renderPage(pdf, pdf.pageNumPending);
                pdf.pageNumPending = null;
            }
        });
    });
}

/**
 * If another page rendering in progress, waits until the rendering is
 * finised. Otherwise, executes rendering immediately.
 */
function queueRenderPage(pdf, num) {
    if (pdf.pageRendering) {
        pdf.pageNumPending = num;
    } else {
        renderPage(pdf, num);
    }
}

export function previousPage(dotNetHelper, elementId) {
    let pdf = getPdf(elementId);

    if (pdf.pageNum === 0 || pdf.pageNum === 1)
        return;

    if (pdf.pageNum > 0)
        pdf.pageNum -= 1;

    queueRenderPage(pdf, pdf.pageNum);

    dotNetHelper.invokeMethodAsync('Set', { pageCount: pdf.numPages, pageNumber: pdf.pageNum });
}

export function nextPage(dotNetHelper, elementId) {
    let pdf = getPdf(elementId);

    if (pdf == null || pdf.pageNum === pdf.numPages)
        return;

    if (pdf.pageNum < pdf.numPages)
        pdf.pageNum += 1;

    queueRenderPage(pdf, pdf.pageNum);

    dotNetHelper.invokeMethodAsync('Set', { pageCount: pdf.numPages, pageNumber: pdf.pageNum });
}

export function firstPage(dotNetHelper, elementId) {
    let pdf = getPdf(elementId);

    if (pdf == null || pdf.pageNum === 1)
        return;

    if (pdf.pageNum > 1)
        pdf.pageNum = 1;

    queueRenderPage(pdf, pdf.pageNum);

    dotNetHelper.invokeMethodAsync('Set', { pageCount: pdf.numPages, pageNumber: pdf.pageNum });
}

export function lastPage(dotNetHelper, elementId) {
    let pdf = getPdf(elementId);

    if (pdf == null || (pdf.pageNum === 1 && pdf.pageNum === pdf.numPages))
        return;

    if (pdf.pageNum < pdf.numPages)
        pdf.pageNum = pdf.numPages;

    queueRenderPage(pdf, pdf.pageNum);

    dotNetHelper.invokeMethodAsync('Set', { pageCount: pdf.numPages, pageNumber: pdf.pageNum });
}

// resize
// print
// download
// firstpage
// lastpage
// nextpage
// previouspage
// zoomin
// zoomout
// zoomreset
// rotatecw
// rotateccw









export function initialize(dotNetHelper, elementId, url) {
    const pdf = new Pdf(elementId);

    pdfJS.getDocument(url).promise.then(function (doc) {
        pdf.pdfDoc = doc;
        pdf.numPages = doc.numPages;
        renderPage(pdf, pdf.pageNum);
        dotNetHelper.invokeMethodAsync('Set', { pageCount: pdf.numPages, pageNumber: pdf.pageNum });
    });
}

/* helpers */
function _isDomSupported() {
    return typeof window !== 'undefined' && typeof document !== 'undefined';
}
