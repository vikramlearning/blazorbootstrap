namespace BlazorBootstrap;

public class CarouselEventArgs : EventArgs
{
    /// <summary>
    /// The direction in which the <see cref="Carousel" /> is sliding (either "left" or "right").
    /// </summary>
    public string? Direction { get; set; }

    /// <summary>
    /// The index of the current item.
    /// </summary>
    public int From { get; set; }

    /// <summary>
    /// The index of the next item.
    /// </summary>
    public int To { get; set; }
}
