using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class PaginationLink : BaseComponent
{
    #region Members

    #endregion

    #region Methods

    protected override void OnParametersSet()
    {
        // TODO: check this is not working
        Console.WriteLine($"AriaLabel: {AriaLabel}");
        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            Attributes?.Add("aria-label", AriaLabel);
        }

        base.OnParametersSet();
    }

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.PaginationLink());

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties

    [Parameter] public string Text { get; set; }

    [Parameter] public string LinkText { get; set; }

    [Parameter] public IconName LinkIcon { get; set; }

    [Parameter] public string AriaLabel { get; set; }

    #endregion
}

