namespace BlazorBootstrap;

public class ToastEventArgs : EventArgs
{
    #region Constructors

    public ToastEventArgs(Guid toastId, string elementId)
    {
        ToastId = toastId;
        ElementId = elementId;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the elementId.
    /// </summary>
    public string ElementId { get; set; }

    /// <summary>
    /// Gets the toast Id.
    /// </summary>
    public Guid ToastId { get; set; }

    #endregion
}
