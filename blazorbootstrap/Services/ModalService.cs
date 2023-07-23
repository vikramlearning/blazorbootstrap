namespace BlazorBootstrap;

public class ModalService
{
    internal event Func<ModalOption, Task> OnShow = default!;

    public Task ShowAsync(ModalOption modalOption)
    {
        OnShow?.Invoke(modalOption);

        return Task.CompletedTask;
    }
}
