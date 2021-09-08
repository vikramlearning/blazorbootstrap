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
}