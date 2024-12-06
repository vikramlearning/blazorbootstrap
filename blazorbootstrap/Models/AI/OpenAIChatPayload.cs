namespace BlazorBootstrap;

public record OpenAIChatPayload
{
    #region Properties, Indexers

    /// <summary>
    /// How much to penalize new tokens based on their existing frequency in the text so far.
    /// Decreases the model's likelihood to repeat the same line verbatim.
    /// Minimum 1 and the maximum is 2.
    /// </summary>
    /// <remarks>Default value is 0.</remarks>
    [JsonPropertyName("frequency_penalty")]
    public double FrequencyPenalty { get; set; } = 0;

    /// <summary>
    /// The maximum number of tokens to generate shared between the prompt and completion.
    /// The exact limit varies by model. (One token is roughly 4 characters for standard English text)
    /// Minimum 1 and the maximum tokens is 4096.
    /// </summary>
    /// <remarks>Default value is 2048. This value is limited by gpt-3.5-turbo.</remarks>
    [JsonPropertyName("max_tokens")]
    public long MaximumTokens { get; set; } = 2048;

    [JsonPropertyName("messages")] 
    public List<OpenAIChatMessage>? Messages { get; set; }

    /// <summary>
    /// How much to penalize new tokens based on whether they appear in the text so far.
    /// Increases the model's likelihood to talk about new topics.
    /// Minimum 1 and the maximum is 2.
    /// </summary>
    /// <remarks>Default value is 0.</remarks>
    [JsonPropertyName("presence_penalty")]
    public float PresencePenalty { get; set; } = 0;

    [JsonPropertyName("stream")] public bool Stream { get; } = true;

    /// <summary>
    /// Controls randomness: Lowering results in less random completions.
    /// As the temperature approaches zero, the model will become deterministic and repetitive.
    /// Minimum 1 and the maximum is 2.
    /// </summary>
    /// <remarks>Default value is 1.</remarks>
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 1;

    /// <summary>
    /// Controls diversity via nucleus sampling: 0.5 means half of all likelihood-weighted options are considered.
    /// </summary>
    /// <remarks>Default value is 1.</remarks>
    [JsonPropertyName("top_p")]
    public double TopP { get; set; } = 1;

    #endregion
}
