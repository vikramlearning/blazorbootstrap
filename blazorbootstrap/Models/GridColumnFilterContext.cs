namespace BlazorBootstrap.Models;

public sealed record GridColumnFilterContext(string? EnumFilterSelectText, GridFiltersTranslationDelegate FiltersTranslationProvider, bool FixedHeader)
{
}
