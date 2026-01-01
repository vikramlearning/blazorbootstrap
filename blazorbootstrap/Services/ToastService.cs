namespace BlazorBootstrap;

public class ToastService
{
    #region Events

    internal event Func<ToastMessage, Task> OnNotify = default!;
    
    #endregion

    #region Methods
    public void Notify(ToastMessage toastMessage) => OnNotify?.Invoke(toastMessage);

    public async Task NotifyAsync(ToastMessage toastMessage)
    {
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
