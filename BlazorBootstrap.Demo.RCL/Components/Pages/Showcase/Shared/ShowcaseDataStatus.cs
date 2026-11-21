namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// The status of a Showcase data screen's data, shown by <see cref="ShowcaseDataState" />.
/// </summary>
public enum ShowcaseDataStatus
{
    /// <summary>The data is ready.</summary>
    Ready,

    /// <summary>The data is loading.</summary>
    Loading,

    /// <summary>The data could not be loaded.</summary>
    Failed
}
