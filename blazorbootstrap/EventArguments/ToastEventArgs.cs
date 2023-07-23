namespace BlazorBootstrap;

public class ToastEventArgs : EventArgs
{
    public ToastEventArgs(Guid toastId, string elementId)
    {
        ToastId = toastId;
        ElementId = elementId;
    }

    public Guid ToastId { get; set; }
    public string ElementId { get; set; }
}
