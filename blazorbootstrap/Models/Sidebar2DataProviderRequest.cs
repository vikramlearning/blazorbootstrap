namespace BlazorBootstrap;

public class Sidebar2DataProviderRequest
{
    #region Methods

    public Sidebar2DataProviderResult ApplyTo(IEnumerable<NavItem> data)
    {
        if (data is null)
        {
            return new Sidebar2DataProviderResult { Data = Enumerable.Empty<NavItem>() };
        }

        var result = new List<NavItem>();
        var parentNavItems = data.Where(x => string.IsNullOrWhiteSpace(x.ParentId))?.OrderBy(x => x.Sequence)?.ToList();

        if (!parentNavItems?.Any() ?? true)
        {
            return new Sidebar2DataProviderResult { Data = Enumerable.Empty<NavItem>() };
        }

        result.AddRange(parentNavItems!);

        foreach (var navItem in result)
        {
            if (string.IsNullOrWhiteSpace(navItem.Id))
                continue;

            navItem.Level = 0;

            var childNavItems = data.Where(x => x.ParentId == navItem.Id)?.OrderBy(x => x.Sequence)?.ToList();

            if (childNavItems?.Any() ?? false)
            {
                navItem.HasChildItems = true;
                navItem.ChildItems = childNavItems;

                try
                {
                    SetLevel(data, navItem.ChildItems, 1, new HashSet<string>());
                }
                catch (CircularReferenceException ex)
                {
                    // Handle circular reference exception (e.g., log the error, skip processing the item)
                    Console.WriteLine($"Circular reference detected: {ex.Message}");
                }
            }
        }

        return new Sidebar2DataProviderResult { Data = result };
    }

    private void SetLevel(IEnumerable<NavItem> data, List<NavItem> items, int currentLevel, HashSet<string> visitedIds)
    {
        foreach (var item in items)
        {
            item.Level = currentLevel;

            if (string.IsNullOrWhiteSpace(item.Id))
                continue;

            if (visitedIds.Contains(item.Id))
            {
                throw new CircularReferenceException($"Circular reference detected: Item {item.Id} has a child referring back to it or an ancestor.");
            }

            visitedIds.Add(item.Id);

            var childItems = data.Where(x => x.ParentId == item.Id)?.OrderBy(x => x.Sequence)?.ToList();

            if (childItems?.Any() ?? false)
            {
                item.HasChildItems = true;
                item.ChildItems = childItems;

                SetLevel(data, childItems, currentLevel + 1, new HashSet<string>(visitedIds)); // Pass a copy of visitedIds
            }

            visitedIds.Remove(item.Id); // Remove from visitedIds after processing the item
        }
    }

    #endregion
}

// Add a custom exception class for circular references (optional)
public class CircularReferenceException : Exception
{
    public CircularReferenceException(string message) : base(message)
    {
    }
}
