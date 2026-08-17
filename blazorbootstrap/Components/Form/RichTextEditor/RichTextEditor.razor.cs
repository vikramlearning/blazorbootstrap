namespace BlazorBootstrap;

/// <summary>
/// Provides a dependency-free rich-text editing control with a grouped Bootstrap toolbar.
/// </summary>
public partial class RichTextEditor : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private static readonly string[] defaultAllowedImageFileTypes = { "jpg", "jpeg", "png", "gif", "webp" };
    private static readonly HashSet<string> safeImageFileTypes = new(StringComparer.OrdinalIgnoreCase) { "jpg", "jpeg", "png", "gif", "webp" };
    private CancellationTokenSource uploadCancellationTokenSource = new();
    private FieldIdentifier fieldIdentifier;
    private string? imageUploadError;
    private string? lastRenderedValue;
    private DotNetObjectReference<RichTextEditor>? objRef;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            uploadCancellationTokenSource.Cancel();
            uploadCancellationTokenSource.Dispose();

            if (Id is not null)
                await RichTextEditorJsInterop.DisposeAsync(objRef!, Id);

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            objRef ??= DotNetObjectReference.Create(this);
            await RichTextEditorJsInterop.InitializeAsync(objRef!, Id!, AllowedLinkDomains, AllowedImageDomains);
            lastRenderedValue = Value;
        }
        //else if (lastRenderedValue != Value)
        //{
        //    await RichTextEditorJsInterop.SetValueAsync(Id!, Value);
        //    lastRenderedValue = Value;
        //}

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        if (ValueExpression is not null)
            fieldIdentifier = FieldIdentifier.Create(ValueExpression);

        base.OnInitialized();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Clears the editor content.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Clears the editor content.")]
    public Task ClearAsync() => RichTextEditorJsInterop.ClearAsync(objRef!, Id!);

    /// <summary>
    /// Focuses the editable area.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Focuses the editable area.")]
    public Task FocusAsync() => RichTextEditorJsInterop.FocusAsync(objRef!, Id!);

    public async Task OnToolbarButtonClickAsync(string toolbarElementId, RichTextEditorToolbarItem item)
    {
        if (Disabled || ReadOnly)
            return;

        await RichTextEditorJsInterop.ExecuteAsync(objRef!, Id!, toolbarElementId, item.ToCommandName()!, string.Empty);
    }

    /// <summary>
    /// Raises the committed editor value callback.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Raises the committed editor value callback.")]
    [JSInvokable]
    public async Task OnEditorValueChangedAsync(string html)
    {
        imageUploadError = null;
        lastRenderedValue = html;
        var text = ToPlainText(html);
        await ValueChanged.InvokeAsync(new RichTextEditorChange(html, text, text.Length, CountWords(text)));

        if (ValueExpression is not null)
            EditContext?.NotifyFieldChanged(fieldIdentifier);
    }

    /// <summary>
    /// Raises the transient editor status callback.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Raises the transient editor status callback.")]
    [JSInvokable]
    public Task OnEditorStatusChangedAsync(string status) => StatusChanged.InvokeAsync(status);

    #endregion

    #region Private Methods

    private static int CountWords(string text) => string.IsNullOrWhiteSpace(text) ? 0 : text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    private async Task OnImageFileChangedAsync(InputFileChangeEventArgs e)
    {
        imageUploadError = null;
        var file = e.File;

        if (ImageUploadHandler is null)
        {
            await SetImageUploadErrorAsync("An image upload handler is not configured.");
            return;
        }

        if (MaxImageFileSize <= 0)
        {
            await SetImageUploadErrorAsync("The maximum image size must be greater than zero.");
            return;
        }

        var extension = Path.GetExtension(file.Name).TrimStart('.').ToLowerInvariant();
        if (!NormalizedAllowedImageFileTypes.Contains(extension))
        {
            await SetImageUploadErrorAsync("The selected image type is not allowed.");
            return;
        }

        if (file.Size == 0 || file.Size > MaxImageFileSize)
        {
            await SetImageUploadErrorAsync("The selected image exceeds the maximum allowed size.");
            return;
        }

        if (!HasExpectedImageContentType(file.ContentType, extension))
        {
            await SetImageUploadErrorAsync("The selected file does not have the expected image content type.");
            return;
        }

        uploadCancellationTokenSource.Cancel();
        uploadCancellationTokenSource.Dispose();
        uploadCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = uploadCancellationTokenSource.Token;

        try
        {
            if (!await HasExpectedImageSignatureAsync(file, extension, cancellationToken))
            {
                await SetImageUploadErrorAsync("The selected file content does not match its image type.");
                return;
            }

            var request = new RichTextEditorImageUploadRequest(file, string.Empty, cancellationToken);
            var result = await ImageUploadHandler.Invoke(request);

            if (cancellationToken.IsCancellationRequested)
                return;

            if (result is null || !IsAllowedHttpsImageUrl(result.Url))
            {
                await SetImageUploadErrorAsync("The upload did not return a permitted HTTPS image URL.");
                return;
            }

            await RichTextEditorJsInterop.PrepareUploadedImageAsync(Id!, result.Url);
            await OnEditorStatusChangedAsync("Image uploaded. Review its accessibility options, then insert it.");
        }
        catch (OperationCanceledException)
        {
            // A newer upload or disposal cancelled this request.
        }
        catch (Exception)
        {
            await SetImageUploadErrorAsync("The image could not be uploaded.");
        }
    }

    private static bool HasExpectedImageContentType(string? contentType, string extension) =>
        extension switch
        {
            "jpg" or "jpeg" => string.Equals(contentType, "image/jpeg", StringComparison.OrdinalIgnoreCase),
            "png" => string.Equals(contentType, "image/png", StringComparison.OrdinalIgnoreCase),
            "gif" => string.Equals(contentType, "image/gif", StringComparison.OrdinalIgnoreCase),
            "webp" => string.Equals(contentType, "image/webp", StringComparison.OrdinalIgnoreCase),
            _ => false
        };

    private static async Task<bool> HasExpectedImageSignatureAsync(IBrowserFile file, string extension, CancellationToken cancellationToken)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream(file.Size, cancellationToken);
        var bytesRead = await stream.ReadAsync(header.AsMemory(), cancellationToken);

        return extension switch
        {
            "jpg" or "jpeg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "png" => bytesRead >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
            "gif" => bytesRead >= 6 && header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38 && (header[4] == 0x37 || header[4] == 0x39) && header[5] == 0x61,
            "webp" => bytesRead >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
            _ => false
        };
    }

    private bool IsAllowedHttpsImageUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(uri.UserInfo)
            || string.IsNullOrWhiteSpace(uri.Host))
            return false;

        var configuredDomains = AllowedImageDomains?.Where(domain => !string.IsNullOrWhiteSpace(domain)).ToArray() ?? Array.Empty<string>();
        if (configuredDomains.Length == 0)
            return true;

        var host = uri.Host.TrimEnd('.');
        return configuredDomains
            .Select(NormalizeDomain)
            .Where(domain => domain is not null)
            .Any(domain => host.Equals(domain, StringComparison.OrdinalIgnoreCase) || host.EndsWith($".{domain}", StringComparison.OrdinalIgnoreCase));
    }

    private static string? NormalizeDomain(string domain)
    {
        var candidate = domain.Trim().TrimEnd('.');
        if (candidate.StartsWith("*.", StringComparison.Ordinal))
            candidate = candidate[2..];
        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
            candidate = uri.Host;
        return Uri.CheckHostName(candidate) is UriHostNameType.Dns or UriHostNameType.IPv4 or UriHostNameType.IPv6 ? candidate : null;
    }

    private async Task SetImageUploadErrorAsync(string message)
    {
        imageUploadError = message;
        await RichTextEditorJsInterop.ShowImageUploadErrorAsync(Id!, message);
        await OnEditorStatusChangedAsync(message);
    }

    private static string ToPlainText(string html) => System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ")).Trim();

    #endregion

    #endregion

    #region Properties, Indexers

    private string Accept => string.Join(',', NormalizedAllowedImageFileTypes.Select(fileType => $".{fileType}"));

    private string EditorId => $"{Id}-editor";

    private string ImageInputId => $"{Id}-image-upload-file";

    private HashSet<string> NormalizedAllowedImageFileTypes =>
        (AllowedImageFileTypes ?? defaultAllowedImageFileTypes)
            .Select(fileType => fileType.Trim().TrimStart('.').ToLowerInvariant())
            .Where(fileType => safeImageFileTypes.Contains(fileType))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets or sets the accessible label for the editor.</summary>
    [AddedVersion("4.0.0")]
    [DefaultValue("Rich text editor")]
    [Description("Gets or sets the accessible label for the editor.")]
    [Parameter]
    public string AriaLabel { get; set; } = "Rich text editor";

    /// <summary>Gets or sets the permitted image file extensions.</summary>
    [AddedVersion("4.0.0")]
    [Description("Gets or sets the permitted image file extensions.")]
    [Parameter]
    public IEnumerable<string>? AllowedImageFileTypes { get; set; }

    /// <summary>Gets or sets the permitted HTTP(S) link domains. Subdomains are also permitted.</summary>
    [AddedVersion("4.0.0")]
    [Description("Gets or sets the permitted HTTP(S) link domains. Subdomains are also permitted.")]
    [Parameter]
    public IEnumerable<string>? AllowedLinkDomains { get; set; }

    /// <summary>Gets or sets the permitted HTTPS image domains. Subdomains are also permitted.</summary>
    [AddedVersion("4.0.0")]
    [Description("Gets or sets the permitted HTTPS image domains. Subdomains are also permitted.")]
    [Parameter]
    public IEnumerable<string>? AllowedImageDomains { get; set; }

    /// <summary>Gets or sets the delay, in milliseconds, before editor changes are raised.</summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(300)]
    [Description("Gets or sets the delay, in milliseconds, before editor changes are raised.")]
    [Parameter]
    public int DebounceInterval { get; set; } = 300;

    /// <summary>Gets or sets whether the editor is disabled.</summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the editor is disabled.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Gets or sets the image upload handler.</summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the image upload handler.")]
    [Parameter]
    public RichTextEditorImageUploadDelegate? ImageUploadHandler { get; set; }

    /// <summary>Gets or sets the maximum allowed image size in bytes.</summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(5242880)]
    [Description("Gets or sets the maximum allowed image size in bytes.")]
    [Parameter]
    public long MaxImageFileSize { get; set; } = 5 * 1024 * 1024;

    /// <summary>Gets or sets the maximum plain-text character count.</summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the maximum plain-text character count.")]
    [Parameter]
    public int? MaxLength { get; set; }

    /// <summary>Gets or sets the placeholder text.</summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the placeholder text.")]
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>Gets or sets whether the editor is read-only.</summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the editor is read-only.")]
    [Parameter]
    public bool ReadOnly { get; set; }

    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the toolbar items.")]
    [Parameter]
    public RichTextEditorToolbarItem[]? ToolbarItems { get; set; }

    /// <summary>Gets or sets the HTML value.</summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the HTML value.")]
    [Parameter]
    public string Value { get; set; } = string.Empty;

    /// <summary>Fires after the editor commits a content change.</summary>
    [AddedVersion("4.0.0")]
    [Description("Fires after the editor commits a content change.")]
    [Parameter]
    public EventCallback<RichTextEditorChange> ValueChanged { get; set; }

    /// <summary>Fires for transient editor activity and error status.</summary>
    [AddedVersion("4.0.0")]
    [Description("Fires for transient editor activity and error status.")]
    [Parameter]
    public EventCallback<string> StatusChanged { get; set; }

    /// <summary>Gets or sets the expression that identifies the bound value.</summary>
    [AddedVersion("4.0.0")]
    [Description("Gets or sets the expression that identifies the bound value.")]
    [Parameter]
    public Expression<Func<string>>? ValueExpression { get; set; }

    [CascadingParameter] 
    private EditContext? EditContext { get; set; }

    [Inject] 
    private RichTextEditorJsInterop RichTextEditorJsInterop { get; set; } = default!;

    #endregion
}
