namespace BlazorBootstrap;

public class ModalService
{
    internal event Func<ModalOption, Task> OnShow = default!;
    internal event Func<ModalOption, Task> OnHide = default!;

    public Task ShowAsync(ModalOption modalOption) => OnShow?.Invoke(modalOption);
    public Task HideAsync(ModalOption modalOption) => OnHide?.Invoke(modalOption);
}

public class ModalOption
{
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public ModalType Type { get; set; }
    public bool ShowFooterButton { get; set; } = true;
    public string FooterButtonText { get; set; } = "OK";
    public ButtonColor FooterButtonColor { get; set; }
    public string FooterButtonCSSClass { get; set; } = default!;
}

public enum ModalType
{
    Default,
    Success,
    Danger,
    Warning,
    Info
}