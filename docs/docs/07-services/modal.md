---
title: Blazor Modal Service
description: Use Blazor Bootstrap modal service to show quick dialogs to your site for lightboxes, user notifications, or completely custom content.
image: https://i.imgur.com/Tze7msN.png

sidebar_label: Modal Service
sidebar_position: 1
---

# Blazor Modal Service

Use Blazor Bootstrap modal service to show quick dialogs to your site for lightboxes, user notifications, or completely custom content.


<img src="https://i.imgur.com/Tze7msN.png" alt="Blazor Modal Service" />
<br />
<a href="https://demos.blazorbootstrap.com/modal-service">See blazor modal service demo here.</a>

## Methods

| Name | Return Type | Descritpion | Added Version |
|:--|:--|
| ShowAsync(**ModalOption** modalOption) | Task | Show the modal. | 1.9.0 |

## ModalOption Members

| Property Name | Type | Description | Required | Default | Added Version |
|:--|:--|:--|:--|:--|
| FooterButtonColor | `ButtonColor` | Gets or sets the footer button color. | | `ButtonColor.Secondary` | 1.9.0 |
| FooterButtonCSSClass | string | Gets or sets the footer button custom CSS class. | | null | 1.9.0 |
| FooterButtonText | string | Gets or sets the footer button text. | | OK | 1.9.0 |
| IsVerticallyCentered | bool | Gets or sets the IsVerticallyCentered. | | false | 1.9.0 |
| Message | string | Gets or sets the modal message. | ✔️ | null | 1.9.0 |
| ShowFooterButton | string | Shows or hides the footer button. | | true | 1.9.0 |
| Size | `ModalSize` | Gets or sets the modal size. | | `ModalSize.Regular` | 1.9.0 |
| Title | string | Gets or sets the modal title. | ✔️ | null | 1.9.0 |
| Type | `ModalType` | Gets or sets the modal type. | | `ModalType.Light` | 1.9.0 |

