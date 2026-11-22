using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Onboarding templates (HR admin): a <see cref="Grid{TItem}" /> of every template (name linking to its page, which its
/// stretched area makes the whole row do; description, task count, and the plans started from it, matched by name, with
/// links to the people), with sorting, the column chooser, CSV export, and a saved view that can be reset; and a validated
/// form to create one (rule WR-002: a unique name, checked as you type, and a description of up to 200 characters;
/// <see cref="HrState.SaveTemplate" />), which then opens the new template's page to add its tasks. After a template is
/// deleted on its own page, this page says so (<c>?deleted=1</c>, filled after the page has rendered so the status region
/// announces it).
/// </summary>
public partial class AdminTemplates : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The description as typed, for the live counter (the bound value changes only on change).</summary>
    private string descriptionText = string.Empty;

    private EditContext editContext = default!;

    private bool focusStatus;

    private TemplateForm form = new();

    private Grid<TemplateRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private ShowcaseUniqueCheck? nameCheck;

    /// <summary>Whether the "template deleted" message waits for the status region to be on the page.</summary>
    private bool pendingDeletedNotice;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The saved view lives in the tab's session storage, so it loads once the page is interactive; then the grid shows.
        if (firstRender)
        {
            await view.LoadAsync();
            viewLoaded = true;
            StateHasChanged();
        }

        // The status region renders once the role guard shows the page; fill it in the render after, so it is announced.
        if (pendingDeletedNotice && statusRegion is not null)
        {
            pendingDeletedNotice = false;
            Announce(true, "Deleted the template. Plans already started from it keep their tasks.");
            StateHasChanged();
        }

        if (focusStatus)
        {
            focusStatus = false;
            await HrDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        editContext = new EditContext(form);
        view = GridViews.Get(State.SiteSlug, "onboarding-templates", HrState.MaxTemplates);
        State.Changed += OnStateChanged;
        pendingDeletedNotice = Deleted == 1;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>
    /// Creates the template once the form is valid (the uniqueness check finishes first, then every rule runs, and a failed
    /// submit shows the summary), and opens its page to add tasks. The state checks again; its answer maps to a message.
    /// </summary>
    private async Task CreateAsync()
    {
        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();

        if (!await editContext.ValidateAsync() || !nameFree)
            return;

        var result = State.SaveTemplate(0, form.Name.Trim(), form.Description, out var saved);

        switch (result)
        {
            case HrResult.Ok when saved is not null:
                Navigation.NavigateTo(AdminHelpers.TemplateUrl(saved.Id) + "?created=1");

                return;

            case HrResult.Duplicate:
                Announce(false, "Another template already has that name. Choose another; nothing was created.");

                break;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can create templates. Nothing was created.");

                break;

            case HrResult.Invalid:
                Announce(false, State.Templates.Count >= HrState.MaxTemplates
                    ? $"There are {HrState.MaxTemplates} templates, the most the portal keeps. Nothing was created."
                    : "The template couldn't be created: the storage for this demo tab is full, or a value is out of range.");

                break;

            default:
                Announce(false, "That didn't work, and nothing was created. Try again.");

                break;
        }

        focusStatus = true;
    }

    /// <summary>Whether another template already has this name.</summary>
    private Task<bool> IsNameTakenAsync(string value) =>
        Task.FromResult(State.Templates.Any(item => string.Equals(item.Name, value, StringComparison.OrdinalIgnoreCase)));

    private void OnDescriptionInput(ChangeEventArgs args) => descriptionText = args.Value?.ToString() ?? string.Empty;

    /// <summary>A template or a plan changed (here or elsewhere): the grid reads the templates again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The templates for the grid, by name unless a column is sorted, each with the people whose plans came from it.</summary>
    private Task<GridDataProviderResult<TemplateRow>> ProvideTemplatesAsync(GridDataProviderRequest<TemplateRow> request)
    {
        var plans = State.Onboardings;
        var rows = State.Templates.Select(template =>
        {
            var people = plans.Where(plan => string.Equals(plan.TemplateName, template.Name, StringComparison.OrdinalIgnoreCase))
                .Select(plan => State.FindEmployee(plan.EmployeeId) is { } person ? new PlanPerson(person.Id, person.Name) : new PlanPerson(null, "Someone who left"))
                .ToList();

            return new TemplateRow(template.Id, template.Name, template.Description, template.Tasks.Count == 0 ? "None yet" : template.Tasks.Count.ToString(CultureInfo.InvariantCulture),
                template.Tasks.Count, people.Count == 0 ? "None" : $"{HrDisplay.Plural(people.Count, "plan")}: {string.Join(", ", people.Select(person => person.Name))}", people);
        }).ToList();

        return view.ApplyAsync(request, rows);
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the status.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        gridVersion++;
        Announce(true, "The view is reset: templates by name, and every column shown.");
        focusStatus = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets 1 when a template was just deleted on its own page.</summary>
    [SupplyParameterFromQuery(Name = "deleted")] public int? Deleted { get; set; }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Someone whose plan was started from a template: a link to their profile, or a name only once they left.</summary>
    private sealed record PlanPerson(int? Id, string Name);

    /// <summary>The new-template form's fields, with the limits of <see cref="HrState.SaveTemplate" />.</summary>
    private sealed class TemplateForm
    {
        [StringLength(HrState.MaxDescriptionLength, ErrorMessage = "Shorten the description to 200 characters or fewer.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Give the template a name.")]
        [StringLength(HrState.MaxNameLength, ErrorMessage = "Use 80 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>One template as a grid row: the words each column shows and exports, and the values it sorts by.</summary>
    private sealed record TemplateRow(int Id, string Name, string Description, string Tasks, int TaskCount, string Plans, IReadOnlyList<PlanPerson> People);

    #endregion
}
