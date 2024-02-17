﻿namespace BlazorBootstrap;

public partial class PaginationLink : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.PaginationLink);

        base.BuildClasses();
    }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(LinkAriaLabel)) Attributes?.Add("aria-label", LinkAriaLabel); // TODO: this is not working revisit again

        base.OnParametersSet();
    }

    #endregion

    #region Properties, Indexers

    [Parameter] public string? LinkAriaLabel { get; set; }

    [Parameter] public IconName LinkIcon { get; set; }

    [Parameter] public string? LinkText { get; set; }

    [Parameter] public string? Text { get; set; }

    #endregion
}
