using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBootstrap;

public abstract class Annotation
{
  /// <summary>
  /// The type of the annotation
  /// </summary>
  public abstract string Type { get; }

  ///<summary>
  /// TRUE
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public bool? AdjustScaleRange { get; set; }
  ///<summary>
  /// options.color
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BackgroundColor { get; set; }
  ///<summary>
  /// options.color
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderColor { get; set; }
  ///<summary>
  /// []
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public List<double?>? BorderDash { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderDashOffset { get; set; }
  ///<summary>
  /// 'transparent'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderShadowColor { get; set; }
  ///<summary>
  /// TRUE
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public bool? Display { get; set; }
  ///<summary>
  /// 'afterDatasetsDraw'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public DrawTime? DrawTime { get; set; }
  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public bool? Init { get; set; }
  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? Id { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? ShadowBlur { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? ShadowOffsetX { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? ShadowOffsetY { get; set; }
  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? XMax { get; set; }

  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? XMin { get; set; }

  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? XScaleID { get; set; }
  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? YMin { get; set; }
  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? YMax { get; set; }
  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? YScaleID { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Z { get; set; }

}

public class LineAnnotation : Annotation
{
  public override string Type => "line";

  /// <summary>
  /// The border width, defaults to 2.
  /// </summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderWidth { get; set; }

  /// <summary>
  /// Whether or not a quadratic Bézier curve is drawn.
  /// Defaults to <see langword=false/>
  /// </summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public bool? Curve { get; set; }

  /// <summary>
  /// End two of the line when a single scale is specified.
  /// </summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? EndValue { get; set; }

  /// <summary>
  /// Defines options for the line annotation label.
  /// </summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public ChartAnnotationLabel? Label { get; set; }

  /// <summary>
  /// ID of the scale in single scale mode. If unset, xScaleID and yScaleID are used.
  /// If scaleID is set, then value and endValue must also be set to indicate the endpoints of the line. The 
  /// </summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? ScaleID { get; set; }

  /// <summary>
  /// End one of the line when a single scale is specified.
  /// </summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Value { get; set; }

  /// <summary>
  /// If curve is enabled, it configures the control point to drawn the curve, calculated in pixels. 
  /// It can be set by a string in percentage format 'number%' which are representing the percentage of the distance between the start and end point from the center.
  /// </summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? ControlPoint { get; set; }

}

public class ChartAnnotationLabel
{
  ///<summary>
  /// Background color of the label container.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string BackgroundColor { get; set; }

  ///<summary>
  /// The color of shadow of the box where the label is located. See MDN
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string BackgroundShadowColor { get; set; }

  ///<summary>
  /// Cap style of the border.
  /// Supported values are 'butt', 'round', and 'square'.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderCapStyle { get; set; }

  ///<summary>
  /// The border line color.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderColor { get; set; }

  ///<summary>
  /// Length and spacing of dashes. See MDN
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public List<double>? BorderDash { get; set; }

  ///<summary>
  /// Offset for border line dashes. See MDN
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderDashOffset { get; set; }

  ///<summary>
  /// Border line join style. See MDN
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderJoinStyle { get; set; }

  ///<summary>
  /// Radius of label box corners in pixels.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderRadius { get; set; }

  ///<summary>
  /// The color of border shadow of the box where the label is located. See MDN
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderShadowColor { get; set; }

  ///<summary>
  /// The border line width (in pixels).
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderWidth { get; set; }

  ///<summary>
  /// Text color.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public List<string?>? Color { get; set; }

  ///<summary>
  /// The content to show in the label
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string Content { get; set; }

  ///<summary>
  /// Whether or not the label is shown.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public bool? Display { get; set; }

  ///<summary>
  /// See drawTime. Defaults to the line annotation draw time if unset.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public DrawTime? DrawTime { get; set; }

  ///<summary>
  /// Label font.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? Font { get; set; }

  ///<summary>
  /// Overrides the height of the image or canvas element. Could be set in pixel by a number, or in percentage of current height of image or canvas element by a string. If undefined, uses the height of the image or canvas element. It is used only when the content is an image or canvas element.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? Height { get; set; }

  ///<summary>
  /// Overrides the opacity of the image or canvas element. Could be set a number in the range 0.0 to 1.0, inclusive. If undefined, uses the opacity of the image or canvas element. It is used only when the content is an image or canvas element.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Opacity { get; set; }

  ///<summary>
  /// The padding to add around the text label.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? Padding { get; set; }

  ///<summary>
  /// Anchor position of label on line. Possible options are: 'start', 'center', 'end'. It can be set by a string in percentage format 'number%' which are representing the percentage on the width of the line where the label will be located.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public Alignment? Position { get; set; }

  ///<summary>
  /// Rotation of label, in degrees, or 'auto' to use the degrees of the line.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Rotation { get; set; }

  ///<summary>
  /// The amount of blur applied to shadow of the box where the label is located. See MDN
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? ShadowBlur { get; set; }

  ///<summary>
  /// The distance that shadow, of the box where the label is located, will be offset horizontally. See MDN
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? ShadowOffsetX { get; set; }

  ///<summary>
  /// The distance that shadow, of the box where the label is located, will be offset vertically. See MDN
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? ShadowOffsetY { get; set; }

  ///<summary>
  /// Text alignment of label content when there's more than one line. Possible options are: 'start', 'center', 'end'.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public TitleAlignment? TextAlign { get; set; }

  ///<summary>
  /// The color of the stroke around the text.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? TextStrokeColor { get; set; }

  ///<summary>
  /// Stroke width around the text.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? TextStrokeWidth { get; set; }

  ///<summary>
  /// Overrides the width of the image or canvas element. Could be set in pixel by a number, or in percentage of current width of image or canvas element by a string. If undefined, uses the width of the image or canvas element. It is used only when the content is an image or canvas element.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? Width { get; set; }

  ///<summary>
  /// Adjustment along x-axis (left-right) of label relative to computed position. Negative values move the label left, positive right.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? XAdjust { get; set; }

  ///<summary>
  /// Adjustment along y-axis (top-bottom) of label relative to computed position. Negative values move the label up, positive down.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? YAdjust { get; set; }

  ///<summary>
  /// It determines the drawing stack level of the label element, with same drawTime.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Z { get; set; }


}

public class BoxAnnotation : Annotation
{
  public override string Type => "box";

  ///<summary>
  /// 'transparent'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BackgroundShadowColor { get; set; }
  ///<summary>
  /// 'butt'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderCapStyle { get; set; }
  ///<summary>
  /// 'miter'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderJoinStyle { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? BorderRadius { get; set; }
  ///<summary>
  /// 1
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderWidth { get; set; }
  ///<summary>
  /// 
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public ChartAnnotationLabel? Label { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Rotation { get; set; }
}


public class EllipseAnnotation : Annotation
{
  public override string Type => "ellipse";

  ///<summary>
  /// 'transparent'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BackgroundShadowColor { get; set; }
  ///<summary>
  /// 1
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderWidth { get; set; }
  ///<summary>
  /// 
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public ChartAnnotationLabel? label { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Rotation { get; set; }
}

public class LabelAnnotation : Annotation
{
  public override string Type => "label";

  ///<summary>
  /// The color of shadow of the box where the label is located. See MDN
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string BackgroundShadowColor { get; set; }

  ///<summary>
  /// Cap style of the border.
  /// Supported values are 'butt', 'round', and 'square'.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderCapStyle { get; set; }

  ///<summary>
  /// 'miter'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderJoinStyle { get; set; }

  ///<summary>
  /// Radius of label box corners in pixels.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderRadius { get; set; }

  ///<summary>
  /// The border line width (in pixels).
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderWidth { get; set; }

  ///<summary>
  /// Text color.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public List<string?>? Color { get; set; }

  ///<summary>
  /// The content to show in the label
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string Content { get; set; }

  ///<summary>
  /// Label font.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? Font { get; set; }

  ///<summary>
  /// Overrides the height of the image or canvas element. Could be set in pixel by a number, or in percentage of current height of image or canvas element by a string. If undefined, uses the height of the image or canvas element. It is used only when the content is an image or canvas element.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? Height { get; set; }

  ///<summary>
  /// Overrides the opacity of the image or canvas element. Could be set a number in the range 0.0 to 1.0, inclusive. If undefined, uses the opacity of the image or canvas element. It is used only when the content is an image or canvas element.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Opacity { get; set; }

  ///<summary>
  /// The padding to add around the text label.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? Padding { get; set; }

  ///<summary>
  /// Anchor position of label on line. Possible options are: 'start', 'center', 'end'. It can be set by a string in percentage format 'number%' which are representing the percentage on the width of the line where the label will be located.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public Alignment? Position { get; set; }

  ///<summary>
  /// Rotation of label, in degrees, or 'auto' to use the degrees of the line.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Rotation { get; set; }

  ///<summary>
  /// Text alignment of label content when there's more than one line. Possible options are: 'start', 'center', 'end'.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public TitleAlignment? TextAlign { get; set; }

  ///<summary>
  /// The color of the stroke around the text.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? TextStrokeColor { get; set; }

  ///<summary>
  /// Stroke width around the text.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? TextStrokeWidth { get; set; }

  ///<summary>
  /// Overrides the width of the image or canvas element. Could be set in pixel by a number, or in percentage of current width of image or canvas element by a string. If undefined, uses the width of the image or canvas element. It is used only when the content is an image or canvas element.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? Width { get; set; }

  ///<summary>
  /// Adjustment along x-axis (left-right) of label relative to computed position. Negative values move the label left, positive right.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? XAdjust { get; set; }

  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? XValue { get; set; }

  ///<summary>
  /// Adjustment along y-axis (top-bottom) of label relative to computed position. Negative values move the label up, positive down.
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? YAdjust { get; set; }


  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? YValue { get; set; }
}


public class PointAnnotation : Annotation
{
  public override string Type => "point";

  ///<summary>
  /// 'transparent'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BackgroundShadowColor { get; set; }
  ///<summary>
  /// 1
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderWidth { get; set; }
  ///<summary>
  /// 'circle'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public PointStyle? PointStyle { get; set; }
  ///<summary>
  /// 10
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Radius { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Rotation { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? XAdjust { get; set; }
  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? XValue { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? YAdjust { get; set; }
  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? YValue { get; set; }
}

public class PolygonAnnotation : Annotation
{
  public override string Type => "polygon";

  ///<summary>
  /// 'transparent'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BackgroundShadowColor { get; set; }
  ///<summary>
  /// 'butt'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderCapStyle { get; set; }
  ///<summary>
  /// 'miter'
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public string? BorderJoinStyle { get; set; }
  ///<summary>
  /// 1
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? BorderWidth { get; set; }
  ///<summary>
  /// {radius: 0}
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public PointAnnotation? Point { get; set; }
  ///<summary>
  /// 10
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Radius { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Rotation { get; set; }
  ///<summary>
  /// 3
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? Sides { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? XAdjust { get; set; }
  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? XValue { get; set; }
  ///<summary>
  /// 0
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public double? YAdjust { get; set; }
  ///<summary>
  /// undefined
  /// <summary>
  [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
  public object? YValue { get; set; }

}

