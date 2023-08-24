namespace BlazorBootstrap;

public partial class PaginationItem : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.PaginationItem());
        builder.Append(BootstrapClassProvider.PaginationItemActive(), Active);
        builder.Append(BootstrapClassProvider.PaginationItemDisabled(), Disabled);

        base.BuildClasses(builder);
    }

    protected override void OnParametersSet()
    {
        if (Active)
            Attributes?.Add("aria-current", "page");

        base.OnParametersSet();
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Indicate the currently active page.
    /// </summary>
    [Parameter]
    public bool Active
    {
        get => State.Active;
        set
        {
            if (value == State.Active)
                return;

            State = State with { Active = value };

            DirtyClasses();
        }
    }

    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>
    /// Used for links that appear un-clickable.
    /// </summary>
    [Parameter]
    public bool Disabled
    {
        get => State.Disabled;
        set
        {
            if (value == State.Disabled)
                return;

            State = State with { Disabled = value };

            DirtyClasses();
        }
    }

    [Parameter] public IconName LinkIcon { get; set; }

    [Parameter] public string? LinkText { get; set; }

    /// <summary>
    /// Gets the reference to the pagination item state object.
    /// </summary>
    protected PaginationItemState State { get; private set; } = new();

    [Parameter] public string? Text { get; set; }

    #endregion
}
