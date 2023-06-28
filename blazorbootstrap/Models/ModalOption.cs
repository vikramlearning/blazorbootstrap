namespace BlazorBootstrap;

public class ModalOption
{
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public ModalType Type { get; set; }
    public ModalSize Size { get; set; }
    public bool IsVerticallyCentered { get; set; }
    public bool ShowFooterButton { get; set; } = true;
    public string FooterButtonText { get; set; } = "OK";
    public ButtonColor FooterButtonColor { get; set; }
    public string FooterButtonCSSClass { get; set; } = default!;
}
