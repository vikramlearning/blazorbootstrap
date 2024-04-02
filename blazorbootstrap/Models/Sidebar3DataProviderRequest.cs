namespace BlazorBootstrap;

public class Sidebar3DataProviderRequest
{
    #region Methods

    public Sidebar3DataProviderResult ApplyTo(IEnumerable<Sidebar3NavItem> data)
    {
        if (data is null)
            return new Sidebar3DataProviderResult { Data = Enumerable.Empty<Sidebar3NavItem>() };

        return new Sidebar3DataProviderResult { Data = data };
    }

    #endregion
}

