namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The Markdown of one assistant reply on the chat page. Its parameters are immutable values, so Blazor skips it while the
/// text is unchanged, and the page can re-render every streaming tick without re-parsing every reply.
/// </summary>
public partial class ChatReplyBody : ComponentBase
{
    #region Properties, Indexers

    /// <summary>Gets or sets whether the reply is still streaming (a finished reply gets a fresh Markdown).</summary>
    [Parameter] public bool IsStreaming { get; set; }

    /// <summary>Gets or sets the reply's message number, for the Markdown key.</summary>
    [Parameter, EditorRequired] public int MessageId { get; set; }

    /// <summary>Gets or sets the Markdown to show (a template's text as far as it has streamed).</summary>
    [Parameter, EditorRequired] public string Text { get; set; } = string.Empty;

    #endregion
}
