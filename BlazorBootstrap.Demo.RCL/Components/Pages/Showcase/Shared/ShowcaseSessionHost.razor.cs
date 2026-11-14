using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// Wraps a site's layout. After the first interactive render it restores the site's snapshot from the tab's
/// <c>sessionStorage</c> (JS interop isn't available while prerendering), then saves a fresh snapshot shortly after every
/// change and every half minute, so the site clock carries over a reload. It cascades a <see cref="ShowcaseSession" />.
/// </summary>
public partial class ShowcaseSessionHost : ComponentBase, IAsyncDisposable
{
    #region Fields and Constants

    private static readonly TimeSpan saveDelay = TimeSpan.FromMilliseconds(250);

    private static readonly TimeSpan clockSaveInterval = TimeSpan.FromSeconds(30);

    private readonly CancellationTokenSource lifetime = new();

    private PeriodicTimer? clockTimer;

    private int generation;

    private CancellationTokenSource? pendingSave;

    private ShowcaseSession session = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var restored = await Store.RestoreAsync(State);
        session.IsRestored = true;

        if (restored)
        {
            generation++;
            State.NotifyRestored();
        }

        StateHasChanged();
        _ = SaveClockPeriodicallyAsync();
    }

    protected override void OnInitialized()
    {
        session = new ShowcaseSession(State);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening, cancels pending saves, and stops the clock timer.</summary>
    public ValueTask DisposeAsync()
    {
        State.Changed -= OnStateChanged;
        pendingSave?.Cancel();
        lifetime.Cancel();
        clockTimer?.Dispose();
        lifetime.Dispose();

        return ValueTask.CompletedTask;
    }

    #endregion

    #region Private Methods

    private void OnStateChanged() => _ = InvokeAsync(() =>
    {
        StateHasChanged();
        ScheduleSave();
    });

    private async Task SaveClockPeriodicallyAsync()
    {
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
        if (!session.IsRestored || session.IsResetting)
            return;

        pendingSave?.Cancel();
        pendingSave = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
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

    private async Task SaveNowAsync()
    {
        if (session.IsRestored && !session.IsResetting && !lifetime.IsCancellationRequested)
            await Store.SaveAsync(State);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the site's layout content.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets the site state to restore and save.</summary>
    [Parameter, EditorRequired] public IShowcaseSiteState State { get; set; } = default!;

    [Inject] private ShowcaseSessionStore Store { get; set; } = default!;

    #endregion
}
