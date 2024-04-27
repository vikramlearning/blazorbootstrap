namespace BlazorBootstrap;

public class SidebarDataProviderRequest
{
    #region Methods

    public SidebarDataProviderResult ApplyTo(IEnumerable<NavItem> data)
    {
        if (data is null)
            return new SidebarDataProviderResult { Data = Enumerable.Empty<NavItem>() };

        var result = new List<NavItem>();
        var parentNavItems = data.Where(x => string.IsNullOrWhiteSpace(x.ParentId))?.OrderBy(x => x.Sequence);

        if (parentNavItems is null || !parentNavItems.Any())
            return new SidebarDataProviderResult { Data = Enumerable.Empty<NavItem>() };

        result.AddRange(parentNavItems);

        foreach (var navItem in parentNavItems)
        {
            if (string.IsNullOrWhiteSpace(navItem.Id))
                continue;

            var childNavItems = data.Where(x => x.ParentId == navItem.Id)?.OrderBy(x => x.Sequence)?.ToList();

            if (childNavItems is not null && childNavItems.Any())
            {
                navItem.HasChildItems = true;
                navItem.ChildItems = childNavItems;
            }
        }

        return new SidebarDataProviderResult { Data = result };
    }

    #endregion
}
