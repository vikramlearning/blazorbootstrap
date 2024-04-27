---
title: Blazor Bootstrap v1.5.0
authors:
  name: Vikram Reddy
  title: Creator
  url: https://github.com/gvreddy04
  image_url: https://avatars.githubusercontent.com/u/2337067
tags: [v1.5.0, blazor, bootstrap, bootstrap5, bootstrap-5, blazorbootstrap, buttons, blazorbuttons, dateinput, datepicker, blazordateinput, blazordatepicker, offcanvas, blazoroffcanvas, modal, blazormodal]
---

We are excited to release 1.5.0 with new DateInput component and other updates!!!

<img src="https://i.imgur.com/1mVjqQv.png" alt="Blazor Bootstrap: DateInput Component" />

<!--truncate-->

## What's new

- `DateInput` component
  - Generic type: DateOnly, DateOnly?, DateTime, and DateTime? data types supported
  - Max and Min range
  - Disable
  - Validations
  - ValueChanged event

## What's changed

- `Offcanvas` component
  - Render different components dynamically within the modal without iterating through possible types or using conditional logic.
  - If dynamically-rendered components have component parameters, pass them as an `IDictionary<string, object>`.
  - Pass event callbacks to a dynamic component.

- `Button` component
  - Dynamic tooltip support added

- `Tooltip` component
  - Dynamic tooltip support added

## Links
- [Demo Website - Blazor Server](https://demos.blazorbootstrap.com/)
- [Demo Website - Blazor WebAssembly](https://demos.getblazorbootstrap.com/)
- [Blazor DateInput Documentation](https://getblazorbootstrap.com/docs/forms/date-input)
- [Blazor Offcanvas Documentation](https://getblazorbootstrap.com/docs/components/offcanvas)
- [Blazor Button Documentation](https://getblazorbootstrap.com/docs/components/buttons)
- [Blazor Tooltip Documentation](https://getblazorbootstrap.com/docs/components/tooltips)