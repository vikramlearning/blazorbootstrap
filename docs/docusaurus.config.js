// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

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
    favicon: 'img/favicon.ico',
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
            }),
        ],
    ],

    themeConfig:
        /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
        ({
            navbar: {
                title: 'BlazorBootstrap',
                logo: {
                    alt: 'BlazorBootstrap',
                    src: 'img/logo.svg',
                },
                items: [
                    {
                        type: 'doc',
                        docId: 'intro',
                        position: 'left',
                        label: 'Docs',
                    },
                    //{
                    //    to: '/blog',
                    //    position: 'left',
                    //    label: 'Blog'
                    //},
                    {
                        href: 'https://github.com/vikramlearning/blazorbootstrap',
                        position: 'right',
                        label: 'GitHub',
                    },
                ],
            },
            footer: {
                style: 'dark',
                links: [
                    {
                        title: 'Docs',
                        items: [
                            {
                                label: 'Introduction',
                                to: '/docs/intro',
                            },
                        ],
                    },
                    {
                        title: 'Community',
                        items: [
                            {
                                label: 'Stack Overflow',
                                href: 'https://stackoverflow.com/questions/tagged/blazorbootstrap',
                            },
                            {
                                label: 'Twitter',
                                href: 'https://twitter.com/blazorbootstrap',
                            },
                        ],
                    },
                    {
                        title: 'More',
                        items: [
                            //{
                            //    label: 'Blog',
                            //    to: '/blog',
                            //},
                            {
                                label: 'GitHub',
                                href: 'https://github.com/vikramlearning/blazorbootstrap',
                            },
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
        }),
    customFields: {
        version: '0.0.2'
    }
};

module.exports = config;
