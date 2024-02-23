namespace BlazorBootstrap;

public class PreloadService
{
    #region Events

    internal event Action OnHide = default!;
    internal event Action<SpinnerColor, string?> OnShow = default!;

    #endregion

    #region Methods

    public void Hide() => OnHide?.Invoke();

    public void Show(SpinnerColor spinnerColor = SpinnerColor.Light, string? loadingText = null) => OnShow?.Invoke(spinnerColor, loadingText);

    #endregion
}
