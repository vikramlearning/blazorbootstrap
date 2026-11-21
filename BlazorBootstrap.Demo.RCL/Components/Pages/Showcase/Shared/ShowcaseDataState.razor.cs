namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// Shows a data screen's loading placeholder, its empty state with a next action, or its error state with Retry, and the
/// screen's content otherwise (rule WR-001). Showcase data is in memory and never fails, so visitors can see the error
/// state through a labelled demo control, "Demo: simulate a loading error".
/// </summary>
public partial class ShowcaseDataState : ComponentBase
{
    #region Fields and Constants

    private bool simulatedFailure;

    #endregion

    #region Methods

    #region Private Methods

    private async Task RetryAsync()
    {
        simulatedFailure = false;

        if (OnRetry.HasDelegate)
            await OnRetry.InvokeAsync();
    }

    private void SimulateFailure() => simulatedFailure = true;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the content shown when the data is ready and not empty.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets the empty state's next action, for example a "New ticket" button or "Clear filters".</summary>
    [Parameter] public RenderFragment? EmptyActions { get; set; }

    /// <summary>Gets or sets the empty state's icon.</summary>
    [Parameter] public IconName EmptyIcon { get; set; } = IconName.Inbox;

    /// <summary>Gets or sets the empty state's explanation, for example what to do next.</summary>
    [Parameter] public string? EmptyText { get; set; }

    /// <summary>Gets or sets the empty state's title, for example "No tickets yet".</summary>
    [Parameter] public string EmptyTitle { get; set; } = "Nothing here yet";

    /// <summary>Gets or sets whether the data is empty. The empty state shows only when the status is ready.</summary>
    [Parameter] public bool IsEmpty { get; set; }

    /// <summary>Gets or sets how many placeholder lines the loading state shows.</summary>
    [Parameter] public int LoadingRows { get; set; } = 5;

    /// <summary>Gets or sets the loading announcement for screen readers.</summary>
    [Parameter] public string LoadingText { get; set; } = "Loading…";

    /// <summary>Gets or sets what Retry does, for example reload the data.</summary>
    [Parameter] public EventCallback OnRetry { get; set; }

    /// <summary>Gets or sets the data's status.</summary>
    [Parameter] public ShowcaseDataStatus Status { get; set; } = ShowcaseDataStatus.Ready;

    #endregion
}
