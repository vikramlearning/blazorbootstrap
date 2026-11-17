using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// A person's directory profile (everyone signed in): name, pronouns, title, department, location, work email
/// (<c>mailto:</c>), phone (<c>tel:</c>), start date and time here, the bio as plain text, the manager and direct reports as
/// links, and a link to their place in the org chart (<c>/org-chart?focus={id}</c>). On your own profile, a link to edit it.
/// Someone unknown, or someone who left (for anyone but HR), shows the site's not-found screen; HR sees a left person's page
/// with when they left.
/// </summary>
public partial class Person : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string FirstName(string name) => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? name;

    /// <summary>Keeps the digits and a leading plus of a phone number for a <c>tel:</c> link.</summary>
    private static string TelNumber(string phone)
    {
        var digits = new string(phone.Where(char.IsAsciiDigit).ToArray());

        return phone.TrimStart().StartsWith('+') ? "+" + digits : digits;
    }

    /// <summary>Says how long someone has worked here (until their last day, for someone who left).</summary>
    private static string TenureText(HrEmployee person, DateTime today) =>
        person.StartDate > today ? "Hasn't started yet"
        : person.EndDate is { } end ? $"{HrDisplay.Tenure(person.StartDate, end)} (until {HrDisplay.Date(end)})"
        : HrDisplay.Tenure(person.StartDate, today);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the employee ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private HrState State { get; set; } = default!;

    #endregion
}
