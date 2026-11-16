using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The admin's categories: a table of each category's team and how many tickets (all and open) and articles (drafts
/// included) use it, a dialog to add or edit one (name, team, and an icon from a radio group of icon tiles), and a delete
/// that first moves the category's tickets and articles to another category the admin chooses. Focus returns to the button
/// that opened a dialog, or to the status message when that button is gone.
/// </summary>
public partial class AdminCategories : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The accessible names of the category icons, in words.</summary>
    private static readonly Dictionary<IconName, string> iconLabels = new()
    {
        [IconName.CreditCard] = "Credit card",
        [IconName.PersonLock] = "Person with a lock",
        [IconName.Bug] = "Bug",
        [IconName.Plug] = "Plug",
        [IconName.Lightbulb] = "Light bulb",
        [IconName.ShieldLock] = "Shield with a lock",
        [IconName.Gear] = "Gear",
        [IconName.Phone] = "Phone",
        [IconName.Globe] = "Globe",
        [IconName.Truck] = "Truck",
        [IconName.BoxSeam] = "Parcel",
        [IconName.QuestionCircle] = "Question mark",
    };

    private Button? addButton;

    private bool deleted;

    private readonly Dictionary<string, Button> deleteButtons = new();

    private HelpDeskCategory? deleting;

    private Modal deleteModal = default!;

    private readonly Dictionary<string, Button> editButtons = new();

    /// <summary>The category being edited; <see langword="null" /> while adding one.</summary>
    private HelpDeskCategory? editing;

    private Modal editorModal = default!;

    private int formErrorCount;

    private string? formError;

    private IconName iconName;

    private string? moveError;

    private ElementReference moveSelect;

    private string moveToId = string.Empty;

    private string name = string.Empty;

    private string? nameError;

    private ElementReference nameInput;

    private string? pageError;

    private bool saved;

    private string? savedId;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private string? teamError;

    private string teamId = string.Empty;

    private ElementReference teamSelect;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string CharacterCount(string? value, int max) =>
        string.Create(CultureInfo.InvariantCulture, $"{value?.Length ?? 0} of {max} characters.");

    private static string Count(int count, string singular, string plural) =>
        count == 1 ? $"1 {singular}" : string.Create(CultureInfo.InvariantCulture, $"{count} {plural}");

    /// <summary>An icon's name in words, for its radio button; unknown icons fall back to their enum name split into words.</summary>
    private static string IconLabel(IconName icon) =>
        iconLabels.TryGetValue(icon, out var label)
            ? label
            : string.Concat(icon.ToString().Select((letter, index) => index > 0 && char.IsUpper(letter) ? $" {char.ToLowerInvariant(letter)}" : letter.ToString()));

    private Task AddAsync() => OpenEditorAsync(null);

    private async Task AskDeleteAsync(HelpDeskCategory category)
    {
        deleting = category;
        deleted = false;
        moveToId = string.Empty;
        moveError = null;
        pageError = null;

        await deleteModal.ShowAsync();
    }

    private async Task ConfirmDeleteAsync()
    {
        if (deleting is not { } category)
            return;

        if (State.Categories.All(other => other.Id != moveToId) || moveToId == category.Id)
        {
            moveError = "Choose the category to move its tickets and articles to.";
            await HelpDeskDisplay.FocusAsync(moveSelect);

            return;
        }

        var counts = Usage().GetValueOrDefault(category.Id);
        var target = State.FindCategory(moveToId);

        switch (State.DeleteCategory(category.Id, moveToId, out var moved))
        {
            case HelpDeskAdminResult.Ok:
                deleted = true;
                SetStatus(moved == 0
                    ? $"The category {category.Name} is deleted. Nothing needed moving."
                    : $"The category {category.Name} is deleted. {Count(moved, "item", "items")} moved to {target.Name}: {Count(counts.Tickets, "ticket", "tickets")} and {Count(counts.Articles, "article", "articles")}.");
                break;
            case HelpDeskAdminResult.Invalid:
                moveError = "Choose another category to move them to; that one is no longer available.";
                await HelpDeskDisplay.FocusAsync(moveSelect);

                return;
            case HelpDeskAdminResult.InUse:
                pageError = $"{category.Name} is the only category, so it can't be deleted.";
                break;
            case HelpDeskAdminResult.NotFound:
                pageError = $"The category {category.Name} is already gone.";
                break;
            default:
                pageError = "Only admins can delete categories. Sign in as an admin to make this change.";
                break;
        }

        await deleteModal.HideAsync();
    }

    private Task EditAsync(HelpDeskCategory category) => OpenEditorAsync(category);

    private string? NameProblem(string value)
    {
        var trimmed = value.Trim();

        if (trimmed.Length == 0)
            return "Enter the category name.";

        if (trimmed.Length > HelpDeskState.MaxGroupNameLength)
            return string.Create(CultureInfo.InvariantCulture, $"Use {HelpDeskState.MaxGroupNameLength} characters or fewer.");

        return State.Categories.Any(category => category.Id != editing?.Id && string.Equals(category.Name, trimmed, StringComparison.OrdinalIgnoreCase))
            ? "Another category already has that name."
            : null;
    }

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: after a delete the row is gone, so focus goes to the status.</summary>
    private async Task OnDeleteHiddenAsync()
    {
        var category = deleting;
        deleting = null;

        if (deleted || category is null || !deleteButtons.TryGetValue(category.Id, out var button))
        {
            deleted = false;
            await HelpDeskDisplay.FocusAsync(statusRegion);
        }
        else
        {
            await HelpDeskDisplay.FocusAsync(button.Element);
        }
    }

    private async Task OnDeleteShownAsync()
    {
        if (deleting is not null && !IsLastCategory)
            await HelpDeskDisplay.FocusAsync(moveSelect);
    }

    /// <summary>
    /// Returns focus after the editor closes: to the category's Edit button after an edit, to the status after adding one, or
    /// to the button that opened the dialog when it was cancelled.
    /// </summary>
    private async Task OnEditorHiddenAsync()
    {
        var wasNew = editing is null;
        var wasSaved = saved;
        var id = wasSaved ? savedId : editing?.Id;

        editing = null;
        saved = false;
        savedId = null;

        if (wasSaved && wasNew)
            await HelpDeskDisplay.FocusAsync(statusRegion);
        else if (id is not null && editButtons.TryGetValue(id, out var button) && State.Categories.Any(category => category.Id == id))
            await HelpDeskDisplay.FocusAsync(button.Element);
        else if (wasNew)
            await HelpDeskDisplay.FocusAsync(addButton?.Element);
        else
            await HelpDeskDisplay.FocusAsync(statusRegion);
    }

    private Task OnEditorShownAsync() => HelpDeskDisplay.FocusAsync(nameInput);

    private void OnMoveChanged()
    {
        if (State.Categories.Any(category => category.Id == moveToId && category.Id != deleting?.Id))
            moveError = null;
    }

    private void OnNameInput()
    {
        if (nameError is not null && NameProblem(name) is null)
            nameError = null;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnTeamChanged()
    {
        if (State.FindTeam(teamId) is not null)
            teamError = null;
    }

    private async Task OpenEditorAsync(HelpDeskCategory? category)
    {
        editing = category;
        saved = false;
        savedId = null;
        name = category?.Name ?? string.Empty;
        teamId = category?.TeamId ?? State.Teams.FirstOrDefault()?.Id ?? string.Empty;
        iconName = category?.IconName ?? HelpDeskSeedData.CategoryIcons()[^1];
        nameError = null;
        teamError = null;
        formError = null;
        formErrorCount = 0;
        pageError = null;

        await editorModal.ShowAsync();
    }

    private async Task SaveAsync()
    {
        nameError = NameProblem(name);
        teamError = State.FindTeam(teamId) is null ? "Choose the team this category's requests go to." : null;
        formError = null;

        if (nameError is not null)
        {
            await HelpDeskDisplay.FocusAsync(nameInput);

            return;
        }

        if (teamError is not null)
        {
            await HelpDeskDisplay.FocusAsync(teamSelect);

            return;
        }

        var previousTeamId = editing is null ? null : State.Categories.FirstOrDefault(category => category.Id == editing.Id)?.TeamId;

        switch (State.SaveCategory(editing?.Id, name, iconName, teamId, out var result))
        {
            case HelpDeskAdminResult.Ok when result is not null:
                var team = TeamName(result.TeamId);
                SetStatus(editing is null
                    ? $"The category {result.Name} is added. New requests in it go to {team}."
                    : previousTeamId is not null && previousTeamId != result.TeamId
                        ? $"{result.Name} is saved. Its unassigned tickets now show in the {team} queue, and new requests go there."
                        : $"{result.Name} is saved.");
                break;
            case HelpDeskAdminResult.NoChange:
                SetStatus($"Nothing changed for {editing?.Name ?? name.Trim()}.");
                break;
            case HelpDeskAdminResult.Duplicate:
                nameError = "Another category already has that name.";
                await HelpDeskDisplay.FocusAsync(nameInput);

                return;
            case HelpDeskAdminResult.LimitReached:
                SetFormError(string.Create(CultureInfo.InvariantCulture, $"There can be up to {HelpDeskState.MaxCategories} categories. Delete one before adding another."));

                return;
            case HelpDeskAdminResult.Invalid:
                SetFormError("Check the name, team, and icon: one of them isn't valid.");

                return;
            case HelpDeskAdminResult.NotFound:
                SetFormError("This category no longer exists. Cancel to see the current list.");

                return;
            default:
                SetFormError("Only admins can change categories. Sign in as an admin to make this change.");

                return;
        }

        saved = true;
        savedId = result?.Id ?? editing?.Id;

        await editorModal.HideAsync();
    }

    /// <summary>Shows a dialog error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void SetFormError(string message)
    {
        formErrorCount++;
        formError = formErrorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrorCount})");
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    private string TeamName(string? id) => State.FindTeam(id)?.Name ?? "No team";

    /// <summary>Per category ID: its tickets (all and not resolved) and its articles, drafts included.</summary>
    private Dictionary<string, (int Tickets, int Open, int Articles)> Usage()
    {
        var usage = State.Categories.ToDictionary(category => category.Id, _ => (Tickets: 0, Open: 0, Articles: 0));

        foreach (var ticket in State.Tickets)
        {
            if (usage.TryGetValue(ticket.CategoryId, out var counts))
                usage[ticket.CategoryId] = (counts.Tickets + 1, counts.Open + (ticket.Status == HelpDeskStatus.Resolved ? 0 : 1), counts.Articles);
        }

        foreach (var article in State.AllArticles)
        {
            if (usage.TryGetValue(article.CategoryId, out var counts))
                usage[article.CategoryId] = (counts.Tickets, counts.Open, counts.Articles + 1);
        }

        return usage;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets whether only one category is left; it can't be deleted.</summary>
    private bool IsLastCategory => State.Categories.Count <= 1;

    [Inject] private HelpDeskState State { get; set; } = default!;

    #endregion
}
