namespace BlazorBootstrap;

public class ToastMessage : IEquatable<ToastMessage>
{
    #region Constructors

    public ToastMessage()
    {
        Id = Guid.NewGuid();
    }

    public ToastMessage(ToastType type, string message)
    {
        Id = Guid.NewGuid();
        Type = type;
        Message = message;
    }

    public ToastMessage(ToastType type, string title, string message)
    {
        Id = Guid.NewGuid();
        Type = type;
        Title = title;
        Message = message;
    }

    public ToastMessage(ToastType type, IconName iconName, string title, string message)
    {
        Id = Guid.NewGuid();
        Type = type;
        IconName = iconName;
        Title = title;
        Message = message;
    }

    public ToastMessage(ToastType type, string customIconName, string title, string message)
    {
        Id = Guid.NewGuid();
        Type = type;
        CustomIconName = customIconName;
        Title = title;
        Message = message;
    }

    public ToastMessage(ToastType type, IconName iconName, string title, string helpText, string message)
    {
        Id = Guid.NewGuid();
        Type = type;
        IconName = iconName;
        Title = title;
        HelpText = helpText;
        Message = message;
    }

    public ToastMessage(ToastType type, string customIconName, string title, string helpText, string message)
    {
        Id = Guid.NewGuid();
        Type = type;
        CustomIconName = customIconName;
        Title = title;
        HelpText = helpText;
        Message = message;
    }

    public ToastMessage(ToastType type, string customIconName, string title, string helpText, string message, bool autoHide)
    {
        Id = Guid.NewGuid();
        Type = type;
        CustomIconName = customIconName;
        Title = title;
        HelpText = helpText;
        Message = message;
        AutoHide = autoHide;
    }

    #endregion

    #region Methods

    public bool Equals(ToastMessage? other) => other != null && Id.Equals(other?.Id);

    internal void SetElementId(string elementId) => ElementId = elementId;

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets a value indicating whether the Toast should automatically hide.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the Toast should automatically hide.")]
    public bool AutoHide { get; set; }

    /// <summary>
    /// Gets or sets the UI content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the UI content to be rendered within the component.")]
    public RenderFragment? Content { get; set; }

    /// <summary>
    /// Specify custom icons of your own, like fontawesome. Example: 'fas fa-alarm-clock'
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Specify custom icons of your own, like fontawesome. Example: 'fas fa-alarm-clock'")]
    public string? CustomIconName { get; set; }

    internal string? ElementId { get; private set; }

    /// <summary>
    /// Gets or sets the help text associated with this element.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the help text associated with this element.")]
    public string? HelpText { get; set; }

    /// <summary>
    /// Gets or sets the icon to display for the associated element.
    /// <para>
    /// Default value is <see cref="IconName.None"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(IconName.None)]
    [Description("Gets or sets the icon to display for the associated element.")]
    public IconName IconName { get; set; } = IconName.None;

    internal Guid Id { get; }

    /// <summary>
    /// Gets or sets the message content associated with this instance.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the message content associated with this instance.")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the title associated with the object.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the title associated with the object.")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the type of toast notification to display.
    /// <para>
    /// Default value is <see cref="ToastType.Primary"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(ToastType.Primary)]
    [Description("Gets or sets the type of toast notification to display.")]
    public ToastType Type { get; set; } = ToastType.Primary;

    #endregion
}
