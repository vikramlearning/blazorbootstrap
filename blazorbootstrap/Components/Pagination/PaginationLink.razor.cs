namespace BlazorBootstrap;

public partial class PaginationLink : BlazorBootstrapComponentBase
{
    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.PaginationLink)
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(LinkAriaLabel)) 
            AdditionalAttributes?.Add("aria-label", LinkAriaLabel); // TODO: this is not working revisit again

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
