using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// The signed-in guest's conversations with hosts, the most recent first: the host's initials and name, the stay, the
/// booking and its phase when the conversation is about one, a plain-text preview of the last message, when it was
/// sent, and "Unread" when the host wrote something the guest hasn't opened. Each row opens the conversation. Without
/// conversations, the page explains that hosts are contacted from a stay page.
/// </summary>
public partial class Messages : ComponentBase, IDisposable
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

    #region Private Methods

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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private TravelState State { get; set; } = default!;

    #endregion
}
