namespace BlazorBootstrap;

/// <summary>
/// Provides a dependency-free rich-text editing control with a grouped toolbar.
/// </summary>
public partial class RichTextEditor : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private static readonly string[] defaultAllowedImageFileTypes = { "jpg", "jpeg", "png", "gif", "webp" };
    private CancellationTokenSource uploadCancellationTokenSource = new();
    private FieldIdentifier fieldIdentifier;
    private string? imageUploadError;
    private string? lastRenderedValue;
    private DotNetObjectReference<RichTextEditor>? objRef;

    #endregion

    #region Properties, Indexers

    private string Accept => string.Join(',', NormalizedAllowedImageFileTypes.Select(fileType => $".{fileType}"));

    private string EditorId => $"{Id}-editor";

    internal IReadOnlyCollection<RichTextEditorToolbarItem> EnabledToolbarItems
    {
        get
        {
            var items = ToolbarItems?.Distinct().ToHashSet() ?? Enum.GetValues<RichTextEditorToolbarItem>().ToHashSet();

            if (ImageUploadHandler is null)
                items.Remove(RichTextEditorToolbarItem.UploadImage);

            return items;
        }
    }

    private string fieldCssClasses => ValueExpression is null ? string.Empty : EditContext?.FieldCssClass(fieldIdentifier) ?? string.Empty;

    private string ImageInputId => $"{Id}-image-input";

    private HashSet<string> NormalizedAllowedImageFileTypes =>
        (AllowedImageFileTypes ?? defaultAllowedImageFileTypes)
            .Select(fileType => fileType.Trim().TrimStart('.').ToLowerInvariant())
            .Where(fileType => !string.IsNullOrWhiteSpace(fileType))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    protected override string? ClassNames => BuildClassNames(Class, ("bb-rich-text-editor", true));

    #endregion

    #region Methods

    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            uploadCancellationTokenSource.Cancel();
            uploadCancellationTokenSource.Dispose();

            if (Id is not null)
                await RichTextEditorJsInterop.DisposeEditorAsync(Id);

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            objRef ??= DotNetObjectReference.Create(this);
            await RichTextEditorJsInterop.InitializeAsync(Id!, objRef, DebounceInterval, MaxLength, ReadOnly, Disabled);
            lastRenderedValue = Value;
        }
        else if (lastRenderedValue != Value)
        {
            await RichTextEditorJsInterop.SetValueAsync(Id!, Value);
            lastRenderedValue = Value;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        if (ValueExpression is not null)
            fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        base.OnInitialized();
    }

    /// <summary>
    /// Clears the editor content.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Clears the editor content.")]
    public async Task ClearAsync()
    {
        await RichTextEditorJsInterop.ClearAsync(Id!);
    }

    /// <summary>
    /// Focuses the editable area.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Focuses the editable area.")]
    public Task FocusAsync() => RichTextEditorJsInterop.FocusAsync(Id!);

    [JSInvokable]
    public async Task OnEditorValueChangedAsync(string html)
    {
        imageUploadError = null;
        lastRenderedValue = html;
        await ValueChanged.InvokeAsync(html);

        if (ValueExpression is not null)
            EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    private async Task OnImageFileChangedAsync(InputFileChangeEventArgs e)
    {
        imageUploadError = null;
        var file = e.File;

        if (ImageUploadHandler is null)
        {
            imageUploadError = "An image upload handler is not configured.";
            return;
        }

        var extension = Path.GetExtension(file.Name).TrimStart('.');
        if (!NormalizedAllowedImageFileTypes.Contains(extension))
        {
            imageUploadError = "The selected image type is not allowed.";
            return;
        }

        if (file.Size > MaxImageFileSize)
        {
            imageUploadError = "The selected image exceeds the maximum allowed size.";
            return;
        }

        uploadCancellationTokenSource.Cancel();
        uploadCancellationTokenSource.Dispose();
        uploadCancellationTokenSource = new CancellationTokenSource();

        try
        {
            var request = new RichTextEditorImageUploadRequest(file, string.Empty, uploadCancellationTokenSource.Token);
            var result = await ImageUploadHandler.Invoke(request);

            if (result is null || !Uri.TryCreate(result.Url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            {
                imageUploadError = "The upload did not return a valid HTTPS image URL.";
                return;
            }

            await RichTextEditorJsInterop.InsertImageAsync(Id!, result.Url, string.Empty);
        }
        catch (OperationCanceledException)
        {
            // A newer upload or disposal cancelled this request.
        }
        catch (Exception)
        {
            imageUploadError = "The image could not be uploaded.";
        }
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the accessible label for the editor.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue("Rich text editor")]
    [Description("Gets or sets the accessible label for the editor.")]
    [Parameter]
    public string AriaLabel { get; set; } = "Rich text editor";

    /// <summary>
    /// Gets or sets the permitted image file extensions.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Gets or sets the permitted image file extensions.")]
    [Parameter]
    public IEnumerable<string>? AllowedImageFileTypes { get; set; }

    /// <summary>
    /// Gets or sets the delay, in milliseconds, before editor changes are raised.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(300)]
    [Description("Gets or sets the delay, in milliseconds, before editor changes are raised.")]
    [Parameter]
    public int DebounceInterval { get; set; } = 300;

    /// <summary>
    /// Gets or sets whether the editor is disabled.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the editor is disabled.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the image upload handler.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the image upload handler.")]
    [Parameter]
    public RichTextEditorImageUploadDelegate? ImageUploadHandler { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed image size in bytes.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(5242880)]
    [Description("Gets or sets the maximum allowed image size in bytes.")]
    [Parameter]
    public long MaxImageFileSize { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum plain-text character count.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the maximum plain-text character count.")]
    [Parameter]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the placeholder text.")]
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets whether the editor is read-only.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the editor is read-only.")]
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the enabled toolbar commands.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the enabled toolbar commands.")]
    [Parameter]
    public IEnumerable<RichTextEditorToolbarItem>? ToolbarItems { get; set; }

    /// <summary>
    /// Gets or sets the HTML value.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the HTML value.")]
    [Parameter]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Fires when the HTML value changes.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Fires when the HTML value changes.")]
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression that identifies the bound value.
    /// </summary>
    [Parameter]
    public Expression<Func<string>>? ValueExpression { get; set; }

    [CascadingParameter] private EditContext? EditContext { get; set; }

    [Inject] private RichTextEditorJsInterop RichTextEditorJsInterop { get; set; } = default!;

    #endregion
}
