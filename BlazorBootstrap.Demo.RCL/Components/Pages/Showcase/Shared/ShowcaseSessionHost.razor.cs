using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// Wraps a site's layout. After the first interactive render it restores the site's snapshot from the tab's
/// <c>sessionStorage</c> (JS interop isn't available while prerendering). From then on it saves a fresh snapshot shortly
/// after every change, every half minute (so the site clock carries over a reload), and when the site is left; only one
/// save runs at a time. It owns the "Reset demo data" dialog and cascades a <see cref="ShowcaseSession" />.
/// </summary>
public partial class ShowcaseSessionHost : ComponentBase, IAsyncDisposable
{
    #region Fields and Constants

    private static readonly TimeSpan clockSaveInterval = TimeSpan.FromSeconds(30);

    private static readonly TimeSpan saveDelay = TimeSpan.FromMilliseconds(250);

    private readonly CancellationTokenSource lifetime = new();

    private PeriodicTimer? clockTimer;

    private bool disposed;

    private bool hasUnsavedChange;

    private int generation;

    private CancellationTokenSource? pendingSave;

    private Func<Task>? resetCancelled;

    private bool resetConfirmed;

    private Modal resetModal = default!;

    private bool saveAgain;

    private Task? saveTask;

    private ShowcaseSession session = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var result = await Store.RestoreAsync(State);

        if (disposed)
            return;

        session.IsRestored = true;
        session.IsStorageAvailable = result != ShowcaseRestoreResult.Unavailable;

        if (result == ShowcaseRestoreResult.Restored)
        {
            generation++;
            State.NotifyRestored();
        }

        StateHasChanged();

        if (session.IsStorageAvailable)
            _ = SaveClockPeriodicallyAsync();
    }

    protected override void OnInitialized()
    {
        session = new ShowcaseSession(State) { ResetRequested = OpenResetAsync };
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Saves any pending change, then stops listening and stops the timers.</summary>
    public async ValueTask DisposeAsync()
    {
        State.Changed -= OnStateChanged;
        CancelPendingSave();

        // Leaving the site inside the same circuit or app: keep the last change.
        if (hasUnsavedChange && CanSave)
        {
            try
            {
                await SaveNowAsync();
            }
            catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException)
            {
                // The circuit has gone; nothing to keep.
            }
        }

        disposed = true;
        lifetime.Cancel();
        clockTimer?.Dispose();
    }

    #endregion

    #region Private Methods

    private void CancelPendingSave()
    {
        pendingSave?.Cancel();
        pendingSave?.Dispose();
        pendingSave = null;
    }

    private void OnStateChanged() => _ = InvokeAsync(() =>
    {
        StateHasChanged();
        ScheduleSave();
    });

    private async Task OnResetHiddenAsync()
    {
        if (resetConfirmed || resetCancelled is null)
            return;

        var callback = resetCancelled;
        resetCancelled = null;
        await callback();
    }

    private async Task OpenResetAsync(Func<Task>? onCancelled)
    {
        resetCancelled = onCancelled;
        await resetModal.ShowAsync();
    }

    /// <summary>
    /// Stops saving, waits for a save in progress, clears the site's snapshot, and reloads the site, which starts it from
    /// seed data, signed out. Waiting first means an in-flight save can't write the snapshot back after the clear.
    /// </summary>
    private async Task ResetAsync()
    {
        resetConfirmed = true;
        session.IsResetting = true;
        CancelPendingSave();

        if (saveTask is { IsCompleted: false } running)
            await running;

        await Store.ClearAsync(State.SiteSlug);
        Navigation.NavigateTo(SiteBase, forceLoad: true);
    }

    private async Task SaveClockPeriodicallyAsync()
    {
        if (lifetime.IsCancellationRequested)
            return;

        clockTimer = new PeriodicTimer(clockSaveInterval);

        try
        {
            while (await clockTimer.WaitForNextTickAsync(lifetime.Token))
                await InvokeAsync(SaveNowAsync);
        }
        catch (OperationCanceledException)
        {
            // The layout was disposed.
        }
    }

    /// <summary>Saves once changes settle. Nothing is saved before the restore, so seed data never overwrites a stored snapshot.</summary>
    private void ScheduleSave()
    {
        if (!CanSave)
            return;

        CancelPendingSave();
        pendingSave = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        hasUnsavedChange = true;
        _ = SaveLaterAsync(pendingSave.Token);
    }

    private async Task SaveLaterAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(saveDelay, token);
            await InvokeAsync(SaveNowAsync);
        }
        catch (OperationCanceledException)
        {
            // A newer change scheduled its own save.
        }
    }

    /// <summary>Runs one save at a time; a request while a save runs makes it save once more when it finishes.</summary>
    private async Task SaveNowAsync()
    {
        if (!CanSave)
            return;

        if (saveTask is { IsCompleted: false } running)
        {
            saveAgain = true;
            await running;

            return;
        }

        saveTask = SaveLoopAsync();
        await saveTask;
    }

    private async Task SaveLoopAsync()
    {
        do
        {
            saveAgain = false;
            hasUnsavedChange = false;
            await Store.SaveAsync(State);
        }
        while (saveAgain && CanSave);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private bool CanSave => session.IsRestored && session.IsStorageAvailable && !session.IsResetting && !disposed;

    /// <summary>Gets or sets the site's layout content.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets or sets the site's public start page, where Reset reloads to.</summary>
    [Parameter, EditorRequired] public string SiteBase { get; set; } = default!;

    /// <summary>Gets or sets the site's name, used in the reset confirmation.</summary>
    [Parameter, EditorRequired] public string SiteName { get; set; } = default!;

    /// <summary>Gets or sets the site state to restore and save.</summary>
    [Parameter, EditorRequired] public IShowcaseSiteState State { get; set; } = default!;

    [Inject] private ShowcaseSessionStore Store { get; set; } = default!;

    #endregion
}
