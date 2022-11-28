namespace BlazorBootstrap;

public class PreloadService
{
    internal event Action<SpinnerColor> OnShow;
    internal event Action OnHide;

    public void Show(SpinnerColor spinnerColor = SpinnerColor.Light) => OnShow?.Invoke(spinnerColor);
    public void Hide() => OnHide?.Invoke();
}
