namespace BlazorBootstrap;

public partial class OTPInput : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private string[] otpValues = Array.Empty<string>();

    #endregion

    #region Methods

    // Auto focus
    // Color

    protected override void OnParametersSet()
    {
        if (otpValues.Length != Length)
        {
            otpValues = new string[Length];
            Array.Fill(otpValues, string.Empty);
        }
    }

    /// <summary>
    /// Clears the OTP input fields.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Clears the OTP input fields.")]
    public async Task ClearAsync()
    {
        otpValues = new string[Length];
        Array.Fill(otpValues, string.Empty);
        await NotifyChangesAsync();

        if (Length > 0)
            await JSRuntime.InvokeVoidAsync(JsInteropUtils.FocusInputElement, GetInputId(0));

        await InvokeAsync(StateHasChanged);
    }

    private string GetInputId(int index) => $"{Id}-otp-input-{index}";

    private async Task NotifyChangesAsync()
    {
        Console.WriteLine(">> NotifyChangesAsync called");
        var otpValue = string.Join(string.Empty, otpValues);
        Console.WriteLine($">> otpValue: {otpValue}");
        await OnOTPChanged.InvokeAsync(otpValue);

        if (otpValue.Length == Length && !otpValues.Any(string.IsNullOrWhiteSpace))
            await OnOTPCompleted.InvokeAsync(otpValue);
    }

    private async Task OnInput(ChangeEventArgs e, int index)
    {
        var rawValue = e.Value?.ToString();
        var numericValue = new string(rawValue?.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(numericValue))
        {
            otpValues[index] = string.Empty;

            // Clear the input element if it contained invalid characters
            if (!string.IsNullOrEmpty(rawValue))
            {
                await JSRuntime.InvokeVoidAsync(JsInteropUtils.SetInputElementValue, GetInputId(index), string.Empty);
            }

            await NotifyChangesAsync();
            return;
        }

        // If multiple digits were entered (e.g. fast typing or paste), use the last one
        var digit = numericValue.Length > 1 ? numericValue[^1].ToString() : numericValue;

        otpValues[index] = digit;

        // Reset the input value on the client side if it doesn't match the sanitized digit
        if (rawValue != digit)
        {
            await JSRuntime.InvokeVoidAsync(JsInteropUtils.SetInputElementValue, GetInputId(index), digit);
        }

        // Move focus to the next input field
        if (index < Length - 1)
        {
            await JSRuntime.InvokeVoidAsync(JsInteropUtils.FocusInputElement, GetInputId(index + 1));
        }

        await NotifyChangesAsync();
    }

    private async Task OnKeyUp(KeyboardEventArgs e, int index)
    {
        // Handle backspace key to clear the current input and focus on the previous one
        if (e.Key == "Backspace" && index > 0)
        {
            otpValues[index] = string.Empty;
            await JSRuntime.InvokeVoidAsync(JsInteropUtils.FocusInputElement, GetInputId(index - 1));

            // Notify changes
            await NotifyChangesAsync();
        }

        // Handle left arrow key to focus on the previous input
        if (e.Key == "ArrowLeft" && index > 0)
            await JSRuntime.InvokeVoidAsync(JsInteropUtils.FocusInputElement, GetInputId(index - 1));

        // Handle right arrow key to focus on the next input
        if (e.Key == "ArrowRight" && index < Length - 1)
            await JSRuntime.InvokeVoidAsync(JsInteropUtils.FocusInputElement, GetInputId(index + 1));
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.FormControl, true),
            (BootstrapClass.TextCenter, true),
            (BootstrapClass.MarginEnd2, true)
        );

    protected override string? StyleNames =>
        BuildClassNames(
            Style,
            ("width:40px;", true),
            ("height:40px;", true)
        );

    private string? ContainerClassNames =>
        BuildClassNames(
            ContainerCssClass,
            (BootstrapClass.Flex, true),
            (BootstrapClass.FlexRow, true)
        );

    /// <summary>
    /// Gets or sets the CSS class for the container element.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the CSS class for the container element.")]
    [Parameter]
    public string? ContainerCssClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS style for the container element.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the CSS style for the container element.")]
    [Parameter]
    public string? ContainerCssStyle { get; set; }

    private string? ContainerStyleNames =>
        BuildClassNames(
            ContainerCssStyle
        );

    /// <summary>
    /// Gets or sets the OTP input length.
    /// <para>
    /// Default value is 6.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(6)]
    [Description("Gets or sets the OTP input length.")]
    [Parameter]
    public int Length { get; set; } = 6;

    /// <summary>
    /// This event fires when the OTP input value changes.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("This event fires when the OTP input value changes.")]
    [Parameter]
    public EventCallback<string> OnOTPChanged { get; set; }

    // Disabled
    // Divider

    /// <summary>
    /// This event fires when the OTP input is completed.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("This event fires when the OTP input is completed.")]
    [Parameter]
    public EventCallback<string> OnOTPCompleted { get; set; }

    #endregion

    // Size
    // Style
    // Width
}
