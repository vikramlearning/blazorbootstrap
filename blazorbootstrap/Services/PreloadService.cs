namespace BlazorBootstrap;

public class PreloadService
{
    public event Action OnShow;
    public event Action OnHide;

    public void Show() { OnShow?.Invoke(); }
    public void Hide() { OnHide?.Invoke(); }
}
