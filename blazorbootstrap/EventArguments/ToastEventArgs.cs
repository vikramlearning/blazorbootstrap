namespace BlazorBootstrap;

public class ToastEventArgs : EventArgs
{
    public ToastEventArgs(Guid toastId, string elementId)
    {
        this.ToastId = toastId;
        this.ElementId = elementId;
    }

    public Guid ToastId { get; set; }
    public string ElementId { get; set; }
}
