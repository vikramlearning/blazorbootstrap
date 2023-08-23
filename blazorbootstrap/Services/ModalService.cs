namespace BlazorBootstrap;

public class ModalService
{
    #region Events

    internal event Func<ModalOption, Task> OnShow = default!;

    #endregion

    #region Methods

    public Task ShowAsync(ModalOption modalOption)
    {
        OnShow?.Invoke(modalOption);

        return Task.CompletedTask;
    }

    #endregion
}