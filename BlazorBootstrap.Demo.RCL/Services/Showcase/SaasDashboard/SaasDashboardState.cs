using BlazorBootstrap.Demo.RCL.Models.Showcase.SaasDashboard;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.SaasDashboard;

/// <summary>
/// In-memory state of the SaaS admin dashboard for the current session. Nothing is persisted or sent.
/// </summary>
public sealed class SaasDashboardState
{
    #region Fields and Constants

    private readonly List<SaasAccount> accounts = SaasDashboardSeedData.Accounts();

    private readonly List<SaasActivity> activities;

    private readonly List<SaasNotification> notifications = SaasDashboardSeedData.Notifications();

    private readonly decimal seedMrr;

    private SaasAnalyticsRange analyticsRange = SaasAnalyticsRange.Last30Days;

    private bool isDarkTheme;

    private int nextAccountId = 1001 + SaasDashboardSeedData.AccountCount;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Seeds a fresh session.</summary>
    public SaasDashboardState()
    {
        activities = SaasDashboardSeedData.Activities(accounts);
        seedMrr = Mrr;
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the history of an account, newest first.</summary>
    public IReadOnlyList<SaasActivity> ActivityFor(int accountId) =>
        activities.Where(activity => activity.AccountId == accountId).OrderByDescending(activity => activity.At).ToList();

    /// <summary>Adds an account in memory and returns it.</summary>
    public SaasAccount AddAccount(string name, string email, string company, SaasAccountRole role, string planId, int seats, SaasAccountStatus status)
    {
        var account = new SaasAccount(
            nextAccountId++, name.Trim(), email.Trim(), company.Trim(), role, planId, seats, status,
            "United States", SaasDashboardSeedData.Today, SaasDashboardSeedData.Today, ApiUsagePercent: 0, StorageUsagePercent: 0);

        accounts.Insert(0, account);
        Log(account.Id, "Account created", $"Added by you on the {PlanFor(account).Name} plan", IconName.Flag);
        Changed?.Invoke();

        return account;
    }

    /// <summary>Finds an account by its ID, or returns <see langword="null" />.</summary>
    public SaasAccount? FindAccount(int accountId) => accounts.FirstOrDefault(account => account.Id == accountId);

    /// <summary>Marks every notification as read.</summary>
    public void MarkNotificationsRead()
    {
        if (UnreadNotificationCount == 0)
            return;

        for (var index = 0; index < notifications.Count; index++)
            notifications[index] = notifications[index] with { IsRead = true };

        Changed?.Invoke();
    }

    /// <summary>Returns the monthly revenue of one account: seats times the plan price, and zero unless it is active.</summary>
    public decimal MonthlyRevenueFor(SaasAccount account) =>
        account.Status == SaasAccountStatus.Active ? account.Seats * PlanFor(account).PricePerSeat : 0m;

    /// <summary>Returns the plan of an account.</summary>
    public SaasPlan PlanFor(SaasAccount account) => Plans.First(plan => plan.Id == account.PlanId);

    /// <summary>Replaces the workspace settings.</summary>
    public void SaveSettings(SaasWorkspaceSettings settings)
    {
        Settings = settings with { WorkspaceName = settings.WorkspaceName.Trim(), SupportEmail = settings.SupportEmail.Trim() };
        Changed?.Invoke();
    }

    /// <summary>Records the color mode the theme switcher applied, so charts can redraw in matching colors.</summary>
    public void SetTheme(string? themeName)
    {
        var isDark = string.Equals(themeName, "dark", StringComparison.OrdinalIgnoreCase);

        if (isDarkTheme == isDark)
            return;

        isDarkTheme = isDark;
        ThemeChanged?.Invoke();
    }

    /// <summary>Replaces an account with an edited copy and records what changed.</summary>
    public void UpdateAccount(SaasAccount updated)
    {
        var index = accounts.FindIndex(account => account.Id == updated.Id);

        if (index < 0)
            return;

        var previous = accounts[index];
        updated = updated with { Name = updated.Name.Trim(), Email = updated.Email.Trim(), Company = updated.Company.Trim() };

        if (updated == previous)
            return;

        accounts[index] = updated;

        if (previous.Status != updated.Status)
            Log(updated.Id, updated.Status == SaasAccountStatus.Suspended ? "Account suspended" : $"Status changed to {updated.Status}", "Changed by you", IconName.ShieldExclamation);
        else if (previous.PlanId != updated.PlanId)
            Log(updated.Id, $"Plan changed to {PlanFor(updated).Name}", $"Was {PlanFor(previous).Name}", IconName.ArrowLeftRight);
        else
            Log(updated.Id, "Profile updated", "Edited by you", IconName.Pencil);

        Changed?.Invoke();
    }

    #endregion

    #region Private Methods

    /// <summary>Records a change one minute after the newest entry, so changes made in this session always list first.</summary>
    private void Log(int accountId, string title, string detail, IconName iconName) =>
        activities.Add(new(accountId, activities.Max(activity => activity.At).AddMinutes(1), title, detail, iconName));

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets every customer account, newest additions first.</summary>
    public IReadOnlyList<SaasAccount> Accounts => accounts;

    /// <summary>Gets the number of accounts in good standing.</summary>
    public int ActiveCount => accounts.Count(account => account.Status == SaasAccountStatus.Active);

    /// <summary>Gets or sets the period shown on the analytics screen.</summary>
    public SaasAnalyticsRange AnalyticsRange
    {
        get => analyticsRange;
        set
        {
            if (analyticsRange == value)
                return;

            analyticsRange = value;
            Changed?.Invoke();
        }
    }

    /// <summary>Gets the last six invoices of the workspace.</summary>
    public IReadOnlyList<SaasInvoice> Invoices { get; } = SaasDashboardSeedData.Invoices();

    /// <summary>Gets whether the dark color mode is applied.</summary>
    public bool IsDarkTheme => isDarkTheme;

    /// <summary>Gets the monthly recurring revenue of all active accounts.</summary>
    public decimal Mrr => accounts.Sum(MonthlyRevenueFor);

    /// <summary>Gets the change of <see cref="Mrr" /> against last month, in percent.</summary>
    public double MrrGrowthPercent
    {
        get
        {
            var history = RevenueHistory;
            var lastMonth = history[^2];

            return lastMonth == 0 ? 0 : (history[^1] - lastMonth) / lastMonth * 100;
        }
    }

    /// <summary>Gets the header notifications, newest first.</summary>
    public IReadOnlyList<SaasNotification> Notifications => notifications;

    /// <summary>Gets the seats on paid plans across active accounts.</summary>
    public int PaidSeats => accounts.Where(account => MonthlyRevenueFor(account) > 0).Sum(account => account.Seats);

    /// <summary>Gets the subscription plans.</summary>
    public IReadOnlyList<SaasPlan> Plans { get; } = SaasDashboardSeedData.Plans();

    /// <summary>Gets the newest history entries across all accounts.</summary>
    public IReadOnlyList<SaasActivity> RecentActivity => activities.OrderByDescending(activity => activity.At).Take(6).ToList();

    /// <summary>Gets monthly recurring revenue for the last 12 months; the last value is the live <see cref="Mrr" />.</summary>
    public IReadOnlyList<double> RevenueHistory => SaasDashboardSeedData.RevenueHistory(seedMrr, Mrr);

    /// <summary>Gets the revenue target for the last 12 months.</summary>
    public IReadOnlyList<double> RevenueTargets => SaasDashboardSeedData.RevenueTargets(seedMrr);

    /// <summary>Gets the workspace settings.</summary>
    public SaasWorkspaceSettings Settings { get; private set; } = SaasDashboardSeedData.Settings();

    /// <summary>Gets the number of suspended accounts.</summary>
    public int SuspendedCount => accounts.Count(account => account.Status == SaasAccountStatus.Suspended);

    /// <summary>Gets the number of accounts in a free trial.</summary>
    public int TrialCount => accounts.Count(account => account.Status == SaasAccountStatus.Trial);

    /// <summary>Gets the number of unread notifications.</summary>
    public int UnreadNotificationCount => notifications.Count(notification => !notification.IsRead);

    #endregion

    #region Events

    /// <summary>Raised when accounts, notifications, settings, or the analytics period change.</summary>
    public event Action? Changed;

    /// <summary>Raised when the applied color mode changes.</summary>
    public event Action? ThemeChanged;

    #endregion
}
