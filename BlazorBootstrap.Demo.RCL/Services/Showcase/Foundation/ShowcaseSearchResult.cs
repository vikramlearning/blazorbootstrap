namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// One match of a site's global search: a record the signed-in person may open.
/// </summary>
/// <param name="Kind">The kind of record, for example "Ticket" or "Employee".</param>
/// <param name="Label">The record's name or number, as people know it.</param>
/// <param name="Detail">An optional second line, for example a status or an email address.</param>
/// <param name="Url">The link to the record, for example <c>/showcase/help-desk/agent/tickets/4182</c>.</param>
public sealed record ShowcaseSearchResult(string Kind, string Label, string? Detail, string Url);
