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
            var myOffcanvas = document.getElementById(elementId);

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

            var bsOffcanvas = new bootstrap.Offcanvas(myOffcanvas);
            bsOffcanvas?.show();
        },
        hide: (elementId) => {
            var myOffcanvas = document.getElementById(elementId);

            //myOffcanvas.removeEventListener('show.bs.offcanvas', function () { }, false);
            //myOffcanvas.removeEventListener('shown.bs.offcanvas', function () { }, false);
            //myOffcanvas.removeEventListener('hide.bs.offcanvas', function () { }, false);
            //myOffcanvas.removeEventListener('hidden.bs.offcanvas', function () { }, false);

            var bsOffcanvas = new bootstrap.Offcanvas(myOffcanvas);
            bsOffcanvas?.hide();
        }
    }
}