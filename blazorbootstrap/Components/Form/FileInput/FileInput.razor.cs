namespace BlazorBootstrap;

public partial class FileInput : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private int dragDepth;

    private readonly List<IBrowserFile> files = new();

    private readonly List<string> validationErrors = new();

    #endregion

    #region Methods

    #region Private Methods

    private string GetFileValidationError(IBrowserFile file)
    {
        if (MaxFileSize.HasValue && file.Size > MaxFileSize.Value)
            return $"{file.Name}: the file exceeds the maximum allowed size.";

        var allowedFileTypes = NormalizedAllowedFileTypes;
        if (allowedFileTypes.Count > 0 && !allowedFileTypes.Contains(NormalizeExtension(Path.GetExtension(file.Name))))
            return $"{file.Name}: the file extension is not allowed.";

        return string.Empty;
    }

    private string NormalizeExtension(string? extension) => extension?.Trim().TrimStart('.').ToUpperInvariant() ?? string.Empty;

    private void OnDragEnter(DragEventArgs _) => dragDepth++;

    private void OnDragLeave(DragEventArgs _)
    {
        if (dragDepth > 0)
            dragDepth--;
    }

    private async Task OnDrop(DragEventArgs _)
    {
        dragDepth = 0;
        await Task.CompletedTask;
    }

    private async Task OnInputFileChangedAsync(InputFileChangeEventArgs e)
    {
        validationErrors.Clear();

        var selectedFiles = Multiple ? e.GetMultipleFiles(int.MaxValue) : new List<IBrowserFile> { e.File };
        var hasChanged = false;

        if (!Multiple)
        {
            files.Clear();
            hasChanged = true;

            if (e.FileCount > 1)
                validationErrors.Add("Only one file can be selected.");
        }

        foreach (var selectedFile in selectedFiles)
        {
            if (MaxFileCount.HasValue && files.Count >= MaxFileCount.Value)
            {
                validationErrors.Add($"{selectedFile.Name}: the maximum number of files has been reached.");
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

    private async Task RemoveFileAsync(IBrowserFile file)
    {
        if (!files.Remove(file))
            return;

        await FilesChanged.InvokeAsync(files.AsReadOnly());
    }

    #endregion

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-file-input", true),
            (BackgroundColor.ToBackgroundAndTextClass(), BackgroundColor != BackgroundColor.None),
            (Size.ToFileInputSizeClass(), Size != FileInputSize.Default));

    private string Accept => string.Join(',', NormalizedAllowedFileTypes.Select(fileType => $".{fileType.ToLowerInvariant()}"));

    private string DropZoneClassNames => BuildClassNames(
        ("bb-file-input__drop-zone", true),
        ("bb-file-input__drop-zone--drag-over", dragDepth > 0));

    private HashSet<string> NormalizedAllowedFileTypes =>
        AllowedFileTypes?
            .Select(NormalizeExtension)
            .Where(fileType => !string.IsNullOrWhiteSpace(fileType))
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
        ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the allowed file extensions.
    /// <para>
    /// An empty collection allows all extensions. Values may include a leading dot and are matched without regard to case.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the allowed file extensions.")]
    [Parameter]
    public IEnumerable<string>? AllowedFileTypes { get; set; }

    /// <summary>
    /// Gets or sets the accessible label for the file selection area.
    /// <para>
    /// Default value is Select files.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue("Select files")]
    [Description("Gets or sets the accessible label for the file selection area.")]
    [Parameter]
    public string AriaLabel { get; set; } = "Select files";

    /// <summary>
    /// Gets or sets the contextual background color.
    /// <para>
    /// Default value is <see cref="BackgroundColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(BackgroundColor.None)]
    [Description("Gets or sets the contextual background color.")]
    [Parameter]
    public BackgroundColor BackgroundColor { get; set; } = BackgroundColor.None;

    /// <summary>
    /// This event fires when the selected files change.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("This event fires when the selected files change.")]
    [Parameter]
    public EventCallback<IReadOnlyList<IBrowserFile>> FilesChanged { get; set; }

    /// <summary>
    /// Gets or sets the hint text displayed below the component.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the hint text displayed below the component.")]
    [Parameter]
    public string? HintText { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of selected files.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the maximum number of selected files.")]
    [Parameter]
    public int? MaxFileCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed file size in bytes.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the maximum allowed file size in bytes.")]
    [Parameter]
    public long? MaxFileSize { get; set; }

    /// <summary>
    /// Gets or sets whether multiple files can be selected.
    /// <para>
    /// Default value is false.
    /// </para>
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
    /// Gets or sets the component size.
    /// <para>
    /// Default value is <see cref="FileInputSize.Default" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(FileInputSize.Default)]
    [Description("Gets or sets the component size.")]
    [Parameter]
    public FileInputSize Size { get; set; } = FileInputSize.Default;

    #endregion
}
