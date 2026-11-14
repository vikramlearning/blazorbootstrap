using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The sub-navigation at the top of every customer account page: Overview (with the number of new notices), Orders,
/// Addresses, Wishlist, and Profile. The link of the current page carries <c>aria-current="page"</c>; the list wraps on phones.
/// </summary>
public partial class EcommerceAccountNav : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The account overview page.</summary>
    public const string OverviewPath = EcommerceLayout.Base + "/account";

    /// <summary>The customer's orders page.</summary>
    public const string OrdersPath = OverviewPath + "/orders";

    /// <summary>The customer's address book page.</summary>
    public const string AddressesPath = OverviewPath + "/addresses";

    /// <summary>The customer's wishlist page.</summary>
    public const string WishlistPath = OverviewPath + "/wishlist";

    /// <summary>The customer's profile and password page.</summary>
    public const string ProfilePath = OverviewPath + "/profile";

    private static readonly (string Path, string Label, IconName Icon)[] items =
    {
        (OverviewPath, "Overview", IconName.PersonCircle),
        (OrdersPath, "Orders", IconName.BoxSeam),
        (AddressesPath, "Addresses", IconName.GeoAlt),
        (WishlistPath, "Wishlist", IconName.Heart),
        (ProfilePath, "Profile", IconName.PersonGear),
    };

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        Navigation.LocationChanged += OnLocationChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and navigation changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        Navigation.LocationChanged -= OnLocationChanged;
    }

    #endregion

    #region Private Methods

    /// <summary>Compares the current path, without query string, fragment, or trailing slash, with a link's path.</summary>
    private bool IsCurrent(string path)
    {
        var current = Navigation.ToBaseRelativePath(Navigation.Uri);
        var end = current.IndexOfAny(new[] { '?', '#' });

        if (end >= 0)
            current = current[..end];

        return string.Equals(current.Trim('/'), path.Trim('/'), StringComparison.OrdinalIgnoreCase);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args) => InvokeAsync(StateHasChanged);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private EcommerceState State { get; set; } = default!;

    private int UnreadCount => State.Identity.Current is { } account ? State.UnreadNoticeCount(account.Id) : 0;

    #endregion
}
