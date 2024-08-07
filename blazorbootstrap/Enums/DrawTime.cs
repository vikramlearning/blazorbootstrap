namespace BlazorBootstrap;

public enum DrawTime
{
  /// <summary>
  /// Occurs before any drawing takes place
  /// </summary>
  BeforeDraw,

  /// <summary>
  /// Occurs after drawing of axes, but before datasets
  /// </summary>
  BeforeDatasetsDraw,

  /// <summary>
  /// Occurs after drawing of datasets but before items such as the tooltip
  /// </summary>
  AfterDatasetsDraw,

  /// <summary>
  /// After other drawing is completed.
  /// </summary>
  AfterDraw
}
