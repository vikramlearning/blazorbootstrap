using System.Globalization;

namespace BlazorBootstrap;

/// <summary>
/// Stacks <see cref="Avatar" /> components with overlap, shows at most <see cref="Max" /> of them,
/// and adds a "+N" button that reveals the names of the hidden avatars.
/// </summary>
public partial class AvatarGroup : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private readonly List<Avatar> avatars = new();

    private bool isDisposing;

    private bool isOverflowListVisible;

    private int? previousMax;

    private AvatarShape previousShape = AvatarShape.Circle;

    private AvatarSize previousSize = AvatarSize.Medium;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
            isDisposing = true;

        return base.DisposeAsyncCore(disposing);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Max is < 1)
            throw new InvalidOperationException($"{nameof(AvatarGroup)} {nameof(Max)} must be at least 1, or null to show every avatar. The current value is {Max}.");

        if (previousMax != Max || previousShape != Shape || previousSize != Size)
        {
            previousMax = Max;
            previousShape = Shape;
            previousSize = Size;

            CloseOverflowListWhenEmpty();
            RefreshAvatars();
        }

        base.OnParametersSet();
    }

    #endregion

    #region Internal Methods

    internal bool IsVisible(Avatar avatar)
    {
        if (Max is null)
            return true;

        var index = avatars.IndexOf(avatar);

        return index >= 0 && index < Max.Value;
    }

    internal void NotifyAvatarChanged()
    {
        if (isDisposing)
            return;

        CloseOverflowListWhenEmpty();
        StateHasChanged();
    }

    internal void Register(Avatar avatar)
    {
        if (avatars.Contains(avatar))
            return;

        avatars.Add(avatar);

        StateHasChanged(); // the "+N" count depends on every registered avatar
    }

    internal void Unregister(Avatar avatar)
    {
        if (isDisposing || !avatars.Remove(avatar))
            return;

        // Removing an earlier avatar can bring a hidden one into view.
        CloseOverflowListWhenEmpty();
        RefreshAvatars();
        StateHasChanged();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Closes the hidden-names list once there is nothing left to list, so it does not reopen by itself later.
    /// </summary>
    private void CloseOverflowListWhenEmpty()
    {
        if (!HasHiddenNames)
            isOverflowListVisible = false;
    }

    private void RefreshAvatars()
    {
        foreach (var avatar in avatars.ToArray())
            avatar.Refresh();
    }

    private void ToggleOverflowList()
    {
        // With no named hidden avatars there is no list to show, so the button stays collapsed.
        if (HasHiddenNames)
            isOverflowListVisible = !isOverflowListVisible;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the avatars to display.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the avatars to display.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-avatar-group", true),
            ("d-inline-flex", true),
            ("align-items-center", true),
            (Size.ToAvatarSizeClass(), true));

    private bool HasHiddenNames => HiddenNames.Any();

    private int HiddenCount => Max is null ? 0 : Math.Max(0, avatars.Count - Max.Value);

    private IEnumerable<string> HiddenNames =>
        Max is null
            ? Enumerable.Empty<string>()
            : avatars.Skip(Max.Value)
                     .Select(avatar => avatar.Name)
                     .Where(name => !string.IsNullOrWhiteSpace(name))
                     .Select(name => name!.Trim());

    /// <summary>
    /// Gets or sets the maximum number of avatars to show, not counting the "+N" button.
    /// When it is <see langword="null" />, every avatar is shown. A value below 1 is not allowed.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the maximum number of avatars to show, not counting the \"+N\" button. When it is <b>null</b>, every avatar is shown. A value below 1 is not allowed.")]
    [Parameter]
    public int? Max { get; set; }

    private string OverflowAriaExpanded => ShowOverflowList ? "true" : "false";

    private string OverflowButtonClassNames =>
        BuildClassNames(
            ("bb-avatar", true),
            ("bb-avatar-overflow", true),
            ("d-inline-flex", true),
            ("align-items-center", true),
            ("justify-content-center", true),
            ("border-0", true),
            ("p-0", true),
            ("text-bg-secondary", true),
            (Shape.ToAvatarShapeClass(), true));

    private string OverflowListId => $"{Id}-overflow";

    private string OverflowText => string.Format(CultureInfo.CurrentCulture, OverflowTextFormat, HiddenCount);

    /// <summary>
    /// Gets or sets the format of the accessible text of the "+N" button. <c>{0}</c> is replaced with the number of hidden avatars.
    /// <para>
    /// Default value is <c>{0} more</c>.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("{0} more")]
    [Description("Gets or sets the format of the accessible text of the \"+N\" button. <b>{0}</b> is replaced with the number of hidden avatars.")]
    [Parameter]
    public string OverflowTextFormat { get; set; } = "{0} more";

    private string? OverflowTitle
    {
        get
        {
            var title = string.Join(", ", HiddenNames);

            return title.Length == 0 ? null : title;
        }
    }

    /// <summary>
    /// Gets or sets the shape of every avatar in the group and of the "+N" button.
    /// <para>
    /// Default value is <see cref="AvatarShape.Circle" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(AvatarShape.Circle)]
    [Description("Gets or sets the shape of every avatar in the group and of the \"+N\" button.")]
    [Parameter]
    public AvatarShape Shape { get; set; } = AvatarShape.Circle;

    private bool ShowOverflowList => isOverflowListVisible && HasHiddenNames;

    /// <summary>
    /// Gets or sets the size of every avatar in the group and of the "+N" button.
    /// Set the <c>--bb-avatar-size</c> CSS custom property on the group to use any other size,
    /// and <c>--bb-avatar-group-overlap</c> to change the overlap.
    /// <para>
    /// Default value is <see cref="AvatarSize.Medium" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(AvatarSize.Medium)]
    [Description("Gets or sets the size of every avatar in the group and of the \"+N\" button. Set the <b>--bb-avatar-size</b> CSS custom property on the group to use any other size, and <b>--bb-avatar-group-overlap</b> to change the overlap.")]
    [Parameter]
    public AvatarSize Size { get; set; } = AvatarSize.Medium;

    #endregion
}
