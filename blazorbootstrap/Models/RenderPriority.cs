namespace BlazorBootstrap;

public record RenderPriority
{
    public RenderPriority()
    {
        Priority = Priority.Low;
        Order = DateTime.Now.Ticks;
    }

    public RenderPriority(Priority priority)
    {
        Priority = priority;
    }

    public Priority Priority { get; init; }

    public long Order { get; init; }
}
