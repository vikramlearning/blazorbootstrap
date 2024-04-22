const lightCodeTheme = require('prism-react-renderer/themes/vsDark');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

/** @type {import('@docusaurus/types').Config} */
const config = {
    title: 'Blazor Bootstrap Docs',
    tagline: 'High-performance, lightweight, and responsive blazor bootstrap components in a single package from the developers for the developers.',
    url: 'https://docs.blazorbootstrap.com',
    baseUrl: '/',
    onBrokenLinks: 'throw',
    onBrokenMarkdownLinks: 'warn',
    favicon: 'img/logo/logo-color.svg',
    organizationName: 'vikramlearning',
    projectName: 'blazorbootstrap',
    trailingSlash: false,
    deploymentBranch: 'gh-pages',
    presets: [
        [
            '@docusaurus/preset-classic',
            /** @type {import('@docusaurus/preset-classic').Options} */
            ({
                docs: {
                    routeBasePath: '/', // Serve the docs at the site's root
                    sidebarPath: require.resolve('./sidebars.js'),
                    editUrl: 'https://github.com/vikramlearning/blazorbootstrap/edit/master/docs/',
                },
                blog: {
                    showReadingTime: true,
                    editUrl: 'https://github.com/vikramlearning/blazorbootstrap/edit/master/docs/blog/',
                },
                theme: {
                    customCss: require.resolve('./src/css/custom.css'),
                },
                // googleAnalytics: {
                //     trackingID: 'UA-214301343-1',
                //     // Optional fields.
                //     anonymizeIP: true, // Should IPs be anonymized?
                // },
                gtag: {
                    trackingID: 'G-60QGZHW8TG',
                    anonymizeIP: true,
                },
            }),
        ],
    ],
    themeConfig:
        /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
        ({
            announcementBar: {
                id: 'support_us',
                content: '⭐️ If you like Blazor Bootstrap, give it a star on <a target="_blank" rel="noopener noreferrer" href="https://github.com/vikramlearning/blazorbootstrap">GitHub</a>! ⭐️',
                //backgroundColor: '#fafbfc',
                //textColor: '#091E42',
                isCloseable: true,
            },
            navbar: {
                title: 'Blazor Bootstrap',
                logo: {
                    alt: 'Blazor Bootstrap',
                    src: 'img/logo.svg',
                },
                items: [
                    { type: 'doc', docId: 'getting-started/blazor-webassembly-net-8', label: 'Docs', position: 'left' },
                    { href: 'https://demos.blazorbootstrap.com', label: 'Demos', position: 'left' },
                    { to: '/blog', label: 'Blog', position: 'left' },
                    { href: 'https://github.com/vikramlearning/blazorbootstrap', label: 'GitHub', position: 'right', },
                    { href: 'https://twitter.com/blazorbootstrap', label: 'Twitter', position: 'right', },
                ],
            },
            footer: {
                style: 'dark',
                links: [
                    {
                        title: 'Links',
                        items: [
                            { label: 'Home', to: '/', },
                            { label: 'Demos', href: 'https://demos.blazorbootstrap.com/', },
                            { label: 'Blog', to: '/blog', },
                        ],
                    },
                    {
                        title: 'Guides',
                        items: [
                            { label: 'Getting started', to: '/getting-started/blazor-webassembly-net-8', },
                            { label: 'Install Nuget', to: '/getting-started/blazor-webassembly-net-8#install-nuget-package', },
                            { label: 'Starter templates', to: 'https://github.com/vikramlearning/blazorbootstrap-starter-templates', },
                        ],
                    },
                    {
                        title: 'Community',
                        items: [
                            { label: 'Issues', href: 'https://github.com/vikramlearning/blazorbootstrap/issues', },
                            { label: 'Discussions', href: 'https://github.com/vikramlearning/blazorbootstrap/discussions', },
                            { label: 'Stack Overflow', href: 'https://stackoverflow.com/questions/tagged/blazor-bootstrap', },
                            { label: 'Twitter', href: 'https://twitter.com/blazorbootstrap', },
                        ],
                    },
                ],
                copyright: `Copyright © ${new Date().getFullYear()} Blazor Bootstrap.`,
            },
            prism: {
                theme: lightCodeTheme,
                darkTheme: darkCodeTheme,
                additionalLanguages: ['csharp', 'cshtml'],
            },
        }),
    customFields: {
        version: '0.0.4',
        logo_white: '/img/logo/logo-white.svg',
        logo_color: '/img/logo/logo-color.svg'
    },
    plugins: [
        [
            require.resolve('@easyops-cn/docusaurus-search-local'),
            {
                indexDocs: true,
                indexPages: true,
                indexBlog: false,
                docsDir: 'docs',
                docsRouteBasePath: '/docs'
            }
        ],
        [
            '@docusaurus/plugin-client-redirects',
            {
                redirects: [
                    {
                        from: '/getting-started/blazor-webassembly',
                        to: '/getting-started/blazor-webassembly-net-8',
                    },
                    {
                        from: '/getting-started/blazor-server',
                        to: '/getting-started/blazor-webapp-server-global-net-8',
                    },
                    {
                        from: '/docs/getting-started/maui-blazor',
                        to: '/getting-started/maui-blazor-net-8',
                    },
                    {
                        from: '/getting-started/maui-blazor',
                        to: '/getting-started/maui-blazor-net-8',
                    },
                    {
                        from: '/docs/layout/blazor-webassembly',
                        to: '/layout/blazor-webassembly',
                    },
                    {
                        from: '/docs/layout/blazor-server',
                        to: '/layout/blazor-server',
                    },
                    {
                        from: '/docs/content/icons',
                        to: '/content/icons',
                    },
                    {
                        from: '/docs/forms/autocomplete',
                        to: '/forms/autocomplete',
                    },
                    {
                        from: '/docs/forms/currency-input',
                        to: '/forms/currency-input',
                    },
                    {
                        from: '/docs/forms/date-input',
                        to: '/forms/date-input',
                    },
                    {
                        from: '/docs/forms/number-input',
                        to: '/forms/number-input',
                    },
                    {
                        from: '/docs/forms/switch',
                        to: '/forms/switch',
                    },
                    {
                        from: '/docs/forms/time-input',
                        to: '/forms/time-input',
                    },
                    {
                        from: '/docs/components/accordion',
                        to: '/components/accordion',
                    },
                    {
                        from: '/docs/components/alerts',
                        to: '/components/alerts',
                    },
                    {
                        from: '/docs/components/badge',
                        to: '/components/badge',
                    },
                    {
                        from: '/docs/components/breadcrumb',
                        to: '/components/breadcrumb',
                    },
                    {
                        from: '/docs/components/buttons',
                        to: '/components/buttons',
                    },
                    {
                        from: '/docs/components/callout',
                        to: '/components/callout',
                    },
                    {
                        from: '/docs/components/card',
                        to: '/components/card',
                    },
                    {
                        from: '/docs/components/charts',
                        to: '/components/charts',
                    },
                    {
                        from: '/docs/components/collapse',
                        to: '/components/collapse',
                    },
                    {
                        from: '/docs/components/confirm-dialog',
                        to: '/components/confirm-dialog',
                    },
                    {
                        from: '/docs/components/dropdown',
                        to: '/components/dropdown',
                    },
                    {
                        from: '/docs/components/grid',
                        to: '/components/grid',
                    },
                    {
                        from: '/docs/components/modal',
                        to: '/components/modal',
                    },
                    {
                        from: '/docs/components/offcanvas',
                        to: '/components/offcanvas',
                    },
                    {
                        from: '/docs/components/pagination',
                        to: '/components/pagination',
                    },
                    {
                        from: '/docs/components/placeholders',
                        to: '/components/placeholders',
                    },
                    {
                        from: '/docs/components/preload',
                        to: '/components/preload',
                    },
                    {
                        from: '/docs/components/progress',
                        to: '/components/progress',
                    },
                    {
                        from: '/docs/components/sidebar',
                        to: '/components/sidebar',
                    },
                    {
                        from: '/docs/components/tabs',
                        to: '/components/tabs',
                    },
                    {
                        from: '/docs/components/toasts',
                        to: '/components/toasts',
                    },
                    {
                        from: '/docs/components/tooltips',
                        to: '/components/tooltips',
                    },
                    {
                        from: '/docs/data-visualization/bar-chart',
                        to: '/data-visualization/bar-chart',
                    },
                    {
                        from: '/docs/data-visualization/doughnut-chart',
                        to: '/data-visualization/doughnut-chart',
                    },
                    {
                        from: '/docs/data-visualization/line-chart',
                        to: '/data-visualization/line-chart',
                    },
                    {
                        from: '/docs/data-visualization/pie-chart',
                        to: '/data-visualization/pie-chart',
                    },
                    {
                        from: '/docs/services/modal',
                        to: '/services/modal',
                    },
                ]
            }
        ]
    ],
};

module.exports = config;
