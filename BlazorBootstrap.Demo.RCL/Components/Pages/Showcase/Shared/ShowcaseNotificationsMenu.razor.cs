using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// The header's notifications menu for a site that implements <see cref="IShowcaseEnterpriseState" />: a bell with the
/// unread count, the signed-in account's newest notifications as links, and "Mark all as read". Opening a notification
/// marks it read.
/// </summary>
public partial class ShowcaseNotificationsMenu : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => Session.State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening for state changes.</summary>
    public void Dispose() => Session.State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Formats how long ago something happened on the site's clock, for example "5 min ago" or "3 days ago".</summary>
    private string FormatAge(DateTime at)
    {
        var age = Session.State.Clock.Now - at;

        if (age < TimeSpan.FromMinutes(1))
            return "Just now";

        if (age < TimeSpan.FromHours(1))
            return $"{(int)age.TotalMinutes} min ago";

        if (age < TimeSpan.FromDays(1))
            return $"{(int)age.TotalHours} h ago";

        var days = (int)age.TotalDays;

        return days == 1 ? "Yesterday" : $"{days} days ago";
    }

    private void OnStateChanged() => _ = InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets extra CSS classes.</summary>
    [Parameter] public string? Class { get; set; }

    private IShowcaseEnterpriseState? Enterprise => Session.State as IShowcaseEnterpriseState;

    /// <summary>Gets or sets how many notifications the menu lists.</summary>
    [Parameter] public int MaxShown { get; set; } = 8;

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    #endregion
}
