namespace BlazorBootstrap;

public class ModalService
{
    internal event Func<ModalOption, Task> OnShow = default!;
    internal event Func<ModalOption, Task> OnHide = default!;

    public Task ShowAsync(ModalOption modalOption) => OnShow?.Invoke(modalOption);
    public Task HideAsync(ModalOption modalOption) => OnHide?.Invoke(modalOption);
}
