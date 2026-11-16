using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The chat, for active members: the conversation list (<see cref="ChatConversationList" />, beside the chat on wide screens
/// and in an <see cref="Offcanvas" /> on narrow ones) with search, pin, rename, and delete (confirmed in a <see cref="Modal" />);
/// the messages, with the member's text as plain text and attachments by name and size, and the assistant's replies as
/// Markdown from the fixed templates; and the composer, with the model, attachments (names and sizes only: no file is
/// opened), saved prompts, and Enter-to-send. A reply streams by the clock: while one does, a timer re-renders the page
/// every 150 ms, only complete lines are shown (a code fence still open is closed for now), the reply is <c>aria-busy</c>,
/// and a Stop button is offered; its start and its end are each announced once. A finished reply can be rated, and the
/// last one regenerated. A new chat shows a welcome with four starters, and moves to the new conversation's address after
/// the first message. <c>?prompt={id}</c> (the prompt library's "Use in chat") puts that prompt in the message box. An
/// unknown conversation shows the not-found panel.
/// </summary>
public partial class Chat : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string AnsweringText = $"{AiSeedData.ProductName} is answering…";

    /// <summary>The character count shows when the message is this close to the limit.</summary>
    private const int CountWarningMargin = 500;

    private const string MessagesId = "pa-chat-messages";

    private static readonly Starter[] starters =
    {
        new("Plan a product launch", "A four-week plan, step by step", "Help me plan the launch of our new mobile app over the next four weeks.", IconName.Calendar3),
        new("Write a SQL query", "Monthly churn from an orders table", "Write a SQL query that shows monthly customer churn from our orders table.", IconName.Database),
        new("Draft an email", "Tell the design team about a new date", "Draft an email to the design team saying the launch date moved to December 2.", IconName.Envelope),
        new("Explain a concept", "A race condition, in plain words", "Explain what a race condition is, with an everyday example.", IconName.Lightbulb),
    };

    private static readonly TimeSpan streamInterval = TimeSpan.FromMilliseconds(150);

    private readonly List<AiAttachment> attachments = new();

    /// <summary>Handlers that stop following a streaming reply once the member scrolls, clicks, or types in the messages.</summary>
    private readonly Dictionary<string, object> followHandlers;

    private readonly Dictionary<int, ElementReference> helpfulButtons = new();

    /// <summary>The replies of the open conversation that were streaming at the last check.</summary>
    private readonly HashSet<int> streamingIds = new();

    private ElementReference? composerInput;

    private Button? conversationsButton;

    /// <summary>The open conversation, just deleted: shown as a new chat (not "not found") until the address changes.</summary>
    private int? deletedOpenId;

    /// <summary>What the dialog does: rename or delete.</summary>
    private ChatConversationList.ListAction dialog;

    /// <summary>Whether the dialog opens once the drawer it was asked from has closed.</summary>
    private bool dialogAfterDrawer;

    private AiConversation? dialogConversation;

    private Modal dialogModal = default!;

    /// <summary>The element that gets focus back when the dialog closes.</summary>
    private ElementReference? dialogOpener;

    private string draft = string.Empty;

    private Offcanvas drawer = default!;

    /// <summary>Whether the drawer closes because a link in it was followed (focus then goes to the new heading).</summary>
    private bool drawerClosedForNavigation;

    private bool drawerOpen;

    /// <summary>The message as it was when a plain Enter was pressed; the input event that follows sends it.</summary>
    private string? enterDraft;

    private InputFile? fileInput;

    /// <summary>Changes after files are chosen or sent, so the native file input starts empty again.</summary>
    private int fileInputKey;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>Whether the message list keeps scrolling to the end while a reply streams (after the member sent or regenerated).</summary>
    private bool followStream;

    private int? handledPromptId;

    private ElementReference? heading;

    /// <summary>Whether the draft and attachments survive the next change of address (moving to a new conversation after sending).</summary>
    private bool keepComposer;

    /// <summary>Whether the status message survives the next change of address.</summary>
    private bool keepStatus;

    private bool loaded;

    private int? loadedId;

    private string? newChatModelId;

    /// <summary>The confirmed dialog action, run once the dialog is hidden; it returns whether the opener should get focus back.</summary>
    private Func<bool>? pendingAction;

    private bool promptChosen;

    private string promptFilter = string.Empty;

    private ElementReference? promptFilterInput;

    private Modal promptModal = default!;

    private Button? promptsButton;

    private string? renameError;

    private ElementReference? renameInput;

    private string renameText = string.Empty;

    private bool scrollToEnd;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private ElementReference? stopButton;

    /// <summary>Whether the Stop button has focus, so focus can move on when it disappears.</summary>
    private bool stopFocused;

    private string? streamStatus;

    private CancellationTokenSource? streamTimer;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Prepares the handlers that stop following a streaming reply.</summary>
    public Chat()
    {
        var stop = EventCallback.Factory.Create(this, StopFollowing);
        followHandlers = new Dictionary<string, object> { ["onwheel"] = stop, ["onpointerdown"] = stop, ["onkeydown"] = stop };
    }

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusTarget is { } target)
        {
            focusTarget = null;
            await AiDisplay.FocusAsync(target());
        }

        if (focusStatus)
        {
            focusStatus = false;
            await AiDisplay.FocusAsync(statusRegion);
        }

        if (scrollToEnd)
        {
            scrollToEnd = false;
            await ScrollToEndAsync();
        }

        // Only interactive renders get here, so the timer never runs while prerendering.
        if (streamTimer is null && streamingIds.Count > 0)
            StartStreamTimer();
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (!loaded || loadedId != Id)
        {
            loaded = true;
            loadedId = Id;
            deletedOpenId = null;
            helpfulButtons.Clear();
            stopFocused = false;

            if (!keepComposer)
            {
                draft = string.Empty;
                enterDraft = null;
                attachments.Clear();
                fileInputKey++;
                newChatModelId = null;
            }

            if (!keepStatus)
            {
                statusMessage = null;
                statusIsError = false;
            }

            keepComposer = keepStatus = false;

            // A reply already streaming here (the first one of a new chat, or one left mid-way) is announced once.
            streamingIds.Clear();
            streamingIds.UnionWith(StreamingReplyIds());
            streamStatus = streamingIds.Count > 0 ? AnsweringText : null;
            followStream = streamingIds.Count > 0;
            scrollToEnd = Conversation is not null;

            if (streamingIds.Count == 0)
                StopStreamTimer();
        }

        ApplyPromptQuery();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes and stops the streaming timer.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        StopStreamTimer();
    }

    #endregion

    #region Private Methods

    private static string Clip(string text) => text.Length <= AiState.MaxMessageLength ? text : text[..AiState.MaxMessageLength];

    private static string MessageCountText(AiConversation conversation) => conversation.Messages.Count == 1 ? "1 message" : $"{conversation.Messages.Count} messages";

    private static int NewLines(string text) => text.Count(character => character == '\n');

    private static string Number(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private bool ApplyDelete(AiConversation conversation)
    {
        var title = conversation.Title;
        var wasOpen = conversation.Id == Id;
        var result = State.DeleteConversation(conversation.Id);
        var gone = result is AiResult.Ok or AiResult.NotFound;

        Announce(gone, result switch
        {
            AiResult.Ok => $"Deleted “{title}”.",
            AiResult.NotFound => $"“{title}” was already deleted.",
            _ => "Sign in as an active member to delete conversations. Nothing was deleted.",
        });

        if (gone && wasOpen)
        {
            deletedOpenId = conversation.Id;
            keepStatus = true;
            Navigation.NavigateTo(NewChatUrl);
        }

        // The conversation's row, and the button that opened the dialog, are gone: the status message takes focus.
        return !gone;
    }

    /// <summary>
    /// Puts the prompt from <c>?prompt={id}</c> in the message box and counts a use, once, then drops the query from the
    /// address so a reload or Back doesn't count it again. Until the tab's data is restored nobody is signed in, so it waits.
    /// </summary>
    private void ApplyPromptQuery()
    {
        if (PromptId is not int promptId || handledPromptId == promptId || !State.IsActiveMember)
            return;

        handledPromptId = promptId;

        if (State.FindPrompt(promptId) is { } prompt)
        {
            draft = Clip(prompt.Text);
            State.UsePrompt(prompt.Id);
            Announce(true, $"“{prompt.Title}” is in the message box. Edit it if you like, then send.");
            focusTarget = () => composerInput;
        }
        else
        {
            Announce(false, "That saved prompt no longer exists, so nothing was added to the message box.");
        }

        // Same conversation, so the message box and the status message stay as they are.
        Navigation.NavigateTo(Id is int id ? AiDisplay.ConversationUrl(id) : NewChatUrl, replace: true);
    }

    private bool ApplyRename(AiConversation conversation, string title)
    {
        var result = State.RenameConversation(conversation.Id, title);

        Announce(result is AiResult.Ok or AiResult.NoChange, result switch
        {
            AiResult.Ok => $"Renamed to “{title}”.",
            AiResult.NoChange => "The title is unchanged.",
            AiResult.Invalid => $"Not renamed: a title needs 1 to {AiState.MaxTitleLength} characters.",
            AiResult.NotFound => "That conversation no longer exists, so it wasn't renamed.",
            _ => "Sign in as an active member to rename conversations. Nothing changed.",
        });

        return result != AiResult.NotFound;
    }

    private async Task ChoosePromptAsync(AiPrompt prompt)
    {
        var result = State.UsePrompt(prompt.Id);

        if (result == AiResult.Ok)
        {
            draft = draft.Trim().Length == 0 ? Clip(prompt.Text) : Clip($"{draft.TrimEnd()}\n\n{prompt.Text}");
            Announce(true, $"Added “{prompt.Title}” to the message box.");
            promptChosen = true;
        }
        else
        {
            Announce(false, result == AiResult.NotFound
                ? $"“{prompt.Title}” no longer exists, so nothing was added."
                : "Sign in as an active member to use saved prompts. Nothing was added.");
        }

        await promptModal.HideAsync();
    }

    /// <summary>Whether a model is one the workspace enables now.</summary>
    private bool IsEnabled(string? modelId) => modelId is not null && State.Policy.EnabledModelIds.Contains(modelId);

    private string ModelName(string? modelId) => State.FindModel(modelId)?.Name ?? modelId ?? "the model";

    private void OnDialogHidden()
    {
        var action = pendingAction;
        pendingAction = null;
        var opener = dialogOpener;

        if (action is not null && !action())
            focusStatus = true;
        else
            focusTarget = () => opener;
    }

    private void OnDialogShown()
    {
        if (dialog == ChatConversationList.ListAction.Rename)
            focusTarget = () => renameInput;
    }

    private void OnDraftInput(ChangeEventArgs e)
    {
        var value = e.Value?.ToString() ?? string.Empty;

        // A plain Enter with Enter-to-send on: the browser has already added a line break, so send the text as it was before it.
        if (enterDraft is { } before)
        {
            enterDraft = null;

            if (NewLines(value) > NewLines(before))
            {
                Send(before);
                return;
            }
        }

        draft = value;
    }

    /// <summary>
    /// Blazor can't cancel a single key without script, so a plain Enter (with Enter-to-send on) is only noted here, and the
    /// input event that follows (events arrive in order on both hosts) sends the text as it was before the line break.
    /// Shift+Enter adds a line; Ctrl+Enter or ⌘+Enter always sends.
    /// </summary>
    private void OnDraftKeyDown(KeyboardEventArgs e)
    {
        enterDraft = null;

        if (e.Key != "Enter" || e.IsComposing || e.AltKey)
            return;

        if (e.CtrlKey || e.MetaKey)
        {
            Send(draft);
            return;
        }

        if (e.ShiftKey || !State.Preferences.EnterToSend)
            return;

        // At the length limit the browser adds no line break, so no input event follows: send now.
        if (draft.Length >= AiState.MaxMessageLength)
        {
            Send(draft);
            return;
        }

        enterDraft = draft;
    }

    /// <summary>A dialog asked from the drawer opens now that the drawer is closed (two focus traps at once would fight).</summary>
    private async Task OnDrawerHiddenAsync()
    {
        drawerOpen = false;

        if (dialogAfterDrawer)
        {
            dialogAfterDrawer = false;
            await dialogModal.ShowAsync();
            return;
        }

        if (drawerClosedForNavigation)
        {
            drawerClosedForNavigation = false;
            focusTarget = () => heading;
        }
        else
        {
            focusTarget = () => conversationsButton?.Element;
        }
    }

    private void OnDrawerShown() => drawerOpen = true;

    /// <summary>Adds the chosen files by name, size, and type (a file's content is never opened), within the limits.</summary>
    private void OnFilesChosen(InputFileChangeEventArgs e)
    {
        var chosen = e.GetMultipleFiles(Math.Max(1, e.FileCount));
        var limitBytes = State.Policy.MaxAttachmentMb * 1024L * 1024L;
        var added = new List<string>();
        var skipped = new List<string>();

        foreach (var file in chosen)
        {
            if (attachments.Count >= AiState.MaxAttachments)
            {
                skipped.Add($"{file.Name} (a message takes at most {AiState.MaxAttachments} files)");
            }
            else if (file.Size > limitBytes)
            {
                skipped.Add($"{file.Name} ({AiDisplay.FileSize(file.Size)}, over the {State.Policy.MaxAttachmentMb} MB limit)");
            }
            else if (string.IsNullOrWhiteSpace(file.Name) || file.Name.Length > AiState.MaxAttachmentNameLength)
            {
                skipped.Add($"a file whose name is longer than {AiState.MaxAttachmentNameLength} characters");
            }
            else
            {
                var type = file.ContentType ?? string.Empty;
                attachments.Add(new AiAttachment(file.Name, file.Size, type.Length <= 100 ? type : type[..100]));
                added.Add(file.Name);
            }
        }

        var addedText = added.Count switch
        {
            0 => "No files were attached.",
            1 => $"Attached {added[0]}.",
            _ => $"Attached {added.Count} files.",
        };

        Announce(skipped.Count == 0, skipped.Count == 0 ? addedText : $"{addedText} Not attached: {string.Join("; ", skipped)}.");

        // A fresh native input (it would still count the files picked before), focused again.
        fileInputKey++;
        focusTarget = () => fileInput?.Element;
    }

    /// <summary>A link in the drawer was followed: close the drawer, and move focus to the new heading once it's closed.</summary>
    private async Task OnListNavigateAsync()
    {
        if (!drawerOpen)
            return;

        drawerClosedForNavigation = true;
        await drawer.HideAsync();
    }

    private async Task OnListRequestAsync(ChatConversationList.ListRequest request)
    {
        dialog = request.Action;
        dialogConversation = request.Conversation;
        renameText = request.Conversation.Title;
        renameError = null;
        pendingAction = null;

        if (drawerOpen)
        {
            // The button that asked is in the drawer, which closes first; focus then comes back to the button that opens it.
            dialogOpener = conversationsButton?.Element;
            dialogAfterDrawer = true;
            await drawer.HideAsync();
            return;
        }

        dialogOpener = request.Opener;
        await dialogModal.ShowAsync();
    }

    private void OnListStatus(ChatConversationList.ListStatus status) => Announce(!status.IsError, status.Message);

    private void OnModelChanged(ChangeEventArgs e)
    {
        var modelId = e.Value?.ToString();

        if (Conversation is not { } conversation)
        {
            newChatModelId = IsEnabled(modelId) ? modelId : null;
            return;
        }

        switch (State.SetConversationModel(conversation.Id, modelId ?? string.Empty))
        {
            case AiResult.Ok:
                Announce(true, $"New replies in this conversation use {ModelName(modelId)}.");
                break;
            case AiResult.NoChange:
                break;
            case AiResult.WrongState:
                Announce(false, $"{ModelName(modelId)} was turned off by a workspace admin. Choose another model.");
                break;
            case AiResult.NotFound:
                Announce(false, "This conversation was deleted, so its model can't change.");
                break;
            default:
                Announce(false, "Sign in as an active member to change the model. Nothing changed.");
                break;
        }
    }

    private void OnPromptsHidden() => focusTarget = promptChosen ? () => composerInput : () => promptsButton?.Element;

    private void OnPromptsShown() => focusTarget = () => promptFilterInput;

    private void OnStateChanged() => InvokeAsync(() =>
    {
        TrackStreaming();
        StateHasChanged();
    });

    private async Task OpenDrawerAsync() => await drawer.ShowAsync();

    private async Task OpenPromptsAsync()
    {
        promptFilter = string.Empty;
        promptChosen = false;
        await promptModal.ShowAsync();
    }

    /// <summary>
    /// Regenerates the last reply. When an admin turned the conversation's model off, the reply uses the workspace default,
    /// as the notice above the message box says.
    /// </summary>
    private void Regenerate()
    {
        if (Conversation is not { } conversation)
            return;

        if (!IsEnabled(conversation.ModelId))
            State.SetConversationModel(conversation.Id, State.Policy.DefaultModelId);

        var result = State.Regenerate(conversation.Id, out _);

        if (result == AiResult.Ok)
        {
            statusMessage = null;
            statusIsError = false;
            TrackStreaming();
            followStream = scrollToEnd = true;

            // The Regenerate button is gone; the new reply's Stop button takes its place.
            focusTarget = () => stopButton;
            return;
        }

        Announce(false, result switch
        {
            AiResult.WrongState => conversation.Messages.LastOrDefault() is { } last && State.IsStreaming(last)
                ? "Wait for the answer to finish (or stop it) before regenerating."
                : $"Not regenerated: {ModelName(conversation.ModelId)} was turned off by a workspace admin. Choose another model, then try again.",
            AiResult.OverQuota => $"Not regenerated: a new reply would pass your monthly quota of {AiDisplay.Tokens(State.Policy.MonthlyTokenQuota)} tokens. A workspace admin can raise the quota.",
            AiResult.Invalid => "Not regenerated: this tab's storage is full. Delete a conversation to make room.",
            AiResult.NotFound => "This conversation was deleted, so nothing was regenerated.",
            _ => "Sign in as an active member to regenerate replies. Nothing changed.",
        });
    }

    private void RemoveAttachment(int index)
    {
        if (index < 0 || index >= attachments.Count)
            return;

        var name = attachments[index].Name;
        attachments.RemoveAt(index);
        Announce(true, $"Removed {name}.");
        focusTarget = State.Policy.AllowAttachments ? () => fileInput?.Element : () => composerInput;
    }

    /// <summary>Re-renders the page every 150 ms while a reply of the open conversation streams; stops when none does.</summary>
    private async Task RunStreamTimerAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(streamInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                await InvokeAsync(() =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    scrollToEnd = followStream;
                    TrackStreaming();
                    StateHasChanged();
                });
            }
        }
        catch (OperationCanceledException)
        {
            // The stream ended or the page was left.
        }
        catch (ObjectDisposedException)
        {
            // The page was disposed while a tick was running.
        }
    }

    /// <summary>Scrolls the message list to its end with the library's own helper (no eval).</summary>
    private async Task ScrollToEndAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.scrollToElementBottom", MessagesId);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException)
        {
            // The page or the circuit has gone; nothing to scroll.
        }
    }

    /// <summary>Sends a message; on success the message box empties and keeps focus, and a new chat moves to its own address.</summary>
    private void Send(string text)
    {
        var existing = Conversation;

        // Whenever nothing is sent, the text stays in the box (without an Enter-to-send line break), with focus.
        draft = text;
        focusTarget = () => composerInput;

        if (existing is not null && streamingIds.Count > 0)
        {
            Announce(false, $"{AiSeedData.ProductName} is still answering. Wait for it to finish, or press Stop, then send.");
            return;
        }

        var trimmed = text.Trim();

        if (trimmed.Length == 0)
        {
            draft = string.Empty;
            Announce(false, "Write a message first.");
            return;
        }

        var files = attachments.ToList();
        var modelId = SelectedModelId;
        var result = State.Send(existing?.Id ?? 0, trimmed, files, modelId, out var conversation, out _);

        if (result != AiResult.Ok || conversation is null)
        {
            Announce(false, SendFailure(result, trimmed, files, modelId, existing));
            return;
        }

        draft = string.Empty;
        attachments.Clear();
        fileInputKey++;
        statusMessage = null;
        statusIsError = false;
        followStream = scrollToEnd = true;
        TrackStreaming();

        if (existing is null)
        {
            keepComposer = true;
            Navigation.NavigateTo(AiDisplay.ConversationUrl(conversation.Id));
        }
    }

    /// <summary>Returns the message for a refused send, naming the limit or the setting that stopped it.</summary>
    private string SendFailure(AiResult result, string text, IReadOnlyList<AiAttachment> files, string modelId, AiConversation? existing)
    {
        switch (result)
        {
            case AiResult.OverQuota:
                var used = State.CurrentMember is { } member ? State.MonthTokens(member.Email) : 0;

                return $"Not sent: this message and its reply would pass your monthly quota of {AiDisplay.Tokens(State.Policy.MonthlyTokenQuota)} tokens "
                    + $"(you've used {AiDisplay.Tokens(used)} this month). A workspace admin can raise the quota.";

            case AiResult.WrongState:
                return files.Count > 0 && !State.Policy.AllowAttachments
                    ? "Not sent: a workspace admin turned attachments off. Remove the files, then send again."
                    : $"Not sent: {ModelName(modelId)} was turned off by a workspace admin. Choose another model, then send again.";

            case AiResult.Invalid:
                var limitBytes = State.Policy.MaxAttachmentMb * 1024L * 1024L;

                if (text.Length > AiState.MaxMessageLength)
                    return $"Not sent: the message has {Number(text.Length)} characters; the most is {Number(AiState.MaxMessageLength)}.";

                if (files.Count > AiState.MaxAttachments)
                    return $"Not sent: a message takes at most {AiState.MaxAttachments} files. Remove {files.Count - AiState.MaxAttachments}.";

                if (files.FirstOrDefault(file => file.SizeBytes > limitBytes) is { } large)
                    return $"Not sent: {large.Name} is over the {State.Policy.MaxAttachmentMb} MB limit. Remove it, then send again.";

                if (files.Any(file => file.Name.Length > AiState.MaxAttachmentNameLength))
                    return $"Not sent: a file name is longer than {AiState.MaxAttachmentNameLength} characters. Remove that file, then send again.";

                if (existing is not null && existing.Messages.Count + 2 > AiState.MaxMessages)
                    return $"Not sent: this conversation has reached {AiState.MaxMessages} messages, the most one can hold. Start a new chat to go on.";

                if (existing is null && State.Conversations.Count >= AiState.MaxConversations)
                    return $"Not sent: you have {AiState.MaxConversations} conversations, the most this demo keeps. Delete one to start another.";

                return "Not sent: this tab's storage is full. Delete a conversation to make room.";

            case AiResult.NotFound:
                return "Not sent: this conversation was deleted. Start a new chat.";

            case AiResult.NotAllowed:
                return $"Not sent: sign in as an active member of {AiSeedData.WorkspaceName} to chat.";

            default:
                return "Not sent. Try again.";
        }
    }

    private void SetFeedback(AiMessage message, AiFeedback pressed)
    {
        if (Conversation is not { } conversation)
            return;

        // Pressing the pressed button clears the rating.
        var feedback = message.Feedback == pressed ? AiFeedback.None : pressed;

        switch (State.SetFeedback(conversation.Id, message.Id, feedback))
        {
            case AiResult.Ok:
                Announce(true, feedback switch
                {
                    AiFeedback.Helpful => "Marked as helpful. Thanks for the feedback.",
                    AiFeedback.NotHelpful => "Marked as not helpful. Thanks; Regenerate gives another answer.",
                    _ => "Your rating was cleared.",
                });
                break;
            case AiResult.NoChange:
                break;
            case AiResult.NotFound:
                Announce(false, "That reply no longer exists, so it wasn't rated.");
                break;
            default:
                Announce(false, "Sign in as an active member to rate replies. Nothing changed.");
                break;
        }
    }

    private void StartStreamTimer()
    {
        streamTimer = new CancellationTokenSource();
        _ = RunStreamTimerAsync(streamTimer.Token);
    }

    private void StopFollowing() => followStream = false;

    private void StopReply(AiMessage message)
    {
        if (Conversation is not { } conversation)
            return;

        var result = State.Stop(conversation.Id, message.Id);

        if (result is AiResult.Ok or AiResult.NoChange)
        {
            // "Stopped" (or "Answer finished") is announced as the stream ends; the rating buttons take the Stop button's place.
            stopFocused = false;
            TrackStreaming();
            focusTarget = () => helpfulButtons.TryGetValue(message.Id, out var button) ? button : composerInput;
            return;
        }

        Announce(false, result == AiResult.NotFound ? "That reply no longer exists." : "Sign in as an active member to stop replies.");
    }

    private void StopStreamTimer()
    {
        if (streamTimer is null)
            return;

        streamTimer.Cancel();
        streamTimer.Dispose();
        streamTimer = null;
    }

    private IEnumerable<int> StreamingReplyIds() =>
        Conversation?.Messages.Where(message => message.Role == AiRole.Assistant && State.IsStreaming(message)).Select(message => message.Id) ?? Enumerable.Empty<int>();

    private void Submit()
    {
        enterDraft = null;
        Send(draft);
    }

    /// <summary>Checks the dialog's field, then closes it; the action runs once it is hidden.</summary>
    private async Task SubmitDialogAsync()
    {
        if (pendingAction is not null || dialogConversation is not { } conversation)
            return;

        if (dialog == ChatConversationList.ListAction.Rename)
        {
            var title = renameText.Trim();
            renameError = title.Length == 0 ? "Enter a title."
                : title.Length > AiState.MaxTitleLength ? $"Use at most {AiState.MaxTitleLength} characters."
                : null;

            if (renameError is not null)
            {
                focusTarget = () => renameInput;
                return;
            }

            pendingAction = () => ApplyRename(conversation, title);
        }
        else
        {
            pendingAction = () => ApplyDelete(conversation);
        }

        await dialogModal.HideAsync();
    }

    /// <summary>
    /// Notes which replies started or stopped streaming since the last check and says so once in the streaming status:
    /// "Parley is answering…" when one starts, "Answer finished" or "Stopped" when it ends. Stops the timer when none streams.
    /// </summary>
    private void TrackStreaming()
    {
        var now = StreamingReplyIds().ToHashSet();
        var started = now.Except(streamingIds).Any();
        var ended = streamingIds.Except(now).Select(id => Conversation?.Messages.FirstOrDefault(message => message.Id == id)).OfType<AiMessage>().ToList();

        if (started)
            streamStatus = AnsweringText;
        else if (ended.Count > 0)
            streamStatus = ended.Any(message => message.IsStopped) ? "Stopped" : "Answer finished";

        // Focus was on the Stop button, which is gone: move it to the finished reply's first rating button.
        if (ended.Count > 0 && stopFocused)
        {
            stopFocused = false;
            var endedId = ended[^1].Id;
            focusTarget = () => helpfulButtons.TryGetValue(endedId, out var button) ? button : composerInput;
        }

        streamingIds.Clear();
        streamingIds.UnionWith(now);

        if (now.Count == 0)
        {
            followStream = false;
            StopStreamTimer();
        }
    }

    private void UseStarter(string text)
    {
        draft = text;
        focusTarget = () => composerInput;
    }

    /// <summary>
    /// The Markdown to show for a reply. While it streams, only its complete lines (so a half-written list item or fence line
    /// doesn't flicker), with a code fence that is still open closed for now; a stopped reply is cut the same way.
    /// </summary>
    private string VisibleReply(AiMessage message, bool streaming)
    {
        var text = State.ReplyMarkdown(message);

        if (!streaming && !message.IsStopped)
            return text;

        var cut = text.LastIndexOf('\n');

        if (cut >= 0)
            text = text[..cut].TrimEnd('\r');
        else if (streaming)
            return string.Empty;

        if (text.Split('\n').Count(line => line.TrimStart().StartsWith("```", StringComparison.Ordinal)) % 2 == 1)
            text += "\n```";

        return text;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string CharacterCount => $"{Number(draft.Length)} of {Number(AiState.MaxMessageLength)} characters.";

    private AiConversation? Conversation => Id is int id ? State.FindConversation(id) : null;

    private string DialogTitle => dialog == ChatConversationList.ListAction.Delete
        ? $"Delete “{dialogConversation?.Title}”?"
        : "Rename the conversation";

    /// <summary>The stop-following handlers, attached only while following a streaming reply (so scrolling doesn't re-render otherwise).</summary>
    private Dictionary<string, object>? FollowHandlers => followStream && streamingIds.Count > 0 ? followHandlers : null;

    /// <summary>Gets or sets the conversation number from the route, or <see langword="null" /> for a new chat.</summary>
    [Parameter] public int? Id { get; set; }

    /// <summary>An unknown conversation, once the member's data is there (not the open one just deleted, while the address changes).</summary>
    private bool IsNotFound => Id is int id && id != deletedOpenId && Conversation is null && State.IsActiveMember;

    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private static string NewChatUrl => $"{AiLayout.Base}/chat";

    private static string ProfileUrl => $"{AiLayout.Base}/profile";

    private string? PromptFilterSummary => promptFilter.Trim().Length == 0
        ? null
        : PromptGroups.Sum(group => group.Items.Count) switch
        {
            0 => "No prompts match.",
            1 => "1 prompt matches.",
            var count => $"{count} prompts match.",
        };

    /// <summary>The member's own prompts and the shared ones, filtered by title, text, or tag.</summary>
    private IEnumerable<(string Key, string Label, string Empty, List<AiPrompt> Items)> PromptGroups
    {
        get
        {
            var term = promptFilter.Trim();

            bool Matches(AiPrompt prompt) => term.Length == 0 || prompt.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                || prompt.Text.Contains(term, StringComparison.OrdinalIgnoreCase) || prompt.Tags.Any(tag => tag.Contains(term, StringComparison.OrdinalIgnoreCase));

            yield return ("mine", "Your prompts", term.Length == 0 ? "You haven't saved any prompts yet. Save them in the prompt library." : "None of your prompts match.",
                State.MyPrompts.Where(Matches).ToList());
            yield return ("shared", $"Shared with {AiSeedData.WorkspaceName}", term.Length == 0 ? "Workspace admins haven't shared any prompts yet." : "No shared prompts match.",
                State.SharedPrompts.Where(Matches).ToList());
        }
    }

    /// <summary>Gets or sets the saved prompt to put in the message box (<c>?prompt={id}</c> from the prompt library).</summary>
    [SupplyParameterFromQuery(Name = "prompt")] public int? PromptId { get; set; }

    private static string PromptsUrl => $"{AiLayout.Base}/prompts";

    /// <summary>
    /// The model the next message uses: an open conversation's (the workspace default when an admin turned it off), else the
    /// one picked for this new chat, else the member's preferred model.
    /// </summary>
    private string SelectedModelId => Conversation is { } conversation
        ? IsEnabled(conversation.ModelId) ? conversation.ModelId : State.Policy.DefaultModelId
        : newChatModelId is { } chosen && IsEnabled(chosen) ? chosen : State.PreferredModelId;

    [Inject] private AiState State { get; set; } = default!;

    /// <summary>The open conversation's model (for a new chat, the member's preferred one) when an admin turned it off.</summary>
    private AiModel? TurnedOffModel => Conversation is { } conversation
        ? IsEnabled(conversation.ModelId) ? null : State.FindModel(conversation.ModelId)
        : newChatModelId is null && State.Preferences.ModelId is { } preferred && !IsEnabled(preferred) ? State.FindModel(preferred) : null;

    #endregion

    #region Nested Types

    /// <summary>A starter on the empty new chat: its button text, a hint, the sample message it puts in the box, and an icon.</summary>
    private sealed record Starter(string Label, string Hint, string Text, IconName Icon);

    #endregion
}
