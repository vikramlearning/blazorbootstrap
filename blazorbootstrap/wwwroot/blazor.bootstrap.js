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
            if (menuEl.classList.contains('offcanvas')) {
                let wasShown = menuEl.classList.contains('show') || menuEl.classList.contains('showing');
                bootstrap?.Offcanvas?.getInstance(menuEl)?.dispose(); // also removes its backdrop
                if (wasShown)
                    window.blazorBootstrap.navbar.resetScrollbar();
            }
            else if (!state.isExternal) {
                bootstrap?.Collapse?.getInstance(menuEl)?.dispose();
            }
        },
        getInstance: (menuEl) => {
            if (menuEl.classList.contains('offcanvas'))
                return bootstrap?.Offcanvas?.getOrCreateInstance(menuEl);

            return bootstrap?.Collapse?.getOrCreateInstance(menuEl, { toggle: false });
        },
        hide: (elementId) => {
            let state = window.blazorBootstrap.navbar.instances[elementId];
            if (state == null || !state.menuEl.classList.contains('show'))
                return;

            window.blazorBootstrap.navbar.getInstance(state.menuEl)?.hide();
        },
        initialize: (elementId, menuId, isExternal) => {
            let navbarEl = document.getElementById(elementId);
            let menuEl = document.getElementById(menuId);
            if (navbarEl == null || menuEl == null)
                return;

            let togglerEl = navbarEl.querySelector('.navbar-toggler');
            let state = { menuEl: menuEl, togglerEl: togglerEl, isExternal: isExternal, listeners: [] };
            let listen = (target, type, handler) => {
                target.addEventListener(type, handler);
                state.listeners.push([target, type, handler]);
            };

            if (menuEl.classList.contains('offcanvas') && togglerEl != null) {
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

            window.blazorBootstrap.navbar.dispose(elementId);
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
