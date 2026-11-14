using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// The demo sign-in of one site: its roles, its seeded demo accounts, accounts registered in this tab, and who is signed
/// in. Nothing authenticates against a server; accounts live only in the tab's state and snapshot.
/// </summary>
public sealed partial class ShowcaseIdentity
{
    #region Fields and Constants

    /// <summary>The longest name a profile may have.</summary>
    public const int MaxNameLength = 80;

    /// <summary>The longest email address a profile may have.</summary>
    public const int MaxEmailLength = 120;

    /// <summary>The shortest password Register and Change password accept.</summary>
    public const int MinPasswordLength = 8;

    /// <summary>The most accounts a site keeps, including the seeded ones.</summary>
    public const int MaxAccounts = 200;

    private readonly List<ShowcaseAccount> accounts;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Creates the identity of a site with its roles and seeded demo accounts, signed out.</summary>
    public ShowcaseIdentity(IReadOnlyList<ShowcaseRole> roles, IReadOnlyList<ShowcaseDemoAccount> demoAccounts)
    {
        Roles = roles;
        DemoAccounts = demoAccounts;
        accounts = demoAccounts.Select(demo =>
        {
            // A fixed salt per seeded account keeps a fresh load deterministic; registered accounts get a random one.
            var salt = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"seed:{demo.Id}")))[..32];

            return new ShowcaseAccount(demo.Id, demo.RoleId, demo.Name, demo.Email, demo.Initials, salt, Hash(salt, demo.Password), IsSeeded: true);
        }).ToList();
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Applies a snapshot that <see cref="TryValidate" /> accepted.</summary>
    public void Apply(ShowcaseIdentitySnapshot validated)
    {
        accounts.Clear();
        accounts.AddRange(validated.Accounts!);
        Current = FindAccount(validated.CurrentId);
    }

    /// <summary>Returns the snapshot of the accounts and the signed-in account.</summary>
    public ShowcaseIdentitySnapshot Capture() => new() { Accounts = accounts.ToList(), CurrentId = Current?.Id };

    /// <summary>
    /// Changes the signed-in account's password after checking the current one. Seeded demo accounts keep the password the
    /// sign-in page lists.
    /// </summary>
    public ShowcaseAccountResult ChangePassword(string currentPassword, string newPassword)
    {
        if (Current is not { IsSeeded: false } account)
            return ShowcaseAccountResult.NotAllowed;

        if (!Matches(account, currentPassword))
            return ShowcaseAccountResult.WrongPassword;

        var salt = NewSalt();
        Replace(account with { PasswordSalt = salt, PasswordHash = Hash(salt, newPassword) });

        return ShowcaseAccountResult.Success;
    }

    /// <summary>Finds an account by its ID, or returns <see langword="null" />.</summary>
    public ShowcaseAccount? FindAccount(string? accountId) => accounts.FirstOrDefault(account => account.Id == accountId);

    /// <summary>Finds a role by its ID, or returns <see langword="null" />.</summary>
    public ShowcaseRole? FindRole(string? roleId) => Roles.FirstOrDefault(role => role.Id == roleId);

    /// <summary>Returns whether the signed-in account has one of the roles.</summary>
    public bool IsInRole(IEnumerable<string> roleIds) => Current is { } account && roleIds.Contains(account.RoleId);

    /// <summary>Creates an account in a role and signs it in.</summary>
    public ShowcaseAccountResult Register(string roleId, string name, string email, string password, out ShowcaseAccount? account)
    {
        account = null;

        if (FindRole(roleId) is null || accounts.Count >= MaxAccounts)
            return ShowcaseAccountResult.NotAllowed;

        if (IsEmailTaken(email, exceptId: null))
            return ShowcaseAccountResult.EmailInUse;

        var number = accounts.Select(existing => RegisteredIdPattern().Match(existing.Id)).Where(match => match.Success).Select(match => int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)).DefaultIfEmpty(0).Max() + 1;
        var salt = NewSalt();
        account = new ShowcaseAccount($"u-{number}", roleId, name.Trim(), email.Trim(), InitialsOf(name), salt, Hash(salt, password), IsSeeded: false);
        accounts.Add(account);
        Current = account;
        Changed?.Invoke();

        return ShowcaseAccountResult.Success;
    }

    /// <summary>Signs in with an email address and password; returns the account, or <see langword="null" /> when they don't match.</summary>
    public ShowcaseAccount? SignIn(string? email, string? password)
    {
        var account = accounts.FirstOrDefault(candidate => string.Equals(candidate.Email, email?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (account is null || !Matches(account, password ?? string.Empty))
            return null;

        Current = account;
        Changed?.Invoke();

        return account;
    }

    /// <summary>Signs in a seeded demo account directly (the sign-in page's one-click buttons).</summary>
    public ShowcaseAccount? SignInAs(string accountId)
    {
        if (FindAccount(accountId) is not { IsSeeded: true } account)
            return null;

        Current = account;
        Changed?.Invoke();

        return account;
    }

    /// <summary>Signs out.</summary>
    public void SignOut()
    {
        if (Current is null)
            return;

        Current = null;
        Changed?.Invoke();
    }

    /// <summary>
    /// Validates a snapshot read back from storage without applying it: every seeded account present, unique IDs and
    /// emails, known roles, bounded text, well-formed salt and hash, and a signed-in ID that exists.
    /// </summary>
    public bool TryValidate(ShowcaseIdentitySnapshot? snapshot, out ShowcaseIdentitySnapshot validated)
    {
        validated = new();

        if (snapshot?.Accounts is not { Count: > 0 and <= MaxAccounts } list)
            return false;

        var ids = new HashSet<string>(StringComparer.Ordinal);
        var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var account in list)
        {
            if (account is null
                || account.Id is null || !AccountIdPattern().IsMatch(account.Id) || !ids.Add(account.Id)
                || FindRole(account.RoleId) is null
                || string.IsNullOrWhiteSpace(account.Name) || account.Name.Length > MaxNameLength
                || string.IsNullOrWhiteSpace(account.Email) || account.Email.Length > MaxEmailLength || !account.Email.Contains('@') || !emails.Add(account.Email)
                || account.Initials is null || account.Initials.Length is < 1 or > 3
                || account.PasswordSalt is null || !HexPattern().IsMatch(account.PasswordSalt) || account.PasswordSalt.Length != 32
                || account.PasswordHash is null || !HexPattern().IsMatch(account.PasswordHash) || account.PasswordHash.Length != 64
                || account.IsSeeded != DemoAccounts.Any(demo => demo.Id == account.Id))
                return false;
        }

        // Seeded accounts keep the email the sign-in page lists.
        if (DemoAccounts.Any(demo => list.FirstOrDefault(account => account.Id == demo.Id) is not { } seeded || !string.Equals(seeded.Email, demo.Email, StringComparison.OrdinalIgnoreCase)))
            return false;

        if (DemoAccounts.Any(demo => !ids.Contains(demo.Id)) || (snapshot.CurrentId is not null && !ids.Contains(snapshot.CurrentId)))
            return false;

        validated = new() { Accounts = list.ToList(), CurrentId = snapshot.CurrentId };

        return true;
    }

    /// <summary>Updates the signed-in account's name and email address. Seeded demo accounts keep the email the sign-in page lists.</summary>
    public ShowcaseAccountResult UpdateProfile(string name, string email)
    {
        if (Current is not { } account || (account.IsSeeded && !string.Equals(account.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)))
            return ShowcaseAccountResult.NotAllowed;

        if (IsEmailTaken(email, account.Id))
            return ShowcaseAccountResult.EmailInUse;

        Replace(account with { Name = name.Trim(), Email = email.Trim(), Initials = InitialsOf(name) });

        return ShowcaseAccountResult.Success;
    }

    #endregion

    #region Private Methods

    [GeneratedRegex("^(?:[a-z0-9]+(?:-[a-z0-9]+)*)$")]
    private static partial Regex AccountIdPattern();

    private static string Hash(string salt, string password) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{password}")));

    [GeneratedRegex("^[0-9A-F]+$")]
    private static partial Regex HexPattern();

    private static string InitialsOf(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..1].ToUpperInvariant(),
            _ => string.Concat(parts[0][..1], parts[^1][..1]).ToUpperInvariant(),
        };
    }

    private bool IsEmailTaken(string email, string? exceptId) =>
        accounts.Any(account => account.Id != exceptId && string.Equals(account.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));

    private static bool Matches(ShowcaseAccount account, string password) =>
        CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(account.PasswordHash), Encoding.ASCII.GetBytes(Hash(account.PasswordSalt, password)));

    private static string NewSalt() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

    [GeneratedRegex(@"^u-(\d{1,6})$")]
    private static partial Regex RegisteredIdPattern();

    private void Replace(ShowcaseAccount updated)
    {
        var index = accounts.FindIndex(account => account.Id == updated.Id);
        accounts[index] = updated;

        if (Current?.Id == updated.Id)
            Current = updated;

        Changed?.Invoke();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets every account of the site, seeded and registered.</summary>
    public IReadOnlyList<ShowcaseAccount> Accounts => accounts;

    /// <summary>Gets the signed-in account, or <see langword="null" /> when signed out.</summary>
    public ShowcaseAccount? Current { get; private set; }

    /// <summary>Gets the seeded demo accounts the sign-in page lists.</summary>
    public IReadOnlyList<ShowcaseDemoAccount> DemoAccounts { get; }

    /// <summary>Gets the role of the signed-in account, or <see langword="null" />.</summary>
    public ShowcaseRole? CurrentRole => FindRole(Current?.RoleId);

    /// <summary>Gets whether an account is signed in.</summary>
    public bool IsSignedIn => Current is not null;

    /// <summary>Gets the site's roles.</summary>
    public IReadOnlyList<ShowcaseRole> Roles { get; }

    #endregion

    #region Events

    /// <summary>Raised when someone signs in or out, registers, or edits an account.</summary>
    public event Action? Changed;

    #endregion
}
