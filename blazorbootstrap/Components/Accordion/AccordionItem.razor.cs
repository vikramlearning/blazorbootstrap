namespace BlazorBootstrap;

/// <summary>
/// Represents an item within an <see cref="Accordion"/>.
/// </summary>
public partial class AccordionItem : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private Collapse collapse = default!;

    private bool isCollapsed = true;

    #endregion

    #region Methods
    
    
    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (TitleTemplate is not null && !string.IsNullOrWhiteSpace(Title)) 
            throw new InvalidOperationException($"{nameof(AccordionItem)} requires one of {nameof(TitleTemplate)} or {nameof(Title)}, but both were specified.");
        
        base.OnParametersSet();
    }

    internal ValueTask HideAsync() => collapse.HideAsync();

    internal ValueTask ShowAsync() => collapse.ShowAsync();

    private async Task OnCollapseHiddenAsync()
    {
        if (Parent.OnHidden.HasDelegate)
            await Parent.OnHidden.InvokeAsync(new AccordionEventArgs(Name, Title));
    }

    private async Task OnCollapseHidingAsync()
    {
        isCollapsed = true;

        if (Parent.OnHiding.HasDelegate)
            await Parent.OnHiding.InvokeAsync(new AccordionEventArgs(Name, Title));
    }

    private async Task OnCollapseShowingAsync()
    {
        isCollapsed = false;

        if (Parent.OnShowing.HasDelegate)
            await Parent.OnShowing.InvokeAsync(new AccordionEventArgs(Name, Title));
    }

    private async Task OnCollapseShownAsync()
    {
        if (Parent.OnShown.HasDelegate)
            await Parent.OnShown.InvokeAsync(new AccordionEventArgs(Name, Title));
    }

    private ValueTask ToggleAsync() => collapse.ToggleAsync();


    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Id = IdUtility.GetNextId(); // This is required

        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(Active): Active = (bool)parameter.Value; break;
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(Name): Name = (string)parameter.Value; break;
                case nameof(Parent): Parent = (Accordion)parameter.Value; break;
                case nameof(Title): Title = (string)parameter.Value; break;
                case nameof(TitleTemplate): TitleTemplate = (RenderFragment)parameter.Value; break;
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }
        
        Parent.Add(this);
        return base.SetParametersAsync(ParameterView.Empty);
    }


    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the active state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Active { get; set; }
    
    private string ButtonCollapsedStateCssClass => isCollapsed ? "collapsed" : string.Empty;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter, EditorRequired]
    internal Accordion Parent { get; set; } = default!; 

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string Title { get; set; } = default!;

    /// <summary>
    /// Gets or sets the title template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment? TitleTemplate { get; set; } = default!;
    
    #endregion
}
