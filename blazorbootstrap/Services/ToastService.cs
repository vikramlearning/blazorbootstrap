namespace BlazorBootstrap;

public class ToastService
{
    #region Events

    internal event Action<ToastMessage> OnNotify = default!;

    #endregion

    #region Methods

    /// <summary>
    /// Notifies subscribers of a new toast message event.
    /// </summary>
    /// <remarks>If no subscribers are registered, this method has no effect. This method is typically used to
    /// display transient notifications to users.</remarks>
    /// <param name="toastMessage">The toast message to be delivered to all registered listeners. Cannot be null.</param>
    [AddedVersion("1.0.0")]
    [Description("Notifies subscribers of a new toast message event.")]
    public void Notify(ToastMessage toastMessage) => OnNotify?.Invoke(toastMessage);

    #endregion
}
