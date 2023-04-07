---
title: Blazor Accordion Component
description: Build vertically collapsing accordions in combination with our Collapse component.
image: https://i.imgur.com/g4zpMXp.png

sidebar_label: Accordion
sidebar_position: 1
---

# Blazor Accordion

Build vertically collapsing accordions in combination with our Collapse component.

<img src="https://i.imgur.com/g4zpMXp.png" alt="Blazor Bootstrap: Accordion Component" />

## Accordion Parameters

| Name | Type | Default | Required | Descritpion | Added Version |
|:--|:--|:--|:--|:--|:--|
| ChildContent | RenderFragment | null | ✔️ | Specifies the content to be rendered inside this <see cref="Collapse"/>.| 1.7.0 |
| Flush | bool | false | | Removes borders and rounded corners to render accordions edge-to-edge with their parent container. | 1.7.0 |
| AlwaysOpen | bool | false | | It makes accordion items stay open when another item is opened. | 1.7.0 |

## Accordion Methods

| Name | Description | Added Version |
|:--|:--|:--|
| ShowFirstAccordionItemAsync | Show the first `AccordionItem`. | 1.7.0 |
| ShowLastAccordionItemAsync | Show the last `AccordionItem`. | 1.7.0 |
| ShowAccordionItemByNameAsync | Show the `AccordionItem` by name. | 1.7.0 |
| ShowAccordionItemByIndexAsync | Show the `AccordionItem` by index. | 1.7.0 |

## Accordion Events

| Name | Description | Added Version |
|:--|:--|:--|
| OnShowing | This event fires immediately when the show method is called. | 1.7.0 |
| OnShown | This event is fired when a accordion item has been made visible to the user (will wait for CSS transitions to complete). | 1.7.0 |
| OnHiding | This event is fired immediately when the hide method has been called. | 1.7.0 |
| OnHidden | This event is fired when a accordion item has been hidden from the user (will wait for CSS transitions to complete). | 1.7.0 |

## AccordionItem Parameters

| Name | Type | Default | Required | Descritpion | Added Version |
|:--|:--|:--|:--|:--|:--|
| Content | RenderFragment | null | ✔️ | Specifies the content to be rendered inside the `AccordionItem`. | 1.7.0 |
| IsActive | bool | false | | Gets or sets the active `AccordionItem`. | 1.7.0 |
| Name | string | null | | Gets or sets the name. | 1.7.0 |
| Title | string | null | | Gets or sets the `AccordionItem` title. | 1.7.0 |
| TitleTemplate | RenderFragment | null | | Gets or sets the `AccordionItem` title template. | 1.7.0 |

## Examples

### Accordion

Click the accordions below to expand/collapse the accordion content.

<img src="https://i.imgur.com/IcfmBek.png" alt="Blazor Bootstrap: Accordion Component - Examples" />

```cshtml {} showLineNumbers
<Accordion>
    <AccordionItem Title="Accordion Item #1">
        <Content>
            <b>This is the first item's accordion body.</b> It is shown by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Accordion Item #2">
        <Content>
            <b>This is the second item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Accordion Item #3">
        <Content>
            <b>This is the third item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
</Accordion>
```
[See demo here.](https://demos.blazorbootstrap.com/accordion#examples)

### Title with Icon

To customize the accordion title, use `TitleTemplate`, as shown in the below example.

<img src="https://i.imgur.com/nkIyXbb.png" alt="Blazor Bootstrap: Accordion Component - Title with Icon" />

```cshtml {} showLineNumbers
<Accordion>
    <AccordionItem>
        <TitleTemplate>
            <Icon Name="IconName.HouseFill" Class="me-1" /> Accordion Item #1
        </TitleTemplate>
        <Content>
            <b>This is the first item's accordion body.</b> It is shown by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem>
        <TitleTemplate>
            <Icon Name="IconName.PersonFill" Class="me-1" /> Accordion Item #2
        </TitleTemplate>
        <Content>
            <b>This is the second item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem>
        <TitleTemplate>
            <Icon Name="IconName.PhoneFill" Class="me-1" /> Accordion Item #3
        </TitleTemplate>
        <Content>
            <b>This is the third item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
</Accordion>
```
[See demo here.](https://demos.blazorbootstrap.com/accordion#title-with-icon)

### Flush

Set the `Flush` parameter to `true` to remove borders and rounded corners to render accordions edge-to-edge with their parent container.

<img src="https://i.imgur.com/8yqF7iY.png" alt="Blazor Bootstrap: Accordion Component - Flush" />

```cshtml {} showLineNumbers
<Accordion Flush="true">
    <AccordionItem Title="Accordion Item #1">
        <Content>
            <b>This is the first item's accordion body.</b> It is shown by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Accordion Item #2">
        <Content>
            <b>This is the second item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Accordion Item #3">
        <Content>
            <b>This is the third item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
</Accordion>
```
[See demo here.](https://demos.blazorbootstrap.com/accordion#flush)

### Set default active accordion item

Set the `IsActive` parameter to `true` to keep the accordion item open by default.

<img src="https://i.imgur.com/FkbNpZN.png" alt="Blazor Bootstrap: Accordion Component - Set default active accordion item" />

```cshtml {} showLineNumbers
<Accordion>
    <AccordionItem Title="Accordion Item #1">
        <Content>
            <b>This is the first item's accordion body.</b> It is shown by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Accordion Item #2" IsActive="true">
        <Content>
            <b>This is the second item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Accordion Item #3">
        <Content>
            <b>This is the third item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
</Accordion>
```
[See demo here.](https://demos.blazorbootstrap.com/accordion#set-default-active-accordion-item)

### Always open

Set the `AlwaysOpen` parameter to `true` to keep accordion items open when another item is opened.

<img src="https://i.imgur.com/E2AbjKh.png" alt="Blazor Bootstrap: Accordion Component - Always open" />

```cshtml {} showLineNumbers
<Accordion AlwaysOpen="true">
    <AccordionItem Title="Accordion Item #1">
        <Content>
            <b>This is the first item's accordion body.</b> It is shown by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Accordion Item #2">
        <Content>
            <b>This is the second item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Accordion Item #3">
        <Content>
            <b>This is the third item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
</Accordion>
```
[See demo here.](https://demos.blazorbootstrap.com/accordion#always-open)

### Activate individual accordion items

You can activate individual accordion items in several ways. 
Use predefined methods `ShowFirstAccordionItemAsync`, `ShowLastAccordionItemAsync`, `ShowAccordionItemByNameAsync`, and `ShowAccordionItemByIndexAsync` as shown below.

<img src="https://i.imgur.com/JpzHrws.png" alt="Blazor Bootstrap: Accordion Component - Activate individual accordion items" />

```cshtml {} showLineNumbers
<Accordion @ref="accordion1" Class="mb-3">
    <AccordionItem Title="Home" Name="AccordionItem1">
        <Content>
            <b>This is the first item's accordion body.</b> It is shown by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Profile" Name="AccordionItem2">
        <Content>
            <b>This is the second item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Contact" Name="AccordionItem3">
        <Content>
            <b>This is the third item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Products" Name="Products">
        <Content>
            <b>This is the fourth item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="FAQs" Name="FAQ">
        <Content>
            <b>This is the fifth item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="About" Name="AccordionItem6">
        <Content>
            <b>This is the sixth item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
</Accordion>

<Button Color="ButtonColor.Primary" @onclick="ShowFirstAccordionItemAsync">First Accordion Item</Button>
<Button Color="ButtonColor.Primary" @onclick="ShowSecondAccordionItemAsync">Second Accordion Item</Button>
<Button Color="ButtonColor.Primary" @onclick="ShowThirdAccordionItemAsync">Third Accordion Item</Button>
<Button Color="ButtonColor.Primary" @onclick="ShowProductsAccordionItemAsync">Products Accordion Item</Button>
<Button Color="ButtonColor.Primary" @onclick="ShowFaqsAccordionItemAsync">FAQs Accordion Item</Button>
<Button Color="ButtonColor.Primary" @onclick="ShowLastAccordionItemAsync">Last Accordion Item</Button>
```

```cshtml {} showLineNumbers
@code {
    private Accordion accordion1 = default!;

    private async Task ShowFirstAccordionItemAsync() => await accordion1.ShowFirstAccordionItemAsync();
    private async Task ShowSecondAccordionItemAsync() => await accordion1.ShowAccordionItemByIndexAsync(1);
    private async Task ShowThirdAccordionItemAsync() => await accordion1.ShowAccordionItemByIndexAsync(2);
    private async Task ShowProductsAccordionItemAsync() => await accordion1.ShowAccordionItemByNameAsync("Products");
    private async Task ShowFaqsAccordionItemAsync() => await accordion1.ShowAccordionItemByNameAsync("FAQ");
    private async Task ShowLastAccordionItemAsync() => await accordion1.ShowLastAccordionItemAsync();
}
```

[See demo here.](https://demos.blazorbootstrap.com/accordion#activate-individual-accordion-items)

### Events Example

Blazor Bootstrap Accordion component exposes a few events for hooking into accordion functionality.

<img src="https://i.imgur.com/IcfmBek.png" alt="Blazor Bootstrap: Accordion Component - Events Example" />

```cshtml {} showLineNumbers
<Accordion @ref="accordion1"
           OnShowing="OnShowingAsync"
           OnShown="OnShownAsync"
           OnHiding="OnHidingAsync"
           OnHidden="OnHiddenAsync">
    <AccordionItem Title="Accordion Item #1" Name="AccordionItem1">
        <Content>
            <b>This is the first item's accordion body.</b> It is shown by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Accordion Item #2" Name="AccordionItem2">
        <Content>
            <b>This is the second item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
    <AccordionItem Title="Accordion Item #3" Name="AccordionItem3">
        <Content>
            <b>This is the third item's accordion body.</b> It is hidden by default, until the collapse plugin adds the appropriate classes that we use to style each element. These classes control the overall appearance, as well as the showing and hiding via CSS transitions. You can modify any of this with custom CSS or overriding our default variables. It's also worth noting that just about any HTML can go within the .accordion-body, though the transition does limit overflow.
        </Content>
    </AccordionItem>
</Accordion>
```

```cshtml {} showLineNumbers
@code {
    private Accordion accordion1 = default!;

    [Inject] ToastService ToastService { get; set; } = default!;

    private void OnShowingAsync(AccordionEventArgs args)
    {
        ToastService.Notify(new ToastMessage(
            type: ToastType.Primary,
            message: $"Event Name: Showing, AccordionItemName: {args.Name}, AccordionItemTitle: {args.Title}"));
    }

    private void OnShownAsync(AccordionEventArgs args)
    {
        ToastService.Notify(new ToastMessage(
            type: ToastType.Primary,
            message: $"Event Name: OnShown, AccordionItemName: {args.Name}, AccordionItemTitle: {args.Title}"));
    }

    private void OnHidingAsync(AccordionEventArgs args)
    {
        ToastService.Notify(new ToastMessage(
            type: ToastType.Primary,
            message: $"Event Name: OnHiding, AccordionItemName: {args.Name}, AccordionItemTitle: {args.Title}"));
    }

    private void OnHiddenAsync(AccordionEventArgs args)
    {
        ToastService.Notify(new ToastMessage(
            type: ToastType.Primary,
            message: $"Event Name: OnHidden, AccordionItemName: {args.Name}, AccordionItemTitle: {args.Title}"));
    }
}
```

[See demo here.](https://demos.blazorbootstrap.com/accordion#events)