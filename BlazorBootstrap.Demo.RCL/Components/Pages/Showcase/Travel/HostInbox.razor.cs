using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// The host's inbox: conversations with guests about the signed-in host's listings, the most recent first. Each row shows
/// the guest's initials and name, the listing, the booking and its phase when the conversation is about one, a plain-text
/// preview of the last message ("You:" when the host wrote it), when it was sent, and "Unread" when the guest wrote
/// something the host hasn't opened. Each row opens the conversation. Without conversations, the page says where they come
/// from. It also holds the small helpers the other host inbox, review, and earnings pages share.
/// </summary>
public partial class HostInbox : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The longest preview of a last message, in characters.</summary>
    private const int PreviewLength = 140;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the initials of a guest's name, like "JA" for "Jordan Avery".</summary>
    internal static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length switch
        {
            0 => "?",
            1 => char.ToUpperInvariant(parts[0][0]).ToString(),
            _ => string.Concat(char.ToUpperInvariant(parts[0][0]), char.ToUpperInvariant(parts[^1][0])),
        };
    }

    /// <summary>Returns the host-facing label of a booking phase.</summary>
    internal static string PhaseLabel(TravelTripPhase phase) => phase switch
    {
        TravelTripPhase.Requested => "Request to answer",
        TravelTripPhase.Upcoming => "Confirmed",
        TravelTripPhase.InProgress => "Staying now",
        TravelTripPhase.Completed => "Completed",
        TravelTripPhase.Cancelled => "Cancelled by guest",
        TravelTripPhase.Expired => "Request expired",
        _ => "Declined",
    };

    /// <summary>Returns the host's route of a reservation.</summary>
    internal static string ReservationUrl(string reference) => $"{HostLayout.Base}/reservations/{Uri.EscapeDataString(reference)}";

    /// <summary>Returns the host's route of a conversation.</summary>
    internal static string ThreadUrl(int threadId) => FormattableString.Invariant($"{InboxPath}/{threadId}");

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Returns the message on one line, cut at a word near <see cref="PreviewLength" /> characters.</summary>
    private static string Preview(string body)
    {
        var text = Regex.Replace(body, @"\s+", " ").Trim();

        if (text.Length <= PreviewLength)
            return text;

        var cut = text.LastIndexOf(' ', PreviewLength);

        return $"{text[..(cut > PreviewLength / 2 ? cut : PreviewLength)].TrimEnd()}…";
    }

    private static string UnreadText(int count) =>
        count == 1 ? "1 conversation has an unread message." : $"{count} conversations have unread messages.";

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the route of the host's inbox.</summary>
    internal static string InboxPath => $"{HostLayout.Base}/inbox";

    [Inject] private TravelState State { get; set; } = default!;

    #endregion
}
