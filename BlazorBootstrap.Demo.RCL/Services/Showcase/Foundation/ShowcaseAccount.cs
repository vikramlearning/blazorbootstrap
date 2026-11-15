namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>A role a site's accounts can have, such as "customer" or "retailer".</summary>
/// <param name="Id">The role ID used in code and in snapshots.</param>
/// <param name="Label">The role name shown to people.</param>
/// <param name="HomePath">Where the role lands after signing in, as a path inside the site.</param>
/// <param name="AccountPath">The role's account page, as a path inside the site.</param>
/// <param name="Description">One sentence on what the role can do, shown on the sign-in page.</param>
public sealed record ShowcaseRole(string Id, string Label, string HomePath, string AccountPath, string Description);

/// <summary>A seeded demo account whose email and password the sign-in page shows.</summary>
public sealed record ShowcaseDemoAccount(string Id, string RoleId, string Name, string Email, string Password, string Initials);

/// <summary>
/// An account of a site, held in the tab only. The password is kept as a salted SHA-256 hash so that text a person types
/// is never stored as is; this is a demo safeguard, not a production password scheme.
/// </summary>
public sealed record ShowcaseAccount(string Id, string RoleId, string Name, string Email, string Initials, string PasswordSalt, string PasswordHash, bool IsSeeded);

/// <summary>The outcome of registering an account or editing a profile.</summary>
public enum ShowcaseAccountResult
{
    /// <summary>The change was applied.</summary>
    Success,

    /// <summary>Another account already uses the email address.</summary>
    EmailInUse,

    /// <summary>The current password did not match.</summary>
    WrongPassword,

    /// <summary>No account is signed in, or the role does not allow it.</summary>
    NotAllowed,
}

/// <summary>The identity part of a stored snapshot.</summary>
public sealed class ShowcaseIdentitySnapshot
{
    #region Properties, Indexers

    /// <summary>Gets or sets every account of the site, seeded and registered.</summary>
    public List<ShowcaseAccount>? Accounts { get; set; }

    /// <summary>Gets or sets the signed-in account ID, or <see langword="null" /> when signed out.</summary>
    public string? CurrentId { get; set; }

    #endregion
}
