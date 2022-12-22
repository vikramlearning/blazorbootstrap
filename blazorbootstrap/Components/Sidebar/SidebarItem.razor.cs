namespace BlazorBootstrap;

public partial class SidebarItem : BaseComponent
{
    #region Events

    #endregion Events

    #region Members

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    { 
        Attributes ??= new Dictionary<string, object>();

        await base.OnInitializedAsync();
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public string Href { get; set; }

    [Parameter] public IconName PrefixIconName { get; set; }

    [Parameter] public string Text { get; set; }

    // TODO: add target support

    #endregion Properties
}
