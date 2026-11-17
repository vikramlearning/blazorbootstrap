using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// Makes a form's validation errors reach screen readers (WCAG 3.3.1, 4.1.3): each visible validation message is linked
/// to its field through <c>aria-describedby</c>, and after a submit that leaves errors, focus moves to the first invalid
/// field. Place it inside an <see cref="EditForm" />; it follows the form's <see cref="EditContext" />.
/// </summary>
public partial class ShowcaseFormErrors : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How long after a submit an error still moves focus (covers checks that finish after the submit handler).</summary>
    private static readonly TimeSpan SubmitWindow = TimeSpan.FromSeconds(3);

    private ElementReference anchor;
    private DateTime? submittedAt;
    private EditContext? subscribed;
    private bool syncPending;

    #endregion

    #region Methods

    #region Public Methods

    /// <inheritdoc />
    public void Dispose() => Unsubscribe();

    #endregion

    #region Protected Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!syncPending)
            return;

        syncPending = false;
        var focus = FocusFirstInvalid && submittedAt is { } at && DateTime.UtcNow - at < SubmitWindow;

        // The submit is answered once the form shows errors; the window stays open only for checks still running.
        if (EditContext?.GetValidationMessages().Any() == true)
            submittedAt = null;

        // Let every component that reacts to the same validation change render first, so the script sees the messages.
        await Task.Yield();
        try
        {
            await JS.InvokeAsync<bool>("showcaseForms.sync", anchor, focus);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or JSException or InvalidOperationException)
        {
            // The circuit or page is going away, the browser has an older script without showcaseForms, or the page is
            // still prerendering. The form keeps working without the extra announcements.
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (ReferenceEquals(EditContext, subscribed))
            return;

        Unsubscribe();
        subscribed = EditContext;
        if (subscribed is null)
            return;

        subscribed.OnValidationRequested += OnValidationRequested;
        subscribed.OnValidationStateChanged += OnValidationStateChanged;
    }

    /// <inheritdoc />
    protected override bool ShouldRender() => syncPending;

    #endregion

    #region Private Methods

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        submittedAt = DateTime.UtcNow;
        RequestSync();
    }

    private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs e) => RequestSync();

    private void RequestSync()
    {
        syncPending = true;
        _ = InvokeAsync(StateHasChanged);
    }

    private void Unsubscribe()
    {
        if (subscribed is null)
            return;

        subscribed.OnValidationRequested -= OnValidationRequested;
        subscribed.OnValidationStateChanged -= OnValidationStateChanged;
        subscribed = null;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets whether a failed submit moves focus to the first invalid field. Set it to <see langword="false" /> on
    /// a page that moves focus itself, so focus moves once.
    /// </summary>
    [Parameter] public bool FocusFirstInvalid { get; set; } = true;

    [CascadingParameter] private EditContext? EditContext { get; set; }

    [Inject] private IJSRuntime JS { get; set; } = default!;

    #endregion
}
