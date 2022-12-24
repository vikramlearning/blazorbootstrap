namespace BlazorBootstrap;

public partial class Sidebar : BaseComponent
{
    #region Events

    #endregion Events

    #region Members

    private bool expanded = true;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append("bb-sidebar d-flex flex-column flex-shrink-0");
        builder.Append("expanded", expanded);
        builder.Append("collapsed", !expanded);

        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        Attributes ??= new Dictionary<string, object>();

        await base.OnInitializedAsync();
    }

    private void OnExpandTogglClick()
    {
        expanded = !expanded;
        DirtyClasses();
        StateHasChanged();
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    #endregion Properties
}
