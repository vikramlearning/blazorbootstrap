namespace BlazorBootstrap;

public class ToastMessage : IEquatable<ToastMessage>
{
    public Guid Id { get; private set; }

    public ToastType Type { get; set; }

    public IconName IconName { get; set; }

    /// <summary>
    /// Specify custom icons of your own, like fontawesome. Example: 'fas fa-alarm-clock'
    /// </summary>
    public string CustomIconName { get; set; }

    public string Title { get; set; }

    public string HelpText { get; set; }

    public string Message { get; set; }

    public ToastMessage()
    {
        this.Id = Guid.NewGuid();
    }

    public ToastMessage(ToastType type, string message)
    {
        Type = type;
        Message = message;
    }

    public ToastMessage(ToastType type, string title, string message)
    {
        Type = type;
        Title = title;
        Message = message;
    }

    public ToastMessage(ToastType type, IconName iconName, string title, string message)
    {
        Type = type;
        IconName = iconName;
        Title = title;
        Message = message;
    }

    public ToastMessage(ToastType type, string customIconName, string title, string message)
    {
        Type = type;
        CustomIconName = customIconName;
        Title = title;
        Message = message;
    }

    public ToastMessage(ToastType type, IconName iconName, string title, string helpText, string message)
    {
        Type = type;
        IconName = iconName;
        Title = title;
        HelpText = helpText;
        Message = message;
    }

    public ToastMessage(ToastType type, string customIconName, string title, string helpText, string message)
    {
        Type = type;
        CustomIconName = customIconName;
        Title = title;
        HelpText = helpText;
        Message = message;
    }

    public bool Equals(ToastMessage other)
    {
        if(other == null) 
            return false;

        return this.Id.Equals(other.Id);
    }
}
