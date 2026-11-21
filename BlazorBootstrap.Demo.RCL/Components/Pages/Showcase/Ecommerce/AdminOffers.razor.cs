using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's offers and coupons: a year of coupons in a <see cref="Grid{TItem}" /> (code, description, discount,
/// minimum order, start and end dates, uses, rules, and status in words: active, scheduled, paused, expired, or used up)
/// with paging, sorting, the column chooser, and CSV export, searched by code or description and filtered by status; its
/// view is saved for the visit and can be reset. A coupon has no page of its own: the row's stretched button opens its
/// editor (as does <c>?code=</c>, from the header's search), and each row can be paused, resumed, or deleted after a
/// confirmation. The editor validates with one behavior (rule WR-002): a unique code checked while typing, a percentage
/// from 1 to 90 or an amount above 0, a minimum of 0 or more, a start date on or before the end date, and a usage limit
/// not below the uses so far.
/// </summary>
public partial class AdminOffers : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private readonly Dictionary<string, Button> deleteButtons = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, ElementReference> editButtons = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The <c>?code=</c> value last applied, so a re-render doesn't open the editor again.</summary>
    private string? appliedCodeQuery;

    private ShowcaseUniqueCheck? codeCheck;

    private InputText? codeInput;

    private ElementReference? countRegion;

    private bool deleted;

    private Modal deleteModal = default!;

    private string? deletingCode;

    private EditContext editContext = default!;

    private string? editingCode;

    private Modal editorModal = default!;

    private CouponForm form = new();

    private Grid<CouponRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private ValidationMessageStore messageStore = default!;

    private Button newButton = default!;

    /// <summary>The coupon <c>?code=</c> asks to open once the page is interactive.</summary>
    private string? openCode;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>The code of the coupon just saved, whose row focus moves to; empty after creating, so focus goes to the status.</summary>
    private string? savedCode;

    private bool saved;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    private int statusCount;

    private string statusFilter = string.Empty;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive, then shows the grid; opens the coupon <c>?code=</c> names.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (openCode is { } code && editorModal is not null)
        {
            openCode = null;

            if (State.FindCoupon(code) is { } coupon)
                await OpenEditorAsync(coupon);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "offers", DefaultPageSize, "starts", SortDirection.Descending);
        ResetForm(new CouponForm());
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        var query = string.IsNullOrWhiteSpace(CodeQuery) ? null : CodeQuery.Trim();

        if (string.Equals(query, appliedCodeQuery, StringComparison.OrdinalIgnoreCase))
            return;

        appliedCodeQuery = query;
        openCode = query is { Length: <= 20 } ? query : null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

    #endregion

    #region Private Methods

    private static string CodeTakenMessage(string code) => $"{code.Trim().ToUpperInvariant()} is already another coupon's code. Choose a code no other coupon uses.";

    private static string DiscountText(EcommerceCoupon coupon) => coupon.Kind == EcommerceCouponKind.Percent
        ? $"{coupon.Value.ToString("0.##", CultureInfo.InvariantCulture)}% off"
        : $"{EcommerceDisplay.Money(coupon.Value)} off";

    private static BadgeColor StatusColor(CouponStatus status) => status switch
    {
        CouponStatus.Active => BadgeColor.Success,
        CouponStatus.Scheduled => BadgeColor.Info,
        CouponStatus.Paused => BadgeColor.Secondary,
        CouponStatus.Expired => BadgeColor.Danger,
        _ => BadgeColor.Warning,
    };

    private static string StatusLabel(CouponStatus status) => status switch
    {
        CouponStatus.Active => "Active",
        CouponStatus.Scheduled => "Scheduled",
        CouponStatus.Paused => "Paused",
        CouponStatus.Expired => "Expired",
        _ => "Used up",
    };

    private async Task AskDeleteAsync(string code)
    {
        deletingCode = code;
        deleted = false;

        await deleteModal.ShowAsync();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = statusFilter = string.Empty;
        await RefreshAsync();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    private async Task ConfirmDeleteAsync()
    {
        if (deletingCode is not null && State.DeleteCoupon(deletingCode))
        {
            deleted = true;
            SetStatus($"Coupon {deletingCode} is deleted. Past orders keep the code they used.");
        }

        await deleteModal.HideAsync();
    }

    private Task EditAsync(string code) => State.FindCoupon(code) is { } coupon ? OpenEditorAsync(coupon) : Task.CompletedTask;

    private Task<bool> IsCodeTakenAsync(string code) => Task.FromResult(State.IsCouponCodeTaken(code, editingCode));

    private IEnumerable<EcommerceCoupon> Matching()
    {
        IEnumerable<EcommerceCoupon> coupons = State.Coupons;
        var term = search.Trim();

        if (term.Length > 0)
            coupons = coupons.Where(coupon => coupon.Code.Contains(term, StringComparison.OrdinalIgnoreCase) || coupon.Description.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (Enum.TryParse<CouponStatus>(statusFilter, out var status) && Enum.IsDefined(status))
            coupons = coupons.Where(coupon => StatusOf(coupon) == status);

        return coupons;
    }

    private Task NewAsync() => OpenEditorAsync(null);

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: after a delete the row is gone, so focus goes to the status.</summary>
    private async Task OnDeleteHiddenAsync()
    {
        var code = deletingCode;
        deletingCode = null;

        if (deleted)
        {
            deleted = false;
            await EcommerceDisplay.FocusAsync(statusRegion);
        }
        else if (code is not null && deleteButtons.TryGetValue(code, out var button))
        {
            await EcommerceDisplay.FocusAsync(button.Element);
        }
    }

    /// <summary>
    /// Returns focus after the editor closes: to the edited row's code button (its code may have changed), to the status
    /// after creating a coupon, or to the button that opened the dialog when it was cancelled.
    /// </summary>
    private async Task OnEditorHiddenAsync()
    {
        var original = editingCode;
        var wasSaved = saved;
        var code = savedCode;

        editingCode = null;
        saved = false;
        savedCode = null;
        ResetForm(new CouponForm());

        if (wasSaved && original is null)
            await EcommerceDisplay.FocusAsync(statusRegion);
        else if ((wasSaved ? code : original) is string target && editButtons.TryGetValue(target, out var editButton))
            await EcommerceDisplay.FocusAsync(editButton);
        else if (original is null)
            await EcommerceDisplay.FocusAsync(newButton.Element);
        else
            await EcommerceDisplay.FocusAsync(statusRegion);
    }

    private Task OnEditorShownAsync() => EcommerceDisplay.FocusAsync(codeInput?.Element);

    /// <summary>A refusal from the state clears once its field changes; a new code is shown in capitals as it is saved.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        messageStore.Clear(args.FieldIdentifier);

        if (args.FieldIdentifier.FieldName == nameof(CouponForm.Code) && form.Code != form.Code.ToUpperInvariant())
            form.Code = form.Code.ToUpperInvariant();
    }

    private async Task OnSearchChanged(string value)
    {
        searchBoxValue = value;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    /// <summary>Filters on every keystroke without changing the box's bound value, so older text is never written back into it.</summary>
    private async Task OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusFilterChanged(ChangeEventArgs args)
    {
        statusFilter = Enum.TryParse<CouponStatus>(args.Value?.ToString(), out var status) && Enum.IsDefined(status) ? status.ToString() : string.Empty;
        await RefreshAsync();
    }

    private async Task OpenEditorAsync(EcommerceCoupon? coupon)
    {
        editingCode = coupon?.Code;
        saved = false;
        savedCode = null;
        ResetForm(coupon is null
            ? new CouponForm { Today = State.Clock.Today }
            : new CouponForm
            {
                Code = coupon.Code,
                Description = coupon.Description,
                Kind = coupon.Kind,
                Value = coupon.Value,
                MinSubtotal = coupon.MinSubtotal,
                FirstOrderOnly = coupon.FirstOrderOnly,
                StartsOn = coupon.StartsOn?.Date,
                ExpiresOn = coupon.ExpiresOn?.Date,
                UsageLimit = coupon.UsageLimit,
                IsActive = coupon.IsActive,
                OriginalExpiresOn = coupon.ExpiresOn?.Date,
                UsesSoFar = coupon.Uses,
                Today = State.Clock.Today,
            });

        await editorModal.ShowAsync();
    }

    /// <summary>The page of coupons the grid asked for: the newest start date first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<CouponRow>> ProvideCouponsAsync(GridDataProviderRequest<CouponRow> request) =>
        view.ApplyAsync(request, Matching().Select(coupon =>
        {
            var status = StatusOf(coupon);

            return new CouponRow(
                coupon.Code,
                coupon.Description,
                DiscountText(coupon),
                coupon.Kind == EcommerceCouponKind.Percent ? coupon.Value : 1000 + coupon.Value,
                coupon.MinSubtotal,
                coupon.StartsOn,
                coupon.ExpiresOn,
                coupon.Uses,
                coupon.UsageLimit is { } limit ? $"{coupon.Uses} of {limit}" : coupon.Uses.ToString(CultureInfo.InvariantCulture),
                coupon.FirstOrderOnly ? "First order only" : string.Empty,
                StatusLabel(status),
                status,
                coupon.IsActive);
        }).ToList());

    /// <summary>A filter changed: back to the first page of the filtered coupons.</summary>
    private async Task RefreshAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();

        StateHasChanged();
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private void ResetForm(CouponForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        editContext.OnFieldChanged += OnFieldChanged;
        messageStore = new ValidationMessageStore(editContext);
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        SetStatus("The view is reset: the newest start dates first, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    /// <summary>
    /// Saves once the form is valid: the code check finishes first, then every rule runs, and a failed submit shows the
    /// summary (rule WR-002). The state checks again; a refusal shows on the field it is about.
    /// </summary>
    private async Task SaveAsync()
    {
        messageStore.Clear();
        editContext.NotifyValidationStateChanged();

        var codeFree = codeCheck is null || await codeCheck.CheckNowAsync();

        if (!editContext.Validate() || !codeFree)
            return;

        var coupon = new EcommerceCoupon(
            form.Code,
            form.Description,
            form.Kind,
            Math.Round(form.Value ?? 0m, 2),
            Math.Round(form.MinSubtotal ?? 0m, 2),
            form.FirstOrderOnly,
            form.ExpiresOn?.Date,
            form.IsActive,
            form.UsageLimit,
            Uses: 0,
            StartsOn: form.StartsOn?.Date);

        if (!State.SaveCoupon(coupon, editingCode))
        {
            var (field, message) = State.IsCouponCodeTaken(form.Code, editingCode) ? (FieldIdentifier.Create(() => form.Code), CodeTakenMessage(form.Code))
                : editingCode is not null && State.FindCoupon(editingCode) is null ? (FieldIdentifier.Create(() => form.Code), "This coupon was deleted meanwhile, so it can't be saved. Cancel and create it again.")
                : (FieldIdentifier.Create(() => form.UsageLimit), "The coupon couldn't be saved with these values. Check the usage limit against its uses so far.");
            messageStore.Add(field, message);
            editContext.NotifyValidationStateChanged();
            await EcommerceDisplay.FocusAsync(codeInput?.Element);

            return;
        }

        var code = form.Code.ToUpperInvariant();
        saved = true;
        savedCode = code;
        SetStatus(editingCode is null
            ? $"Coupon {code} is created{(form.IsActive ? "." : " and paused.")}"
            : $"Coupon {code} is saved.");

        await editorModal.HideAsync();
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    private CouponStatus StatusOf(EcommerceCoupon coupon)
    {
        if (!coupon.IsActive)
            return CouponStatus.Paused;

        if (coupon.ExpiresOn is { } expires && State.Clock.Today > expires.Date)
            return CouponStatus.Expired;

        if (coupon.UsageLimit is { } limit && coupon.Uses >= limit)
            return CouponStatus.UsedUp;

        if (coupon.StartsOn is { } starts && State.Clock.Today < starts.Date)
            return CouponStatus.Scheduled;

        return CouponStatus.Active;
    }

    /// <summary>Pauses or resumes a coupon; the same button stays in place with its new label, so focus stays on it.</summary>
    private void ToggleActive(string code)
    {
        if (State.FindCoupon(code) is not { } coupon)
            return;

        var active = !coupon.IsActive;

        if (State.SetCouponActive(coupon.Code, active))
            SetStatus(active ? $"Coupon {coupon.Code} is resumed." : $"Coupon {coupon.Code} is paused. Shoppers can't use it until you resume it.");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the code of the coupon to open in the editor, from <c>?code=</c> (the header's search).</summary>
    [SupplyParameterFromQuery(Name = "code")] public string? CodeQuery { get; set; }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0;

    private string LeadText
    {
        get
        {
            var total = State.Coupons.Count;
            var active = State.Coupons.Count(coupon => StatusOf(coupon) == CouponStatus.Active);
            var scheduled = State.Coupons.Count(coupon => StatusOf(coupon) == CouponStatus.Scheduled);
            var uses = State.Coupons.Sum(coupon => coupon.Uses);

            return $"{EcommerceDisplay.Plural(total, "coupon")} · {active} usable at checkout today · {scheduled} scheduled · {EcommerceDisplay.Count(uses)} uses in all";
        }
    }

    private string MatchText
    {
        get
        {
            var count = Matching().Count();

            return count == State.Coupons.Count ? $"Showing all {EcommerceDisplay.Plural(count, "coupon")}." : $"{count} of {EcommerceDisplay.Plural(State.Coupons.Count, "coupon")} match.";
        }
    }

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Where a coupon stands today, in words.</summary>
    private enum CouponStatus
    {
        Active,
        Scheduled,
        Paused,
        Expired,
        UsedUp,
    }

    /// <summary>One coupon as the grid shows and exports it: every exported column is a property.</summary>
    private sealed record CouponRow(
        string Code,
        string Description,
        string Discount,
        decimal DiscountOrder,
        decimal Minimum,
        DateTime? StartsOn,
        DateTime? ExpiresOn,
        int Uses,
        string UsesText,
        string Rules,
        string Status,
        CouponStatus StatusValue,
        bool IsActive)
    {
        /// <summary>Gets the end date as it is exported, like "2026-11-30", or empty.</summary>
        public string EndsText => ExpiresOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

        /// <summary>Gets the start date as it is exported, like "2026-11-27", or empty.</summary>
        public string StartsText => StartsOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>
    /// The coupon editor's fields, with the limits of <see cref="EcommerceState.SaveCoupon" />: a code's form, a description,
    /// and the cross-field rules (the value's range follows the discount type, the start date is on or before the end date,
    /// a past end date is kept only when unchanged, and the usage limit isn't below the uses so far).
    /// </summary>
    private sealed class CouponForm : IValidatableObject
    {
        private string code = string.Empty;

        private string description = string.Empty;

        [Required(ErrorMessage = "Enter a code of 3 to 20 letters and digits.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Use 3 to 20 characters for the code.")]
        [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Use letters and digits only in the code, with no spaces.")]
        public string Code { get => code; set => code = value?.Trim() ?? string.Empty; }

        [Required(ErrorMessage = "Describe the offer, like “15% off the autumn edit”.")]
        [StringLength(120, ErrorMessage = "Use 120 characters or fewer for the description.")]
        public string Description { get => description; set => description = value?.Trim() ?? string.Empty; }

        public DateTime? ExpiresOn { get; set; }

        public bool FirstOrderOnly { get; set; }

        public bool IsActive { get; set; } = true;

        public EcommerceCouponKind Kind { get; set; } = EcommerceCouponKind.Percent;

        [Required(ErrorMessage = "Enter the minimum order, or 0 for any order.")]
        [Range(0d, 1_000_000d, ErrorMessage = "Enter a minimum order from $0.00 to $1,000,000.00.")]
        public decimal? MinSubtotal { get; set; } = 0m;

        /// <summary>The end date the coupon had when the editor opened; a past one may stay as it is.</summary>
        public DateTime? OriginalExpiresOn { get; init; }

        public DateTime? StartsOn { get; set; }

        /// <summary>The site's today, for the date rules.</summary>
        public DateTime Today { get; init; }

        [Range(1, 1_000_000, ErrorMessage = "Enter a usage limit of at least 1, or leave it empty for unlimited uses.")]
        public int? UsageLimit { get; set; }

        /// <summary>How many orders used the coupon so far (0 for a new one).</summary>
        public int UsesSoFar { get; init; }

        [Required(ErrorMessage = "Enter how much the coupon takes off.")]
        public decimal? Value { get; set; } = 10m;

        /// <summary>Runs once the fields are filled in: the rules that depend on more than one field or on today.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Value is { } value)
            {
                var rounded = Math.Round(value, 2);

                if (Kind == EcommerceCouponKind.Percent && (rounded < 1 || rounded > 90))
                    yield return new ValidationResult("Enter a percentage from 1 to 90.", new[] { nameof(Value) });
                else if (Kind == EcommerceCouponKind.Amount && (rounded < 0.01m || rounded > 10_000m))
                    yield return new ValidationResult("Enter an amount from $0.01 to $10,000.00.", new[] { nameof(Value) });
            }

            if (StartsOn is { } starts && (starts.Date < Today.AddYears(-1) || starts.Date > Today.AddYears(2)))
                yield return new ValidationResult("Choose a start date within a year before today and two years after it.", new[] { nameof(StartsOn) });

            if (ExpiresOn is { } expires)
            {
                if (expires.Date < Today && expires.Date != OriginalExpiresOn)
                    yield return new ValidationResult("Choose today or a later end date.", new[] { nameof(ExpiresOn) });
                else if (expires.Date > Today.AddYears(2))
                    yield return new ValidationResult("Choose an end date within the next two years.", new[] { nameof(ExpiresOn) });
                else if (StartsOn is { } from && expires.Date < from.Date)
                    yield return new ValidationResult($"The end date can't be before the start date ({from.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}).", new[] { nameof(ExpiresOn) });
            }

            if (UsageLimit is { } limit && limit < UsesSoFar)
                yield return new ValidationResult($"This coupon was used {UsesSoFar} times already, so the limit can't be lower than {UsesSoFar}.", new[] { nameof(UsageLimit) });
        }
    }

    #endregion
}
