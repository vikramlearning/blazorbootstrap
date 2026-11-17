using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Onboarding templates (HR admin): a table of every template (name linking to its page, description, task count, and the
/// plans started from it, matched by name, with links to the people) and a form to create one (name and description;
/// <see cref="HrState.SaveTemplate" />, a taken name is shown under the field), which then opens the new template's page to
/// add its tasks. After a template is deleted on its own page, this page says so (<c>?deleted=1</c>, filled after the page
/// has rendered so the status region announces it).
/// </summary>
public partial class AdminTemplates : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string description = string.Empty;

    private string? descriptionError;

    private ElementReference? descriptionInput;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private string name = string.Empty;

    private string? nameError;

    private ElementReference? nameInput;

    /// <summary>Whether the "template deleted" message waits for the status region to be on the page.</summary>
    private bool pendingDeletedNotice;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The status region renders once the role guard shows the page; fill it in the render after, so it is announced.
        if (pendingDeletedNotice && statusRegion is not null)
        {
            pendingDeletedNotice = false;
            Announce(true, "Deleted the template. Plans already started from it keep their tasks.");
            StateHasChanged();
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await HrDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
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

    /// <summary>Checks the form, creates the template, and opens its page to add tasks.</summary>
    private void Create()
    {
        var trimmedName = name.Trim();

        nameError = AdminHelpers.TextProblem(name, HrState.MaxNameLength, "Give the template a name.", "name")
            ?? (State.Templates.FirstOrDefault(item => string.Equals(item.Name, trimmedName, StringComparison.OrdinalIgnoreCase)) is { } same
                ? $"That name is taken by “{same.Name}”. Choose another."
                : null);
        descriptionError = description.Trim().Length > HrState.MaxDescriptionLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the description to {HrState.MaxDescriptionLength} characters or fewer (now {description.Trim().Length}).")
            : null;

        focusTarget = nameError is not null ? () => nameInput : descriptionError is not null ? () => descriptionInput : null;

        if (focusTarget is not null)
        {
            Announce(false, "Fix the highlighted fields, then create the template again.");

            return;
        }

        var result = State.SaveTemplate(0, trimmedName, description, out var saved);

        switch (result)
        {
            case HrResult.Ok when saved is not null:
                Navigation.NavigateTo(AdminHelpers.TemplateUrl(saved.Id) + "?created=1");

                return;

            case HrResult.Duplicate:
                nameError = "That name is taken by another template. Choose another.";
                focusTarget = () => nameInput;
                Announce(false, "That name is taken. Nothing was created.");

                return;

            case HrResult.NotAllowed:
                Announce(false, "Only the HR admin can create templates. Nothing was created.");

                return;

            case HrResult.Invalid:
                Announce(false, State.Templates.Count >= HrState.MaxTemplates
                    ? $"There are {HrState.MaxTemplates} templates, the most the portal keeps. Nothing was created."
                    : "The template couldn't be created: the storage for this demo tab is full, or a value is out of range.");

                return;

            default:
                Announce(false, "That didn't work, and nothing was created. Try again.");

                return;
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets 1 when a template was just deleted on its own page.</summary>
    [SupplyParameterFromQuery(Name = "deleted")] public int? Deleted { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private HrState State { get; set; } = default!;

    #endregion
}
