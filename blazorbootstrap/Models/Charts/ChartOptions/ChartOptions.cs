namespace BlazorBootstrap;

public class ChartOptions
{
    public Scales Scales { get; set; }
}

public class Scales
{
    public X X { get; set; }
}

public class X
{
    public Title Title { get; set; }
}

public class Title
{
    public bool Display { get; set; }
    public string Text { get; set; }
}
