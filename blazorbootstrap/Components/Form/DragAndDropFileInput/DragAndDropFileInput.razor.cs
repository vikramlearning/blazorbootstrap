using System.ComponentModel;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBootstrap;

public partial class DragAndDropFileInput : FileInputBase
{
    #region Fields and Constants

    private int dragDepth;

    #endregion

    #region Methods

    #region Protected Methods

    protected void OnDragEnter(DragEventArgs _)
    {
        dragDepth++;
        StateHasChanged();
    }

    protected void OnDragLeave(DragEventArgs _)
    {
        dragDepth = Math.Max(0, dragDepth - 1);
        StateHasChanged();
    }

    protected void OnDrop(DragEventArgs _)
    {
        dragDepth = 0;
        StateHasChanged();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the contextual background color of the drag-and-drop area.
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(BackgroundColor.None)]
    [Description("Gets or sets the contextual background color of the drag-and-drop area.")]
    [Parameter]
    public BackgroundColor BackgroundColor { get; set; } = BackgroundColor.None;

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-drag-and-drop-file-input", true),
            (BackgroundColor.ToBackgroundClass(), BackgroundColor != BackgroundColor.None),
            ("bb-drag-and-drop-file-input--dragging", dragDepth > 0));

    #endregion
}
