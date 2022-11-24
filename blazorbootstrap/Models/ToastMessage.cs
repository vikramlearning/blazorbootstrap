namespace BlazorBootstrap;

public class ToastMessage : IEquatable<ToastMessage>
{
    internal Guid Id { get; private set; }

    public ToastType Type { get; set; }

    public IconName IconName { get; set; }

    /// <summary>
    /// Specify custom icons of your own, like fontawesome. Example: 'fas fa-alarm-clock'
    /// </summary>
    public string CustomIconName { get; set; }

    public string Title { get; set; }

    public string HelpText { get; set; }

    public string Message { get; set; }

    internal string ElementId { get; private set; }

    public ToastMessage()
    {
        this.Id = Guid.NewGuid();
    }

    public ToastMessage(ToastType type, string message)
    {
        this.Id = Guid.NewGuid();
        this.Type = type;
        this.Message = message;
    }

    public ToastMessage(ToastType type, string title, string message)
    {
        this.Id = Guid.NewGuid();
        this.Type = type;
        this.Title = title;
        this.Message = message;
    }

    public ToastMessage(ToastType type, IconName iconName, string title, string message)
    {
        this.Id = Guid.NewGuid();
        this.Type = type;
        this.IconName = iconName;
        this.Title = title;
        this.Message = message;
    }

    public ToastMessage(ToastType type, string customIconName, string title, string message)
    {
        this.Id = Guid.NewGuid();
        this.Type = type;
        this.CustomIconName = customIconName;
        this.Title = title;
        this.Message = message;
    }

    public ToastMessage(ToastType type, IconName iconName, string title, string helpText, string message)
    {
        this.Id = Guid.NewGuid();
        this.Type = type;
        this.IconName = iconName;
        this.Title = title;
        this.HelpText = helpText;
        this.Message = message;
    }

    public ToastMessage(ToastType type, string customIconName, string title, string helpText, string message)
    {
        this.Id = Guid.NewGuid();
        this.Type = type;
        this.CustomIconName = customIconName;
        this.Title = title;
        this.HelpText = helpText;
        this.Message = message;
    }

    internal void SetElementId(string elementId) => this.ElementId= elementId;

    public bool Equals(ToastMessage other)
    {
        if(other == null) 
            return false;

        return this.Id.Equals(other.Id);
    }
}
