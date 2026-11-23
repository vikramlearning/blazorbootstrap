# Blazor Bootstrap documentation

This folder holds the source of the Blazor Bootstrap documentation site, <https://docs.blazorbootstrap.com/>. It is built with [Docusaurus 3](https://docusaurus.io/), a static website generator, and needs Node.js 20 or later.

## Installation

```
npm install
```

## Local development

```
npm start
```

This command starts a local development server and opens a browser window. Most changes are reflected live without restarting the server.

## Build

```
npm run build
```

This command generates static content into the `build` folder. To check the production build locally, run `npm run serve`.

## Deployment

The site is deployed to GitHub Pages by the `.github/workflows/publish-docs.yml` workflow, which runs `npm run deploy` when a production release is requested.
