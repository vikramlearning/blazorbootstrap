namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Runs a callback every <see cref="Services.Showcase.Monitoring.MonitoringSeedData.TickSeconds" /> seconds until it is
/// disposed (finalize OI-09). A page creates one after its first render and disposes it with the page, so leaving the
/// page stops the updates.
/// </summary>
public sealed class MonitoringLiveTimer : IDisposable
{
    #region Fields and Constants

    private readonly CancellationTokenSource cancellation = new();

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts calling <paramref name="onTick" /> on every tick.</summary>
    public MonitoringLiveTimer(TimeSpan interval, Func<Task> onTick) => _ = RunAsync(interval, onTick, cancellation.Token);

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Stops the timer; a tick in progress finishes, and no new tick starts.</summary>
    public void Dispose()
    {
        if (cancellation.IsCancellationRequested)
            return;

        cancellation.Cancel();
        cancellation.Dispose();
    }

    #endregion

    #region Private Methods

    private static async Task RunAsync(TimeSpan interval, Func<Task> onTick, CancellationToken token)
    {
        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(token))
                await onTick();
        }
        catch (OperationCanceledException)
        {
            // The page was left.
        }
        catch (ObjectDisposedException)
        {
            // The page was disposed while a tick was running.
        }
    }

    #endregion

    #endregion
}
