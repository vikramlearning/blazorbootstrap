namespace BlazorBootstrap;

public class PreloadService
{
    internal event Action OnShow;
    internal event Action OnHide;

    public void Show() { OnShow?.Invoke(); }
    public void Hide() { OnHide?.Invoke(); }
}
