namespace BlazorBootstrap;

/// <summary>
/// A link in a <see cref="NavbarNav" />. It is marked active, with <c>aria-current="page"</c>, when its <see cref="Href" /> matches the current URL. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/navbar/#nav" />
/// </summary>
/// <remarks>
/// <see cref="BlazorBootstrapComponentBase.Id" />, <see cref="BlazorBootstrapComponentBase.Class" />, <see cref="BlazorBootstrapComponentBase.Style" />, and additional attributes apply to the link.
/// </remarks>
[AddedVersion("4.1.0")]
public partial class NavbarItem : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool isActive;

    private bool isLocationChangedSubscribed;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing && isLocationChangedSubscribed)
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
            isLocationChangedSubscribed = false;
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        NavigationManager.LocationChanged += OnLocationChanged;
        isLocationChangedSubscribed = true;
    }

    protected override void OnParametersSet()
    {
        isActive = ShouldMatch(NavigationManager.Uri);

        base.OnParametersSet();
    }

    /// <summary>
    /// Removes the query string and fragment, so they do not affect matching.
    /// </summary>
    private static string GetPath(string uri)
    {
        var index = uri.IndexOfAny(new[] { '?', '#' });

        return index < 0 ? uri : uri[..index];
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var shouldBeActive = ShouldMatch(e.Location);

        if (shouldBeActive == isActive)
            return;

        isActive = shouldBeActive;
        _ = InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Matches the current URL the way <see cref="NavLink" /> does: the whole path for <see cref="NavLinkMatch.All" />,
    /// or the path and any sub-path for <see cref="NavLinkMatch.Prefix" />. The query string and fragment are ignored.
    /// </summary>
    private bool ShouldMatch(string currentUri)
    {
        if (Disabled || Href is null)
            return false;

        var hrefPath = GetPath(NavigationManager.ToAbsoluteUri(Href).AbsoluteUri).TrimEnd('/');
        var currentPath = GetPath(currentUri).TrimEnd('/');

        if (string.Equals(currentPath, hrefPath, StringComparison.OrdinalIgnoreCase))
            return true;

        return Match == NavLinkMatch.Prefix
               && currentPath.StartsWith(hrefPath + "/", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content of the link.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the link.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.NavLink, true),
            (BootstrapClass.Active, isActive),
            (BootstrapClass.Disabled, Disabled));

    /// <summary>
    /// Gets or sets whether the link is disabled. A disabled link has no <c>href</c> and is marked <c>aria-disabled="true"</c>.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the link is disabled. A disabled link has no <b>href</b> and is marked <b>aria-disabled=\"true\"</b>.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the URL of the link.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the URL of the link.")]
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets how the link matches the current URL to become active.
    /// Use <see cref="NavLinkMatch.All" /> for a home link, so it is not active on every page.
    /// <para>
    /// Default value is <see cref="NavLinkMatch.Prefix" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(NavLinkMatch.Prefix)]
    [Description("Gets or sets how the link matches the current URL to become active. Use <b>NavLinkMatch.All</b> for a home link, so it is not active on every page.")]
    [Parameter]
    public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    #endregion
}
