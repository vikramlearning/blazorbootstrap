namespace BlazorBootstrap;

/// <summary>
/// Provides a simple publish/subscribe mechanism for displaying toast messages from anywhere in the application.
/// </summary>
/// <remarks>
/// Register a <see cref="Toasts" /> host in the render tree to display messages published by this service.
/// </remarks>
public class ToastService
{
    #region Events

    internal event Func<ToastMessage, Task> OnNotify = default!;

    #endregion

    #region Methods

    /// <summary>
    /// Notifies subscribers of a new toast message event.
    /// </summary>
    /// <remarks>If no subscribers are registered, this method has no effect. Use this overload for the common
    /// fire-and-forget scenario where a single <see cref="Toasts" /> host is registered. If you need to await
    /// multiple subscribers, use <see cref="NotifyAsync(ToastMessage)" /> instead.</remarks>
    /// <param name="toastMessage">The toast message to be delivered to all registered listeners. Cannot be null.</param>
    [AddedVersion("1.0.0")]
    [Description("Notifies subscribers of a new toast message event.")]
    public void Notify(ToastMessage toastMessage) => OnNotify?.Invoke(toastMessage);

    /// <summary>
    /// Notifies all subscribers of a new toast message event and awaits their completion.
    /// </summary>
    /// <remarks>If no subscribers are registered, this method has no effect. Use this overload when multiple
    /// subscribers may be attached and the caller needs to observe completion or failures from each handler.</remarks>
    /// <param name="toastMessage">The toast message to be delivered to all registered listeners. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous notification operation.</returns>
    [AddedVersion("4.0.0")]
    [Description("Notifies all subscribers of a new toast message event and awaits their completion.")]
    public async Task NotifyAsync(ToastMessage toastMessage)
    {
        // Multicast delegate Invoke returns only the last Task, so enumerate subscribers when every handler must complete.
        Delegate[] subscribers = OnNotify?.GetInvocationList() ?? Array.Empty<Delegate>();
        if (subscribers.Length > 0)
        {
            await Task.WhenAll(subscribers.Select(d =>
            {
                Func<ToastMessage, Task> subscriber = (Func<ToastMessage, Task>)d;
                return subscriber.Invoke(toastMessage);
            }));
        }
    }

    #endregion
}
