if (!window.blazorBootstrap) {
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
    de: {
        DELETE: 46,
        BACKSPACE: 8,
        TAB: 9,
        ENTER: 13,
        ESCAPE: 27,
        ARROWLEFT: 37,
        ARROWUP: 38,
        ARROWRIGHT: 39,
        ARROWDOWN: 40,
        SPACE: 32,
        END: 35,
        HOME: 36,
        PAGE_UP: 33,
        PAGE_DOWN: 34,
        PAGEUP: 33,
        PAGEDOWN: 34
    },
    fe: {
        DELETE: "Delete",
        BACKSPACE: "Backspace",
        TAB: "Tab",
        ENTER: "Enter",
        ESC: "Escape",
        ARROWLEFT: "ArrowLeft",
        ARROWUP: "ArrowUp",
        ARROWRIGHT: "ArrowRight",
        ARROWDOWN: "ArrowDown",
        SPACE: "Space",
        END: "End",
        HOME: "Home",
        PAGE_UP: "PageUp",
        PAGE_DOWN: "PageDown"
    },
    ge: {
        CTRL: "CTRL",
        ALT: "ALT",
        SHIFT: "SHIFT"
    },
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
        },
        focusListItem: (ul, key, startIndex) => {
            if (!ul || startIndex < -1) return;

            let childNodes = ul.getElementsByTagName('LI');

            if (!childNodes || childNodes.length === 0) return;

            if (startIndex === undefined || startIndex === null)
                startIndex = -1;

            let nextSelectedIndex = startIndex;

            if (key === window.blazorBootstrap.fe.ARROWDOWN) {
                if (nextSelectedIndex < childNodes.length - 1)
                    nextSelectedIndex++;
            }
            else if (key === window.blazorBootstrap.fe.ARROWUP) {
                if (nextSelectedIndex > 0 && nextSelectedIndex <= childNodes.length - 1)
                    nextSelectedIndex--;
            }
            else if (key === window.blazorBootstrap.fe.HOME) {
                nextSelectedIndex = 0;
            }
            else if (key === window.blazorBootstrap.fe.END) {
                nextSelectedIndex = childNodes.length - 1;
            }
            else
                return;

            // reset li element focus
            let highlighted = ul.querySelectorAll('.dropdown-item-highlight');
            if (highlighted.length) {
                for (let i = 0; i < highlighted.length; i++) {
                    highlighted[i].classList.remove('dropdown-item-highlight');
                }
            }

            // focus on the next li element
            if (nextSelectedIndex >= 0 && nextSelectedIndex <= childNodes.length - 1) {
                childNodes[nextSelectedIndex].classList.add('dropdown-item-highlight');
                ul.scrollTop = childNodes[nextSelectedIndex].offsetTop;
            }

            return nextSelectedIndex;
        }
    },
    collapse: {
        initialize: (elementId, parent, toggle, dotNetHelper) => {
            let collapseEl = document.getElementById(elementId);

            collapseEl.addEventListener('show.bs.collapse', (event) => {
                dotNetHelper.invokeMethodAsync('bsShowCollapse');
            });
            collapseEl.addEventListener('shown.bs.collapse', (event) => {
                dotNetHelper.invokeMethodAsync('bsShownCollapse');
            });
            collapseEl.addEventListener('hide.bs.collapse', (event) => {
                dotNetHelper.invokeMethodAsync('bsHideCollapse');
            });
            collapseEl.addEventListener('hidden.bs.collapse', (event) => {
                dotNetHelper.invokeMethodAsync('bsHiddenCollapse');
            });

            let options = { parent: parent, toggle: toggle };
            bootstrap?.Collapse?.getOrCreateInstance(collapseEl, options);
        },
        show: (elementId) => {
            bootstrap?.Collapse?.getOrCreateInstance(document.getElementById(elementId))?.show();
        },
        hide: (elementId) => {
            bootstrap?.Collapse?.getOrCreateInstance(document.getElementById(elementId))?.hide();
        },
        toggle: (elementId) => {
            bootstrap?.Collapse?.getOrCreateInstance(document.getElementById(elementId))?.toggle();
        },
        dispose: (elementId) => {
            bootstrap?.Collapse?.getOrCreateInstance(document.getElementById(elementId))?.dispose();
        }
    },
    confirmDialog: {
        show: (elementId) => {
            let confirmDialogEl = document.getElementById(elementId);
            if (confirmDialogEl)
                setTimeout(() => confirmDialogEl.classList.add('show'), 90); // added delay for server

            let bodyEl = document.getElementsByTagName('body');
            if (bodyEl.length > 0)
                bodyEl[0].style['overflow'] = 'hidden';
        },
        hide: (elementId) => {
            let confirmDialogEl = document.getElementById(elementId);
            if (confirmDialogEl)
                confirmDialogEl.classList.remove('show');

            let bodyEl = document.getElementsByTagName('body');
            if (bodyEl.length > 0)
                bodyEl[0].style['overflow'] = 'auto';
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
                        if (blazorBootstrap.hasInvalidChars(event.data, validChars))
                            event.preventDefault();
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
        getFormattedValue: (value, locales, options) => {

            let extractedValue = value.toString();
            let parsedValue = Number.parseFloat(extractedValue);

            if (isNaN(parsedValue))
                parsedValue = 0;

            return new Intl.NumberFormat(locales, options).format(parsedValue);
        }
    },
    dateInput: {
        getFormattedValue: (value) => {
            let extractedValue = value.toString();
            if (extractedValue.length === 0)
                return '';

            let isValid = !isNaN(Date.parse(extractedValue));
            if (!isValid)
                return '';

            let _date = new Date(extractedValue);
            return _date.toLocaleDateString();
        },
        setValue: (elementId, value) => {
            document.getElementById(elementId).value = value;
        }
    },
    dropdown: {
        dispose: (elementId) => {
            bootstrap?.Dropdown?.getOrCreateInstance(document.getElementById(elementId))?.dispose();
        },
        hide: (elementId) => {
            bootstrap?.Dropdown?.getOrCreateInstance(document.getElementById(elementId))?.hide();
        },
        initialize: (elementId, dotNetHelper) => {
            let dropdownEl = document.getElementById(elementId);

            dropdownEl.addEventListener('hide.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsHideDropdown');
            });
            dropdownEl.addEventListener('hidden.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsHiddenDropdown');
            });
            dropdownEl.addEventListener('show.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsShowDropdown');
            });
            dropdownEl.addEventListener('shown.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsShownDropdown');
            });

            bootstrap?.Dropdown?.getOrCreateInstance(dropdownEl);
        },
        show: (elementId) => {
            bootstrap?.Dropdown?.getOrCreateInstance(document.getElementById(elementId))?.show();
        },
        toggle: (elementId) => {
            bootstrap?.Dropdown?.getOrCreateInstance(document.getElementById(elementId))?.toggle();
        },
        update: (elementId) => {
            bootstrap?.Dropdown?.getOrCreateInstance(document.getElementById(elementId))?.update();
        }
    },
    grid: {
        checkOrUnCheckAll: (cssSelector, isChecked) => {
            let chkEls = document.querySelectorAll(cssSelector);
            if (chkEls.length === 0)
                return;

            chkEls.forEach((ele, index) => {
                ele.checked = isChecked;
            });
        },
        scroll: (elementId) => {
            let tableEl = document.getElementById(elementId);
            if (tableEl === null)
                return;

            if (tableEl.parentElement.scrollLeft === 0) {
                let colEls = tableEl.querySelectorAll('.freeze-column');
                if (colEls.length === 0)
                    return;

                colEls.forEach((e, i) => {
                    e.classList.remove('freeze-column-active');
                });
            }
            else if (tableEl.parentElement.scrollLeft > 0) {
                let colEls = tableEl.querySelectorAll('.freeze-column');
                if (colEls.length === 0)
                    return;

                colEls.forEach((e, i) => {
                    e.classList.add('freeze-column-active');
                });
            }
        },
        setSelectAllCheckboxState: (elementId, state) => {
            let checkboxEl = document.getElementById(elementId);
            if (checkboxEl) {
                if (state === 1) { // checked 
                    checkboxEl.checked = true;
                    checkboxEl.indeterminate = false;
                }
                else if (state === 2) { // unchecked
                    checkboxEl.checked = false;
                    checkboxEl.indeterminate = false;
                }
                else if (state === 3) { // indeterminate 
                    checkboxEl.checked = false;
                    checkboxEl.indeterminate = true;
                }
            }
        },
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
    offcanvas: {
        initialize: (elementId, useStaticBackdrop, closeOnEscape, isScrollable, dotNetHelper) => {
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

            let options = { backdrop: useStaticBackdrop ? 'static' : true, keyboard: closeOnEscape, scroll: isScrollable };
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
    sidebar: {
        initialize: (elementId, dotNetHelper) => {
            window.addEventListener("resize", () => {
                dotNetHelper.invokeMethodAsync('bsWindowResize', window.innerWidth);
            });
        },
        windowSize: () => window.innerWidth
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
    timeInput: {
        setValue: (elementId, value) => {
            document.getElementById(elementId).value = value;
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
        initialize: (elementRef) => {
            bootstrap?.Tooltip?.getOrCreateInstance(elementRef);
        },
        update: (elementRef) => {
            bootstrap?.Tooltip?.getOrCreateInstance(elementRef)?.update();
        },
        dispose: (elementRef) => {
            bootstrap?.Tooltip?.getOrCreateInstance(elementRef)?.dispose();
        }
    },
    // global function
    invokeMethodAsync: (callbackEventName, dotNetHelper) => {
        dotNetHelper.invokeMethodAsync(callbackEventName);
    },
    hasInvalidChars: (input, validChars) => {
        if (input.length <= 0 || validChars.length <= 0)
            return false;

        let inputCharArr = input.split('');
        for (let i = 0; i < inputCharArr.length; i++) {
            if (!validChars.includes(inputCharArr[i]))
                return true;
        }

        return false;
    },
}

window.blazorChart = {
    create: (elementId, type, data, options, plugins) => {
        let chartEl = document.getElementById(elementId);
        let _plugins = [];

        if (plugins && plugins.length > 0) {
            // register `ChartDataLabels` plugin
            if (plugins.includes('ChartDataLabels')) {
                _plugins.push(ChartDataLabels);
            }
        }

        const config = {
            type: type,
            data: data,
            options: options,
            plugins: _plugins
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
    initialize: (elementId, type, data, options, plugins) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) return;
        else
            window.blazorChart.create(elementId, type, data, options, plugins);
    },
    resize: (elementId, width, height) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            chart.canvas.parentNode.style.height = height;
            chart.canvas.parentNode.style.width = width;
        }
    },
    update: (elementId, type, data, options) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            if (chart.config.plugins && chart.config.plugins.findIndex(x => x.id == 'datalabels') > -1) {
                // set datalabel background color
                options.plugins.datalabels.backgroundColor = function (context) {
                    return context.dataset.backgroundColor;
                };
            }

            chart.data = data;
            chart.options = options;
            chart.update();
        }
        else {
            console.warn(`The chart is not initialized. Initialize it and then call update.`);
        }
    },
}

window.blazorChart.bar = {
    addDatasetData: (elementId, dataLabel, data) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            const chartData = chart.data;
            const chartDatasetData = data;

            if (!chartData.labels.includes(dataLabel))
                chartData.labels.push(dataLabel);

            const chartDatasets = chartData.datasets;

            if (chartDatasets.length > 0) {
                let datasetIndex = chartDatasets.findIndex(dataset => dataset.label === chartDatasetData.datasetLabel);
                if (datasetIndex > -1) {
                    chartDatasets[datasetIndex].data.push(chartDatasetData.data);
                    chart.update();
                }
            }
        }
    },
    addDatasetsData: (elementId, dataLabel, data) => {
        let chart = window.blazorChart.get(elementId);
        if (chart && data) {
            const chartData = chart.data;

            if (!chartData.labels.includes(dataLabel)) {
                chartData.labels.push(dataLabel);

                if (chartData.datasets.length > 0 && chartData.datasets.length === data.length) {
                    data.forEach(chartDatasetData => {
                        let datasetIndex = chartData.datasets.findIndex(dataset => dataset.label === chartDatasetData.datasetLabel);
                        chartData.datasets[datasetIndex].data.push(chartDatasetData.data);
                    });
                    chart.update();
                }
            }
        }
    },
    addDataset: (elementId, newDataset) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            chart.data.datasets.push(newDataset);
            chart.update();
        }
    },
    create: (elementId, type, data, options, plugins) => {
        let chartEl = document.getElementById(elementId);
        let _plugins = [];

        if (plugins && plugins.length > 0) {
            // register `ChartDataLabels` plugin
            if (plugins.includes('ChartDataLabels')) {
                _plugins.push(ChartDataLabels);
            }
        }

        const config = {
            type: type,
            data: data,
            options: options,
            plugins: _plugins
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
    initialize: (elementId, type, data, options, plugins) => {
        let chart = window.blazorChart.bar.get(elementId);
        if (chart) return;
        else
            window.blazorChart.bar.create(elementId, type, data, options, plugins);
    },
    resize: (elementId, width, height) => {
        let chart = window.blazorChart.bar.get(elementId);
        if (chart) {
            chart.canvas.parentNode.style.height = height;
            chart.canvas.parentNode.style.width = width;
        }
    },
    update: (elementId, type, data, options) => {
        let chart = window.blazorChart.bar.get(elementId);
        if (chart) {
            if (chart.config.plugins && chart.config.plugins.findIndex(x => x.id == 'datalabels') > -1) {
                // set datalabel background color
                options.plugins.datalabels.backgroundColor = function (context) {
                    return context.dataset.backgroundColor;
                };
            }

            chart.data = data;
            chart.options = options;
            chart.update();
        }
        else {
            console.warn(`The chart is not initialized. Initialize it and then call update.`);
        }
    },
}

window.blazorChart.doughnut = {
    addDatasetData: (elementId, dataLabel, data) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            const chartData = chart.data;
            const chartDatasetData = data;

            if (!chartData.labels.includes(dataLabel))
                chartData.labels.push(dataLabel);

            const chartDatasets = chartData.datasets;

            if (chartDatasets.length > 0) {
                let datasetIndex = chartDatasets.findIndex(dataset => dataset.label === chartDatasetData.datasetLabel);
                if (datasetIndex > -1) {
                    chartDatasets[datasetIndex].data.push(chartDatasetData.data);
                    chart.update();
                }
            }
        }
    },
    addDatasetsData: (elementId, dataLabel, data) => {
        let chart = window.blazorChart.get(elementId);
        if (chart && data) {
            const chartData = chart.data;

            if (!chartData.labels.includes(dataLabel)) {
                chartData.labels.push(dataLabel);

                if (chartData.datasets.length > 0 && chartData.datasets.length === data.length) {
                    data.forEach(chartDatasetData => {
                        let datasetIndex = chartData.datasets.findIndex(dataset => dataset.label === chartDatasetData.datasetLabel);
                        chartData.datasets[datasetIndex].data.push(chartDatasetData.data);
                        chartData.datasets[datasetIndex].backgroundColor.push(chartDatasetData.backgroundColor);
                    });
                    chart.update();
                }
            }
        }
    },
    addDataset: (elementId, newDataset) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            chart.data.datasets.push(newDataset);
            chart.update();
        }
    },
    create: (elementId, type, data, options, plugins) => {
        let chartEl = document.getElementById(elementId);
        let _plugins = [];

        if (plugins && plugins.length > 0) {
            // register `ChartDataLabels` plugin
            if (plugins.includes('ChartDataLabels')) {
                _plugins.push(ChartDataLabels);

                // set datalabel background color
                options.plugins.datalabels.backgroundColor = function (context) {
                    return context.dataset.backgroundColor;
                };
            }
        }

        const config = {
            type: type,
            data: data,
            options: options,
            plugins: _plugins
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
    initialize: (elementId, type, data, options, plugins) => {
        let chart = window.blazorChart.doughnut.get(elementId);
        if (chart) return;
        else
            window.blazorChart.doughnut.create(elementId, type, data, options, plugins);
    },
    resize: (elementId, width, height) => {
        let chart = window.blazorChart.doughnut.get(elementId);
        if (chart) {
            chart.canvas.parentNode.style.height = height;
            chart.canvas.parentNode.style.width = width;
        }
    },
    update: (elementId, type, data, options) => {
        let chart = window.blazorChart.doughnut.get(elementId);
        if (chart) {
            if (chart.config.plugins && chart.config.plugins.findIndex(x => x.id == 'datalabels') > -1) {
                // set datalabel background color
                options.plugins.datalabels.backgroundColor = function (context) {
                    return context.dataset.backgroundColor;
                };
            }

            chart.data = data;
            chart.options = options;
            chart.update();
        }
        else {
            console.warn(`The chart is not initialized. Initialize it and then call update.`);
        }
    },
}

window.blazorChart.line = {
    addDatasetData: (elementId, dataLabel, data) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            const chartData = chart.data;
            const chartDatasetData = data;

            if (!chartData.labels.includes(dataLabel))
                chartData.labels.push(dataLabel);

            const chartDatasets = chartData.datasets;

            if (chartDatasets.length > 0) {
                let datasetIndex = chartDatasets.findIndex(dataset => dataset.label === chartDatasetData.datasetLabel);
                if (datasetIndex > -1) {
                    chartDatasets[datasetIndex].data.push(chartDatasetData.data);
                    chart.update();
                }
            }
        }
    },
    addDatasetsData: (elementId, dataLabel, data) => {
        let chart = window.blazorChart.get(elementId);
        if (chart && data) {
            const chartData = chart.data;

            if (!chartData.labels.includes(dataLabel)) {
                chartData.labels.push(dataLabel);

                if (chartData.datasets.length > 0 && chartData.datasets.length === data.length) {
                    data.forEach(chartDatasetData => {
                        let datasetIndex = chartData.datasets.findIndex(dataset => dataset.label === chartDatasetData.datasetLabel);
                        chartData.datasets[datasetIndex].data.push(chartDatasetData.data);
                    });
                    chart.update();
                }
            }
        }
    },
    addDataset: (elementId, newDataset) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            chart.data.datasets.push(newDataset);
            chart.update();
        }
    },
    create: (elementId, type, data, options, plugins) => {
        let chartEl = document.getElementById(elementId);
        let _plugins = [];

        if (plugins && plugins.length > 0) {
            // register `ChartDataLabels` plugin
            if (plugins.includes('ChartDataLabels')) {
                _plugins.push(ChartDataLabels);

                // set datalabel background color
                options.plugins.datalabels.backgroundColor = function (context) {
                    return context.dataset.backgroundColor;
                };
            }
        }

        const config = {
            type: type,
            data: data,
            options: options,
            plugins: _plugins
        };

        if (type === 'line') {
            // tooltipLine block
            const tooltipLine = {
                id: 'tooltipLine',
                beforeDraw: chart => {
                    if (chart.tooltip?._active && chart.tooltip?._active.length) {
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

            config.plugins.push(tooltipLine);
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
    initialize: (elementId, type, data, options, plugins) => {
        let chart = window.blazorChart.line.get(elementId);
        if (chart)
            return;
        else
            window.blazorChart.line.create(elementId, type, data, options, plugins);
    },
    resize: (elementId, width, height) => {
        let chart = window.blazorChart.line.get(elementId);
        if (chart) {
            chart.canvas.parentNode.style.height = height;
            chart.canvas.parentNode.style.width = width;
        }
    },
    update: (elementId, type, data, options) => {
        let chart = window.blazorChart.line.get(elementId);
        if (chart) {
            if (chart.config.plugins && chart.config.plugins.findIndex(x => x.id == 'datalabels') > -1) {
                // set datalabel background color
                options.plugins.datalabels.backgroundColor = function (context) {
                    return context.dataset.backgroundColor;
                };
            }

            chart.data = data;
            chart.options = options;
            chart.update();
        }
        else {
            console.warn(`The chart is not initialized. Initialize it and then call update.`);
        }
    },
}

window.blazorChart.pie = {
    addDatasetData: (elementId, dataLabel, data) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            const chartData = chart.data;
            const chartDatasetData = data;

            if (!chartData.labels.includes(dataLabel))
                chartData.labels.push(dataLabel);

            const chartDatasets = chartData.datasets;

            if (chartDatasets.length > 0) {
                let datasetIndex = chartDatasets.findIndex(dataset => dataset.label === chartDatasetData.datasetLabel);
                if (datasetIndex > -1) {
                    chartDatasets[datasetIndex].data.push(chartDatasetData.data);
                    chart.update();
                }
            }
        }
    },
    addDatasetsData: (elementId, dataLabel, data) => {
        let chart = window.blazorChart.get(elementId);
        if (chart && data) {
            const chartData = chart.data;

            if (!chartData.labels.includes(dataLabel)) {
                chartData.labels.push(dataLabel);

                if (chartData.datasets.length > 0 && chartData.datasets.length === data.length) {
                    data.forEach(chartDatasetData => {
                        let datasetIndex = chartData.datasets.findIndex(dataset => dataset.label === chartDatasetData.datasetLabel);
                        chartData.datasets[datasetIndex].data.push(chartDatasetData.data);
                        chartData.datasets[datasetIndex].backgroundColor.push(chartDatasetData.backgroundColor);
                    });
                    chart.update();
                }
            }
        }
    },
    addDataset: (elementId, newDataset) => {
        let chart = window.blazorChart.get(elementId);
        if (chart) {
            chart.data.datasets.push(newDataset);
            chart.update();
        }
    },
    create: (elementId, type, data, options, plugins) => {
        let chartEl = document.getElementById(elementId);
        let _plugins = [];

        if (plugins && plugins.length > 0) {
            // register `ChartDataLabels` plugin
            if (plugins.includes('ChartDataLabels')) {
                _plugins.push(ChartDataLabels);

                // set datalabel background color
                options.plugins.datalabels.backgroundColor = function (context) {
                    return context.dataset.backgroundColor;
                };
            }
        }

        const config = {
            type: type,
            data: data,
            options: options,
            plugins: _plugins
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
    initialize: (elementId, type, data, options, plugins) => {
        let chart = window.blazorChart.pie.get(elementId);
        if (chart) return;
        else
            window.blazorChart.pie.create(elementId, type, data, options, plugins);
    },
    resize: (elementId, width, height) => {
        let chart = window.blazorChart.pie.get(elementId);
        if (chart) {
            chart.canvas.parentNode.style.height = height;
            chart.canvas.parentNode.style.width = width;
        }
    },
    update: (elementId, type, data, options) => {
        let chart = window.blazorChart.pie.get(elementId);
        if (chart) {
            if (chart.config.plugins && chart.config.plugins.findIndex(x => x.id == 'datalabels') > -1) {
                // set datalabel background color
                options.plugins.datalabels.backgroundColor = function (context) {
                    return context.dataset.backgroundColor;
                };
            }

            chart.data = data;
            chart.options = options;
            chart.update();
        }
        else {
            console.warn(`The chart is not initialized. Initialize it and then call update.`);
        }
    },
}
