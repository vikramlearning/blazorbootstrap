﻿namespace BlazorBootstrap;

public partial class DropdownMenu : BlazorBootstrapComponentBase
{
    #region Properties, Indexers
    
    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.DropdownMenu, true),
            (Position.ToDropdownMenuPositionClass(), true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dropdown menu position.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownMenuPosition.Start" />.
    /// </remarks>
    [Parameter]
    public DropdownMenuPosition Position { get; set; } = DropdownMenuPosition.Start;

    #endregion

    /// <inheritdoc />
    public override Task SetParametersAsync(ParameterView parameters)
    {

        
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value!; break;
                case nameof(Position): Position = (DropdownMenuPosition)parameter.Value!; break;
                default: AdditionalAttributes![parameter.Name] = parameter.Value; break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }
}
