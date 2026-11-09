namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// States that a screen which looks like a real operation is a demo and sends nothing.
/// </summary>
public partial class ShowcaseDemoNotice : ComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets extra CSS classes for the notice.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the notice text.
    /// </summary>
    [Parameter] public string Text { get; set; } = "Demo only: nothing you enter here is sent, stored, or charged.";

    #endregion
}
