if (!window.blazorBootstrap) {
    window.blazorBootstrap = {};
}

window.blazorBootstrap = {
    tooltip: {
        initialize: (elementId) => {
            let ele = document.getElementById(elementId);
            var tooltip = bootstrap.Tooltip.getOrCreateInstance(ele);
        },
    },
    offcanvas: {
        show: (elementId, dotNetHelper) => {
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

            bootstrap?.Offcanvas?.getOrCreateInstance(offcanvasEl)?.show();
        },
        hide: (elementId) => {
            bootstrap?.Offcanvas?.getOrCreateInstance(document.getElementById(elementId))?.hide();
        }
    },
    modal: {
        show: (elementId, dotNetHelper) => {
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

            bootstrap?.Modal?.getOrCreateInstance(modalEl)?.show();

        },
        hide: (elementId) => {
            bootstrap?.Modal?.getOrCreateInstance(document.getElementById(elementId))?.hide();
        }
    }
}