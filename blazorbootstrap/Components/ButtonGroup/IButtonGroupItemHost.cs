namespace BlazorBootstrap;

/// <summary>
/// Implemented by the toggle button groups that host <see cref="ButtonGroupItem{TValue}" /> components.
/// </summary>
/// <typeparam name="TValue">The type of the item values.</typeparam>
internal interface IButtonGroupItemHost<TValue>
{
    #region Methods

    /// <summary>
    /// Returns whether the item with the given value is checked.
    /// </summary>
    bool IsChecked(TValue value);

    /// <summary>
    /// Called when the user checks or unchecks the item with the given value.
    /// </summary>
    Task OnItemCheckedChangedAsync(TValue value, bool isChecked);

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the default color of the items.
    /// </summary>
    ButtonColor Color { get; }

    /// <summary>
    /// Gets whether every item is disabled.
    /// </summary>
    bool Disabled { get; }

    /// <summary>
    /// Gets the input type the items render: <c>radio</c> or <c>checkbox</c>.
    /// </summary>
    string InputType { get; }

    /// <summary>
    /// Gets whether the bound field has validation messages.
    /// </summary>
    bool IsInvalid { get; }

    /// <summary>
    /// Gets the shared <c>name</c> of the item inputs, or <see langword="null" /> when the items do not share one.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets whether the items use the outline style by default.
    /// </summary>
    bool Outline { get; }

    #endregion
}
