namespace BlazorBootstrap;

public partial class OpenAIChat : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private string key = "";
    private string model = "gpt-4";
    private string userQuestion = string.Empty;
    private readonly List<Message> conversationHistory = new();
    private bool isSendingMessage;
    private string? currentCompletion;
    private DotNetObjectReference<OpenAIChat>? objRef;

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();
    }

    private async Task OnKeyPress(KeyboardEventArgs e)
    {
        if (e.Key is not "Enter") return;
        await SendMessageAsync();
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(userQuestion)) return;

        var message = new Message("user", userQuestion);
        conversationHistory.Add(message);

        StateHasChanged();

        await CreateCompletionAsync(message);
        ClearInput();

        StateHasChanged();
    }

    private void ClearInput() => userQuestion = string.Empty;

    private void ClearConversation()
    {
        ClearInput();
        conversationHistory.Clear();
    }

    private async Task CreateCompletionAsync(Message message)
    {
        isSendingMessage = true;
        //await AIJSInterop.CreateChatCompletionsAsync(key, message, objRef!); // OpenAI
        await AIJSInterop.CreateChatCompletions2Async("", message, objRef!); // Azure OpenAI
        //await OpenAIChatJsInterop.CreateChatCompletionsApiAsync(key, message, objRef!); // API
    }

    [JSInvokable]
    public async Task ChartCompletetionsStreamJs(string content, bool done)
    {
        if (isSendingMessage)
            isSendingMessage = false;

        if (done)
        {
            conversationHistory.Add(new Message("system", currentCompletion!));
            currentCompletion = "";
            await InvokeAsync(StateHasChanged);
            return;
        }

        currentCompletion += content;
        await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region Properties, Indexers

    [Inject] private AIJSInterop AIJSInterop { get; set; } = default!;

    #endregion
}

public record Message(string Role, string Content);