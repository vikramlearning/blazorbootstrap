namespace BlazorBootstrap;

public class TreeViewDataProviderRequest
{
    #region Methods

    public TreeViewDataProviderResult ApplyTo(IEnumerable<NavItem> data)
    {
        if (data is null)
            return new TreeViewDataProviderResult { Data = Enumerable.Empty<NavItem>() };

        var result = new List<NavItem>();
        var parentNavItems = data.Where(x => string.IsNullOrWhiteSpace(x.ParentId))?.OrderBy(x => x.Sequence);

        if (parentNavItems is null || !parentNavItems.Any())
            return new TreeViewDataProviderResult { Data = Enumerable.Empty<NavItem>() };

        result.AddRange(parentNavItems);

        foreach (var navItem in parentNavItems)
        {
            if (string.IsNullOrWhiteSpace(navItem.Id))
                continue;

            var childNavItems = data.Where(x => x.ParentId == navItem.Id)?.OrderBy(x => x.Sequence);

            if (childNavItems is not null && childNavItems.Any())
            {
                navItem.HasChildItems = true;
                navItem.ChildItems = childNavItems;
            }
        }

        //void UpdateLevel(IOrderedEnumerable<NavItem> items, int currentLevel = 0)
        //{
        //    var parentNavItems = data?.Where(x => string.IsNullOrWhiteSpace(x.ParentId))?.OrderBy(x => x.Sequence);

        //    if (parentNavItems is null || !parentNavItems.Any())
        //        return;

        //    foreach (var employee in employees)
        //    {
        //        employee.Level = currentLevel;
        //        UpdateLevel(employees.Where(e => e.ParentId == employee.Id).ToList(), currentLevel + 1);
        //    }
        //}

        return new TreeViewDataProviderResult { Data = result };
    }

    #endregion
}
