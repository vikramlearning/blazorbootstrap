using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class PaginationItem : BaseComponent
{
    #region Members

    /// <summary>
    /// Holds the state of this pagination item.
    /// </summary>
    private PaginationItemState state = new();

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.PaginationItem());
        builder.Append(BootstrapClassProvider.PaginationItemActive(), Active);
        builder.Append(BootstrapClassProvider.PaginationItemDisabled(), Disabled);

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the reference to the pagination item state object.
    /// </summary>
    protected PaginationItemState State => state;

    /// <summary>
    /// Indicate the currently active page.
    /// </summary>
    [Parameter]
    public bool Active
    {
        get => state.Active;
        set
        {
            if (value == state.Active)
                return;

            state = state with { Active = value };

            DirtyClasses();
        }
    }

    /// <summary>
    /// Used for links that appear un-clickable.
    /// </summary>
    [Parameter]
    public bool Disabled
    {
        get => state.Disabled;
        set
        {
            if (value == state.Disabled)
                return;

            state = state with { Disabled = value };

            DirtyClasses();
        }
    }

    [Parameter] public string Text { get; set; }

    #endregion
}

