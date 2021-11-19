if (!window.blazorBootstrap) {
    window.blazorBootstrap = {};
}

window.blazorBootstrap = {
    tooltip: {
        initialize: (elementId) => {
            let ele = document.getElementById(elementId);
            var tooltip = new bootstrap.Tooltip(ele);
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

            let bsOffcanvas = new bootstrap.Offcanvas(myOffcanvas);
            bsOffcanvas?.show();
        },
        hide: (elementId) => {
            let myOffcanvas = document.getElementById(elementId);
            let bsOffcanvas = new bootstrap.Offcanvas(myOffcanvas);
            bsOffcanvas?.hide();
        }
    }
}