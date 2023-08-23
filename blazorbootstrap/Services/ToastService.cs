namespace BlazorBootstrap;

public class ToastService
{
    #region Events

    internal event Action<ToastMessage> OnNotify;

    #endregion

    #region Methods

    public void Notify(ToastMessage toastMessage) => OnNotify?.Invoke(toastMessage);

    #endregion
}