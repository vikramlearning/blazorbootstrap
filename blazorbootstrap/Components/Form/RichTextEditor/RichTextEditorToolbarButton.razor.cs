namespace BlazorBootstrap;

public partial class RichTextEditorToolbarButton : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private Button saveButton1 = default!;

    private DotNetObjectReference<RichTextEditorToolbarButton>? objRef;

    #endregion

    #region Methods

    private async Task OnClickAsync()
    {
        Console.WriteLine($"RichTextEditorToolbarButton.OnClickAsync: {Item}");
        if (Disabled)
        {
            return;
        }
        //await RichTextEditor?.OnToolbarButtonClickAsync(Item);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets a value indicating whether the button is disabled.
    /// <para>
    /// Default is <c>false</c>.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the button is disabled.")]
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the toolbar item associated with the button.
    /// <para>
    /// Default is <see cref="RichTextEditorToolbarItem.None"/>.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(RichTextEditorToolbarItem.None)]
    [Description("Gets or sets the toolbar item associated with the button.")]
    [Parameter, EditorRequired]
    public RichTextEditorToolbarItem Item { get; set; } = RichTextEditorToolbarItem.None;

    #endregion
}
