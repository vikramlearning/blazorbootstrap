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
            BuildSubMenu(navItem, data);
        }

        return new SidebarDataProviderResult { Data = result };
    }

    protected virtual void BuildSubMenu(NavItem item, IEnumerable<NavItem> data)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
            return;

        var childNavItems = data.Where(x => x.ParentId == item.Id)?.OrderBy(x => x.Sequence);

        if (childNavItems is not null && childNavItems.Any())
        {
            item.HasChilds = true;
            item.ChildItems = childNavItems;

            // ChildItems can contain another child item. 
            foreach (var subItem in item.ChildItems)
            {
                var childSubNavItems = data.Where(x => x.ParentId == subItem.Id)?.OrderBy(x => x.Sequence);

                if (childSubNavItems is not null && childSubNavItems.Any())
                {
                    BuildSubMenu(subItem, data);
                }
            }
            
        }
    }


    #endregion
}
