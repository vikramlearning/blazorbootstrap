namespace BlazorBootstrap;

public class ToastMessage : IEquatable<ToastMessage>
{
    #region Constructors

    public ToastMessage()
    {
        Id = Guid.NewGuid();
    }

    public ToastMessage(ToastType type)
        : this()
    {
        Type = type;
    }

    public ToastMessage(ToastType type, string message)
        : this(type)
    {
        Message = message;
    }

    public ToastMessage(ToastType type, string title, string message)
        : this(type, message)
    {
        Title = title;
    }

    public ToastMessage(ToastType type, IconName iconName, string title, string message)
        : this(type, title, message)
    {
        IconName = iconName;
    }

    public ToastMessage(ToastType type, string customIconName, string title, string message)
        : this(type, title, message)
    {
        CustomIconName = customIconName;
    }

    public ToastMessage(ToastType type, IconName iconName, string title, string helpText, string message, bool autoHide = false)
        : this(type, iconName, title, message)
    {
        HelpText = helpText;
        AutoHide = autoHide;
    }

    public ToastMessage(ToastType type, string customIconName, string title, string helpText, string message, bool autoHide = false)
        : this(type, customIconName, title, message)
    {
        HelpText = helpText;
        AutoHide = autoHide;
    }

    #endregion

    #region Methods

    public bool Equals(ToastMessage? other) => other != null && Id.Equals(other?.Id);

    internal void SetElementId(string elementId) => ElementId = elementId;

    #endregion

    #region Properties, Indexers

    public bool AutoHide { get; set; }

    /// <summary>
    /// Specify custom icons of your own, like fontawesome. Example: 'fas fa-alarm-clock'
    /// </summary>
    public string? CustomIconName { get; set; }

    internal string? ElementId { get; private set; }

    public string? HelpText { get; set; }

    public IconName IconName { get; set; }

    internal Guid Id { get; }

    public string? Message { get; set; }

    public RenderFragment? Content { get; set; }

    public string? Title { get; set; }

    public ToastType Type { get; set; }

    #endregion
}
