namespace BlazorBootstrap;

public class PreloadService
{
    #region Events

    internal event Action OnHide;
    internal event Action<SpinnerColor, string?> OnShow;

    #endregion

    #region Methods

    public void Hide() => OnHide?.Invoke();

    public void Show(SpinnerColor spinnerColor = SpinnerColor.Light, string? loadingText = null) => OnShow?.Invoke(spinnerColor, loadingText);

    #endregion
}
