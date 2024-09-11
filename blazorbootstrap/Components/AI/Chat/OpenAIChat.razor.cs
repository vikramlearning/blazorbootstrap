namespace BlazorBootstrap;

public partial class OpenAIChat : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private string key = "";
    private string model = "gpt-4";
    private string userPrompt = string.Empty;
    private readonly List<Message> conversationHistory = new();
    private bool isRequestInProgress;
    private string? currentCompletion;
    private DotNetObjectReference<OpenAIChat>? objRef;

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();
    }

    private async Task SendPromptAsync()
    {
        if (string.IsNullOrWhiteSpace(userPrompt)) 
            return;

        var message = new Message("user", userPrompt);
        conversationHistory.Add(message);

        await CreateCompletionAsync(new List<Message> { message });
    }

    private void ClearInput() => userPrompt = string.Empty;

    private async Task CreateCompletionAsync(List<Message> messages)
    {
        isRequestInProgress = true;
        //await AIJSInterop.CreateChatCompletionsAsync(key, messages, objRef!); // OpenAI

        var payload = new OpenAIChatPayload { 
            Messages = messages, 
            MaximumTokens = 200,
            Temperature = 0.7,
            TopP = 0.95,
        };

        await AIJSInterop.CreateChatCompletions2Async(
            url: "",
            key: "", 
            payload: payload, 
            objRef: objRef!); // Azure OpenAI

        //await OpenAIChatJsInterop.CreateChatCompletionsApiAsync(key, messages, objRef!); // API
    }

    [JSInvokable]
    public async Task ChartCompletetionsStreamJs(string content, bool done)
    {
        ClearInput();

        if (isRequestInProgress)
            isRequestInProgress = false;

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

public record class OpenAIChatPayload
{
    [JsonPropertyName("messages")]
    public List<Message>? Messages { get; set; }

    /// <summary>
    /// Controls randomness: Lowering results in less random completions. 
    /// As the temperature approaches zero, the model will become deterministic and repetitive.
    /// Minimum 1 and the maximum is 2.
    /// </summary>
    /// <remarks>Default value is 1.</remarks>
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 1;

    /// <summary>
    /// The maximum number of tokens to generate shared between the prompt and completion. 
    /// The exact limit varies by model. (One token is roughly 4 characters for standard English text)
    /// Minimum 1 and the maximum tokens is 4096.
    /// </summary>
    /// <remarks>Default value is 2048. This value is limited by gpt-3.5-turbo.</remarks>
    [JsonPropertyName("max_tokens")]
    public long MaximumTokens { get; set; } = 2048;

    /// <summary>
    /// Controls diversity via nucleus sampling: 0.5 means half of all likelihood-weighted options are considered.
    /// </summary>
    /// <remarks>Default value is 1.</remarks>
    [JsonPropertyName("top_p")]
    public double TopP { get; set; } = 1;

    /// <summary>
    /// How much to penalize new tokens based on their existing frequency in the text so far. 
    /// Decreases the model's likelihood to repeat the same line verbatim.
    /// Minimum 1 and the maximum is 2.
    /// </summary>
    /// <remarks>Default value is 0.</remarks>
    [JsonPropertyName("frequency_penalty")]
    public double FrequencyPenalty { get; set; } = 0;

    /// <summary>
    /// How much to penalize new tokens based on whether they appear in the text so far. 
    /// Increases the model's likelihood to talk about new topics.
    /// Minimum 1 and the maximum is 2.
    /// </summary>
    /// <remarks>Default value is 0.</remarks>
    [JsonPropertyName("presence_penalty")]
    public float PresencePenalty { get; set; } = 0;

    [JsonPropertyName("stream")]
    public bool Stream { get; } = true;
}
