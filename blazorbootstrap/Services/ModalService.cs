namespace BlazorBootstrap;

public class ModalService
{
    #region Events

    /// <summary>
    /// Event that is raised when the modal is shown.
    /// </summary>
    internal event Func<ModalOption, Task> OnShow = default!;

    #endregion

    #region Fields and Constants

    /// <summary>
    /// Event that is raised when the modal is hidden.
    /// </summary>
    internal Action OnHide = default!;

    #endregion

    #region Methods

    /// <summary>
    /// Shows the modal.
    /// </summary>
    /// <param name="modalOption">The modal options.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ShowAsync(ModalOption modalOption)
    {
        OnShow?.Invoke(modalOption);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Shows the modal and calls the specified callback action when it is hidden.
    /// </summary>
    /// <param name="modalOption">The modal options.</param>
    /// <param name="callbackAction">The callback action.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ShowAsync(ModalOption modalOption, Action callbackAction)
    {
        OnHide = callbackAction;

        OnShow?.Invoke(modalOption);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the modal is closed.
    /// </summary>
    internal void OnClose()
    {
        OnHide?.Invoke();
        OnHide = null;
    }

    #endregion
}
