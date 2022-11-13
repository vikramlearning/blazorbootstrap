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
    favicon: 'img/logo/48X48.png',
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
                title: 'BlazorBootstrap',
                logo: {
                    alt: 'BlazorBootstrap',
                    src: 'img/logo.svg',
                },
                items: [
                    { type: 'doc', docId: 'intro', label: 'Docs', position: 'left' },
                    { to: '/blog', label: 'Blog', position: 'left' },
                    { href: 'https://demos.getblazorbootstrap.com', label: 'Demos', position: 'left' },
                    { href: 'https://github.com/vikramlearning/blazorbootstrap', label: 'GitHub', position: 'right', },
                ],
            },
            footer: {
                style: 'dark',
                links: [
                    {
                        title: 'Docs',
                        items: [
                            { label: 'Introduction', to: '/docs/intro', },
                        ],
                    },
                    {
                        title: 'Community',
                        items: [
                            { label: 'Stack Overflow', href: 'https://stackoverflow.com/questions/tagged/blazorbootstrap', },
                            { label: 'Twitter', href: 'https://twitter.com/blazorbootstrap', },
                        ],
                    },
                    {
                        title: 'More',
                        items: [
                            { label: 'Blog', to: '/blog', },
                            { label: 'GitHub', href: 'https://github.com/vikramlearning/blazorbootstrap', },
                        ],
                    },
                ],
                copyright: `Copyright © ${new Date().getFullYear()} Vikram Learning.`,
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
