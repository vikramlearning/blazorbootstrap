using static System.Net.Mime.MediaTypeNames;

namespace BlazorBootstrap;

/// <summary>
/// Represents a tab within a <see cref="Ribbon"/> component. This component is intended to contain <see cref="RibbonGroup"/> and <see cref="RibbonItem"/> components.
/// </summary>
public partial class RibbonTab : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing && IsRenderComplete)
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.tabs.dispose", Id);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }
        }

        await base.DisposeAsyncCore(disposing);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Id = IdUtility.GetNextId(); // This is required
        Parent.AddTab(this);
    }

    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case var _ when String.Equals(parameter.Name, nameof(Active), StringComparison.OrdinalIgnoreCase): Active = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ChildContent), StringComparison.OrdinalIgnoreCase): ChildContent = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Disabled), StringComparison.OrdinalIgnoreCase): Disabled = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Name), StringComparison.OrdinalIgnoreCase): Name = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(OnClick), StringComparison.OrdinalIgnoreCase): OnClick = (EventCallback<TabEventArgs>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Parent), StringComparison.OrdinalIgnoreCase): Parent = (Ribbon)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Title), StringComparison.OrdinalIgnoreCase): Title = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(TitleTemplate), StringComparison.OrdinalIgnoreCase): TitleTemplate = (RenderFragment)parameter.Value; break;
                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }
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
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the disabled state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the tab name.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] [EditorRequired] public string Name { get; set; } = default!;

    /// <summary>
    /// This event fires when the user clicks the corresponding tab button and the tab is displayed.
    /// </summary>
    [Parameter] public EventCallback<TabEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets the parent ribbon.
    /// </summary>
    [CascadingParameter(Name = "Ribbon1")]
    internal Ribbon Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tab title.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string Title { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tab title template.
    /// </summary>
    [Parameter] public RenderFragment? TitleTemplate { get; set; }


    /// <summary>
    /// Dependency injected Javascript Runtime
    /// </summary>
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    #endregion
}
