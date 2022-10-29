namespace BlazorBootstrap;

public partial class PaginationLink : BaseComponent
{
    #region Members

    #endregion

    #region Methods

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(LinkAriaLabel))
        {
            Attributes?.Add("aria-label", LinkAriaLabel); // TODO: this is not working revisit again
        }

        base.OnParametersSet();
    }

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.PaginationLink());

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties

    [Parameter] public string Text { get; set; }

    [Parameter] public string LinkText { get; set; }

    [Parameter] public IconName LinkIcon { get; set; }

    [Parameter] public string LinkAriaLabel { get; set; }

    #endregion
}

