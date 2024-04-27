namespace BlazorBootstrap;

public partial class PaginationItem : BlazorBootstrapComponentBase
{
    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.PaginationItem)
        .AddClass(BootstrapClass.PaginationItemActive, Active)
        .AddClass(BootstrapClass.PaginationItemDisabled, Disabled)
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

    protected override void OnParametersSet()
    {
        if (Active)
            AdditionalAttributes?.Add("aria-current", "page");

        base.OnParametersSet();
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Indicate the currently active page.
    /// </summary>
    [Parameter]
    public bool Active {  get; set; }

    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>
    /// Used for links that appear un-clickable.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter] public IconName LinkIcon { get; set; }

    [Parameter] public string? LinkText { get; set; }

    [Parameter] public string? Text { get; set; }

    #endregion
}
