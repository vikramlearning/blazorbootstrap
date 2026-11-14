using BlazorBootstrap.Demo.RCL.Models.Showcase.SaasDashboard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.SaasDashboard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.SaasDashboard;

/// <summary>
/// User detail screen: profile, plan usage, and activity of one account, with edit and suspend actions.
/// An unknown ID renders the site's not-found screen.
/// </summary>
public partial class UserDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button? editButton;

    private SaasAccountEditor? editor;

    private string? savedMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet() => savedMessage = null;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private Task EditAsync() => Account is null || editor is null ? Task.CompletedTask : editor.EditAsync(Account, () => editButton?.Element);

    private void OnSaved(SaasAccountSavedEventArgs args) => savedMessage = $"Saved the changes to {args.Account.Name}.";

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void SetStatus(SaasAccountStatus status)
    {
        if (Account is null)
            return;

        State.UpdateAccount(Account with { Status = status });
        savedMessage = status == SaasAccountStatus.Suspended
            ? $"{Account.Name} is suspended. They can't sign in until you reactivate the account."
            : $"{Account.Name} is active again.";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private SaasAccount? Account => State.FindAccount(Id);

    /// <summary>Gets or sets the account ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Overview", Href = SaasDashboardLayout.Base },
        new() { Text = "Users", Href = $"{SaasDashboardLayout.Base}/users" },
        new() { Text = Account?.Name, IsCurrentPage = true },
    };

    private bool IsSuspended => Account?.Status == SaasAccountStatus.Suspended;

    private SaasPlan Plan => State.PlanFor(Account!);

    [Inject] private SaasDashboardState State { get; set; } = default!;

    #endregion
}
