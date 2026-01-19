namespace BlazorBootstrap;

public class PreloadService
{
    #region Events

    internal event Action OnHide = default!;
    internal event Action<SpinnerColor, string?> OnShow = default!;

    #endregion

    #region Methods

    /// <summary>
    /// Hides the preload spinner.
    /// </summary>
    [AddedVersion("1.1.0")]
    [Description("Hides the preload spinner.")]
    public void Hide() => OnHide?.Invoke();

    /// <summary>
    /// Shows the preload spinner.
    /// </summary>
    /// <param name="spinnerColor"></param>
    /// <param name="loadingText"></param>
    [AddedVersion("1.1.0")]
    [Description("Shows the preload spinner.")]
    public void Show(SpinnerColor spinnerColor = SpinnerColor.Light, string? loadingText = null) => OnShow?.Invoke(spinnerColor, loadingText);

    #endregion
}
