using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The admin's roster and teams: a table of agents (team, role, status, open tickets) with a dialog to add or edit one and
/// a confirmed deactivate that reports how many open tickets went back to the queue, and a card per team (members and the
/// categories routed to it) with a dialog to add or edit one and a delete that explains why a team still in use can't go.
/// Focus returns to the button that opened a dialog, or to the section's status message when that button is gone.
/// </summary>
public partial class AdminAgents : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button? addAgentButton;

    private Button? addTeamButton;

    private readonly Dictionary<string, Button> agentActiveButtons = new();

    private string agentEmail = string.Empty;

    private string? agentEmailError;

    private ElementReference agentEmailInput;

    private readonly Dictionary<string, Button> agentEditButtons = new();

    private readonly LiveMessage agentError = new();

    private readonly LiveMessage agentFormError = new();

    private bool agentIsAdmin;

    private Modal agentModal = default!;

    private string agentName = string.Empty;

    private string? agentNameError;

    private ElementReference agentNameInput;

    private bool agentSaved;

    private string? agentSavedId;

    private readonly LiveMessage agentStatus = new();

    private ElementReference agentStatusRegion;

    private string? agentTeamError;

    private string agentTeamId = string.Empty;

    private ElementReference agentTeamSelect;

    private Modal deactivateModal = default!;

    private HelpDeskAgent? deactivating;

    private Modal deleteTeamModal = default!;

    private HelpDeskTeam? deletingTeam;

    /// <summary>The agent being edited; <see langword="null" /> while adding one.</summary>
    private HelpDeskAgent? editingAgent;

    /// <summary>The team being edited; <see langword="null" /> while adding one.</summary>
    private HelpDeskTeam? editingTeam;

    private bool teamDeleted;

    private readonly Dictionary<string, Button> teamDeleteButtons = new();

    private string teamDescription = string.Empty;

    private string? teamDescriptionError;

    private ElementReference teamDescriptionInput;

    private readonly Dictionary<string, Button> teamEditButtons = new();

    private readonly LiveMessage teamError = new();

    private readonly LiveMessage teamFormError = new();

    private Modal teamModal = default!;

    private string teamName = string.Empty;

    private string? teamNameError;

    private ElementReference teamNameInput;

    private bool teamSaved;

    private string? teamSavedId;

    private readonly LiveMessage teamStatus = new();

    private ElementReference teamStatusRegion;

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

    private static bool IsEmail(string value) =>
        value.IndexOf('@') is > 0 and var at && at < value.Length - 1 && !value.Any(char.IsWhiteSpace);

    private Task AddAgentAsync() => OpenAgentAsync(null);

    private Task AddTeamAsync() => OpenTeamAsync(null);

    private string? AgentEmailProblem(string value)
    {
        var email = value.Trim();

        if (email.Length == 0)
            return "Enter the agent's email address.";

        if (email.Length > ShowcaseIdentity.MaxEmailLength)
            return string.Create(CultureInfo.InvariantCulture, $"Use {ShowcaseIdentity.MaxEmailLength} characters or fewer.");

        if (!IsEmail(email))
            return "Enter an email address like name@example.com.";

        return State.Roster.Any(agent => agent.Id != editingAgent?.Id && string.Equals(agent.Email, email, StringComparison.OrdinalIgnoreCase))
            ? "Another agent already has that email address."
            : null;
    }

    private string? AgentNameProblem(string value)
    {
        var name = value.Trim();

        if (name.Length == 0)
            return "Enter the agent's name.";

        if (name.Length > ShowcaseIdentity.MaxNameLength)
            return string.Create(CultureInfo.InvariantCulture, $"Use {ShowcaseIdentity.MaxNameLength} characters or fewer.");

        return State.Roster.Any(agent => agent.Id != editingAgent?.Id && string.Equals(agent.Name, name, StringComparison.OrdinalIgnoreCase))
            ? "Another agent already has that name."
            : null;
    }

    private async Task AskDeleteTeamAsync(HelpDeskTeam team)
    {
        deletingTeam = team;
        teamDeleted = false;
        teamError.Clear();

        await deleteTeamModal.ShowAsync();
    }

    /// <summary>The categories whose unassigned tickets go to a team.</summary>
    private List<HelpDeskCategory> CategoriesOf(string teamId) => State.Categories.Where(category => category.TeamId == teamId).ToList();

    private async Task ConfirmDeactivateAsync()
    {
        if (deactivating is { } agent)
        {
            var result = State.SetAgentActive(agent.Id, false, out var unassigned);

            if (result == HelpDeskAdminResult.Ok)
            {
                agentStatus.Set(unassigned switch
                {
                    0 => $"{agent.Name} is deactivated. They had no open tickets.",
                    1 => $"{agent.Name} is deactivated. 1 open ticket was unassigned and is back in the queue.",
                    _ => string.Create(CultureInfo.InvariantCulture, $"{agent.Name} is deactivated. {unassigned} open tickets were unassigned and are back in the queue."),
                });
            }
            else
            {
                agentError.Set(ActiveProblem(agent, result, activating: false));
            }
        }

        await deactivateModal.HideAsync();
    }

    private async Task ConfirmDeleteTeamAsync()
    {
        if (deletingTeam is { } team)
        {
            switch (State.DeleteTeam(team.Id))
            {
                case HelpDeskAdminResult.Ok:
                    teamDeleted = true;
                    teamStatus.Set($"The team {team.Name} is deleted.");
                    break;
                case HelpDeskAdminResult.InUse:
                    teamError.Set(TeamBlocker(team) ?? $"{team.Name} can't be deleted: it still has agents or routes categories.");
                    break;
                case HelpDeskAdminResult.NotFound:
                    teamError.Set($"The team {team.Name} is already gone.");
                    break;
                default:
                    teamError.Set("Only admins can delete teams. Sign in as an admin to make this change.");
                    break;
            }
        }

        await deleteTeamModal.HideAsync();
    }

    private Task EditAgentAsync(HelpDeskAgent agent) => OpenAgentAsync(agent);

    private Task EditTeamAsync(HelpDeskTeam team) => OpenTeamAsync(team);

    /// <summary>A member count like "3 agents (1 inactive)"; inactive agents still belong to the team.</summary>
    private string MemberText(string teamId)
    {
        var members = State.Roster.Where(agent => agent.TeamId == teamId).ToList();
        var inactive = members.Count(agent => !agent.IsActive);

        if (members.Count == 0)
            return "None";

        var text = Count(members.Count, "agent", "agents");

        return inactive == 0 ? text : string.Create(CultureInfo.InvariantCulture, $"{text} ({inactive} inactive)");
    }

    private void OnAgentEmailInput()
    {
        if (agentEmailError is not null && AgentEmailProblem(agentEmail) is null)
            agentEmailError = null;
    }

    /// <summary>
    /// Returns focus after the agent dialog closes: to the agent's Edit button after an edit, to the status after adding one,
    /// or to the button that opened the dialog when it was cancelled.
    /// </summary>
    private async Task OnAgentHiddenAsync()
    {
        var wasNew = editingAgent is null;
        var wasSaved = agentSaved;
        var id = wasSaved ? agentSavedId : editingAgent?.Id;

        editingAgent = null;
        agentSaved = false;
        agentSavedId = null;

        if (wasSaved && wasNew)
            await HelpDeskDisplay.FocusAsync(agentStatusRegion);
        else if (id is not null && agentEditButtons.TryGetValue(id, out var button))
            await HelpDeskDisplay.FocusAsync(button.Element);
        else if (wasNew)
            await HelpDeskDisplay.FocusAsync(addAgentButton?.Element);
        else
            await HelpDeskDisplay.FocusAsync(agentStatusRegion);
    }

    private void OnAgentNameInput()
    {
        if (agentNameError is not null && AgentNameProblem(agentName) is null)
            agentNameError = null;
    }

    private Task OnAgentShownAsync() => HelpDeskDisplay.FocusAsync(agentNameInput);

    private void OnAgentTeamChanged()
    {
        if (State.FindTeam(agentTeamId) is not null)
            agentTeamError = null;
    }

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: it goes back to the agent's Deactivate/Reactivate button.</summary>
    private async Task OnDeactivateHiddenAsync()
    {
        var agent = deactivating;
        deactivating = null;

        if (agent is not null && agentActiveButtons.TryGetValue(agent.Id, out var button) && State.FindAgent(agent.Id) is not null)
            await HelpDeskDisplay.FocusAsync(button.Element);
        else
            await HelpDeskDisplay.FocusAsync(agentStatusRegion);
    }

    /// <summary>After a delete the card is gone, so focus goes to the teams' status; otherwise back to the Delete button.</summary>
    private async Task OnDeleteTeamHiddenAsync()
    {
        var team = deletingTeam;
        deletingTeam = null;

        if (teamDeleted || team is null || !teamDeleteButtons.TryGetValue(team.Id, out var button))
        {
            teamDeleted = false;
            await HelpDeskDisplay.FocusAsync(teamStatusRegion);
        }
        else
        {
            await HelpDeskDisplay.FocusAsync(button.Element);
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnTeamDescriptionInput()
    {
        if (teamDescriptionError is not null && teamDescription.Trim().Length <= HelpDeskState.MaxTeamDescriptionLength)
            teamDescriptionError = null;
    }

    /// <summary>Returns focus after the team dialog closes, like the agent dialog.</summary>
    private async Task OnTeamHiddenAsync()
    {
        var wasNew = editingTeam is null;
        var wasSaved = teamSaved;
        var id = wasSaved ? teamSavedId : editingTeam?.Id;

        editingTeam = null;
        teamSaved = false;
        teamSavedId = null;

        if (wasSaved && wasNew)
            await HelpDeskDisplay.FocusAsync(teamStatusRegion);
        else if (id is not null && teamEditButtons.TryGetValue(id, out var button) && State.FindTeam(id) is not null)
            await HelpDeskDisplay.FocusAsync(button.Element);
        else if (wasNew)
            await HelpDeskDisplay.FocusAsync(addTeamButton?.Element);
        else
            await HelpDeskDisplay.FocusAsync(teamStatusRegion);
    }

    private void OnTeamNameInput()
    {
        if (teamNameError is not null && TeamNameProblem(teamName) is null)
            teamNameError = null;
    }

    private Task OnTeamShownAsync() => HelpDeskDisplay.FocusAsync(teamNameInput);

    /// <summary>Open (not resolved) tickets per assignee name.</summary>
    private Dictionary<string, int> OpenTicketCounts() =>
        State.Tickets
            .Where(ticket => ticket.Assignee is not null && ticket.Status != HelpDeskStatus.Resolved)
            .GroupBy(ticket => ticket.Assignee!)
            .ToDictionary(group => group.Key, group => group.Count());

    private async Task OpenAgentAsync(HelpDeskAgent? agent)
    {
        editingAgent = agent;
        agentSaved = false;
        agentSavedId = null;
        agentName = agent?.Name ?? string.Empty;
        agentEmail = agent?.Email ?? string.Empty;
        agentTeamId = agent?.TeamId ?? State.Teams.FirstOrDefault()?.Id ?? string.Empty;
        agentIsAdmin = agent?.IsAdmin ?? false;
        agentNameError = null;
        agentEmailError = null;
        agentTeamError = null;
        agentFormError.Clear();
        agentError.Clear();

        await agentModal.ShowAsync();
    }

    private async Task OpenTeamAsync(HelpDeskTeam? team)
    {
        editingTeam = team;
        teamSaved = false;
        teamSavedId = null;
        teamName = team?.Name ?? string.Empty;
        teamDescription = team?.Description ?? string.Empty;
        teamNameError = null;
        teamDescriptionError = null;
        teamFormError.Clear();
        teamError.Clear();

        await teamModal.ShowAsync();
    }

    private async Task SaveAgentAsync()
    {
        agentNameError = AgentNameProblem(agentName);
        agentEmailError = AgentEmailProblem(agentEmail);
        agentTeamError = State.FindTeam(agentTeamId) is null ? "Choose the agent's team." : null;
        agentFormError.Clear();

        if (agentNameError is not null)
        {
            await HelpDeskDisplay.FocusAsync(agentNameInput);

            return;
        }

        if (agentEmailError is not null)
        {
            await HelpDeskDisplay.FocusAsync(agentEmailInput);

            return;
        }

        if (agentTeamError is not null)
        {
            await HelpDeskDisplay.FocusAsync(agentTeamSelect);

            return;
        }

        var oldName = editingAgent is null ? null : State.FindAgent(editingAgent.Id)?.Name;
        var assigned = oldName is null ? 0 : State.Tickets.Count(ticket => ticket.Assignee == oldName);
        var result = State.SaveAgent(editingAgent?.Id, agentName, agentEmail, agentTeamId, agentIsAdmin, out var saved);

        switch (result)
        {
            case HelpDeskAdminResult.Ok when saved is not null:
                agentStatus.Set(oldName is null
                    ? $"{saved.Name} is added to {TeamName(saved.TeamId)} and can be assigned tickets now."
                    : oldName != saved.Name && assigned > 0
                        ? $"{saved.Name} is saved. {Count(assigned, "ticket", "tickets")} assigned to {oldName} now show the new name."
                        : $"{saved.Name} is saved.");
                break;
            case HelpDeskAdminResult.NoChange:
                agentStatus.Set($"Nothing changed for {oldName ?? agentName.Trim()}.");
                break;
            case HelpDeskAdminResult.Duplicate:
                agentFormError.Set("Another agent already has that name or email. Use a different one.");

                return;
            case HelpDeskAdminResult.LimitReached:
                agentFormError.Set(string.Create(CultureInfo.InvariantCulture, $"The roster is full: it holds up to {HelpDeskState.MaxAgents} agents, inactive ones included. Reactivate an inactive agent instead."));

                return;
            case HelpDeskAdminResult.Invalid:
                agentFormError.Set("Check the name, email, and team: one of them isn't valid.");

                return;
            case HelpDeskAdminResult.NotFound:
                agentFormError.Set("This agent is no longer on the roster. Cancel to see the current list.");

                return;
            default:
                agentFormError.Set("Only admins can change the roster. Sign in as an admin to make this change.");

                return;
        }

        agentSaved = true;
        agentSavedId = saved?.Id ?? editingAgent?.Id;

        await agentModal.HideAsync();
    }

    private async Task SaveTeamAsync()
    {
        teamNameError = TeamNameProblem(teamName);
        teamDescriptionError = teamDescription.Trim().Length > HelpDeskState.MaxTeamDescriptionLength
            ? string.Create(CultureInfo.InvariantCulture, $"Use {HelpDeskState.MaxTeamDescriptionLength} characters or fewer.")
            : null;
        teamFormError.Clear();

        if (teamNameError is not null)
        {
            await HelpDeskDisplay.FocusAsync(teamNameInput);

            return;
        }

        if (teamDescriptionError is not null)
        {
            await HelpDeskDisplay.FocusAsync(teamDescriptionInput);

            return;
        }

        var result = State.SaveTeam(editingTeam?.Id, teamName, teamDescription, out var saved);

        switch (result)
        {
            case HelpDeskAdminResult.Ok when saved is not null:
                teamStatus.Set(editingTeam is null
                    ? $"The team {saved.Name} is added. Move agents to it with Edit, and route categories to it on the Categories page."
                    : $"The team {saved.Name} is saved.");
                break;
            case HelpDeskAdminResult.NoChange:
                teamStatus.Set($"Nothing changed for {editingTeam?.Name ?? teamName.Trim()}.");
                break;
            case HelpDeskAdminResult.Duplicate:
                teamNameError = "Another team already has that name.";
                await HelpDeskDisplay.FocusAsync(teamNameInput);

                return;
            case HelpDeskAdminResult.LimitReached:
                teamFormError.Set(string.Create(CultureInfo.InvariantCulture, $"There can be up to {HelpDeskState.MaxTeams} teams. Delete an empty team first."));

                return;
            case HelpDeskAdminResult.Invalid:
                teamFormError.Set("Check the name and description: one of them isn't valid.");

                return;
            case HelpDeskAdminResult.NotFound:
                teamFormError.Set("This team no longer exists. Cancel to see the current teams.");

                return;
            default:
                teamFormError.Set("Only admins can change teams. Sign in as an admin to make this change.");

                return;
        }

        teamSaved = true;
        teamSavedId = saved?.Id ?? editingTeam?.Id;

        await teamModal.HideAsync();
    }

    /// <summary>
    /// Why a team can't be deleted, or <see langword="null" /> when it can: the last team stays, and a team must have no agents
    /// (inactive ones included) and route no categories.
    /// </summary>
    private string? TeamBlocker(HelpDeskTeam team)
    {
        if (State.Teams.Count <= 1)
            return $"{team.Name} is the only team. Every agent and category needs a team, so add another team first.";

        var members = State.Roster.Count(agent => agent.TeamId == team.Id);
        var routed = CategoriesOf(team.Id).Count;

        if (members == 0 && routed == 0)
            return null;

        var uses = new List<string>();
        var steps = new List<string>();

        if (members > 0)
        {
            uses.Add($"has {Count(members, "agent", "agents")}");
            steps.Add("move its agents to another team (Edit on each agent; inactive agents count too)");
        }

        if (routed > 0)
        {
            uses.Add($"routes {Count(routed, "category", "categories")}");
            steps.Add("route its categories to another team on the Categories page");
        }

        return $"It still {string.Join(" and ", uses)}. A team must be empty before it's deleted: {string.Join(", and ", steps)}.";
    }

    private string TeamName(string? teamId) => State.FindTeam(teamId)?.Name ?? "No team";

    private string? TeamNameProblem(string value)
    {
        var name = value.Trim();

        if (name.Length == 0)
            return "Enter the team name.";

        if (name.Length > HelpDeskState.MaxGroupNameLength)
            return string.Create(CultureInfo.InvariantCulture, $"Use {HelpDeskState.MaxGroupNameLength} characters or fewer.");

        return State.Teams.Any(team => team.Id != editingTeam?.Id && string.Equals(team.Name, name, StringComparison.OrdinalIgnoreCase))
            ? "Another team already has that name."
            : null;
    }

    /// <summary>Deactivate asks first (it unassigns tickets); reactivate happens right away.</summary>
    private async Task ToggleActiveAsync(HelpDeskAgent agent)
    {
        agentError.Clear();

        if (agent.IsActive)
        {
            deactivating = agent;
            await deactivateModal.ShowAsync();

            return;
        }

        var result = State.SetAgentActive(agent.Id, true, out _);

        if (result == HelpDeskAdminResult.Ok)
            agentStatus.Set($"{agent.Name} is active again and can be assigned tickets.");
        else
            agentError.Set(ActiveProblem(agent, result, activating: true));
    }

    private string ActiveProblem(HelpDeskAgent agent, HelpDeskAdminResult result, bool activating) => result switch
    {
        HelpDeskAdminResult.NoChange => activating ? $"{agent.Name} is already active." : $"{agent.Name} is already inactive.",
        HelpDeskAdminResult.NotFound => $"{agent.Name} is no longer on the roster.",
        HelpDeskAdminResult.NotAllowed when agent.AccountId is not null => $"{agent.Name} signs in to the demo, so they always stay active.",
        _ => "Only admins can change the roster. Sign in as an admin to make this change.",
    };

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets whether the agent being edited signs in to the demo, so their admin flag comes from their account.</summary>
    private bool IsLinked => editingAgent?.AccountId is not null;

    [Inject] private HelpDeskState State { get; set; } = default!;

    #endregion

    /// <summary>A message for a live region; the same message again gets a counter, so a screen reader announces it again.</summary>
    private sealed class LiveMessage
    {
        #region Fields and Constants

        private int count;

        private string? last;

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>Removes the message.</summary>
        public void Clear()
        {
            count = 0;
            last = null;
            Text = null;
        }

        /// <summary>Shows a message.</summary>
        public void Set(string message)
        {
            count = message == last ? count + 1 : 1;
            last = message;
            Text = count == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({count})");
        }

        #endregion

        #endregion

        #region Properties, Indexers

        /// <summary>Gets the text to show.</summary>
        public string? Text { get; private set; }

        #endregion
    }
}
