const { themes } = require('prism-react-renderer');

const lightCodeTheme = themes.vsDark;
const darkCodeTheme = themes.dracula;

/**
 * Sitemap lastmod for pages without a source file. A docs page or blog post gets the date its file last changed in
 * git (lastmod: 'date'). A blog listing page changes when a post it lists changes, so it gets the newest date among
 * those posts: a tag page from the posts with that tag, every other blog listing (and the search page) from all posts.
 */
async function addListingLastmod({ defaultCreateSitemapItems, ...params }) {
    const items = await defaultCreateSitemapItems(params);
    const lastmodByPath = new Map(items.map(item => [new URL(item.url).pathname, item.lastmod]));
    const archive = params.routes.find(route => route.path === '/blog/archive');
    const posts = archive?.props?.archive?.blogPosts ?? [];
    const newest = dates => dates.filter(Boolean).sort().at(-1);
    const newestPost = newest(posts.map(post => lastmodByPath.get(post.metadata.permalink)));

    return items.map(item => {
        if (item.lastmod) return item;
        const path = new URL(item.url).pathname;
        const tagPath = path.match(/^\/blog\/tags\/[^/]+/)?.[0];
        const lastmod = tagPath
            ? newest(posts.filter(post => post.metadata.tags.some(tag => tag.permalink === tagPath)).map(post => lastmodByPath.get(post.metadata.permalink)))
            : newestPost;
        return lastmod ? { ...item, lastmod } : item;
    });
}

/** @type {import('@docusaurus/types').Config} */
const config = {
    title: 'Blazor Bootstrap Docs',
    tagline: 'High-performance, lightweight, and responsive blazor bootstrap components in a single package from the developers for the developers.',
    url: 'https://docs.blazorbootstrap.com',
    baseUrl: '/',
    onBrokenLinks: 'throw',
    markdown: {
        hooks: {
            onBrokenMarkdownLinks: 'warn',
        },
    },
    favicon: 'favicon.ico',
    headTags: [
        { tagName: 'link', attributes: { rel: 'icon', type: 'image/svg+xml', sizes: 'any', href: '/favicon.svg' } },
        { tagName: 'link', attributes: { rel: 'apple-touch-icon', sizes: '180x180', href: '/apple-touch-icon.png' } },
    ],
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
                    onInlineAuthors: 'ignore', // posts declare their author inline in front matter
                    onUntruncatedBlogPosts: 'ignore',
                    editUrl: 'https://github.com/vikramlearning/blazorbootstrap/edit/master/docs/blog/',
                },
                theme: {
                    customCss: require.resolve('./src/css/custom.css'),
                },
                // Each sitemap entry carries the date its source file last changed in git (work item 26112202).
                sitemap: {
                    lastmod: 'date',
                    createSitemapItems: addListingLastmod,
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
            image: 'img/social-card.png',
            announcementBar: {
                id: 'support_us',
                content: '⭐️ If you like Blazor Bootstrap, give it a star on <a target="_blank" rel="noopener noreferrer" href="https://github.com/vikramlearning/blazorbootstrap">GitHub</a>! ⭐️',
                //backgroundColor: '#fafbfc',
                //textColor: '#091E42',
                isCloseable: true,
            },
            navbar: {
                logo: {
                    alt: 'Blazor Bootstrap home',
                    src: 'img/logo/wordmark.svg',
                    srcDark: 'img/logo/wordmark-night.svg',
                    width: 176,
                    height: 32,
                },
                items: [
                    { type: 'doc', docId: 'getting-started/blazor-webassembly-net-11', label: 'Docs', position: 'left' },
                    { href: 'https://demos.blazorbootstrap.com', label: 'Demos', position: 'left' },
                    { to: '/blog', label: 'Blog', position: 'left' },
                    { href: 'https://github.com/vikramlearning/blazorbootstrap', label: 'GitHub', position: 'right', },
                    { href: 'https://twitter.com/blazorbootstrap', label: 'Twitter', position: 'right', },
                ],
            },
            footer: {
                style: 'light',
                logo: {
                    alt: 'Blazor Bootstrap home',
                    src: 'img/logo/wordmark.svg',
                    srcDark: 'img/logo/wordmark-night.svg',
                    href: '/',
                    target: '_self',
                    width: 176,
                    height: 32,
                },
                links: [
                    {
                        title: 'Product',
                        items: [
                            { label: 'Demos', href: 'https://demos.blazorbootstrap.com/', },
                            { label: 'Docs', to: '/getting-started/blazor-webassembly-net-11', },
                            { label: 'Showcase', href: 'https://demos.blazorbootstrap.com/showcase', },
                            { label: 'Blog', to: '/blog', },
                        ],
                    },
                    {
                        title: 'Resources',
                        items: [
                            { label: 'Getting started', to: '/getting-started/blazor-webassembly-net-11', },
                            { label: 'Install NuGet', to: '/getting-started/blazor-webassembly-net-11#install-nuget-package', },
                            { label: 'Starter templates', href: 'https://github.com/vikramlearning/blazorbootstrap-starter-templates', },
                        ],
                    },
                    {
                        title: 'Community',
                        items: [
                            { label: 'Issues', href: 'https://github.com/vikramlearning/blazorbootstrap/issues', },
                            { label: 'Discussions', href: 'https://github.com/vikramlearning/blazorbootstrap/discussions', },
                            { label: 'Open Collective', href: 'https://opencollective.com/blazorbootstrap', },
                            { label: 'Stack Overflow', href: 'https://stackoverflow.com/questions/tagged/blazor-bootstrap', },
                        ],
                    },
                ],
                copyright: `© ${new Date().getFullYear()} Blazor Bootstrap`,
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
        // Writes build/llms.txt, an index of every docs page for AI tools (work item 26112202).
        require.resolve('./plugins/llms-txt'),
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
                    // The .NET 8 getting-started pages were replaced by .NET 11 and .NET 10 pages (4.1.0).
                    { from: '/getting-started/blazor-webassembly-net-8', to: '/getting-started/blazor-webassembly-net-11' },
                    { from: '/getting-started/blazor-webassembly-standalone-net-8', to: '/getting-started/blazor-webassembly-net-11' },
                    { from: '/getting-started/blazor-webapp-server-global-net-8', to: '/getting-started/blazor-webapp-server-global-net-11' },
                    { from: '/getting-started/blazor-server-net-8', to: '/getting-started/blazor-webapp-server-global-net-11' },
                    { from: '/getting-started/blazor-webapp-auto-global-net-8', to: '/getting-started/blazor-webapp-auto-global-net-11' },
                    { from: '/getting-started/maui-blazor-net-8', to: '/getting-started/maui-blazor-net-11' },
                    {
                        from: '/getting-started/blazor-webassembly',
                        to: '/getting-started/blazor-webassembly-net-11',
                    },
                    {
                        from: '/getting-started/blazor-server',
                        to: '/getting-started/blazor-webapp-server-global-net-11',
                    },
                    {
                        from: '/docs/getting-started/maui-blazor',
                        to: '/getting-started/maui-blazor-net-11',
                    },
                    {
                        from: '/getting-started/maui-blazor',
                        to: '/getting-started/maui-blazor-net-11',
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
