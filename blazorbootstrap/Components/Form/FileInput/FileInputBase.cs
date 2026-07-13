namespace BlazorBootstrap;

public abstract class FileInputBase : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private const string AllowedFileTypesToken = "{AllowedFileTypes}";
    private const string DefaultInvalidFileTypeErrorMessage = "{FileName}: the file extension is not allowed.";
    private const string DefaultMaxFileCountErrorMessage = "{FileName}: the maximum number of files has been reached.";
    private const string DefaultMaxFileSizeErrorMessage = "{FileName}: the file exceeds the maximum allowed size.";
    private const string DefaultSingleFileErrorMessage = "Only one file can be selected.";
    private const string FileNameToken = "{FileName}";
    private const string MaxFileCountToken = "{MaxFileCount}";
    private const string MaxFileSizeToken = "{MaxFileSize}";

    protected readonly List<IBrowserFile> files = new();

    protected readonly List<string> validationErrors = new();

    #endregion

    #region Methods

    #region Protected Methods

    protected string GetFileValidationError(IBrowserFile file)
    {
        if (MaxFileSize.HasValue && file.Size > MaxFileSize.Value)
            return FormatValidationMessage(MaxFileSizeErrorMessage,
                (FileNameToken, file.Name),
                (MaxFileSizeToken, MaxFileSize.Value.ToString()));

        if (NormalizedAllowedFileTypes.Count > 0 && !NormalizedAllowedFileTypes.Contains(NormalizeExtension(Path.GetExtension(file.Name))))
            return FormatValidationMessage(InvalidFileTypeErrorMessage,
                (AllowedFileTypesToken, Accept),
                (FileNameToken, file.Name));

        return string.Empty;
    }

    protected async Task OnInputFileChangedAsync(InputFileChangeEventArgs e)
    {
        validationErrors.Clear();

        var selectedFiles = Multiple ? e.GetMultipleFiles(int.MaxValue) : new List<IBrowserFile> { e.File };
        var hasChanged = false;

        if (!Multiple)
        {
            files.Clear();
            hasChanged = true;

            if (e.FileCount > 1)
                validationErrors.Add(SingleFileErrorMessage);
        }

        foreach (var selectedFile in selectedFiles)
        {
            if (MaxFileCount.HasValue && files.Count >= MaxFileCount.Value)
            {
                validationErrors.Add(FormatValidationMessage(MaxFileCountErrorMessage,
                    (FileNameToken, selectedFile.Name),
                    (MaxFileCountToken, MaxFileCount.Value.ToString())));
                continue;
            }

            var validationError = GetFileValidationError(selectedFile);
            if (!string.IsNullOrEmpty(validationError))
            {
                validationErrors.Add(validationError);
                continue;
            }

            files.Add(selectedFile);
            hasChanged = true;
        }

        if (hasChanged)
            await FilesChanged.InvokeAsync(files.AsReadOnly());
    }

    protected string NormalizeExtension(string? extension) => extension?.Trim().TrimStart('.').ToUpperInvariant() ?? string.Empty;

    protected async Task RemoveFileAsync(IBrowserFile file)
    {
        if (!files.Remove(file))
            return;

        await FilesChanged.InvokeAsync(files.AsReadOnly());
    }

    #endregion

    #region Private Methods

    private static string FormatValidationMessage(string template, params (string Token, string Value)[] tokens)
    {
        foreach (var token in tokens)
            template = template.Replace(token.Token, token.Value, StringComparison.Ordinal);

        return template;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    protected string Accept => string.Join(',', NormalizedAllowedFileTypes.Select(fileType => $".{fileType.ToLowerInvariant()}"));

    protected HashSet<string> NormalizedAllowedFileTypes =>
        AllowedFileTypes?
            .Select(NormalizeExtension)
            .Where(fileType => !string.IsNullOrWhiteSpace(fileType))
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
        ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the allowed file extensions.
    /// <para>An empty collection allows all extensions. Values may include a leading dot and are matched without regard to case.</para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the allowed file extensions.")]
    [Parameter]
    public IEnumerable<string>? AllowedFileTypes { get; set; }

    /// <summary>
    /// Gets or sets the accessible label for the file selection area.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue("Select files")]
    [Description("Gets or sets the accessible label for the file selection area.")]
    [Parameter]
    public string AriaLabel { get; set; } = "Select files";

    /// <summary>
    /// This event fires when the selected files change.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("This event fires when the selected files change.")]
    [Parameter]
    public EventCallback<IReadOnlyList<IBrowserFile>> FilesChanged { get; set; }

    /// <summary>
    /// Gets or sets the hint text displayed below the component.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the hint text displayed below the component.")]
    [Parameter]
    public string? HintText { get; set; }

    /// <summary>
    /// Gets or sets the invalid file type validation message template.
    /// <para>Supported tokens: {FileName} and {AllowedFileTypes}.</para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(DefaultInvalidFileTypeErrorMessage)]
    [Description("Gets or sets the invalid file type validation message template.")]
    [Parameter]
    public string InvalidFileTypeErrorMessage { get; set; } = DefaultInvalidFileTypeErrorMessage;

    /// <summary>
    /// Gets or sets the maximum number of selected files.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the maximum number of selected files.")]
    [Parameter]
    public int? MaxFileCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum file count validation message template.
    /// <para>Supported tokens: {FileName} and {MaxFileCount}.</para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(DefaultMaxFileCountErrorMessage)]
    [Description("Gets or sets the maximum file count validation message template.")]
    [Parameter]
    public string MaxFileCountErrorMessage { get; set; } = DefaultMaxFileCountErrorMessage;

    /// <summary>
    /// Gets or sets the maximum allowed file size in bytes.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the maximum allowed file size in bytes.")]
    [Parameter]
    public long? MaxFileSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum file size validation message template.
    /// <para>Supported tokens: {FileName} and {MaxFileSize}.</para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(DefaultMaxFileSizeErrorMessage)]
    [Description("Gets or sets the maximum file size validation message template.")]
    [Parameter]
    public string MaxFileSizeErrorMessage { get; set; } = DefaultMaxFileSizeErrorMessage;

    /// <summary>
    /// Gets or sets whether multiple files can be selected.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether multiple files can be selected.")]
    [Parameter]
    public bool Multiple { get; set; }

    /// <summary>
    /// Gets the currently selected files.
    /// </summary>
    public IReadOnlyList<IBrowserFile> SelectedFiles => files.AsReadOnly();

    /// <summary>
    /// Gets or sets the single-file selection validation message template.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(DefaultSingleFileErrorMessage)]
    [Description("Gets or sets the single-file selection validation message template.")]
    [Parameter]
    public string SingleFileErrorMessage { get; set; } = DefaultSingleFileErrorMessage;

    #endregion
}
