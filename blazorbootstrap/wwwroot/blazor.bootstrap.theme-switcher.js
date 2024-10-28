// THEMES
const STORAGE_KEY = "blazorbootstrap-theme";
const DEFAULT_THEME = "light";
const SYSTEM_THEME = "system";

const state = {
    chosenTheme: SYSTEM_THEME, // light|dark|system
    appliedTheme: DEFAULT_THEME // light|dark
};

const showActiveTheme = () => {
    let $themeIndicator = document.querySelector(".blazorbootstrap-theme-indicator>i");
    if ($themeIndicator) {
        if (state.appliedTheme === "light") {
            $themeIndicator.className = "bi bi-sun-fill";
        } else if (state.appliedTheme === "dark") {
            $themeIndicator.className = "bi bi-moon-stars-fill";
        } else {
            $themeIndicator.className = "bi bi-circle-half";
        }
    }

    let $themeSwitchers = document.querySelectorAll(".blazorbootstrap-theme-item>button");
    if ($themeSwitchers) {
        $themeSwitchers.forEach((el) => {
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

export function setTheme(theme, save = true) {
    state.chosenTheme = theme;
    state.appliedTheme = theme;

    if (theme === SYSTEM_THEME) {
        state.appliedTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    document.documentElement.setAttribute("data-bs-theme", state.appliedTheme);
    if (save) {
        window.localStorage.setItem(STORAGE_KEY, state.chosenTheme);
    }
    showActiveTheme();
    updateDemoCodeThemeCss(state.appliedTheme);
};

export function initializeTheme() {
    const localTheme = window.localStorage.getItem(STORAGE_KEY);
    if (localTheme) {
        setTheme(localTheme, false);
    } else {
        setTheme(SYSTEM_THEME);
    }

    // register events
    window
        .matchMedia("(prefers-color-scheme: dark)")
        .addEventListener("change", (event) => {
            const theme = event.matches ? "dark" : "light";
            setTheme(theme);
        });
}

export function updateDemoCodeThemeCss(theme) {
    if (theme === "dark") {
        let prismThemeLightLinkEl = document.getElementById('prismThemeLightLink');
        if (prismThemeLightLinkEl)
            prismThemeLightLinkEl?.remove();

        let prismThemeDarkLinkEl = document.createElement("link");
        prismThemeDarkLinkEl.setAttribute("rel", "stylesheet");
        prismThemeDarkLinkEl.setAttribute("href", "/_content/BlazorBootstrap.Demo.RCL/css/prism-vsc-dark-plus.min.css");
        prismThemeDarkLinkEl.setAttribute("id", "prismThemeDarkLink");

        document.head.append(prismThemeDarkLinkEl);
    }
    else if (theme === "light") {
        let prismThemeDarkLinkEl = document.getElementById('prismThemeDarkLink');
        if (prismThemeDarkLinkEl)
            prismThemeDarkLinkEl?.remove();

        let prismThemeLightLinkEl = document.createElement("link");
        prismThemeLightLinkEl.setAttribute("rel", "stylesheet");
        prismThemeLightLinkEl.setAttribute("href", "/_content/BlazorBootstrap.Demo.RCL/css/prism-vs.min.css");
        prismThemeLightLinkEl.setAttribute("id", "prismThemeLightLink");

        document.head.append(prismThemeLightLinkEl);
    }
}