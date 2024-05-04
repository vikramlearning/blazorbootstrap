namespace BlazorBootstrap.Demo.RCL;

public partial class GridMainLayout : MainLayoutBase
{
    internal override IEnumerable<NavItem> GetNavItems()
    {
        navItems ??= new List<NavItem>
        {
            new (){ Id = "1", Text = "Overview", Href = "/grid/overview", IconName = IconName.BrowserEdge }, 
            
            new (){ Id = "2", Text = "Data Binding", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Data parameter - Assign collection
            // Data parameter - Update collection

            new (){ Id = "3", Text = "Filters", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Client side filtering
            // Client side filtering with StringComparision
            // Set default filter
            // Enum filter
            // Guid filter
            // Disable specific column filter
            // Increase filter textbox width

            new (){ Id = "4", Text = "Paging", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Client side paging
            // Dynamic page size
            // Page size selection
            // Auto hide paging

            new (){ Id = "5", Text = "Sorting", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Client side sorting
            // Set default sorting
            // Disable specific column sorting

            new (){ Id = "6", Text = "Selection", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Multiple Selection
            // Disable Selection

            new (){ Id = "7", Text = "Alignment", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Header text alignment
            // Cell alignment
            // Cell formating
            // Cell nowrap

            new (){ Id = "8", Text = "Grid Settings", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Save and Load Grid Settings

            new (){ Id = "9", Text = "Custom CSS class", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Conditional css class for grid row
            // Conditional css class for grid column
            // Column Class
            // Header row css class
            // Filters row css class
                        
            new (){ Id = "10", Text = "Events", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Row click event
            // Row double click event

            new (){ Id = "11", Text = "Translations", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Translations

            new (){ Id = "12", Text = "Fixed Header", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Fixed header
            // Fixed header with filters
            
            new (){ Id = "13", Text = "Freeze columns", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Freeze columns
            // Freeze columns with fixed header
            // Freeze columns with fixed header and filters
            
            new (){ Id = "14", Text = "Other", IconName = IconName.LayoutTextWindowReverse, IconColor = IconColor.Success },
            // Empty data

        };

        return navItems;
    }
}
