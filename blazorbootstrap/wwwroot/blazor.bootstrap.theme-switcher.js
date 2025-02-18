// THEMES
const STORAGE_KEY = "blazorbootstrap-theme";
const DEFAULT_THEME = "light";
const LIGHT_THEME = "light";
const DARK_THEME = "dark";
const SYSTEM_THEME = "system";

const state = {
    chosenTheme: SYSTEM_THEME, // light|dark|system
    appliedTheme: DEFAULT_THEME // light|dark
};

const showActiveTheme = () => {
    let themeIndicatorEls = document.querySelectorAll(".blazorbootstrap-theme-indicator>i");
    if (themeIndicatorEls.length > 0) {
        themeIndicatorEls.forEach((el, index) => {
            if (state.chosenTheme === SYSTEM_THEME
                && state.chosenTheme !== state.appliedTheme) {
                el.className = "bi bi-circle-half";
            }
            else if (state.appliedTheme === LIGHT_THEME) {
                el.className = "bi bi-sun-fill";
            } else if (state.appliedTheme === DARK_THEME) {
                el.className = "bi bi-moon-stars-fill";
            } else {
                el.className = "bi bi-circle-half";
            }
        });
    }

    let themeSwitcherItemEls = document.querySelectorAll(".blazorbootstrap-theme-item>button");
    if (themeSwitcherItemEls) {
        themeSwitcherItemEls.forEach((el) => {
            const bsThemeValue = el.dataset.bsThemeValue;
            const iEl = el.querySelector(".bi.bi-check2");
            if (state.chosenTheme === bsThemeValue) {
                el.classList.add("active");
                if (iEl)
                    iEl.classList.remove("d-none");
            } else {
                el.classList.remove("active");
                if (iEl)
                    iEl.classList.add("d-none");
            }
        });
    }
};

export function setTheme(dotNetHelper, theme, save = true) {
    state.chosenTheme = theme;
    state.appliedTheme = theme;

    if (theme === SYSTEM_THEME) {
        state.appliedTheme = window.matchMedia(`(prefers-color-scheme: ${DARK_THEME})`).matches ? DARK_THEME : LIGHT_THEME;
    }

    document.documentElement.setAttribute("data-bs-theme", state.appliedTheme);
    if (save) {
        window.localStorage.setItem(STORAGE_KEY, state.chosenTheme);
    }
    showActiveTheme();
    if (dotNetHelper)
        dotNetHelper.invokeMethodAsync("OnThemeChangedJS", state.appliedTheme);
};

export function initializeTheme(dotNetHelper) {
    const localTheme = window.localStorage.getItem(STORAGE_KEY);
    if (localTheme) {
        setTheme(dotNetHelper, localTheme, false);
    } else {
        setTheme(dotNetHelper, SYSTEM_THEME);
    }

    // register events
    window
        .matchMedia(`(prefers-color-scheme: ${DARK_THEME})`)
        .addEventListener("change", (event) => {
            //const theme = event.matches ? DARK_THEME : LIGHT_THEME;
            //setTheme(theme);
            setTheme(dotNetHelper, SYSTEM_THEME);
        });
}
