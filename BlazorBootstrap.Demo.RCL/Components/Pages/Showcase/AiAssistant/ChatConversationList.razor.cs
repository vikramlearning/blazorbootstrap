using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The signed-in member's conversations for the chat page: a "New chat" link, a search box (titles and the member's own
/// messages, with the number of matches announced), and "Pinned" and "Recent" groups with the open conversation marked
/// <c>aria-current="page"</c>. Each conversation has a Pin toggle (<c>aria-pressed</c>; done here, with focus kept on the
/// toggle when the conversation moves to the other group), and Rename and Delete buttons that ask the page to open its
/// dialog. Empty states cover no conversations and no matches.
/// </summary>
public partial class ChatConversationList : ComponentBase
{
    #region Fields and Constants

    private readonly Dictionary<int, ElementReference> deleteButtons = new();

    private readonly Dictionary<int, ElementReference> pinButtons = new();

    private readonly Dictionary<int, ElementReference> renameButtons = new();

    /// <summary>The conversation whose Pin toggle takes focus after the next render (it moved to the other group).</summary>
    private int? focusPinId;

    private string search = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusPinId is int id)
        {
            focusPinId = null;

            if (pinButtons.TryGetValue(id, out var button))
                await AiDisplay.FocusAsync(button);
        }
    }

    #endregion

    #region Private Methods

    private void ClearSearch() => search = string.Empty;

    private async Task RequestAsync(ListAction action, AiConversation conversation)
    {
        var buttons = action == ListAction.Rename ? renameButtons : deleteButtons;
        ElementReference? opener = buttons.TryGetValue(conversation.Id, out var button) ? button : null;

        await OnRequest.InvokeAsync(new ListRequest(action, conversation, opener));
    }

    private async Task TogglePinAsync(AiConversation conversation)
    {
        var pin = !conversation.IsPinned;
        var title = conversation.Title;
        var result = State.SetPinned(conversation.Id, pin);

        var message = result switch
        {
            AiResult.Ok => pin ? $"Pinned “{title}”. It's at the top of the list." : $"Unpinned “{title}”. It's back under Recent.",
            AiResult.NoChange => pin ? $"“{title}” was already pinned." : $"“{title}” wasn't pinned.",
            AiResult.NotFound => $"“{title}” no longer exists, so nothing changed.",
            AiResult.NotAllowed => "Sign in as an active member to change conversations. Nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        };

        // The conversation moves to the other group, which re-creates its row: keep focus on its toggle.
        if (result == AiResult.Ok)
            focusPinId = conversation.Id;

        await OnStatus.InvokeAsync(new ListStatus(message, result is not (AiResult.Ok or AiResult.NoChange)));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the open conversation, or <see langword="null" /> on a new chat.</summary>
    [Parameter] public int? CurrentId { get; set; }

    /// <summary>The non-empty groups of the matching conversations: pinned, then recent.</summary>
    private IEnumerable<(string Key, string Label, List<AiConversation> Items)> Groups
    {
        get
        {
            var pinned = Results.Where(conversation => conversation.IsPinned).ToList();
            var recent = Results.Where(conversation => !conversation.IsPinned).ToList();

            if (pinned.Count > 0)
                yield return ("pinned", "Pinned", pinned);

            if (recent.Count > 0)
                yield return ("recent", "Recent", recent);
        }
    }

    private static string NewChatUrl => $"{AiLayout.Base}/chat";

    /// <summary>Gets or sets what runs when a link in the list is followed (the page closes the drawer).</summary>
    [Parameter] public EventCallback OnNavigate { get; set; }

    /// <summary>Gets or sets what runs when Rename or Delete is pressed: the page opens its dialog.</summary>
    [Parameter] public EventCallback<ListRequest> OnRequest { get; set; }

    /// <summary>Gets or sets what runs with the outcome of a Pin toggle, for the page's status message.</summary>
    [Parameter] public EventCallback<ListStatus> OnStatus { get; set; }

    /// <summary>Gets or sets the prefix of the element IDs (the list is on the page twice).</summary>
    [Parameter, EditorRequired] public string Prefix { get; set; } = default!;

    private IReadOnlyList<AiConversation> Results => State.Search(search);

    private string? SearchSummary => search.Trim().Length == 0
        ? null
        : Results.Count switch
        {
            0 => "No conversations match.",
            1 => "1 conversation matches.",
            var count => $"{count} conversations match.",
        };

    [Inject] private AiState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>What a list button asks the page to do.</summary>
    public enum ListAction
    {
        /// <summary>Rename the conversation.</summary>
        Rename,

        /// <summary>Delete the conversation (after a confirmation).</summary>
        Delete,
    }

    /// <summary>A request from the list: the action, the conversation, and the button that asked (for focus after the dialog).</summary>
    public sealed record ListRequest(ListAction Action, AiConversation Conversation, ElementReference? Opener);

    /// <summary>The outcome of a Pin toggle, in words.</summary>
    public sealed record ListStatus(string Message, bool IsError);

    #endregion
}
