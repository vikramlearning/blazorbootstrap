const lightCodeTheme = require('prism-react-renderer/themes/vsDark');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

/** @type {import('@docusaurus/types').Config} */
const config = {
    title: 'Blazor Bootstrap',
    tagline: 'High-performance, lightweight, and responsive blazor bootstrap components in a single package from the developers for the developers.',
    url: 'https://getblazorbootstrap.com',
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
                googleAnalytics: {
                    trackingID: 'UA-214301343-1',
                    // Optional fields.
                    anonymizeIP: true, // Should IPs be anonymized?
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
                    { type: 'doc', docId: 'getting-started/blazor-webassembly', label: 'Docs', position: 'left' },
                    { to: '/blog', label: 'Blog', position: 'left' },
                    { href: 'https://demos.blazorbootstrap.com', label: 'Demos', position: 'left' },
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
                            { label: 'Getting started', to: '/docs/getting-started/blazor-webassembly', },
                            { label: 'Install Nuget', to: '/docs/getting-started/blazor-webassembly#install-nuget-package', },
                            { label: 'Starter templates', to: 'https://github.com/vikramlearning/blazorbootstrap-starter-templates', },
                        ],
                    },
                    {
                        title: 'Community',
                        items: [
                            { label: 'Issues', href: 'https://github.com/vikramlearning/blazorbootstrap/issues', },
                            { label: 'Discussions', href: 'https://github.com/vikramlearning/blazorbootstrap/discussions', },
                            //{ label: 'Stack Overflow', href: 'https://stackoverflow.com/questions/tagged/blazorbootstrap', },
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
            //googleAnalytics: {
            //    trackingID: 'UA-214301343-1',
            //    // Optional fields.
            //    anonymizeIP: true, // Should IPs be anonymized?
            //},
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
        ]
    ],
};

module.exports = config;
