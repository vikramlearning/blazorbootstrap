namespace BlazorBootstrap;

public class TreeViewDataProviderRequest
{
    #region Methods

    public TreeViewDataProviderResult ApplyTo(IEnumerable<NavItem> data)
    {
        if (data is null)
        {
            return new TreeViewDataProviderResult { Data = Enumerable.Empty<NavItem>() };
        }

        var result = new List<NavItem>();
        var parentNavItems = data.Where(x => string.IsNullOrWhiteSpace(x.ParentId))?.OrderBy(x => x.Sequence)?.ToList();

        if (!parentNavItems?.Any() ?? true)
        {
            return new TreeViewDataProviderResult { Data = Enumerable.Empty<NavItem>() };
        }

        result.AddRange(parentNavItems);

        foreach (var navItem in result)
        {
            if (string.IsNullOrWhiteSpace(navItem.Id))
                continue;

            navItem.Level = 0;

            var childNavItems = data.Where(x => x.ParentId == navItem.Id)?.OrderBy(x => x.Sequence)?.ToList();

            if (childNavItems?.Any() ?? false)
            {
                //Console.WriteLine($"Id: {navItem.Id}, Text: {navItem.Text}");

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

        //Console.WriteLine($"{JsonSerializer.Serialize(result)}");

        Console.WriteLine($"{result[2].Text}");
        Console.WriteLine($"{result[2].HasChildItems}");
        Console.WriteLine($"{result[2].ChildItems[2].Text}");
        Console.WriteLine($"{result[2].ChildItems[2].HasChildItems}");
        Console.WriteLine($"{result[2].ChildItems[2].ChildItems[0].Text}");
        Console.WriteLine($"{result[2].ChildItems[2].ChildItems[0].HasChildItems}");

        return new TreeViewDataProviderResult { Data = result };
    }

    private void SetLevel(IEnumerable<NavItem> data, List<NavItem> items, int currentLevel, HashSet<string> visitedIds)
    {
        foreach (var item in items)
        {
            item.Level = currentLevel;

            //Console.WriteLine($"currentLevel: {currentLevel}, item: {item.Id}, {item.Text}");

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
