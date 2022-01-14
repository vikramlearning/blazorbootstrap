if (!window.blazorBootstrap) {
    window.blazorBootstrap = {};
}

window.blazorBootstrap = {
    alert: {
        initialize: (elementId, dotNetHelper) => {
            let alertEl = document.getElementById(elementId);

            alertEl.addEventListener('close.bs.alert', function () {
                dotNetHelper.invokeMethodAsync('bsCloseAlert');
            });
            alertEl.addEventListener('closed.bs.alert', function () {
                dotNetHelper.invokeMethodAsync('bsClosedAlert');
            });

            bootstrap?.Alert?.getOrCreateInstance(alertEl);
        },
        close: (elementId) => {
            bootstrap?.Alert?.getOrCreateInstance(document.getElementById(elementId))?.close();
        },
        dispose: (elementId) => {
            bootstrap?.Alert?.getOrCreateInstance(document.getElementById(elementId))?.dispose();
        }
    },
    modal: {
        show: (elementId, useStaticBackdrop, closeOnEscape, dotNetHelper) => {
            let modalEl = document.getElementById(elementId);

            modalEl.addEventListener('show.bs.modal', function () {
                dotNetHelper.invokeMethodAsync('bsShowModal');
            });
            modalEl.addEventListener('shown.bs.modal', function () {
                dotNetHelper.invokeMethodAsync('bsShownModal');
            });
            modalEl.addEventListener('hide.bs.modal', function () {
                dotNetHelper.invokeMethodAsync('bsHideModal');
            });
            modalEl.addEventListener('hidden.bs.modal', function () {
                dotNetHelper.invokeMethodAsync('bsHiddenModal');
            });
            modalEl.addEventListener('hidePrevented.bs.modal', function () {
                dotNetHelper.invokeMethodAsync('bsHidePreventedModal');
            });

            let options = { backdrop: useStaticBackdrop ? 'static' : true, keyboard: closeOnEscape };
            bootstrap?.Modal?.getOrCreateInstance(modalEl, options)?.show();
        },
        hide: (elementId) => {
            bootstrap?.Modal?.getOrCreateInstance(document.getElementById(elementId))?.hide();
        },
        dispose: (elementId) => {
            bootstrap?.Modal?.getOrCreateInstance(document.getElementById(elementId))?.dispose();
        }
    },
    offcanvas: {
        show: (elementId, useBackdrop, closeOnEscape, isScrollable, dotNetHelper) => {
            let offcanvasEl = document.getElementById(elementId);

            offcanvasEl.addEventListener('show.bs.offcanvas', function () {
                dotNetHelper.invokeMethodAsync('bsShowOffcanvas');
            });
            offcanvasEl.addEventListener('shown.bs.offcanvas', function () {
                dotNetHelper.invokeMethodAsync('bsShownOffcanvas');
            });
            offcanvasEl.addEventListener('hide.bs.offcanvas', function () {
                dotNetHelper.invokeMethodAsync('bsHideOffcanvas');
            });
            offcanvasEl.addEventListener('hidden.bs.offcanvas', function () {
                dotNetHelper.invokeMethodAsync('bsHiddenOffcanvas');
            });

            let options = { backdrop: useBackdrop, keyboard: closeOnEscape, scroll: isScrollable };
            bootstrap?.Offcanvas?.getOrCreateInstance(offcanvasEl, options)?.show();
        },
        hide: (elementId) => {
            bootstrap?.Offcanvas?.getOrCreateInstance(document.getElementById(elementId))?.hide();
        },
        dispose: (elementId) => {
            bootstrap?.Offcanvas?.getOrCreateInstance(document.getElementById(elementId))?.dispose();
        }
    },
    toasts: {
        show: (elementId, autohide, delay, dotNetHelper) => {
            var toastEl = document.getElementById(elementId);

            toastEl.addEventListener('show.bs.toast', function () {
                dotNetHelper.invokeMethodAsync('bsShowToast');
            });
            toastEl.addEventListener('shown.bs.toast', function () {
                dotNetHelper.invokeMethodAsync('bsShownToast');
            });
            toastEl.addEventListener('hide.bs.toast', function () {
                dotNetHelper.invokeMethodAsync('bsHideToast');
            });
            toastEl.addEventListener('hidden.bs.toast', function () {
                dotNetHelper.invokeMethodAsync('bsHiddenToast');
            });

            let options = { animation: true, autohide: autohide, delay: delay }
            bootstrap?.Toast?.getOrCreateInstance(toastEl, options)?.show();
        },
        hide: (elementId) => {
            bootstrap?.Toast?.getOrCreateInstance(document.getElementById(elementId))?.hide();
        },
        dispose: (elementId) => {
            bootstrap?.Toast?.getOrCreateInstance(document.getElementById(elementId))?.dispose();
        }
    },
    tooltip: {
        initialize: (elementId) => {
            let tooltipEl = document.getElementById(elementId);
            bootstrap?.Tooltip?.getOrCreateInstance(tooltipEl);
        },
    },
}