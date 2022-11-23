namespace BlazorBootstrap;

public class ToastService
{
    internal event Action<ToastMessage> OnNotify;

    public void Notify(ToastMessage toastMessage) => OnNotify?.Invoke(toastMessage);
}
