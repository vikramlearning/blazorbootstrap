using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBootstrap;

public enum ZoomMode
{
  None,
  X,
  Y,
  XY
}

public enum ModifierKey
{
  None,
  Ctrl,
  Alt,
  Shift,
  Meta
}

public class ZoomLimitAxisOptions
{
  /// <summary>
  /// Minimum allowed value for scale.min
  /// </summary>
  [JsonIgnore]
  public double? Min { get; set; }

  [JsonIgnore]
  public bool? MinimumIsOriginal { get; set; }

  [JsonInclude]
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonPropertyName( "min" )]
  private string? minText => MinimumIsOriginal.GetValueOrDefault() ? "original" : Min?.ToString();

  /// <summary>
  /// Maximum allowed value for scale.max
  /// </summary>
  [JsonIgnore]
  public double? Max { get; set; }

  [JsonIgnore]
  public bool? MaximumIsOriginal { get; set; }

  [JsonInclude]
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonPropertyName( "max" )]
  private string? maxText => MaximumIsOriginal.GetValueOrDefault() ? "original" : Max?.ToString();

  /// <summary>
  /// Minimum allowed range (max - min). This defines the max zoom level.
  /// </summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonPropertyName( "minRange" )]
  public double? MinRange { get; set; }
}

public class DragOptions
{
  ///<summary>
  /// Enable drag-to-zoom
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public bool? Enabled { get; set; }

  ///<summary>
  /// Fill color
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BackgroundColor { get; set; }

  ///<summary>
  /// Stroke color
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderColor { get; set; }

  ///<summary>
  /// Stroke width
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderWidth { get; set; }

  ///<summary>
  /// When the dragging box is dran on the chart
  /// <summary>
  [JsonIgnore]
  public DrawTime? DrawTime { get; set; }

  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonInclude]
  [JsonPropertyName( "drawTime" )]
  private string? DrawTimeText => DrawTime?.ToString().ToLower();

  ///<summary>
  /// Minimal zoom distance required before actually applying zoom
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Threshold { get; set; }

  ///<summary>
  /// Modifier key required for drag-to-zoom
  /// <summary>
  [JsonIgnore]
  public ModifierKey? ModifierKey { get; set; }

  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonInclude]
  [JsonPropertyName( "modifierKey" )]
  private string? ModifierKeyText => ( ModifierKey == BlazorBootstrap.ModifierKey.None ) ? string.Empty : ModifierKey?.ToString().ToLower();
}

public class PinchOptions
{
  ///<summary>
  ///      /// Enable zooming via pinch gesture
  ///           /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public bool? Enabled { get; set; }
}

public class PanOptions
{///<summary>
 /// Enable panning
 /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public bool? Enabled { get; set; }

  ///<summary>
  /// Allowed panning directions
  /// <summary>
  [JsonIgnore]
  public ZoomMode? Mode { get; set; }

  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonInclude]
  [JsonPropertyName( "mode" )]
  private string? ModeText => ( Mode == ZoomMode.None ) ? string.Empty : Mode?.ToString().ToLower();

  ///<summary>
  /// Modifier key required for panning with mouse
  /// <summary>
  [JsonIgnore]
  public ModifierKey? ModifierKey { get; set; }

  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonInclude]
  [JsonPropertyName( "modifierKey" )]
  private string? ModifierKeyText => ( ModifierKey == BlazorBootstrap.ModifierKey.None ) ? string.Empty : ModifierKey?.ToString().ToLower();

  ///<summary>
  /// Enable panning over a scale for that axis (regardless of mode)
  /// <summary>
  [JsonIgnore]
  public ZoomMode? ScaleMode { get; set; }

  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonInclude]
  [JsonPropertyName( "scaleMode" )]
  private string? ScaleModeText => ( ScaleMode == ZoomMode.None ) ? string.Empty : ScaleMode?.ToString().ToLower();

  ///<summary>
  /// Enable panning over a scale for that axis (but only if mode is also enabled), and disables panning along that axis otherwise. Deprecated.
  /// <summary>
  [JsonIgnore]
  public ZoomMode? OverScaleMode { get; set; }

  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonInclude]
  [JsonPropertyName( "overScaleMode" )]
  private string? OverScaleModeText => ( OverScaleMode == ZoomMode.None ) ? string.Empty : OverScaleMode?.ToString().ToLower();

  ///<summary>
  /// Minimal pan distance required before actually applying pan
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Threshold { get; set; }
}

public class ZoomLimitOptions
{
  public ZoomLimitAxisOptions X { get; set; } = new();
  public ZoomLimitAxisOptions Y { get; set; } = new();

}

public class ZoomOptions
{
  ///<summary>
  /// Options of the mouse wheel behavior
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public WheelOptions? Wheel { get; set; }

  ///<summary>
  /// Options of the drag-to-zoom behavior
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public DragOptions? Drag { get; set; }

  ///<summary>
  /// Options of the pinch behavior
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public PinchOptions? Pinch { get; set; }

  ///<summary>
  /// Allowed zoom directions
  /// <summary>
  [JsonIgnore]
  public ZoomMode? Mode { get; set; }

  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonInclude]
  [JsonPropertyName( "mode" )]
  private string? ModeText => ( Mode == ZoomMode.None ) ? string.Empty : Mode?.ToString().ToLower();

  ///<summary>
  /// Which of the enabled zooming directions should only be available when the mouse cursor is over a scale for that axis
  /// <summary>
  [JsonIgnore]
  public ZoomMode? ScaleMode { get; set; }

  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonInclude]
  [JsonPropertyName( "scaleMode" )]
  private string? ScaleModeText => ( ScaleMode == ZoomMode.None ) ? string.Empty : ScaleMode?.ToString().ToLower();

  ///<summary>
  /// Allowed zoom directions when the mouse cursor is over a scale for that axis (but only if mode is also enabled), and disables zooming along that axis otherwise. Deprecated; use scaleMode instead.
  /// <summary>
  [JsonIgnore]
  public ZoomMode? OverScaleMode { get; set; }

  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonInclude]
  [JsonPropertyName( "overScaleMode" )]
  private string? OverScaleModeText => ( OverScaleMode == ZoomMode.None ) ? string.Empty : OverScaleMode?.ToString().ToLower();

}

public class WheelOptions
{
  ///<summary>
  /// Enable zooming via mouse wheel
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public bool? Enabled { get; set; }

  ///<summary>
  /// Factor of zoom speed via mouse wheel
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Speed { get; set; }

  ///<summary>
  /// Modifier key required for zooming via mouse wheel
  /// <summary>
  [JsonIgnore]
  public ModifierKey? ModifierKey { get; set; }

  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  [JsonInclude]
  [JsonPropertyName( "modifierKey" )]
  private string? ModifierKeyText => ( ModifierKey == BlazorBootstrap.ModifierKey.None ) ? string.Empty : ModifierKey?.ToString().ToLower();
}