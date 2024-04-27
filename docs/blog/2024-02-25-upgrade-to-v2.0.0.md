---
title: Upgrade to v2.x.x
authors:
  name: Vikram Reddy
  title: Creator
  url: https://github.com/gvreddy04
  image_url: https://avatars.githubusercontent.com/u/2337067
---

### Recommendation

We strongly recommend all `BlazorBootstrap` users migrate to version **2.x.x**.

If you are using version `1.x.x` for non-commercial purposes, you may continue using them, but upgrading to version `2.x.x` is strongly recommended.

If you are using version `1.x.x` for commercial purposes, you are no longer permitted to do so due to code usage concerns regarding Blazorise's ClassBuilder and StyleBuilder classes.

### Background of ClassBuilder & StyleBuilder References

In March 2021, we created a component library proof-of-concept (POC) using `ClassBuilder` and `StyleBuilder` from Blazorise. At that time, Blazorise was under the MIT license. We inadvertently neglected to re-verify the Blazorise project's license later.

On February 12th, 2024, Blazorise contacted us regarding a potential licensing concern with both `ClassBuilder` and `StyleBuilder` classes. We respect Blazorise's dedication to open-source software and responsible licensing.

We have now removed all references to ClassBuilder and StyleBuilder from our codebase.

:::danger IMPORTANT
If you wish to continue using version `1.x.x` for commercial purposes, please reach out to the Blazorise team for licensing.
:::
