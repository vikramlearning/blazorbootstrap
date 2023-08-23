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

    public string ElementId { get; set; }

    public Guid ToastId { get; set; }

    #endregion
}