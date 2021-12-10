const lightCodeTheme = require('prism-react-renderer/themes/github');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

/** @type {import('@docusaurus/types').Config} */
const config = {
    title: 'BlazorBootstrap',
    tagline: 'Build fast, responsive sites with BlazorBootstrap',
    url: 'https://getblazorbootstrap.com',
    baseUrl: '/',
    onBrokenLinks: 'throw',
    onBrokenMarkdownLinks: 'warn',
    favicon: 'img/logo/48X48.png',
    organizationName: 'vikramlearning',
    projectName: 'blazorbootstrap',
    trailingSlash: false,
    deploymentBranch: 'gh-pages',

    themeConfig: {
        navbar: {
            title: 'BlazorBootstrap',
            logo: {
                alt: 'BlazorBootstrap',
                src: 'img/logo.svg',
            },
            items: [
                { type: 'doc', docId: 'intro', label: 'Doc', position: 'left' },
                //{ to: '/blog', label: 'Blog', position: 'left' },
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
                        //{ label: 'Blog', to: '/blog', },
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
        googleAnalytics: {
            trackingID: 'UA-214301343-1',
            // Optional fields.
            anonymizeIP: true, // Should IPs be anonymized?
        },
    },
    presets: [
        [
            '@docusaurus/preset-classic',
            {
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
            },
        ],
    ],
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
    customFields: {
        version: '0.0.2'
    }
};

module.exports = config;
