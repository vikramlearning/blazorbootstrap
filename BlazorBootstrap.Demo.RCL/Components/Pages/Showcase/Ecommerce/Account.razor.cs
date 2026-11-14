using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The customer's account overview: a greeting, the account's notices (newest first, new ones marked, with "Mark all as
/// read"), the three most recent orders, the default address, and a wishlist summary, each with its way to the full page.
/// </summary>
public partial class Account : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int MaxNotices = 10;

    private ElementReference noticeStatus;

    private int noticeStatusCount;

    private string? noticeStatusMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string FirstName(string name) =>
        name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? name;

    /// <summary>Names a notice's link after where it goes.</summary>
    private static string LinkText(string link) =>
        link.Contains("/orders/", StringComparison.Ordinal) ? "View order"
        : link.Contains("/products/", StringComparison.Ordinal) ? "View product"
        : "Open";

    /// <summary>Only a path inside the site becomes a link; anything else read back from storage is ignored.</summary>
    private static string? SafeLink(string? link) =>
        link is not null && link.StartsWith(EcommerceLayout.Base + "/", StringComparison.Ordinal) && !link.Contains("//", StringComparison.Ordinal) ? link : null;

    /// <summary>The button disappears once there's nothing unread, so focus moves to the status message.</summary>
    private async Task MarkAllReadAsync(string accountId)
    {
        State.MarkNoticesRead(accountId);

        noticeStatusCount++;
        noticeStatusMessage = noticeStatusCount == 1 ? "All notices are marked as read." : $"All notices are marked as read ({noticeStatusCount}).";
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(noticeStatus);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion
}
