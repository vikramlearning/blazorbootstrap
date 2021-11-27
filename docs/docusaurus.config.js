// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require('prism-react-renderer/themes/github');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'Blazor Bootstrap',
  tagline: 'Build fast, responsive sites with Blazor Bootstrap',
  url: 'https://vikramlearning.github.io',
  baseUrl: '/blazorbootstrap/',
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
          editUrl: 'https://github.com/vikramlearning/blazorbootstrap/edit/masters/docs/docs',
        },
        blog: {
          showReadingTime: true,
          editUrl:
            'https://github.com/vikramlearning/blazorbootstrap/edit/master/docs/blog/',
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
        title: 'Blazor Bootstrap',
        logo: {
          alt: 'My Site Logo',
          src: 'img/logo.svg',
        },
        items: [
          {
            type: 'doc',
            docId: 'intro',
            position: 'left',
            label: 'Tutorial',
          },
          {to: '/blog', label: 'Blog', position: 'left'},
          {
            href: 'https://github.com/vikramlearning/blazorbootstrap',
            label: 'GitHub',
            position: 'right',
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
                label: 'Tutorial',
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
                label: 'Discord',
                href: 'https://discordapp.com/invite/blazorbootstrap',
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
              {
                label: 'Blog',
                to: '/blog',
              },
              {
                label: 'GitHub',
                href: 'https://github.com/vikramlearning/blazorbootstrap',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} Blazor Bootstrap.`,
      },
      prism: {
        theme: lightCodeTheme,
        darkTheme: darkCodeTheme,
      },
    }),
};

module.exports = config;
