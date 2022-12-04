﻿if (!window.blazorBootstrap) {
    window.blazorBootstrap = {};
}

if (!window.blazorChart) {
    window.blazorChart = {};
}

if (!window.blazorChart.line) {
    window.blazorChart.line = {};
}

if (!window.blazorChart.bar) {
    window.blazorChart.bar = {};
}

if (!window.blazorChart.doughnut) {
    window.blazorChart.doughnut = {};
}

if (!window.blazorChart.pie) {
    window.blazorChart.pie = {};
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
    autocomplete: {
        initialize: (elementRef, dotNetHelper) => {
            let dropdownToggleEl = elementRef;

            dropdownToggleEl.addEventListener('show.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsShowAutocomplete');
            });
            dropdownToggleEl.addEventListener('shown.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsShownAutocomplete');
            });
            dropdownToggleEl.addEventListener('hide.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsHideAutocomplete');
            });
            dropdownToggleEl.addEventListener('hidden.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsHiddenAutocomplete');
            });

            bootstrap?.Dropdown?.getOrCreateInstance(elementRef);
        },
        show: (elementRef) => {
            bootstrap?.Dropdown?.getOrCreateInstance(elementRef)?.show();
        },
        hide: (elementRef) => {
            bootstrap?.Dropdown?.getOrCreateInstance(elementRef)?.hide();
        },
        dispose: (elementRef) => {
            bootstrap?.Dropdown?.getOrCreateInstance(elementRef)?.dispose();
        }
    },
    confirmDialog: {
        show: () => {
            let bodyEl = document.getElementsByTagName('body');
            if (bodyEl.length > 0)
                bodyEl[0].style['overflow'] = 'hidden';
        },
        hide: () => {
            let bodyEl = document.getElementsByTagName('body');
            if (bodyEl.length > 0)
                bodyEl[0].style['overflow'] = 'auto';
        }
    },
    modal: {
        initialize: (elementId, useStaticBackdrop, closeOnEscape, dotNetHelper) => {
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
            bootstrap?.Modal?.getOrCreateInstance(modalEl, options);
        },
        show: (elementId) => {
            bootstrap?.Modal?.getOrCreateInstance(document.getElementById(elementId))?.show();
        },
        hide: (elementId) => {
            bootstrap?.Modal?.getOrCreateInstance(document.getElementById(elementId))?.hide();
        },
        dispose: (elementId) => {
            bootstrap?.Modal?.getOrCreateInstance(document.getElementById(elementId))?.dispose();
        }
    },
    numberInput: {
        initialize: (elementId, isFloat, allowNegativeNumbers) => {
            let numberEl = document.getElementById(elementId);

            numberEl?.addEventListener('keydown', function (event) {
                let invalidChars = ["e", "E", "+"];
                if (!isFloat)
                    invalidChars.push("."); // restrict '.' for integer types

                if (!allowNegativeNumbers) {
                    invalidChars.push("-"); // restrict '-'
                }

                if (invalidChars.includes(event.key))
                    event.preventDefault();
            });

            numberEl?.addEventListener('beforeinput', function (event) {
                if (event.inputType === 'insertFromPaste' || event.inputType === 'insertFromDrop') {

                    if (!allowNegativeNumbers) {
                        // restrict 'e', 'E', '+', '-'
                        if (isFloat && /[\e\E\+\-]/gi.test(event.data)) {
                            event.preventDefault();
                        }
                        // restrict 'e', 'E', '.', '+', '-'
                        else if (!isFloat && /[\e\E\.\+\-]/gi.test(event.data)) {
                            event.preventDefault();
                        }
                    }
                    // restrict 'e', 'E', '+'
                    else if (isFloat && /[\e\E\+]/gi.test(event.data)) {
                        event.preventDefault();
                    }
                    // restrict 'e', 'E', '.', '+'
                    else if (!isFloat && /[\e\E\.\+]/gi.test(event.data)) {
                        event.preventDefault();
                    }

                }
            });
        },
        setValue: (elementId, value) => {
            document.getElementById(elementId).value = value;
        }
    },
    currencyInput: {
        initialize: (elementId, isFloat, allowNegativeNumbers) => {
            let currencyEl = document.getElementById(elementId);

            currencyEl?.addEventListener('keydown', function (event) {

                switch (event.keyCode) {
                    case 8:   // backspace
                    case 9:   // tab
                    case 13:  // enter
                    case 37:  // arrows left
                    case 38:  // arrows up
                    case 39:  // arrows right
                    case 40:  // arrows down
                    case 46:  // delete key
                        return;
                }

                let validChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

                if (isFloat) {
                    validChars.push('.'); // TODO: check ',' for specific culture
                }

                if (allowNegativeNumbers) {
                    validChars.push('-');
                }

                if (!validChars.includes(event.key))
                    event.preventDefault();
            });

            currencyEl?.addEventListener('beforeinput', function (event) {
                if (event.inputType === 'insertFromPaste' || event.inputType === 'insertFromDrop') {

                    let validChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

                    if (isFloat) {
                        validChars.push('.'); // TODO: check ',' for specific culture
                    }

                    if (allowNegativeNumbers) {
                        validChars.push('-');
                    }

                    if (event.data && event.data.length > 0) {
                        event.data.split('').forEach(c => {
                            if (!validChars.includes(c)) {
                                event.preventDefault(); // can be improved
                            }
                        });
                    }
                }
            });

            currencyEl?.addEventListener('focusin', function (event) {
                if (currencyEl.dataset.currentValue)
                    currencyEl.value = currencyEl.dataset.currentValue; // assign original value
                else
                    currencyEl.value = ''; // don't assign zero.
            });

            // this event is fired after OnChange event
            currencyEl?.addEventListener('focusout', function (event) {
                // scenario:
                // without changing the value focusout event is triggered
                if (typeof (currencyEl.dataset.currentValue) === 'undefined' || currencyEl.dataset.currentValue === currencyEl.value) {
                    currencyEl.value = currencyEl.dataset.currentFormattedValue; // assign formatted value
                }
            });
        },
        getFormattedValue: (value, locales) => {

            let extractedValue = value.toString();
            let parsedValue = Number.parseFloat(extractedValue);

            if (isNaN(parsedValue))
                parsedValue = 0;

            return new Intl.NumberFormat(locales).format(parsedValue);
        },
        getFormattedValueWithCurrencySymbol: (value, locales, currencySymbol) => {

            let extractedValue = value.toString();
            let parsedValue = Number.parseFloat(extractedValue);

            if (isNaN(parsedValue))
                parsedValue = 0;

            return new Intl.NumberFormat(locales, {
                style: 'currency',
                currency: currencySymbol
            }).format(parsedValue);
        }
    },
    offcanvas: {
        initialize: (elementId, useBackdrop, closeOnEscape, isScrollable, dotNetHelper) => {
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
            bootstrap?.Offcanvas?.getOrCreateInstance(offcanvasEl, options);
        },
        show: (elementId) => {
            bootstrap?.Offcanvas?.getOrCreateInstance(document.getElementById(elementId))?.show();
        },
        hide: (elementId) => {
            bootstrap?.Offcanvas?.getOrCreateInstance(document.getElementById(elementId))?.hide();
        },
        dispose: (elementId) => {
            bootstrap?.Offcanvas?.getOrCreateInstance(document.getElementById(elementId))?.dispose();
        }
    },
    tabs: {
        initialize: (elementId, dotNetHelper) => {
            let navTabsEl = document.getElementById(elementId);
            let triggerTabList = [].slice.call(navTabsEl.querySelectorAll('button.nav-link'));

            triggerTabList.forEach(function (tabEl) {
                tabEl?.addEventListener('show.bs.tab', (event) => {
                    // event.target --> active tab
                    // event.relatedTarget --> previous active tab (if available)
                    dotNetHelper.invokeMethodAsync('bsShowTab', event.target?.id, event.relatedTarget?.id);
                });
                tabEl?.addEventListener('shown.bs.tab', (event) => {
                    // event.target --> active tab
                    // event.relatedTarget --> previous active tab
                    dotNetHelper.invokeMethodAsync('bsShownTab', event.target?.id, event.relatedTarget?.id);
                });
                tabEl?.addEventListener('hide.bs.tab', (event) => {
                    // event.target --> current active tab
                    // event.relatedTarget --> new soon-to-be-active tab
                    dotNetHelper.invokeMethodAsync('bsHideTab', event.relatedTarget?.id, event.target?.id);
                });
                tabEl?.addEventListener('hidden.bs.tab', (event) => {
                    // event.target --> previous active tab
                    // event.relatedTarget --> new active tab
                    dotNetHelper.invokeMethodAsync('bsHiddenTab', event.relatedTarget?.id, event.target?.id);
                });
            });
        },
        show: (elementId) => {
            bootstrap?.Tab?.getOrCreateInstance(document.getElementById(elementId))?.show();
        },
        dispose: (elementId) => {
            bootstrap?.Tab?.getOrCreateInstance(document.getElementById(elementId))?.dispose();
        }
    },
    toasts: {
        show: (elementId, autohide, delay, dotNetHelper) => {
            let toastEl = document.getElementById(elementId);

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

            let options = { animation: true, autohide: autohide, delay: delay };
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
    // global function
    invokeMethodAsync: (callbackEventName, dotNetHelper) => {
        dotNetHelper.invokeMethodAsync(callbackEventName);
    }
}

window.blazorChart = {
    create: (elementId, type, data, options) => {
        let chartEl = document.getElementById(elementId);

        //console.log(elementId);
        //console.log(type);
        //console.log(data);
        //console.log(options); // NOTE: this gives more details in the chrome dev tools

        const config = {
            type: type,
            data: data,
            options: options,
            //plugins: [ChartDataLabels]
        };

        const chart = new Chart(
            chartEl,
            config
        );
    },
    get: (elementId) => {
        let chart;
        Chart.helpers.each(Chart.instances, function (instance) {
            if (instance.canvas.id === elementId) {
                chart = instance;
            }
        });

        return chart;
    },
    initialize: (elementId, type, data, options) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) return;
        else
            window.blazorChart.create(elementId, type, data, options);
    },
    resize: (elementId, width, height) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            chart.canvas.parentNode.style.height = `${width}px`;
            chart.canvas.parentNode.style.width = `${height}px`;
        }
    },
    update: (elementId, type, data, options) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            chart.data = data;
            chart.options = options;
            chart.update();
        } else {
            window.blazorChart.create(elementId, type, data, options);
        }
    },
}

window.blazorChart.bar = {
    create: (elementId, type, data, options) => {
        let chartEl = document.getElementById(elementId);

        const config = {
            type: type,
            data: data,
            options: options
        };

        const chart = new Chart(
            chartEl,
            config
        );
    },
    get: (elementId) => {
        let chart;
        Chart.helpers.each(Chart.instances, function (instance) {
            if (instance.canvas.id === elementId) {
                chart = instance;
            }
        });

        return chart;
    },
    initialize: (elementId, type, data, options) => {
        let chart = window.blazorChart.bar.get(elementId);
        if (chart) return;
        else
            window.blazorChart.bar.create(elementId, type, data, options);
    },
    resize: (elementId, width, height) => {
        let chart = window.blazorChart.bar.get(elementId);
        if (chart) {
            chart.canvas.parentNode.style.height = `${width}px`;
            chart.canvas.parentNode.style.width = `${height}px`;
        }
    },
    update: (elementId, type, data, options) => {
        let chart = window.blazorChart.bar.get(elementId);
        if (chart) {
            chart.data = data;
            chart.options = options;
            chart.update();
        } else {
            window.blazorChart.bar.create(elementId, type, data, options);
        }
    },
}

window.blazorChart.doughnut = {
    create: (elementId, type, data, options) => {
        let chartEl = document.getElementById(elementId);

        const config = {
            type: type,
            data: data,
            options: options
        };

        const chart = new Chart(
            chartEl,
            config
        );
    },
    get: (elementId) => {
        let chart;
        Chart.helpers.each(Chart.instances, function (instance) {
            if (instance.canvas.id === elementId) {
                chart = instance;
            }
        });

        return chart;
    },
    initialize: (elementId, type, data, options) => {
        let chart = window.blazorChart.doughnut.get(elementId);
        if (chart) return;
        else
            window.blazorChart.doughnut.create(elementId, type, data, options);
    },
    resize: (elementId, width, height) => {
        let chart = window.blazorChart.doughnut.get(elementId);
        if (chart) {
            chart.canvas.parentNode.style.height = `${width}px`;
            chart.canvas.parentNode.style.width = `${height}px`;
        }
    },
    update: (elementId, type, data, options) => {
        let chart = window.blazorChart.doughnut.get(elementId);
        if (chart) {
            chart.data = data;
            chart.options = options;
            chart.update();
        } else {
            window.blazorChart.doughnut.create(elementId, type, data, options);
        }
    },
}

window.blazorChart.line = {
    create: (elementId, type, data, options) => {
        let chartEl = document.getElementById(elementId);

        const config = {
            type: type,
            data: data,
            options: options,
        };

        if (type === 'line') {
            // tooltipLine block
            const tooltipLine = {
                id: 'tooltipLine',
                beforeDraw: chart => {
                    if (chart.tooltip._active && chart.tooltip._active.length) {
                        const ctx = chart.ctx;
                        ctx.save();
                        const activePoint = chart.tooltip._active[0];

                        ctx.beginPath();
                        ctx.setLineDash([5, 5]);
                        ctx.moveTo(activePoint.element.x, chart.chartArea.top);
                        ctx.lineTo(activePoint.element.x, activePoint.element.y);
                        ctx.linewidth = 2;
                        ctx.strokeStyle = 'grey';
                        ctx.stroke();
                        ctx.restore();

                        ctx.beginPath();
                        ctx.setLineDash([5, 5]);
                        ctx.moveTo(activePoint.element.x, activePoint.element.y);
                        ctx.lineTo(activePoint.element.x, chart.chartArea.bottom);
                        ctx.linewidth = 2;
                        ctx.strokeStyle = 'grey';
                        ctx.stroke();
                        ctx.restore();
                    }
                },
            };

            config.plugins = [tooltipLine];
        }

        const chart = new Chart(
            chartEl,
            config
        );
    },
    get: (elementId) => {
        let chart;
        Chart.helpers.each(Chart.instances, function (instance) {
            if (instance.canvas.id === elementId) {
                chart = instance;
            }
        });

        return chart;
    },
    initialize: (elementId, type, data, options) => {
        let chart = window.blazorChart.line.get(elementId);
        if (chart) return;
        else
            window.blazorChart.line.create(elementId, type, data, options);
    },
    resize: (elementId, width, height) => {
        let chart = window.blazorChart.line.get(elementId);
        if (chart) {
            chart.canvas.parentNode.style.height = `${width}px`;
            chart.canvas.parentNode.style.width = `${height}px`;
        }
    },
    update: (elementId, type, data, options) => {
        let chart = window.blazorChart.line.get(elementId);
        if (chart) {
            chart.data = data;
            chart.options = options;
            chart.update();
        } else {
            window.blazorChart.line.create(elementId, type, data, options);
        }
    },
}

window.blazorChart.pie = {
    create: (elementId, type, data, options) => {
        let chartEl = document.getElementById(elementId);

        const config = {
            type: type,
            data: data,
            options: options
        };

        const chart = new Chart(
            chartEl,
            config
        );
    },
    get: (elementId) => {
        let chart;
        Chart.helpers.each(Chart.instances, function (instance) {
            if (instance.canvas.id === elementId) {
                chart = instance;
            }
        });

        return chart;
    },
    initialize: (elementId, type, data, options) => {
        let chart = window.blazorChart.pie.get(elementId);
        if (chart) return;
        else
            window.blazorChart.pie.create(elementId, type, data, options);
    },
    resize: (elementId, width, height) => {
        let chart = window.blazorChart.pie.get(elementId);
        if (chart) {
            chart.canvas.parentNode.style.height = `${width}px`;
            chart.canvas.parentNode.style.width = `${height}px`;
        }
    },
    update: (elementId, type, data, options) => {
        let chart = window.blazorChart.pie.get(elementId);
        if (chart) {
            chart.data = data;
            chart.options = options;
            chart.update();
        } else {
            window.blazorChart.pie.create(elementId, type, data, options);
        }
    },
}