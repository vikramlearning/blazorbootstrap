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
            let myOffcanvas = document.getElementById(elementId);

            myOffcanvas.addEventListener('show.bs.offcanvas', function () {
                dotNetHelper.invokeMethodAsync('bsShowOffcanvas');
            });
            myOffcanvas.addEventListener('shown.bs.offcanvas', function () {
                dotNetHelper.invokeMethodAsync('bsShownOffcanvas');
            });
            myOffcanvas.addEventListener('hide.bs.offcanvas', function () {
                dotNetHelper.invokeMethodAsync('bsHideOffcanvas');
            });
            myOffcanvas.addEventListener('hidden.bs.offcanvas', function () {
                dotNetHelper.invokeMethodAsync('bsHiddenOffcanvas');
            });

            bootstrap?.Offcanvas?.getOrCreateInstance(myOffcanvas)?.show();
        },
        hide: (elementId) => {
            bootstrap?.Offcanvas?.getOrCreateInstance(document.getElementById(elementId))?.hide();
        }
    }
}