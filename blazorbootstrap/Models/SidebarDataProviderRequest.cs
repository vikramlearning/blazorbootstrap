using System.Collections.ObjectModel;

namespace BlazorBootstrap;

public class SidebarDataProviderRequest
{
    #region Methods

    public static SidebarDataProviderResult ApplyTo(IReadOnlyCollection<NavItem>? data)
    {
        if (data is null)
            return new SidebarDataProviderResult { Data = Array.Empty<NavItem>() };

        var result = new List<NavItem>();
        var parentNavItems = data.Where(x => string.IsNullOrWhiteSpace(x.ParentId))?.OrderBy(x => x.Sequence).ToArray();

        if (parentNavItems is null || parentNavItems.Length == 0)    
            return new SidebarDataProviderResult { Data = Array.Empty<NavItem>() };

        result.AddRange(parentNavItems);

        foreach (var navItem in parentNavItems)
        {
            if (string.IsNullOrWhiteSpace(navItem.Id))
                continue;

            var childNavItems = data.Where(x => x.ParentId == navItem.Id)?.OrderBy(x => x.Sequence)?.ToList();

            if (childNavItems is not null && childNavItems.Count > 0)
            {
                navItem.HasChildItems = true;
                navItem.ChildItems = childNavItems;
            }
        }

        return new SidebarDataProviderResult { Data = result };
    }

    #endregion
}
