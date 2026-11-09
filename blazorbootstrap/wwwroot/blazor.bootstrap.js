if (!window.blazorBootstrap) {
    window.blazorBootstrap = {};
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
            if (alertEl == null)
                return;

            alertEl.addEventListener('close.bs.alert', function () {
                dotNetHelper.invokeMethodAsync('bsCloseAlert');
            });
            alertEl.addEventListener('closed.bs.alert', function () {
                dotNetHelper.invokeMethodAsync('bsClosedAlert');
            });

            bootstrap?.Alert?.getOrCreateInstance(alertEl);
        },
        close: (elementId) => {
            let alertEl = document.getElementById(elementId);
            if (alertEl != null)
                bootstrap?.Alert?.getOrCreateInstance(alertEl)?.close();
        },
        dispose: (elementId) => {
            let alertEl = document.getElementById(elementId);
            if (alertEl != null)
                bootstrap?.Alert?.getOrCreateInstance(alertEl)?.dispose();
        }
    },
    autocomplete: {
        initialize: (elementRef, dotNetHelper) => {
            let dropdownToggleEl = elementRef;
            if (dropdownToggleEl == null)
                return;

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
            if (elementRef != null)
                bootstrap?.Dropdown?.getOrCreateInstance(elementRef)?.show();
        },
        hide: (elementRef) => {
            if (elementRef != null)
                bootstrap?.Dropdown?.getOrCreateInstance(elementRef)?.hide();
        },
        dispose: (elementRef) => {
            if (elementRef != null)
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
            if (collapseEl == null)
                return;

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
            let collapseEl = document.getElementById(elementId);
            if (collapseEl != null)
                bootstrap?.Collapse?.getOrCreateInstance(collapseEl)?.show();
        },
        hide: (elementId) => {
            let collapseEl = document.getElementById(elementId);
            if (collapseEl != null)
                bootstrap?.Collapse?.getOrCreateInstance(collapseEl)?.hide();
        },
        toggle: (elementId) => {
            let collapseEl = document.getElementById(elementId);
            if (collapseEl != null)
                bootstrap?.Collapse?.getOrCreateInstance(collapseEl)?.toggle();
        },
        dispose: (elementId) => {
            let collapseEl = document.getElementById(elementId);
            if (collapseEl != null)
                bootstrap?.Collapse?.getOrCreateInstance(collapseEl)?.dispose();
        }
    },
    confirmDialog: {
        show: (elementId) => {
            let confirmDialogEl = document.getElementById(elementId);
            if (confirmDialogEl != null)
                setTimeout(() => confirmDialogEl.classList.add('show'), 90); // added delay for server

            let bodyEl = document.getElementsByTagName('body');
            if (bodyEl.length > 0)
                bodyEl[0].style['overflow'] = 'hidden';
        },
        hide: (elementId) => {
            let confirmDialogEl = document.getElementById(elementId);
            if (confirmDialogEl != null)
                confirmDialogEl.classList.remove('show');

            let bodyEl = document.getElementsByTagName('body');
            if (bodyEl.length > 0)
                bodyEl[0].style['overflow'] = 'auto';
        }
    },
    carousel: {
        cycle: (elementId) => {
            let carouselEl = document.getElementById(elementId);
            if (carouselEl != null)
                bootstrap?.Carousel?.getOrCreateInstance(carouselEl)?.cycle();
        },
        dispose: (elementId) => {
            let carouselEl = document.getElementById(elementId);
            if (carouselEl != null)
                bootstrap?.Carousel?.getOrCreateInstance(carouselEl)?.dispose();
        },
        initialize: (elementId, options, dotNetHelper) => {
            let carouselEl = document.getElementById(elementId);
            if (carouselEl == null)
                return;

            carouselEl.addEventListener('slid.bs.carousel', function (e) {
                dotNetHelper.invokeMethodAsync('bsSlid', e);
            });
            carouselEl.addEventListener('slide.bs.carousel', function (e) {
                dotNetHelper.invokeMethodAsync('bslide', e);
            });

            bootstrap?.Carousel?.getOrCreateInstance(carouselEl, options);
        },
        next: (elementId) => {
            let carouselEl = document.getElementById(elementId);
            if (carouselEl != null)
                bootstrap?.Carousel?.getOrCreateInstance(carouselEl)?.next();
        },
        nextWhenVisible: (elementId) => {
            let carouselEl = document.getElementById(elementId);
            if (carouselEl != null)
                bootstrap?.Carousel?.getOrCreateInstance(carouselEl)?.nextWhenVisible();
        },
        pause: (elementId) => {
            let carouselEl = document.getElementById(elementId);
            if (carouselEl != null)
                bootstrap?.Carousel?.getOrCreateInstance(carouselEl)?.pause();
        },
        prev: (elementId) => {
            let carouselEl = document.getElementById(elementId);
            if (carouselEl != null)
                bootstrap?.Carousel?.getOrCreateInstance(carouselEl)?.prev();
        },
        to: (elementId, index) => {
            let carouselEl = document.getElementById(elementId);
            if (carouselEl != null)
                bootstrap?.Carousel?.getOrCreateInstance(carouselEl)?.to(index);
        },
    },
    currencyInput: {
        initialize: (elementId, isFloat, allowNegativeNumbers, decimalSeperator) => {
            let currencyEl = document.getElementById(elementId);
            const modifierShortcutKeys = ['a', 'x', 'c', 'v'];

            currencyEl?.addEventListener('keydown', function (event) {

                switch (event.code) {
                    case "Backspace":
                    case "Tab":
                    case "Enter":
                    case "ArrowLeft":
                    case "ArrowUp":
                    case "ArrowRight":
                    case "ArrowDown":
                    case "Delete":
                        return;
                }

                // allow select-all, cut, copy, and paste keys with Ctrl (Windows/Linux) or Cmd (macOS)
                const isModifierPressed = event.ctrlKey || event.metaKey;
                const normalizedKey = event.key.toLowerCase();
                if (isModifierPressed && modifierShortcutKeys.includes(normalizedKey)) {
                    return;
                }

                let validChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

                if (isFloat) {
                    validChars.push(decimalSeperator);
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
                        validChars.push(decimalSeperator);
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
            let dropdownEl = document.getElementById(elementId);
            if (dropdownEl != null) {
                let toggleEl = dropdownEl.querySelector('[data-bs-toggle="dropdown"]') ?? dropdownEl;
                bootstrap?.Dropdown?.getOrCreateInstance(toggleEl)?.dispose();
            }
        },
        hide: (elementId) => {
            let dropdownEl = document.getElementById(elementId);
            if (dropdownEl != null) {
                let toggleEl = dropdownEl.querySelector('[data-bs-toggle="dropdown"]') ?? dropdownEl;
                bootstrap?.Dropdown?.getOrCreateInstance(toggleEl)?.hide();
            }
        },
        initialize: (elementId, dotNetHelper) => {
            let dropdownEl = document.getElementById(elementId);
            if (dropdownEl == null)
                return;

            let toggleEl = dropdownEl.querySelector('[data-bs-toggle="dropdown"]') ?? dropdownEl;

            toggleEl.addEventListener('hide.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsHideDropdown');
            });
            toggleEl.addEventListener('hidden.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsHiddenDropdown');
            });
            toggleEl.addEventListener('show.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsShowDropdown');
            });
            toggleEl.addEventListener('shown.bs.dropdown', function () {
                dotNetHelper.invokeMethodAsync('bsShownDropdown');
            });

            bootstrap?.Dropdown?.getOrCreateInstance(toggleEl);
        },
        show: (elementId) => {
            let dropdownEl = document.getElementById(elementId);
            if (dropdownEl != null) {
                let toggleEl = dropdownEl.querySelector('[data-bs-toggle="dropdown"]') ?? dropdownEl;
                bootstrap?.Dropdown?.getOrCreateInstance(toggleEl)?.show();
            }
        },
        toggle: (elementId) => {
            let dropdownEl = document.getElementById(elementId);
            if (dropdownEl != null) {
                let toggleEl = dropdownEl.querySelector('[data-bs-toggle="dropdown"]') ?? dropdownEl;
                bootstrap?.Dropdown?.getOrCreateInstance(toggleEl)?.toggle();
            }
        },
        update: (elementId) => {
            let dropdownEl = document.getElementById(elementId);
            if (dropdownEl != null) {
                let toggleEl = dropdownEl.querySelector('[data-bs-toggle="dropdown"]') ?? dropdownEl;
                bootstrap?.Dropdown?.getOrCreateInstance(toggleEl)?.update();
            }
        }
    },
    googlemaps: {
        apiLoaded: false,
        addMarker: (elementId, marker, dotNetHelper) => {
            let mapInstance = window.blazorBootstrap.googlemaps.get(elementId);
            if (mapInstance) {
                let map = mapInstance.map;
                let clickable = mapInstance.clickable;
                let _content;

                if (marker.pinElement) {
                    if (marker.pinElement.useIconFonts) {
                        const content = document.createElement("div");
                        const icon = document.createElement("i");
                        const scale = marker.pinElement.scale ?? 1;

                        content.classList.add("bb-google-marker-content");
                        content.style.alignItems = "center";
                        content.style.backgroundColor = marker.pinElement.background;
                        content.style.border = `2px solid ${marker.pinElement.borderColor ?? "transparent"}`;
                        content.style.borderRadius = "50%";
                        content.style.boxSizing = "border-box";
                        content.style.color = marker.pinElement.glyphColor;
                        content.style.display = "flex";
                        content.style.height = `${48 * scale}px`;
                        content.style.justifyContent = "center";
                        content.style.width = `${48 * scale}px`;

                        icon.className = marker.pinElement.glyph ?? "";
                        content.append(icon);
                        _content = content;
                    } else {
                        const pinOptions = {
                            background: marker.pinElement.background,
                            borderColor: marker.pinElement.borderColor,
                            glyphColor: marker.pinElement.glyphColor,
                            scale: marker.pinElement.scale,
                        };

                        if (marker.pinElement.glyphSrc) {
                            pinOptions.glyphSrc = marker.pinElement.glyphSrc;
                        } else {
                            pinOptions.glyphText = marker.pinElement.glyphText ?? (typeof marker.pinElement.glyph === "string" ? marker.pinElement.glyph : undefined);
                        }

                        _content = new google.maps.marker.PinElement(pinOptions);
                    }
                }
                else if (marker.content) {
                    _content = document.createElement("div");
                    _content.classList.add("bb-google-marker-content");
                    _content.innerHTML = marker.content;
                }

                const markerEl = new google.maps.marker.AdvancedMarkerElement({
                    map,
                    position: marker.position,
                    title: marker.title,
                    gmpClickable: clickable
                });

                if (_content)
                    markerEl.append(_content);

                window.blazorBootstrap.googlemaps.markerEls[elementId].push(markerEl);

                // add a click listener for each marker, and set up the info window.
                if (clickable) {
                    markerEl.addEventListener("gmp-click", () => {
                        const infoWindow = new google.maps.InfoWindow();
                        infoWindow.close();
                        infoWindow.setContent(markerEl.title);
                        infoWindow.open(markerEl.map, markerEl);
                        dotNetHelper.invokeMethodAsync('OnMarkerClickJS', marker);
                    });
                }
            }
        },
        create: (elementId, map, zoom, center, markers, clickable) => {
            window.blazorBootstrap.googlemaps.instances[elementId] = {
                map: map,
                zoom: zoom,
                center: center,
                markers: markers,
                clickable: clickable
            };
        },
        get: (elementId) => {
            return window.blazorBootstrap.googlemaps.instances[elementId];
        },
        initialize: (elementId, zoom, center, markers, clickable, dotNetHelper) => {
            if (!window.blazorBootstrap.googlemaps.apiLoaded) {
                window.blazorBootstrap.googlemaps.pendingInitializations[elementId] = { center, clickable, dotNetHelper, markers, zoom };
                return;
            }

            const element = document.getElementById(elementId);
            if (!element)
                return;

            window.blazorBootstrap.googlemaps.markerEls[elementId] = window.blazorBootstrap.googlemaps.markerEls[elementId] ?? [];

            let mapOptions = { center: center, zoom: zoom, mapId: elementId };
            let map = new google.maps.Map(element, mapOptions);

            window.blazorBootstrap.googlemaps.create(elementId, map, zoom, center, markers, clickable);

            if (markers) {
                for (const marker of markers) {
                    window.blazorBootstrap.googlemaps.addMarker(elementId, marker, dotNetHelper);
                }
            }
        },
        instances: {},
        markerEls: {},
        onApiLoaded: () => {
            window.blazorBootstrap.googlemaps.apiLoaded = true;

            const pendingInitializations = Object.entries(window.blazorBootstrap.googlemaps.pendingInitializations);
            window.blazorBootstrap.googlemaps.pendingInitializations = {};

            for (const [elementId, { center, clickable, dotNetHelper, markers, zoom }] of pendingInitializations) {
                window.blazorBootstrap.googlemaps.initialize(elementId, zoom, center, markers, clickable, dotNetHelper);
            }
        },
        pendingInitializations: {},
        updateMarkers: (elementId, markers, dotNetHelper) => {
            let markerEls = window.blazorBootstrap.googlemaps.markerEls[elementId] ?? [];

            // delete the markers
            if (markerEls.length > 0) {
                for (const markerEl of markerEls) {
                    markerEl.setMap(null);
                }
            }

            if (markers) {
                for (const marker of markers) {
                    window.blazorBootstrap.googlemaps.addMarker(elementId, marker, dotNetHelper);
                }
            }
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
            if (checkboxEl != null) {
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
    listGroup: {
        // Element references by list id. Blazor removes the elements before dispose runs on the server, so dispose cannot look them up.
        instances: {},
        dispose: (elementId) => {
            let state = window.blazorBootstrap.listGroup.instances[elementId];
            if (state == null)
                return;

            delete window.blazorBootstrap.listGroup.instances[elementId];

            state.listeners.forEach(([target, type, handler]) => target.removeEventListener(type, handler));
            state.tabEls.forEach((tabEl) => bootstrap?.Tab?.getInstance(tabEl)?.dispose());
        },
        initializeListbox: (elementId) => {
            window.blazorBootstrap.listGroup.dispose(elementId); // also called again when the list element was replaced

            let listEl = document.getElementById(elementId);
            if (listEl == null)
                return;

            // Arrow keys, Home, and End move focus between the enabled options and keep a single tab stop.
            let handler = (event) => {
                let options = [...listEl.querySelectorAll(':scope > [role="option"]:not(:disabled)')];
                let index = options.indexOf(document.activeElement);
                if (index < 0)
                    return;

                let next = null;
                switch (event.key) {
                    case 'ArrowDown':
                    case 'ArrowRight':
                        next = options[Math.min(index + 1, options.length - 1)];
                        break;
                    case 'ArrowUp':
                    case 'ArrowLeft':
                        next = options[Math.max(index - 1, 0)];
                        break;
                    case 'Home':
                        next = options[0];
                        break;
                    case 'End':
                        next = options[options.length - 1];
                        break;
                    default:
                        return;
                }

                event.preventDefault();
                window.blazorBootstrap.listGroup.setTabStop(listEl, next);
                next.focus();
            };

            // An option focused by a click becomes the tab stop too.
            let focusHandler = (event) => {
                if (event.target.parentElement === listEl && event.target.getAttribute('role') === 'option')
                    window.blazorBootstrap.listGroup.setTabStop(listEl, event.target);
            };

            listEl.addEventListener('keydown', handler);
            listEl.addEventListener('focusin', focusHandler);
            window.blazorBootstrap.listGroup.instances[elementId] = { listEl: listEl, listeners: [[listEl, 'keydown', handler], [listEl, 'focusin', focusHandler]], tabEls: [] };
            window.blazorBootstrap.listGroup.syncListbox(elementId);
        },
        initializeTabs: (elementId, dotNetHelper) => {
            window.blazorBootstrap.listGroup.dispose(elementId); // also called again when tabs were added or removed

            let listEl = document.getElementById(elementId);
            if (listEl == null)
                return;

            let state = { listeners: [], tabEls: [...listEl.querySelectorAll('[data-bs-toggle="list"]')] };
            let nameOf = (el) => el?.getAttribute('data-bb-name') ?? null;
            let listen = (type, callback) => {
                let handler = (event) => {
                    if (state.tabEls.includes(event.target))
                        callback(event);
                };
                listEl.addEventListener(type, handler);
                state.listeners.push([listEl, type, handler]);
            };

            // event.target is the tab being shown (or hidden); event.relatedTarget is the other tab.
            // A lost Server connection rejects the .NET calls; the tabs keep working, so the rejection is ignored.
            listen('show.bs.tab', (event) => dotNetHelper.invokeMethodAsync('bsShowTab', nameOf(event.target), nameOf(event.relatedTarget)).catch(() => { }));
            listen('shown.bs.tab', (event) => dotNetHelper.invokeMethodAsync('bsShownTab', nameOf(event.target), nameOf(event.relatedTarget)).catch(() => { }));
            listen('hide.bs.tab', (event) => dotNetHelper.invokeMethodAsync('bsHideTab', nameOf(event.relatedTarget), nameOf(event.target)).catch(() => { }));
            listen('hidden.bs.tab', (event) => dotNetHelper.invokeMethodAsync('bsHiddenTab', nameOf(event.relatedTarget), nameOf(event.target)).catch(() => { }));

            // Bootstrap creates Tab instances for active tabs only on window load; create them so arrow keys work in the tab list.
            state.tabEls.forEach((tabEl) => bootstrap?.Tab?.getOrCreateInstance(tabEl));

            window.blazorBootstrap.listGroup.instances[elementId] = state;

            // The shown pane was removed: show the first enabled tab, so a pane is visible and the tab list keeps a tab stop.
            if (!state.tabEls.some((tabEl) => tabEl.classList.contains('active'))) {
                let firstEnabled = state.tabEls.find((tabEl) => !tabEl.disabled);
                if (firstEnabled != null)
                    bootstrap?.Tab?.getOrCreateInstance(firstEnabled)?.show();
            }
        },
        getPaneOrder: (elementId) => {
            // The pane elements are in markup order; the group orders its tabs to match.
            // Panes may sit inside wrapper elements; panes of a nested tab list belong to its own .tab-content.
            let contentEl = document.getElementById(elementId)?.closest('.row')?.querySelector('.tab-content');
            return contentEl == null
                ? null
                : [...contentEl.querySelectorAll('.tab-pane')].filter((paneEl) => paneEl.parentElement.closest('.tab-content') === contentEl).map((paneEl) => paneEl.id);
        },
        setTabStop: (listEl, optionEl) => {
            // Exactly one option is reachable by Tab (WAI-ARIA listbox); arrow keys reach the rest.
            listEl.querySelectorAll(':scope > [role="option"]').forEach((el) => el.setAttribute('tabindex', el === optionEl ? '0' : '-1'));
        },
        showTab: (tabId) => {
            let tabEl = document.getElementById(tabId);
            if (tabEl != null)
                bootstrap?.Tab?.getOrCreateInstance(tabEl)?.show();
        },
        syncListbox: (elementId) => {
            // Called after each render: the tab stop is the focused option, else the first selected enabled option, else the first enabled one.
            let state = window.blazorBootstrap.listGroup.instances[elementId];
            let listEl = state?.listEl;
            if (listEl == null || !listEl.isConnected)
                return;

            let options = [...listEl.querySelectorAll(':scope > [role="option"]:not(:disabled)')];
            let tabStop = options.find((el) => el === document.activeElement)
                ?? options.find((el) => el.getAttribute('aria-selected') === 'true')
                ?? options[0]
                ?? null;
            window.blazorBootstrap.listGroup.setTabStop(listEl, tabStop);
        }
    },
    mindMap: {
        instances: {},
        dispose: (elementId) => {
            let instance = window.blazorBootstrap.mindMap.instances[elementId];
            if (!instance)
                return;

            instance.tree.removeEventListener('keydown', instance.onKeyDown);
            instance.canvas.removeEventListener('pointerdown', instance.onPointerDown);
            instance.canvas.removeEventListener('pointermove', instance.onPointerMove);
            instance.canvas.removeEventListener('pointerup', instance.onPointerEnd);
            instance.canvas.removeEventListener('pointercancel', instance.onPointerEnd);
            instance.canvas.classList.remove('bb-mind-map-panning');
            delete window.blazorBootstrap.mindMap.instances[elementId];
        },
        initialize: (elementId, allowPan) => {
            let canvas = document.getElementById(elementId + '-canvas');
            let tree = document.getElementById(elementId + '-tree');
            if (canvas == null || tree == null)
                return;

            window.blazorBootstrap.mindMap.dispose(elementId);

            // Keys the tree handles itself; stop them from scrolling the page.
            let treeKeys = ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Enter', ' '];
            let instance = { allowPan: allowPan, canvas: canvas, pan: null, tree: tree };

            instance.onKeyDown = (event) => {
                if (treeKeys.includes(event.key) && event.target.getAttribute('role') === 'treeitem')
                    event.preventDefault();
            };
            instance.onPointerDown = (event) => {
                if (!instance.allowPan || event.button !== 0 || event.pointerType === 'touch')
                    return;
                if (event.target.closest('.bb-mind-map-card, button, input, a, select, textarea'))
                    return;

                instance.pan = { x: event.clientX, y: event.clientY, left: canvas.scrollLeft, top: canvas.scrollTop, pointerId: event.pointerId };
                canvas.setPointerCapture(event.pointerId);
                canvas.classList.add('bb-mind-map-panning');
            };
            instance.onPointerMove = (event) => {
                if (!instance.pan || event.pointerId !== instance.pan.pointerId)
                    return;

                canvas.scrollLeft = instance.pan.left - (event.clientX - instance.pan.x);
                canvas.scrollTop = instance.pan.top - (event.clientY - instance.pan.y);
            };
            instance.onPointerEnd = () => {
                instance.pan = null;
                canvas.classList.remove('bb-mind-map-panning');
            };

            tree.addEventListener('keydown', instance.onKeyDown);
            canvas.addEventListener('pointerdown', instance.onPointerDown);
            canvas.addEventListener('pointermove', instance.onPointerMove);
            canvas.addEventListener('pointerup', instance.onPointerEnd);
            canvas.addEventListener('pointercancel', instance.onPointerEnd);
            window.blazorBootstrap.mindMap.instances[elementId] = instance;
        },
        scrollRootIntoView: (elementId, direction) => {
            let canvas = document.getElementById(elementId + '-canvas');
            let rootCard = document.getElementById(elementId + '-n-0-c');
            if (canvas == null || rootCard == null)
                return;

            // Keep the root at its own edge (Right, Left, Down, Up) and centre it on the other axis.
            let canvasRect = canvas.getBoundingClientRect();
            let rootRect = rootCard.getBoundingClientRect();
            let centreX = canvas.scrollLeft + (rootRect.left + rootRect.width / 2) - (canvasRect.left + canvasRect.width / 2);
            let centreY = canvas.scrollTop + (rootRect.top + rootRect.height / 2) - (canvasRect.top + canvasRect.height / 2);

            canvas.scrollLeft = direction === 'Right' ? 0 : direction === 'Left' ? canvas.scrollWidth : centreX;
            canvas.scrollTop = direction === 'Down' ? 0 : direction === 'Up' ? canvas.scrollHeight : centreY;
        },
        update: (elementId, allowPan) => {
            let instance = window.blazorBootstrap.mindMap.instances[elementId];
            if (instance)
                instance.allowPan = allowPan;
        }
    },
    modal: {
        initialize: (elementId, useStaticBackdrop, closeOnEscape, dotNetHelper) => {
            let modalEl = document.getElementById(elementId);
            if (modalEl == null)
                return;

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
            let modalEl = document.getElementById(elementId);
            if (modalEl != null)
                bootstrap?.Modal?.getOrCreateInstance(modalEl)?.show();
        },
        hide: (elementId) => {
            let modalEl = document.getElementById(elementId);
            if (modalEl != null)
                bootstrap?.Modal?.getOrCreateInstance(modalEl)?.hide();
        },
        dispose: (elementId) => {
            let modalEl = document.getElementById(elementId);
            if (modalEl != null)
                bootstrap?.Modal?.getOrCreateInstance(modalEl)?.dispose();
        }
    },
    navbar: {
        // Element references by navbar id. Blazor removes the elements before dispose runs on the server, so dispose cannot look them up.
        instances: {},
        dispose: (elementId) => {
            let state = window.blazorBootstrap.navbar.instances[elementId];
            if (state == null)
                return;

            delete window.blazorBootstrap.navbar.instances[elementId];

            state.listeners.forEach(([target, type, handler]) => target.removeEventListener(type, handler));

            let menuEl = state.menuEl;
            let isOffcanvas = menuEl.classList.contains('offcanvas');
            if (!isOffcanvas && state.isExternal)
                return; // the external Collapse component owns its instance

            let instance = isOffcanvas ? bootstrap?.Offcanvas?.getInstance(menuEl) : bootstrap?.Collapse?.getInstance(menuEl);
            if (instance == null)
                return;

            let wasShown = state.phase === 'shown' || state.phase === 'showing';
            let disposeInstance = () => {
                instance.dispose(); // an offcanvas also removes its backdrop
                if (isOffcanvas && wasShown)
                    window.blazorBootstrap.navbar.resetScrollbar();
            };

            // Bootstrap finishes a transition in a queued callback that reads the instance; disposing first makes that callback throw.
            // Bootstrap still runs the callback on a removed element (its transition-end fallback timer), so wait for it.
            if (state.phase === 'showing' || state.phase === 'hiding') {
                let suffix = isOffcanvas ? 'offcanvas' : 'collapse';
                let done = false;
                let finish = (event) => {
                    if (done || (event != null && event.target !== menuEl))
                        return;
                    done = true;
                    menuEl.removeEventListener(`shown.bs.${suffix}`, finish);
                    menuEl.removeEventListener(`hidden.bs.${suffix}`, finish);
                    disposeInstance();
                };
                menuEl.addEventListener(`shown.bs.${suffix}`, finish);
                menuEl.addEventListener(`hidden.bs.${suffix}`, finish);
                setTimeout(() => finish(), 1000); // safety net if no event arrives
                return;
            }

            disposeInstance();
        },
        getInstance: (menuEl) => {
            if (menuEl.classList.contains('offcanvas'))
                return bootstrap?.Offcanvas?.getOrCreateInstance(menuEl);

            return bootstrap?.Collapse?.getOrCreateInstance(menuEl, { toggle: false });
        },
        hide: (elementId) => {
            let state = window.blazorBootstrap.navbar.instances[elementId];
            if (state == null)
                return;

            // Bootstrap ignores hide() while the menu is still opening, so hide it once it has opened.
            if (state.phase === 'showing')
                state.hideWhenShown = true;
            else if (state.phase === 'shown')
                window.blazorBootstrap.navbar.getInstance(state.menuEl)?.hide();
        },
        initialize: (elementId, menuId, isExternal) => {
            // Also called again when the menu or toggler element was replaced.
            let navbarEl = document.getElementById(elementId);
            let menuEl = document.getElementById(menuId);
            let previous = window.blazorBootstrap.navbar.instances[elementId];
            let keepsMenu = previous != null && previous.menuEl === menuEl && previous.isExternal === isExternal;
            if (keepsMenu) {
                // Same menu element (only the toggler changed): keep its Bootstrap instance, so an open menu stays open and still closes.
                delete window.blazorBootstrap.navbar.instances[elementId];
                previous.listeners.forEach(([target, type, handler]) => target.removeEventListener(type, handler));
            }
            else
                window.blazorBootstrap.navbar.dispose(elementId);

            if (navbarEl == null || menuEl == null)
                return;

            let isOffcanvas = menuEl.classList.contains('offcanvas');
            let togglerEl = navbarEl.querySelector('.navbar-toggler');
            let state = {
                menuEl: menuEl,
                togglerEl: togglerEl,
                isExternal: isExternal,
                listeners: [],
                phase: keepsMenu ? previous.phase : menuEl.classList.contains('show') ? 'shown' : 'hidden',
                hideWhenShown: keepsMenu ? previous.hideWhenShown : false
            };
            if (keepsMenu && isOffcanvas && togglerEl != null)
                togglerEl.setAttribute('aria-expanded', state.phase === 'showing' || state.phase === 'shown' ? 'true' : 'false');
            let listen = (target, type, handler) => {
                target.addEventListener(type, handler);
                state.listeners.push([target, type, handler]);
            };

            // Track the transition phase: dispose and hide must not act on Bootstrap mid-transition.
            let suffix = isOffcanvas ? 'offcanvas' : 'collapse';
            let onPhase = (phase) => (event) => {
                if (event.target !== menuEl)
                    return; // a nested collapse inside the menu
                state.phase = phase;
                if (phase === 'shown' && state.hideWhenShown) {
                    state.hideWhenShown = false;
                    window.blazorBootstrap.navbar.getInstance(menuEl)?.hide();
                }
                else if (phase !== 'shown')
                    state.hideWhenShown = false;
            };
            listen(menuEl, `show.bs.${suffix}`, onPhase('showing'));
            listen(menuEl, `shown.bs.${suffix}`, onPhase('shown'));
            listen(menuEl, `hide.bs.${suffix}`, onPhase('hiding'));
            listen(menuEl, `hidden.bs.${suffix}`, onPhase('hidden'));

            if (isOffcanvas && togglerEl != null) {
                // Bootstrap tracks aria-expanded for collapse togglers only.
                listen(menuEl, 'show.bs.offcanvas', () => togglerEl.setAttribute('aria-expanded', 'true'));
                listen(menuEl, 'hidden.bs.offcanvas', () => togglerEl.setAttribute('aria-expanded', 'false'));
            }

            if (isExternal) {
                // Move focus into external content when it opens.
                listen(menuEl, 'shown.bs.collapse', (event) => {
                    if (event.target !== menuEl)
                        return;

                    let focusableEl = menuEl.querySelector('a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])');
                    if (focusableEl == null) {
                        menuEl.setAttribute('tabindex', '-1');
                        focusableEl = menuEl;
                    }
                    focusableEl.focus();
                });
            }

            window.blazorBootstrap.navbar.instances[elementId] = state;
        },
        isCollapsed: (state) => {
            // The menu is collapsed only while the toggler is visible; above the expand breakpoint there is nothing to show or hide.
            return state.togglerEl != null && window.getComputedStyle(state.togglerEl).display !== 'none';
        },
        resetScrollbar: () => {
            // Undo Bootstrap's ScrollBarHelper when an open offcanvas is disposed before its hide transition completes.
            let reset = (el, property) => {
                let value = el.getAttribute(`data-bs-${property}`);
                if (value == null)
                    el.style.removeProperty(property);
                else {
                    el.style.setProperty(property, value);
                    el.removeAttribute(`data-bs-${property}`);
                }
            };
            reset(document.body, 'overflow');
            reset(document.body, 'padding-right');
            document.querySelectorAll('.fixed-top, .fixed-bottom, .is-fixed').forEach((el) => reset(el, 'padding-right'));
            document.querySelectorAll('.sticky-top').forEach((el) => reset(el, 'margin-right'));
        },
        show: (elementId) => {
            let state = window.blazorBootstrap.navbar.instances[elementId];
            if (state == null || !window.blazorBootstrap.navbar.isCollapsed(state))
                return;

            window.blazorBootstrap.navbar.getInstance(state.menuEl)?.show();
        },
        toggle: (elementId) => {
            let state = window.blazorBootstrap.navbar.instances[elementId];
            if (state == null || !window.blazorBootstrap.navbar.isCollapsed(state))
                return;

            window.blazorBootstrap.navbar.getInstance(state.menuEl)?.toggle();
        }
    },
    numberInput: {
        initialize: (elementId, isFloat, allowNegativeNumbers, numberDecimalSeparator) => {
            let numberEl = document.getElementById(elementId);

            numberEl?.addEventListener('keydown', function (event) {
                let invalidChars = ["e", "E", "+"];
                if (!isFloat) {
                    invalidChars.push("."); // restrict '.' for integer types
                    invalidChars.push(numberDecimalSeparator); // restrict ',' for specific culture
                }

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
            if (offcanvasEl == null)
                return;

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
            let offcanvasEl = document.getElementById(elementId);
            if (offcanvasEl != null)
                bootstrap?.Offcanvas?.getOrCreateInstance(offcanvasEl)?.show();
        },
        hide: (elementId) => {
            let offcanvasEl = document.getElementById(elementId);
            if (offcanvasEl != null)
                bootstrap?.Offcanvas?.getOrCreateInstance(offcanvasEl)?.hide();
        },
        dispose: (elementId) => {
            let offcanvasEl = document.getElementById(elementId);
            if (offcanvasEl != null)
                bootstrap?.Offcanvas?.getOrCreateInstance(offcanvasEl)?.dispose();
        }
    },
    popover: {
        // State by popover id. Blazor removes the elements before dispose runs on the server, so dispose cannot look them up.
        instances: {},
        // Ids of open popovers, in the order they opened. Escape closes only the last one (nested popovers close one at a time).
        openOrder: [],
        dispose: (elementId) => {
            let state = window.blazorBootstrap.popover.instances[elementId];
            if (state == null)
                return;

            delete window.blazorBootstrap.popover.instances[elementId];
            window.blazorBootstrap.popover.setOpen(elementId, false);

            state.resizeObserver?.disconnect();
            state.mutationObserver?.disconnect();
            state.listeners.forEach(([target, type, handler, capture]) => target.removeEventListener(type, handler, capture));

            // Move the Blazor-owned elements home first: dispose removes the open popover, and the elements must not go with it.
            state.restore();

            state.triggerEl.removeAttribute('aria-expanded');
            if (state.isWrapperTrigger) {
                state.triggerEl.removeAttribute('tabindex');
                state.triggerEl.removeAttribute('role');
            }

            let popover = bootstrap?.Popover?.getInstance(state.triggerEl);
            if (popover == null)
                return;

            // Bootstrap finishes a fade in a callback queued on the popover element, and that callback reads the instance, so disposing
            // mid-fade makes it throw. Bootstrap runs the callback on 'transitionend'; dispatching one finishes the fade now.
            if (state.phase === 'showing' || state.phase === 'hiding')
                popover.tip?.dispatchEvent(new Event('transitionend'));

            popover.dispose();
        },
        findTrigger: (hostEl) => {
            // The first interactive element inside the host. With none, or a disabled one (which cannot take focus or pointer events),
            // the host itself is the trigger, as Bootstrap recommends for disabled elements.
            let el = hostEl.querySelector('a[href], button, input:not([type="hidden"]), select, textarea, [tabindex]:not([tabindex="-1"]), [role="button"]');
            return el == null || el.matches(':disabled, .disabled, [aria-disabled="true"]') ? hostEl : el;
        },
        finishFade: (elementId) => {
            // A show or hide started during the other's fade is undone when that fade completes (Bootstrap resets its hover tracking
            // in the completion callback). Bootstrap completes a fade on 'transitionend', so dispatch one to finish it now.
            let state = window.blazorBootstrap.popover.instances[elementId];
            if (state?.phase === 'showing' || state?.phase === 'hiding')
                window.blazorBootstrap.popover.getInstance(elementId)?.tip?.dispatchEvent(new Event('transitionend'));
        },
        getInstance: (elementId) => {
            let state = window.blazorBootstrap.popover.instances[elementId];
            return state == null ? null : bootstrap?.Popover?.getInstance(state.triggerEl);
        },
        hide: (elementId) => {
            let state = window.blazorBootstrap.popover.instances[elementId];
            if (state == null)
                return;

            window.blazorBootstrap.popover.finishFade(elementId);
            window.blazorBootstrap.popover.getInstance(elementId)?.hide();
        },
        hideNow: (elementId) => {
            // Hide without waiting for the fade, so OnHiding / OnHidden reach .NET and Bootstrap removes aria-describedby.
            let state = window.blazorBootstrap.popover.instances[elementId];
            if (state?.phase !== 'showing' && state?.phase !== 'shown')
                return;

            window.blazorBootstrap.popover.hide(elementId);
            window.blazorBootstrap.popover.finishFade(elementId);
        },
        initialize: (elementId, options, dotNetHelper) => {
            // Also called again when an option or the trigger changes; drop the old instance first.
            window.blazorBootstrap.popover.dispose(elementId);

            let hostEl = document.getElementById(elementId);
            let homeEl = document.getElementById(`${elementId}-home`);
            if (hostEl == null || homeEl == null)
                return;

            let triggerEl = window.blazorBootstrap.popover.findTrigger(hostEl);
            let isWrapperTrigger = triggerEl === hostEl;
            // Hover and Focus popovers close when focus leaves the trigger, so they are for content that is read, not used.
            let isHoverOrFocus = /focus|hover/.test(options.trigger);

            let titleEl = homeEl.querySelector(':scope > [data-bb-popover-title]');
            let contentEl = homeEl.querySelector(':scope > [data-bb-popover-content]');

            let state = {
                autoClose: options.autoClose,
                dotNetHelper: dotNetHelper,
                enabled: options.enabled,
                hasContent: options.hasContent,
                hasTitle: options.hasTitle,
                isWrapperTrigger: isWrapperTrigger,
                listeners: [],
                mutationObserver: null,
                openedByKeyboard: false,
                options: options,
                phase: 'hidden', // showing, shown, hiding, or hidden
                resizeObserver: null,
                restoreFocus: false,
                triggerEl: triggerEl
            };
            state.restore = () => {
                // Keep the home order (title, then content), so Blazor finds its elements where it left them.
                if (titleEl.parentElement !== homeEl)
                    homeEl.insertBefore(titleEl, homeEl.firstChild);
                if (contentEl.parentElement !== homeEl)
                    homeEl.appendChild(contentEl);
            };

            // Bootstrap decides whether there is anything to show from these, so a title or content that appears or disappears
            // later needs no re-creation.
            let config = {
                // html: true makes Bootstrap append the elements themselves instead of copying their text.
                // No HTML string is ever passed, so the sanitizer (left at its default) has nothing to process.
                html: true,
                title: () => state.hasTitle ? titleEl : '',
                content: () => state.hasContent ? contentEl : '',
                placement: options.placement,
                trigger: options.trigger,
                delay: { show: options.showDelay, hide: options.hideDelay },
                animation: options.animation
            };
            if (options.fallbackPlacements?.length)
                config.fallbackPlacements = options.fallbackPlacements;
            if (options.offset)
                config.offset = options.offset;
            if (options.container)
                config.container = options.container;
            if (options.customClass)
                config.customClass = options.customClass;

            // Create the instance before anything is attached, so an invalid option (for example a Container that matches nothing)
            // leaves nothing behind and no error reaches .NET.
            let popover;
            try {
                popover = bootstrap?.Popover?.getOrCreateInstance(triggerEl, config);
            }
            catch (error) {
                console.warn(`Blazor Bootstrap: popover '${elementId}' was not created.`, error);
                return;
            }
            if (popover == null)
                return;

            // A wrapper takes focus only when focus or a key opens the popover (Click, Focus); Hover-only and Manual popovers get no tab stop.
            if (isWrapperTrigger && /click|focus/.test(options.trigger)) {
                hostEl.setAttribute('tabindex', '0');
                if (/click/.test(options.trigger))
                    hostEl.setAttribute('role', 'button');
            }
            triggerEl.setAttribute('aria-expanded', 'false');

            let listen = (target, type, handler, capture = false) => {
                target.addEventListener(type, handler, capture);
                state.listeners.push([target, type, handler, capture]);
            };
            // A lost Server connection rejects the .NET calls; the popover keeps working, so the rejection is ignored.
            let notify = (method) => dotNetHelper.invokeMethodAsync(method).catch(() => { });
            let instance = () => bootstrap?.Popover?.getInstance(triggerEl);
            let tip = () => instance()?.tip ?? null;
            let isOpen = () => state.phase === 'showing' || state.phase === 'shown';
            let focusableSelector = 'a[href], button:not([disabled]), input:not([disabled]):not([type="hidden"]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

            state.syncTip = () => {
                // Put the title and content into the open popover, or take them out, when they appear or disappear while it is open.
                let tipEl = tip();
                if (tipEl == null)
                    return;

                let headerEl = tipEl.querySelector(':scope > .popover-header');
                if (state.hasTitle && headerEl == null) {
                    headerEl = document.createElement('h3');
                    headerEl.className = 'popover-header';
                    tipEl.insertBefore(headerEl, tipEl.querySelector(':scope > .popover-body'));
                }
                if (state.hasTitle && titleEl.parentElement !== headerEl)
                    headerEl.appendChild(titleEl);
                else if (!state.hasTitle && headerEl != null) {
                    homeEl.insertBefore(titleEl, homeEl.firstChild);
                    headerEl.remove();
                }

                let bodyEl = tipEl.querySelector(':scope > .popover-body');
                if (state.hasContent && bodyEl == null) {
                    bodyEl = document.createElement('div');
                    bodyEl.className = 'popover-body';
                    tipEl.appendChild(bodyEl);
                }
                if (state.hasContent && contentEl.parentElement !== bodyEl)
                    bodyEl.appendChild(contentEl);
                else if (!state.hasContent && bodyEl != null) {
                    homeEl.appendChild(contentEl);
                    bodyEl.remove();
                }
            };

            listen(triggerEl, 'show.bs.popover', () => {
                // Bootstrap's own trigger can show during a hide fade; it then replaces the popover element while the old one's hide
                // completion is still queued, to run later on a changed or disposed instance. Finish the old fade first.
                if (state.phase === 'hiding')
                    tip()?.dispatchEvent(new Event('transitionend'));
                window.blazorBootstrap.popover.setOpen(elementId, true);
                state.phase = 'showing';
                triggerEl.setAttribute('aria-expanded', 'true');
                notify('bsShow');
            });
            listen(triggerEl, 'inserted.bs.popover', () => notify('bsInserted'));
            listen(triggerEl, 'shown.bs.popover', () => {
                if (state.phase === 'showing') // not when a hide already started during the fade
                    state.phase = 'shown';
                // Opened by keyboard: move focus to the first interactive element in the content, if any.
                if (state.openedByKeyboard && !isHoverOrFocus)
                    tip()?.querySelector(focusableSelector)?.focus();
                state.openedByKeyboard = false;
                notify('bsShown');
            });
            listen(triggerEl, 'hide.bs.popover', () => {
                state.phase = 'hiding';
                // Return focus to the trigger only when it was in the content; a Focus trigger would open again on focus.
                state.restoreFocus = !isHoverOrFocus && (tip()?.contains(document.activeElement) ?? false);
                triggerEl.setAttribute('aria-expanded', 'false');
                notify('bsHide');
            });
            listen(triggerEl, 'hidden.bs.popover', () => {
                state.phase = 'hidden';
                window.blazorBootstrap.popover.setOpen(elementId, false);
                // Bootstrap removes the popover element when it hides (and before each show); bring the elements home each time.
                state.restore();
                if (state.restoreFocus)
                    triggerEl.focus();
                state.restoreFocus = false;
                notify('bsHidden');
            });

            listen(triggerEl, 'keydown', (event) => {
                if (event.key !== 'Enter' && event.key !== ' ')
                    return;

                state.openedByKeyboard = true;
                // A wrapper is a span: Enter and Space do not click it, so do it for the Click trigger.
                if (isWrapperTrigger && event.target === hostEl && /click/.test(options.trigger)) {
                    event.preventDefault();
                    hostEl.click();
                }
            });
            listen(triggerEl, 'pointerdown', () => state.openedByKeyboard = false);

            // AutoClose: close on a click outside the popover and its trigger, and when focus leaves the content.
            listen(document, 'click', (event) => {
                if (!state.autoClose || !isOpen())
                    return;
                if (triggerEl.contains(event.target) || tip()?.contains(event.target))
                    return;
                instance()?.hide();
            }, true);

            // Escape closes the popover first. Hover and Focus popovers always close on Escape (WCAG 1.4.13); others when AutoClose is on.
            // Only the last opened popover handles it, and a key another handler already took (defaultPrevented) is left alone.
            let handleEscape = (event) => {
                if (event.key !== 'Escape' || event.defaultPrevented || !isOpen() || (!state.autoClose && !isHoverOrFocus))
                    return false;

                let openOrder = window.blazorBootstrap.popover.openOrder;
                if (openOrder[openOrder.length - 1] !== elementId)
                    return false;

                let tipEl = tip();
                if (tipEl?.querySelector('.dropdown-menu.show'))
                    return false; // Bootstrap's dropdown closes its open menu on the same key, at the document

                let focusWasInside = (tipEl?.contains(document.activeElement) ?? false) || triggerEl.contains(document.activeElement);
                event.preventDefault(); // marks the key as handled
                // Stop it here so a surrounding modal or offcanvas does not close on the same key.
                if (triggerEl.closest('.modal, .offcanvas') || tipEl?.closest('.modal, .offcanvas'))
                    event.stopPropagation();
                window.blazorBootstrap.popover.hide(elementId);
                if (focusWasInside && !isHoverOrFocus)
                    state.restoreFocus = true;
                return true;
            };
            // Focus outside the content: handle Escape in the capture phase, before a surrounding modal or offcanvas sees it.
            listen(document, 'keydown', (event) => {
                if (!(tip()?.contains(event.target) ?? false))
                    handleEscape(event);
            }, true);
            // Focus inside the content: let its own controls (a menu, a combobox) handle Escape first; the popover gets it as it bubbles
            // out of the content, before any modal around it. The popover element is new on every show.
            listen(triggerEl, 'inserted.bs.popover', () => tip()?.addEventListener('keydown', handleEscape));

            // Keep the tab order next to the trigger. The popover is added at the end of the page (or its container), so the browser would
            // skip its content and later move focus from it to unrelated elements. Instead: Tab from the trigger enters the open content,
            // Shift+Tab from its first element goes back to the trigger, and Tab from its last element goes to the element after the
            // trigger, closing the popover when AutoClose is on. Not for Hover and Focus popovers, which close when focus leaves the trigger.
            let isFocusable = (el) => !el.closest('[inert]') && el.getClientRects().length > 0;
            listen(document, 'keydown', (event) => {
                let tipEl = tip();
                if (event.key !== 'Tab' || isHoverOrFocus || !isOpen() || tipEl == null)
                    return;

                let inside = [...tipEl.querySelectorAll(focusableSelector)].filter(isFocusable);
                if (inside.length === 0)
                    return;

                if (!event.shiftKey && document.activeElement === triggerEl) {
                    event.preventDefault();
                    inside[0].focus();
                    return;
                }

                if (!tipEl.contains(document.activeElement))
                    return;

                if (event.shiftKey && document.activeElement === inside[0]) {
                    event.preventDefault();
                    triggerEl.focus();
                }
                else if (!event.shiftKey && document.activeElement === inside[inside.length - 1]) {
                    event.preventDefault();
                    let outside = [...document.querySelectorAll(focusableSelector)].filter((el) => !tipEl.contains(el) && isFocusable(el));
                    let triggerIndex = outside.indexOf(triggerEl);
                    let next = triggerIndex >= 0 ? (outside[triggerIndex + 1] ?? triggerEl) : triggerEl;
                    next.focus();
                    if (state.autoClose)
                        instance()?.hide();
                }
            });
            listen(document, 'focusin', (event) => {
                if (!state.autoClose || !isOpen())
                    return;
                if (triggerEl.contains(event.target) || tip()?.contains(event.target))
                    return;
                // Only when focus moves out of the content; the Focus trigger already closes when the trigger loses focus.
                if (tip()?.contains(event.relatedTarget))
                    instance()?.hide();
            });

            // Reposition when the Blazor-owned content changes size while open.
            if (typeof ResizeObserver !== 'undefined') {
                state.resizeObserver = new ResizeObserver(() => instance()?.update());
                state.resizeObserver.observe(titleEl);
                state.resizeObserver.observe(contentEl);
            }

            // Re-attach when the trigger changes: another first element, or a control that became disabled or enabled. An open popover
            // closes first (with its events), and the new instance takes the live settings, not the ones from the last .NET initialize.
            if (typeof MutationObserver !== 'undefined') {
                state.mutationObserver = new MutationObserver(() => {
                    if (window.blazorBootstrap.popover.instances[elementId] !== state || window.blazorBootstrap.popover.findTrigger(hostEl) === state.triggerEl)
                        return;

                    window.blazorBootstrap.popover.hideNow(elementId);
                    let liveOptions = { ...state.options, autoClose: state.autoClose, enabled: state.enabled, hasContent: state.hasContent, hasTitle: state.hasTitle };
                    window.blazorBootstrap.popover.initialize(elementId, liveOptions, state.dotNetHelper);
                });
                // The attributes findTrigger reads.
                state.mutationObserver.observe(hostEl, { childList: true, subtree: true, attributes: true, attributeFilter: ['disabled', 'class', 'href', 'aria-disabled', 'tabindex', 'role', 'type'] });
            }

            if (!options.enabled)
                popover.disable();

            window.blazorBootstrap.popover.instances[elementId] = state;
        },
        setOpen: (elementId, isOpen) => {
            let openOrder = window.blazorBootstrap.popover.openOrder;
            let index = openOrder.indexOf(elementId);
            if (index >= 0)
                openOrder.splice(index, 1);
            if (isOpen)
                openOrder.push(elementId);
        },
        setAutoClose: (elementId, autoClose) => {
            let state = window.blazorBootstrap.popover.instances[elementId];
            if (state != null)
                state.autoClose = autoClose;
        },
        setContent: (elementId, hasTitle, hasContent) => {
            // The title or content appeared or disappeared: update the open popover in place.
            let state = window.blazorBootstrap.popover.instances[elementId];
            if (state == null)
                return;

            state.hasTitle = hasTitle;
            state.hasContent = hasContent;
            if (state.phase !== 'showing' && state.phase !== 'shown')
                return;

            if (!hasTitle && !hasContent) {
                window.blazorBootstrap.popover.hide(elementId); // Bootstrap never shows an empty popover
                return;
            }

            state.syncTip();
            window.blazorBootstrap.popover.getInstance(elementId)?.update();
        },
        setEnabled: (elementId, enabled) => {
            let state = window.blazorBootstrap.popover.instances[elementId];
            if (state != null)
                state.enabled = enabled;

            let popover = window.blazorBootstrap.popover.getInstance(elementId);
            if (popover == null)
                return;

            if (enabled)
                popover.enable();
            else {
                popover.hide();
                popover.disable();
            }
        },
        show: (elementId) => {
            let state = window.blazorBootstrap.popover.instances[elementId];
            if (state == null)
                return;

            window.blazorBootstrap.popover.finishFade(elementId);
            if (state.phase !== 'shown') // Bootstrap would re-create an open popover
                window.blazorBootstrap.popover.getInstance(elementId)?.show();
        },
        toggle: (elementId) => {
            let state = window.blazorBootstrap.popover.instances[elementId];
            if (state == null)
                return;

            if (state.phase === 'showing' || state.phase === 'shown')
                window.blazorBootstrap.popover.hide(elementId);
            else
                window.blazorBootstrap.popover.show(elementId);
        },
        update: (elementId) => window.blazorBootstrap.popover.getInstance(elementId)?.update()
    },
    radioInput: {
        isChanging: false,
        initialize: (elementId, elementName, dotNetHelper) => {
            let radioEl = document.getElementById(elementId);
            if (radioEl == null)
                return;

            radioEl.addEventListener('change', function () {
                try {
                    dotNetHelper.invokeMethodAsync('OnChangeJS', radioEl.checked);

                    let radioEls = document.getElementsByName(elementName) ?? [];
                    radioEls.forEach((el, index) => {

                        if (window.blazorBootstrap.radioInput.isChanging)
                            return;

                        if (el.id !== radioEl.id) {
                            window.blazorBootstrap.radioInput.isChanging = true;
                            el.checked = false;
                            el.dispatchEvent(new Event('change'));
                        }
                    });
                }
                finally {
                    window.blazorBootstrap.radioInput.isChanging = false;
                }
            });
        }
    },
    rangeInput: {
        initialize: (elementId, dotNetHelper) => {
            let rangeEl = document.getElementById(elementId);
            if (rangeEl == null)
                return;

            rangeEl.addEventListener("input", (event) => {
                dotNetHelper.invokeMethodAsync('bsOnInput', event.target.value);
            });
        }
    },
    scriptLoader: {
        initialize: (elementId, async, defer, scriptId, source, type, dotNetHelper) => {
            let scriptLoaderEl = document.getElementById(elementId);

            if (source.length === 0) {
                console.error(`Invalid src url.`);
                return;
            }

            let existingScriptEl = scriptId == null ? null : document.getElementById(scriptId);

            if (existingScriptEl?.tagName === 'SCRIPT') {
                if (existingScriptEl.dataset.blazorBootstrapScriptLoaderStatus === 'loaded') {
                    dotNetHelper.invokeMethodAsync('OnLoadJS');
                    return;
                }

                if (existingScriptEl.dataset.blazorBootstrapScriptLoaderStatus === 'error') {
                    dotNetHelper.invokeMethodAsync('OnErrorJS', `An error occurred while loading the script: ${source}`);
                    return;
                }

                existingScriptEl.addEventListener("error", (event) => {
                    dotNetHelper.invokeMethodAsync('OnErrorJS', `An error occurred while loading the script: ${source}`);
                }, { once: true });

                existingScriptEl.addEventListener("load", (event) => {
                    dotNetHelper.invokeMethodAsync('OnLoadJS');
                }, { once: true });

                return;
            }

            let scriptEl = document.createElement('script');

            scriptEl.async = async;

            scriptEl.defer = defer;

            if (scriptId != null)
                scriptEl.id = scriptId;

            if (source != null)
                scriptEl.src = source;

            if (type != null)
                scriptEl.type = type;

            scriptEl.addEventListener("error", (event) => {
                scriptEl.dataset.blazorBootstrapScriptLoaderStatus = 'error';
                dotNetHelper.invokeMethodAsync('OnErrorJS', `An error occurred while loading the script: ${source}`);
            });

            scriptEl.addEventListener("load", (event) => {
                scriptEl.dataset.blazorBootstrapScriptLoaderStatus = 'loaded';
                dotNetHelper.invokeMethodAsync('OnLoadJS');
            });

            if (scriptLoaderEl != null)
                scriptLoaderEl.appendChild(scriptEl);
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
    splitView: {
        applyPaneSize: (state, primaryPaneSize) => {
            let normalizedPaneSize = window.blazorBootstrap.splitView.normalizePaneSize(state, primaryPaneSize);
            let availableSize = window.blazorBootstrap.splitView.getAvailableSize(state);
            let primaryPanePixels = availableSize * normalizedPaneSize / 100;

            state.primaryPaneSize = normalizedPaneSize;
            state.primaryPane.style.flexBasis = `${primaryPanePixels}px`;
        },
        clamp: (value, min, max) => Math.min(Math.max(value, min), max),
        createResizeObserver: (state) => {
            if (typeof ResizeObserver === 'undefined')
                return null;

            let resizeObserver = new ResizeObserver(() => {
                if (!state.isDragging)
                    window.blazorBootstrap.splitView.applyPaneSize(state, state.primaryPaneSize);
            });

            resizeObserver.observe(state.root);

            return resizeObserver;
        },
        dispose: (elementId) => {
            let state = window.blazorBootstrap.splitView.instances[elementId];
            if (!state)
                return;

            state.divider.removeEventListener('pointerdown', state.pointerDownHandler);
            state.divider.removeEventListener('pointermove', state.pointerMoveHandler);
            state.divider.removeEventListener('pointerup', state.pointerUpHandler);
            state.divider.removeEventListener('pointercancel', state.pointerUpHandler);
            state.divider.removeEventListener('lostpointercapture', state.lostPointerCaptureHandler);
            state.resizeObserver?.disconnect();
            state.root.classList.remove('bb-split-view-dragging');

            delete window.blazorBootstrap.splitView.instances[elementId];
        },
        emitResizeEvent: (state, methodName) => {
            if (!state.dotNetHelper)
                return;

            let primaryPaneSize = window.blazorBootstrap.splitView.roundPaneSize(state.primaryPaneSize);
            let secondaryPaneSize = window.blazorBootstrap.splitView.roundPaneSize(100 - primaryPaneSize);

            state.dotNetHelper.invokeMethodAsync(methodName, primaryPaneSize, secondaryPaneSize)
                .catch(() => {
                    // do nothing
                });
        },
        getAvailableSize: (state) => {
            let rect = state.root.getBoundingClientRect();
            let dividerRect = state.divider.getBoundingClientRect();
            let dividerSize = state.orientation === 'Horizontal' ? dividerRect.width : dividerRect.height;
            let totalSize = state.orientation === 'Horizontal' ? rect.width : rect.height;

            return Math.max(totalSize - dividerSize, 0);
        },
        getDirectChild: (root, className) => {
            let children = Array.from(root.children);
            return children.find((child) => child.classList?.contains(className)) ?? null;
        },
        initialize: (elementId, orientation, primaryPaneSize, minimumPaneSize, isDisabled, dotNetHelper) => {
            window.blazorBootstrap.splitView.dispose(elementId);

            let root = document.getElementById(elementId);
            if (!root)
                return;

            let primaryPane = window.blazorBootstrap.splitView.getDirectChild(root, 'bb-split-view-pane-primary');
            let divider = window.blazorBootstrap.splitView.getDirectChild(root, 'bb-split-view-divider');
            let secondaryPane = window.blazorBootstrap.splitView.getDirectChild(root, 'bb-split-view-pane-secondary');

            if (!primaryPane || !divider || !secondaryPane)
                return;

            let state = {
                activePointerId: null,
                divider: divider,
                dotNetHelper: dotNetHelper,
                isDisabled: isDisabled,
                isDragging: false,
                lostPointerCaptureHandler: null,
                minimumPaneSize: minimumPaneSize,
                orientation: orientation,
                pointerDownHandler: null,
                pointerMoveHandler: null,
                pointerUpHandler: null,
                primaryPane: primaryPane,
                primaryPaneSize: primaryPaneSize,
                resizeObserver: null,
                root: root,
                secondaryPane: secondaryPane,
                startPosition: 0,
                startPrimaryPaneSize: primaryPaneSize
            };

            state.pointerDownHandler = (event) => window.blazorBootstrap.splitView.onPointerDown(state, event);
            state.pointerMoveHandler = (event) => window.blazorBootstrap.splitView.onPointerMove(state, event);
            state.pointerUpHandler = (event) => window.blazorBootstrap.splitView.stopDragging(state, event);
            state.lostPointerCaptureHandler = (event) => window.blazorBootstrap.splitView.stopDragging(state, event);
            state.resizeObserver = window.blazorBootstrap.splitView.createResizeObserver(state);

            divider.addEventListener('pointerdown', state.pointerDownHandler);
            divider.addEventListener('pointermove', state.pointerMoveHandler);
            divider.addEventListener('pointerup', state.pointerUpHandler);
            divider.addEventListener('pointercancel', state.pointerUpHandler);
            divider.addEventListener('lostpointercapture', state.lostPointerCaptureHandler);

            window.blazorBootstrap.splitView.updateState(state, orientation, primaryPaneSize, minimumPaneSize, isDisabled);

            window.blazorBootstrap.splitView.instances[elementId] = state;
        },
        instances: {},
        normalizePaneSize: (state, value) => {
            let minimumPaneSize = window.blazorBootstrap.splitView.clamp(state.minimumPaneSize, 0, 50);
            return window.blazorBootstrap.splitView.clamp(value, minimumPaneSize, 100 - minimumPaneSize);
        },
        onPointerDown: (state, event) => {
            if (state.isDisabled)
                return;

            if (event.pointerType === 'mouse' && event.button !== 0)
                return;

            event.preventDefault();
            event.stopPropagation();

            state.isDragging = true;
            state.activePointerId = event.pointerId;
            state.startPosition = state.orientation === 'Horizontal' ? event.clientX : event.clientY;
            state.startPrimaryPaneSize = state.primaryPaneSize;

            state.root.classList.add('bb-split-view-dragging');
            state.divider.setPointerCapture?.(event.pointerId);

            window.blazorBootstrap.splitView.emitResizeEvent(state, 'OnResizeStartedJS');
        },
        onPointerMove: (state, event) => {
            if (!state.isDragging || state.activePointerId !== event.pointerId)
                return;

            event.preventDefault();
            event.stopPropagation();

            let currentPosition = state.orientation === 'Horizontal' ? event.clientX : event.clientY;
            let delta = currentPosition - state.startPosition;
            let availableSize = window.blazorBootstrap.splitView.getAvailableSize(state);

            if (availableSize <= 0)
                return;

            let nextPaneSize = state.startPrimaryPaneSize + (delta / availableSize * 100);
            let normalizedPaneSize = window.blazorBootstrap.splitView.normalizePaneSize(state, nextPaneSize);

            if (Math.abs(normalizedPaneSize - state.primaryPaneSize) < 0.01)
                return;

            window.blazorBootstrap.splitView.applyPaneSize(state, normalizedPaneSize);
            window.blazorBootstrap.splitView.emitResizeEvent(state, 'OnResizedJS');
        },
        roundPaneSize: (value) => Math.round(value * 100) / 100,
        stopDragging: (state, event) => {
            if (!state.isDragging)
                return;

            if (event && state.activePointerId !== event.pointerId)
                return;

            state.isDragging = false;
            state.activePointerId = null;
            state.root.classList.remove('bb-split-view-dragging');

            window.blazorBootstrap.splitView.emitResizeEvent(state, 'OnResizeEndedJS');
        },
        update: (elementId, orientation, primaryPaneSize, minimumPaneSize, isDisabled) => {
            let state = window.blazorBootstrap.splitView.instances[elementId];
            if (!state)
                return;

            window.blazorBootstrap.splitView.updateState(state, orientation, primaryPaneSize, minimumPaneSize, isDisabled);
        },
        updateState: (state, orientation, primaryPaneSize, minimumPaneSize, isDisabled) => {
            state.orientation = orientation;
            state.minimumPaneSize = minimumPaneSize;
            state.isDisabled = isDisabled;

            if (state.isDragging && isDisabled)
                window.blazorBootstrap.splitView.stopDragging(state);

            window.blazorBootstrap.splitView.applyPaneSize(state, primaryPaneSize);
        }
    },
    tabs: {
        initialize: (elementId, dotNetHelper) => {
            let navTabsEl = document.getElementById(elementId);
            if (navTabsEl == null)
                return;

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
        initializeNewTab: (tabId, dotNetHelper) => {
            let tabEl = document.getElementById(tabId);
            if (tabEl == null)
                return;

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
        },
        show: (elementId) => {
            let navTabsEl = document.getElementById(elementId);
            if (navTabsEl != null)
                bootstrap?.Tab?.getOrCreateInstance(navTabsEl)?.show();
        },
        dispose: (elementId) => {
            let navTabsEl = document.getElementById(elementId);
            if (navTabsEl != null)
                bootstrap?.Tab?.getOrCreateInstance(navTabsEl)?.dispose();
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
            if (toastEl == null)
                return;

            toastEl.addEventListener('show.bs.toast', function () {
                dotNetHelper.invokeMethodAsync('bsShowToast');
            });
            toastEl.addEventListener('shown.bs.toast', function () {
                dotNetHelper.invokeMethodAsync('bsShownToast');
            });
            toastEl.addEventListener('hide.bs.toast', function () {
                let _this = this;
                if (_this == null || document.getElementById(_this.id) == null) // when a user is redirected to a different page, the HTML element becomes unavailable.
                    return;
                dotNetHelper.invokeMethodAsync('bsHideToast');
            });
            toastEl.addEventListener('hidden.bs.toast', function () {
                let _this = this;
                if (_this == null || document.getElementById(_this.id) == null) // when a user is redirected to a different page, the HTML element becomes unavailable.
                    return;
                dotNetHelper.invokeMethodAsync('bsHiddenToast');
            });

            let options = { animation: true, autohide: autohide, delay: delay };
            bootstrap?.Toast?.getOrCreateInstance(toastEl, options)?.show();
        },
        hide: (elementId) => {
            let toastEl = document.getElementById(elementId);
            if (toastEl != null)
                bootstrap?.Toast?.getOrCreateInstance(toastEl)?.hide();
        },
        dispose: (elementId) => {
            let toastEl = document.getElementById(elementId);
            if (toastEl != null)
                bootstrap?.Toast?.getOrCreateInstance(toastEl)?.dispose();
        }
    },
    tooltip: {
        initialize: (elementRef) => {
            if (elementRef != null)
                bootstrap?.Tooltip?.getOrCreateInstance(elementRef);
        },
        show: (elementRef) => {
            if (elementRef != null)
                bootstrap?.Tooltip?.getOrCreateInstance(elementRef)?.show();
        },
        update: (elementRef, title) => {
            if (elementRef != null) {
                // Bootstrap keeps the old 'data-bs-original-title' when the new title is empty
                if (title)
                    elementRef.setAttribute('data-bs-original-title', title);
                else if (title !== undefined)
                    elementRef.removeAttribute('data-bs-original-title');

                bootstrap?.Tooltip?.getOrCreateInstance(elementRef)?.update();
            }
        },
        dispose: (elementRef) => {
            if (elementRef != null)
                bootstrap?.Tooltip?.getOrCreateInstance(elementRef)?.dispose();
        }
    },
    treeview: {
        initialize: (elementId, dotNetHelper) => {
            window.addEventListener("resize", () => {
                dotNetHelper.invokeMethodAsync('bsWindowResize', window.innerWidth);
            });
        },
        windowSize: () => window.innerWidth
    },
    // global function
    focusInputElement(elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.focus();
        }
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
    invokeMethodAsync: (callbackEventName, dotNetHelper) => {
        dotNetHelper.invokeMethodAsync(callbackEventName);
    },
    scrollToElementBottom: (elementId) => {
        let el = document.getElementById(elementId);
        if (el)
            el.scrollTop = el.scrollHeight;
    },
    scrollToElementTop: (elementId) => {
        let el = document.getElementById(elementId);
        if (el)
            el.scrollTop = 0;
    },
    setInputElementValue: (elementId, value) => {
        const element = document.getElementById(elementId);
        if (element) {
            element.value = value;
        }
    }
}
