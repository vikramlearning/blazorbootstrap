using System.Globalization;

namespace BlazorBootstrap;

/// <summary>
/// Displays a person or entity as an image, initials, or an icon, with an optional status indicator.
/// </summary>
public partial class Avatar : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool imageFailed;

    private bool isParametersSet;

    private string? previousImageUrl;

    private string? previousName;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
            Group?.Unregister(this);

        return base.DisposeAsyncCore(disposing);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Group?.Register(this);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        // A new image source gets a fresh load attempt, even after an earlier failure.
        if (!string.Equals(previousImageUrl, ImageUrl, StringComparison.Ordinal))
        {
            imageFailed = false;
            previousImageUrl = ImageUrl;
        }

        // The group lists hidden names, so it needs to know when a name changes.
        if (isParametersSet && !string.Equals(previousName, Name, StringComparison.Ordinal))
            Group?.NotifyAvatarChanged();

        previousName = Name;
        isParametersSet = true;

        base.OnParametersSet();
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Gets up to two initials from a name: the first text element of the first word and of the last word,
    /// uppercased with the invariant culture. A single word gives one initial.
    /// </summary>
    internal static string? GetInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var words = name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var initials = StringInfo.GetNextTextElement(words[0], 0);

        if (words.Length > 1)
            initials += StringInfo.GetNextTextElement(words[^1], 0);

        return initials.ToUpperInvariant();
    }

    /// <summary>
    /// Re-renders the avatar after its group changed its visibility, size, or shape.
    /// </summary>
    internal void Refresh() => StateHasChanged();

    #endregion

    #region Private Methods

    private void OnImageError(string? failedImageUrl)
    {
        // An error from an image that has since been replaced must not hide the new image.
        if (string.Equals(failedImageUrl, ImageUrl, StringComparison.Ordinal))
            imageFailed = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the accessible text for the <see cref="AvatarStatus.Away" /> status.
    /// <para>
    /// Default value is <c>Away</c>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Away")]
    [Description("Gets or sets the accessible text for the <b>Away</b> status.")]
    [Parameter]
    public string AwayText { get; set; } = "Away";

    /// <summary>
    /// Gets or sets the accessible text for the <see cref="AvatarStatus.Busy" /> status.
    /// <para>
    /// Default value is <c>Busy</c>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Busy")]
    [Description("Gets or sets the accessible text for the <b>Busy</b> status.")]
    [Parameter]
    public string BusyText { get; set; } = "Busy";

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-avatar", true),
            (BootstrapClass.PositionRelative, true),
            ("d-inline-flex", true),
            ("flex-shrink-0", true),
            (Size.ToAvatarSizeClass(), Group is null));

    /// <summary>
    /// Gets or sets the background color of an initials or icon avatar. It has no visible effect while an image is shown.
    /// <para>
    /// Default value is <see cref="AvatarColor.Secondary" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(AvatarColor.Secondary)]
    [Description("Gets or sets the background color of an initials or icon avatar. It has no visible effect while an image is shown.")]
    [Parameter]
    public AvatarColor Color { get; set; } = AvatarColor.Secondary;

    /// <summary>
    /// Gets or sets a custom icon CSS class, such as a Font Awesome class, used when no image or initials are shown.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a custom icon CSS class, such as a Font Awesome class, used when no image or initials are shown.")]
    [Parameter]
    public string? CustomIconName { get; set; }

    private string? DisplayInitials => string.IsNullOrWhiteSpace(Initials) ? GetInitials(Name) : Initials.Trim();

    private AvatarShape EffectiveShape => Group?.Shape ?? Shape;

    private string? FaceAccessibleName
    {
        get
        {
            if (HasConsumerLabel || string.IsNullOrWhiteSpace(Name))
                return null;

            var name = Name.Trim();
            var statusText = StatusText;

            return string.IsNullOrWhiteSpace(statusText) ? name : $"{name}, {statusText}";
        }
    }

    private string? FaceAriaHidden => FaceAccessibleName is null ? "true" : null;

    private string FaceClassNames =>
        BuildClassNames(
            ("bb-avatar-face", true),
            ("w-100", true),
            ("h-100", true),
            ("d-inline-flex", true),
            ("align-items-center", true),
            ("justify-content-center", true),
            ("overflow-hidden", true),
            ("user-select-none", true),
            (EffectiveShape.ToAvatarShapeClass(), true),
            ("object-fit-cover", ShowImage),
            (Color.ToAvatarColorClass(), !ShowImage && Color != AvatarColor.None));

    /// <summary>
    /// Gets or sets the parent <see cref="AvatarGroup" />, when the avatar is placed inside one.
    /// </summary>
    [CascadingParameter]
    internal AvatarGroup? Group { get; set; }

    private bool HasConsumerLabel =>
        AdditionalAttributes is not null
        && AdditionalAttributes.Keys.Any(key => string.Equals(key, "aria-label", StringComparison.OrdinalIgnoreCase)
                                             || string.Equals(key, "aria-labelledby", StringComparison.OrdinalIgnoreCase));

    private bool HasCustomIcon => IconName != IconName.None || !string.IsNullOrWhiteSpace(CustomIconName);

    /// <summary>
    /// Gets or sets the Bootstrap icon shown when no image or initials are available.
    /// When neither this nor <see cref="CustomIconName" /> is set, a person icon is shown.
    /// <para>
    /// Default value is <see cref="IconName.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(IconName.None)]
    [Description("Gets or sets the Bootstrap icon shown when no image or initials are available. When neither this nor <b>CustomIconName</b> is set, a person icon is shown.")]
    [Parameter]
    public IconName IconName { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the image URL. When it is empty or fails to load, the avatar shows initials, then an icon.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the image URL. When it is empty or fails to load, the avatar shows initials, then an icon.")]
    [Parameter]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the initials to show instead of the ones computed from <see cref="Name" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the initials to show instead of the ones computed from <b>Name</b>.")]
    [Parameter]
    public string? Initials { get; set; }

    private bool IsShown => Group is null || Group.IsVisible(this);

    /// <summary>
    /// Gets or sets the name of the person or entity. It provides the initials and the accessible name.
    /// An avatar without a name is treated as decorative.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the name of the person or entity. It provides the initials and the accessible name. An avatar without a name is treated as decorative.")]
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the accessible text for the <see cref="AvatarStatus.Offline" /> status.
    /// <para>
    /// Default value is <c>Offline</c>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Offline")]
    [Description("Gets or sets the accessible text for the <b>Offline</b> status.")]
    [Parameter]
    public string OfflineText { get; set; } = "Offline";

    /// <summary>
    /// Gets or sets the accessible text for the <see cref="AvatarStatus.Online" /> status.
    /// <para>
    /// Default value is <c>Online</c>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Online")]
    [Description("Gets or sets the accessible text for the <b>Online</b> status.")]
    [Parameter]
    public string OnlineText { get; set; } = "Online";

    private string? RootRole => HasConsumerLabel ? "img" : null;

    /// <summary>
    /// Gets or sets the avatar shape. Inside an <see cref="AvatarGroup" />, the group's shape is used instead.
    /// <para>
    /// Default value is <see cref="AvatarShape.Circle" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(AvatarShape.Circle)]
    [Description("Gets or sets the avatar shape. Inside an <b>AvatarGroup</b>, the group's shape is used instead.")]
    [Parameter]
    public AvatarShape Shape { get; set; } = AvatarShape.Circle;

    private bool ShowImage => !string.IsNullOrWhiteSpace(ImageUrl) && !imageFailed;

    /// <summary>
    /// Gets or sets the avatar size. Inside an <see cref="AvatarGroup" />, the group's size is used instead.
    /// Set the <c>--bb-avatar-size</c> CSS custom property to use any other size.
    /// <para>
    /// Default value is <see cref="AvatarSize.Medium" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(AvatarSize.Medium)]
    [Description("Gets or sets the avatar size. Inside an <b>AvatarGroup</b>, the group's size is used instead. Set the <b>--bb-avatar-size</b> CSS custom property to use any other size.")]
    [Parameter]
    public AvatarSize Size { get; set; } = AvatarSize.Medium;

    /// <summary>
    /// Gets or sets the presence status shown as an indicator at the bottom-end corner.
    /// <para>
    /// Default value is <see cref="AvatarStatus.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(AvatarStatus.None)]
    [Description("Gets or sets the presence status shown as an indicator at the bottom-end corner.")]
    [Parameter]
    public AvatarStatus Status { get; set; } = AvatarStatus.None;

    private string StatusClassNames =>
        BuildClassNames(
            ("bb-avatar-status", true),
            (BootstrapClass.PositionAbsolute, true),
            ("bottom-0", true),
            ("end-0", true),
            ("rounded-circle", true),
            (Status.ToAvatarStatusClass(), true));

    private string? StatusText =>
        Status switch
        {
            AvatarStatus.Online => OnlineText,
            AvatarStatus.Away => AwayText,
            AvatarStatus.Busy => BusyText,
            AvatarStatus.Offline => OfflineText,
            _ => null
        };

    #endregion
}
