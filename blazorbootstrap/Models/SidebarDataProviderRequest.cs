namespace BlazorBootstrap;

public class SidebarDataProviderRequest
{
    #region Methods

    public SidebarDataProviderResult ApplyTo(IEnumerable<NavItem> data)
    {
        if (data is null)
            return new SidebarDataProviderResult { Data = Enumerable.Empty<NavItem>() };

        return new SidebarDataProviderResult { Data = data };
    }

    #endregion
}

