namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// A quantity stepper: minus and plus buttons around a number box, clamped between 1 and <see cref="Max" />.
/// The buttons stay enabled at the limits so keyboard focus never drops; a press past a limit does nothing.
/// </summary>
public partial class EcommerceQuantityInput : ComponentBase
{
    #region Methods

    #region Private Methods

    private Task DecreaseAsync() => SetAsync(Value - 1);

    private Task IncreaseAsync() => SetAsync(Value + 1);

    private async Task SetAsync(int value)
    {
        var clamped = Math.Clamp(value, 1, Math.Max(1, Max));

        if (clamped == Value)
            return;

        Value = clamped;
        await ValueChanged.InvokeAsync(clamped);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets extra CSS classes for the group.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Gets or sets the number box ID, for an external label.</summary>
    [Parameter] public string? Id { get; set; }

    /// <summary>Gets or sets the item name used in the buttons' and box's accessible names.</summary>
    [Parameter, EditorRequired] public string ItemName { get; set; } = default!;

    /// <summary>Gets or sets the highest quantity.</summary>
    [Parameter] public int Max { get; set; } = 10;

    /// <summary>Gets or sets whether the small input-group size is used.</summary>
    [Parameter] public bool Small { get; set; }

    /// <summary>Gets or sets the quantity.</summary>
    [Parameter] public int Value { get; set; } = 1;

    /// <summary>Gets or sets the callback fired with the new quantity.</summary>
    [Parameter] public EventCallback<int> ValueChanged { get; set; }

    #endregion
}
