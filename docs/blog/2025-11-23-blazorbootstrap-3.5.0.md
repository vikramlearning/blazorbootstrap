---
title: Blazor Bootstrap v3.5.0
authors:
  name: Vikram Reddy
  title: Creator
  url: https://github.com/gvreddy04
  image_url: https://avatars.githubusercontent.com/u/2337067
tags: [v3.5.0, blazor, bootstrap, blazorbootstrap, enum, enuminput, pdf, pdfviewer]
---

We're excited to release version **3.5.0**. This update introduces new input and viewing capabilities (Enum Input, password‑protected PDF viewing), improves developer ergonomics, upgrades framework targeting to .NET 10, and includes documentation and UX refinements.

![image](https://private-user-images.githubusercontent.com/2337067/517874250-8a624590-1dfc-470a-8f9d-7636851d6c95.png?jwt=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3NjM5MjAxNzEsIm5iZiI6MTc2MzkxOTg3MSwicGF0aCI6Ii8yMzM3MDY3LzUxNzg3NDI1MC04YTYyNDU5MC0xZGZjLTQ3MGEtOGY5ZC03NjM2ODUxZDZjOTUucG5nP1gtQW16LUFsZ29yaXRobT1BV1M0LUhNQUMtU0hBMjU2JlgtQW16LUNyZWRlbnRpYWw9QUtJQVZDT0RZTFNBNTNQUUs0WkElMkYyMDI1MTEyMyUyRnVzLWVhc3QtMSUyRnMzJTJGYXdzNF9yZXF1ZXN0JlgtQW16LURhdGU9MjAyNTExMjNUMTc0NDMxWiZYLUFtei1FeHBpcmVzPTMwMCZYLUFtei1TaWduYXR1cmU9ZmVlNDlmZDZkNGQ5ZWUxMjE5MGM3NmEzMzc2OTdhYzk0MjY4YTU3NTk5M2JmMGIxNDNkYjI0ZWYyM2UwMDA5YyZYLUFtei1TaWduZWRIZWFkZXJzPWhvc3QifQ.hvU9-57w9n41vwE3eyy3ZcDPDYXbQ3p_JW1RmDmT-c8 "Blazor Bootstrap Enum Input Component")

<!--truncate-->

### ✨ New Features & Enhancements

- PdfViewer: Added support for password‑protected PDFs (Issue [#1184](https://github.com/vikramlearning/blazorbootstrap/issues/1184), PR [#1185](https://github.com/vikramlearning/blazorbootstrap/pull/1185)). When a PDF requires a password, the viewer now prompts the user instead of failing silently.
- Enum Input Component (Issue [#1196](https://github.com/vikramlearning/blazorbootstrap/issues/1196)). Introduces a strongly-typed input for enum values, simplifying binding and reducing boilerplate select markup.
- Documentation: Added Blazor Express sections and updated chart documentation (PR [#1169](https://github.com/vikramlearning/blazorbootstrap/pull/1169)) to clarify usage patterns and configuration options.

### 🐞 Bug Fixes

- SortableList: Grabcursor not showing (PR [#1152](https://github.com/vikramlearning/blazorbootstrap/pull/1152)). Corrected CSS so the expected drag cursor appears, improving usability for sortable interactions.
- Buttons Docs: Corrected the name of enum `ButtonSize` in `buttons.mdx` (PR [#1147](https://github.com/vikramlearning/blazorbootstrap/pull/1147)) ensuring accuracy for developers referencing the docs.

### ⬆️ Upgrades

- Target Frameworks: Added / upgraded multi-targeting to include **.NET 10.0** (PR [#1201](https://github.com/vikramlearning/blazorbootstrap/pull/1201)). Ensures forward compatibility and access to latest runtime features.
- Configuration: Google Map key handling updates (PR [#1165](https://github.com/vikramlearning/blazorbootstrap/pull/1165)) to streamline external service setup.

Special thanks to the community for continued feedback and adoption.

### Links
- [Docs Website - Blazor Bootstrap](https://docs.blazorbootstrap.com/)
- [Demos Website - Blazor Bootstrap](https://demos.blazorbootstrap.com/)
