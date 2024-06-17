namespace BlazorBootstrap;

public class Sidebar2DataProviderRequest
{
    #region Methods

    public static Sidebar2DataProviderResult ApplyTo(IReadOnlyCollection<NavItem>? data)
    {
        if (data is null)
        {
            return new Sidebar2DataProviderResult { Data = Array.Empty<NavItem>() };
        }

        var result = new List<NavItem>();
        var parentNavItems = data.Where(x => string.IsNullOrWhiteSpace(x.ParentId))?.OrderBy(x => x.Sequence)?.ToList();

        if (parentNavItems == null || parentNavItems.Count == 0)
            return new Sidebar2DataProviderResult { Data = Array.Empty<NavItem>() };

        result.AddRange(parentNavItems!);

        foreach (var navItem in result)
        {
            if (string.IsNullOrWhiteSpace(navItem.Id))
                continue;

            navItem.Level = 0;

            var childNavItems = data.Where(x => x.ParentId == navItem.Id)?.OrderBy(x => x.Sequence)?.ToList();

            if (childNavItems is { Count: > 0})
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

    private static void SetLevel(IReadOnlyCollection<NavItem> data, List<NavItem> items, int currentLevel, HashSet<string> visitedIds)
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

            if (childItems is { Count: > 0})
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
