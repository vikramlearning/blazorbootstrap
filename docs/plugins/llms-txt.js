// @ts-check
/**
 * Writes build/llms.txt (llmstxt.org format) for the docs site: an H1, a summary, then one H2 section per docs
 * category linking every docs page by its HTML URL with its description. Blog posts are left out (work item
 * 26112202, D-007). No dependency beyond Docusaurus itself; any error fails the build.
 */
const fs = require('fs');
const path = require('path');

/** Link text: drop bold markers and escape only what can end the link text early. @param {string} value */
const linkText = value => value.replace(/\*\*/g, '').replace(/\\/g, '\\\\').replace(/\[/g, '\\[').replace(/\]/g, '\\]');

/** @param {string} value */
const oneLine = value => value.replace(/\s+/g, ' ').trim();

/** @type {import('@docusaurus/types').PluginModule} */
module.exports = function llmsTxtPlugin(context) {
    /** @type {{ title: string, description: string, permalink: string, category: string, position: number }[]} */
    let pages = [];

    return {
        name: 'llms-txt',

        async allContentLoaded({ allContent }) {
            const docsContent = /** @type {any} */ (allContent['docusaurus-plugin-content-docs'])?.default;
            const version = docsContent?.loadedVersions?.[0];
            if (!version) throw new Error('llms-txt: the docs plugin content was not found.');

            pages = version.docs
                .filter(doc => !doc.unlisted && !doc.draft)
                .map(doc => ({
                    title: doc.title,
                    description: doc.description ?? '',
                    // The site uses trailingSlash: false, so an index page's "/showcase/" is served at "/showcase".
                    permalink: doc.permalink.length > 1 ? doc.permalink.replace(/\/$/, '') : doc.permalink,
                    category: (doc.sourceDirName ?? '').split('/')[0],
                    position: doc.sidebarPosition ?? Number.MAX_SAFE_INTEGER,
                }));
        },

        async postBuild({ outDir, siteConfig }) {
            const docsDir = path.join(context.siteDir, 'docs');
            const categories = [...new Set(pages.map(page => page.category))].sort();
            const label = category => {
                const file = path.join(docsDir, category, '_category_.json');
                return fs.existsSync(file) ? JSON.parse(fs.readFileSync(file, 'utf8')).label : category;
            };

            const lines = [
                '# Blazor Bootstrap',
                '',
                '> Blazor Bootstrap is a free, open-source library of Bootstrap 5.3 components for Blazor Server, Blazor WebAssembly '
                + 'and .NET MAUI Blazor Hybrid apps, published on NuGet as Blazor.Bootstrap for .NET 8, 9, 10 and 11. '
                + 'These docs cover setup, layout, every component\'s parameters, methods and events, and services. '
                + 'Live examples with source code are on the demos site.',
                '',
                '## Start here',
                '',
                `- [Docs home](${siteConfig.url}/): what Blazor Bootstrap is and where to begin`,
                '',
            ];

            for (const category of categories) {
                lines.push(`## ${label(category)}`, '');
                for (const page of pages.filter(p => p.category === category).sort((a, b) => a.position - b.position || a.title.localeCompare(b.title))) {
                    const note = page.description ? `: ${oneLine(page.description)}` : '';
                    lines.push(`- [${linkText(page.title)}](${siteConfig.url}${page.permalink})${note}`);
                }
                lines.push('');
            }

            lines.push('## Optional', '');
            lines.push(`- [Demos site](https://demos.blazorbootstrap.com/): live examples of every component with source code`);
            lines.push(`- [Demos site llms.txt](https://demos.blazorbootstrap.com/llms.txt): index of the live examples`);
            lines.push(`- [Blog](${siteConfig.url}/blog): release notes and upgrade guides`);
            lines.push('');

            fs.writeFileSync(path.join(outDir, 'llms.txt'), lines.join('\n'), 'utf8');
        },
    };
};
