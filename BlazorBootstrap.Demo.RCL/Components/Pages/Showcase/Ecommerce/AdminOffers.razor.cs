using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's offers and coupons: every coupon with its discount, rules, and status in words (active, paused,
/// expired, or used up), with pause and resume, a dialog to create or edit a coupon, and a confirmed delete. A duplicate
/// code is reported on the code field, and focus returns to the button that opened a dialog.
/// </summary>
public partial class AdminOffers : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<string, Button> deleteButtons = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, Button> editButtons = new(StringComparer.OrdinalIgnoreCase);

    private TextInput codeInput = default!;

    private bool deleted;

    private Modal deleteModal = default!;

    private string? deletingCode;

    private EditContext editContext = default!;

    private string? editingCode;

    private DateTime? editingExpiresOn;

    private Modal editorModal = default!;

    private CouponForm form = new();

    private ValidationMessageStore messageStore = default!;

    private Button newButton = default!;

    /// <summary>The code of the coupon just saved, whose row focus moves to; empty after creating, so focus goes to the status.</summary>
    private string? savedCode;

    private bool saved;

    private int statusCount;

    private string? statusMessage;

    private string? statusText;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        ResetForm(new CouponForm());
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

    #endregion

    #region Private Methods

    private static string DiscountText(EcommerceCoupon coupon) => coupon.Kind == EcommerceCouponKind.Percent
        ? $"{coupon.Value.ToString("0.##", CultureInfo.InvariantCulture)}% off"
        : $"{EcommerceDisplay.Money(coupon.Value)} off";

    private static BadgeColor StatusColor(CouponStatus status) => status switch
    {
        CouponStatus.Active => BadgeColor.Success,
        CouponStatus.Paused => BadgeColor.Secondary,
        CouponStatus.Expired => BadgeColor.Danger,
        _ => BadgeColor.Warning,
    };

    private static string StatusLabel(CouponStatus status) => status switch
    {
        CouponStatus.Active => "Active",
        CouponStatus.Paused => "Paused",
        CouponStatus.Expired => "Expired",
        _ => "Used up",
    };

    private async Task AskDeleteAsync(EcommerceCoupon coupon)
    {
        deletingCode = coupon.Code;
        deleted = false;

        await deleteModal.ShowAsync();
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

    private Task EditAsync(EcommerceCoupon coupon) => OpenEditorAsync(coupon);

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
    /// Returns focus after the editor closes: to the edited row's Edit button (its code may have changed), to the status
    /// after creating a coupon, or to the button that opened the dialog when it was cancelled.
    /// </summary>
    private async Task OnEditorHiddenAsync()
    {
        var original = editingCode;
        var wasSaved = saved;
        var code = savedCode;

        editingCode = null;
        editingExpiresOn = null;
        saved = false;
        savedCode = null;
        ResetForm(new CouponForm());

        if (wasSaved && original is null)
            await EcommerceDisplay.FocusAsync(statusRegion);
        else if ((wasSaved ? code : original) is string target && editButtons.TryGetValue(target, out var editButton))
            await EcommerceDisplay.FocusAsync(editButton.Element);
        else if (original is null)
            await EcommerceDisplay.FocusAsync(newButton.Element);
        else
            await EcommerceDisplay.FocusAsync(statusRegion);
    }

    /// <summary>A server-side error (a duplicate code, a value outside its kind's range) clears once the field changes.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        messageStore.Clear(args.FieldIdentifier);

        // The value's range depends on the kind, so changing the kind re-checks it.
        if (args.FieldIdentifier.FieldName == nameof(CouponForm.Kind))
            messageStore.Clear(editContext.Field(nameof(CouponForm.Value)));

        editContext.NotifyValidationStateChanged();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenEditorAsync(EcommerceCoupon? coupon)
    {
        editingCode = coupon?.Code;
        editingExpiresOn = coupon?.ExpiresOn?.Date;
        saved = false;
        savedCode = null;
        ResetForm(coupon is null
            ? new CouponForm()
            : new CouponForm
            {
                Code = coupon.Code,
                Description = coupon.Description,
                Kind = coupon.Kind,
                Value = coupon.Value,
                MinSubtotal = coupon.MinSubtotal,
                FirstOrderOnly = coupon.FirstOrderOnly,
                ExpiresOn = coupon.ExpiresOn?.Date,
                UsageLimit = coupon.UsageLimit,
                IsActive = coupon.IsActive,
            });

        await editorModal.ShowAsync();
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

    private IEnumerable<string> Rules(EcommerceCoupon coupon)
    {
        if (coupon.MinSubtotal > 0)
            yield return $"Subtotal of {EcommerceDisplay.Money(coupon.MinSubtotal)} or more";

        if (coupon.FirstOrderOnly)
            yield return "First order only";

        if (coupon.ExpiresOn is { } expires)
            yield return $"{(State.Clock.Today > expires.Date ? "Expired" : "Expires")} {expires.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}";
        else
            yield return "No expiry date";

        yield return coupon.UsageLimit is { } limit
            ? $"{coupon.Uses} of {limit} uses"
            : coupon.Uses == 1 ? "1 use" : $"{coupon.Uses} uses";
    }

    private async Task SaveAsync()
    {
        // Rules the attributes can't express: the value's range depends on the kind, and a past expiry date is kept only when it's unchanged.
        messageStore.Clear();
        var value = Math.Round(form.Value, 2);

        if (form.Kind == EcommerceCouponKind.Percent && (value < 1 || value > 100))
            messageStore.Add(() => form.Value, "Enter a percentage from 1 to 100.");
        else if (form.Kind == EcommerceCouponKind.Amount && (value < 0.01m || value > 10_000m))
            messageStore.Add(() => form.Value, "Enter an amount from $0.01 to $10,000.00.");

        var today = State.Clock.Today;

        if (form.ExpiresOn is { } expires)
        {
            if (expires.Date < today && expires.Date != editingExpiresOn)
                messageStore.Add(() => form.ExpiresOn, "Choose today or a later date.");
            else if (expires.Date > today.AddYears(2))
                messageStore.Add(() => form.ExpiresOn, "Choose a date within the next two years.");
        }

        if (editContext.GetValidationMessages().Any())
        {
            editContext.NotifyValidationStateChanged();
            return;
        }

        var coupon = new EcommerceCoupon(
            form.Code,
            form.Description,
            form.Kind,
            value,
            Math.Round(form.MinSubtotal, 2),
            form.FirstOrderOnly,
            form.ExpiresOn?.Date,
            form.IsActive,
            form.UsageLimit,
            Uses: 0);

        if (!State.SaveCoupon(coupon, editingCode))
        {
            messageStore.Add(() => form.Code, "Another coupon already uses this code.");
            editContext.NotifyValidationStateChanged();
            await EcommerceDisplay.FocusAsync(codeInput.Element);
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

        return CouponStatus.Active;
    }

    /// <summary>Pauses or resumes a coupon; the same button stays in place with its new label, so focus stays on it.</summary>
    private void ToggleActive(EcommerceCoupon coupon)
    {
        var active = !coupon.IsActive;

        if (State.SetCouponActive(coupon.Code, active))
            SetStatus(active ? $"Coupon {coupon.Code} is resumed." : $"Coupon {coupon.Code} is paused. Shoppers can't use it until you resume it.");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string LeadText
    {
        get
        {
            var total = State.Coupons.Count;
            var active = State.Coupons.Count(coupon => StatusOf(coupon) == CouponStatus.Active);

            return $"{total} {(total == 1 ? "coupon" : "coupons")} · {active} usable at checkout today";
        }
    }

    private IReadOnlyList<EcommerceCoupon> SortedCoupons => State.Coupons.OrderBy(coupon => coupon.Code, StringComparer.Ordinal).ToList();

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private enum CouponStatus
    {
        Active,
        Paused,
        Expired,
        UsedUp,
    }

    private sealed class CouponForm
    {
        private string code = string.Empty;

        private string description = string.Empty;

        [Required(ErrorMessage = "Enter a code.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Use 3 to 20 characters.")]
        [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Use letters and digits only, with no spaces.")]
        public string Code { get => code; set => code = value?.Trim() ?? string.Empty; }

        [Required(ErrorMessage = "Describe the offer.")]
        [StringLength(120, ErrorMessage = "Use 120 characters or fewer.")]
        public string Description { get => description; set => description = value?.Trim() ?? string.Empty; }

        public DateTime? ExpiresOn { get; set; }

        public bool FirstOrderOnly { get; set; }

        public bool IsActive { get; set; } = true;

        public EcommerceCouponKind Kind { get; set; } = EcommerceCouponKind.Percent;

        [Range(0d, 1_000_000d, ErrorMessage = "Enter a minimum from $0.00 to $1,000,000.00.")]
        public decimal MinSubtotal { get; set; }

        [Range(1, 1_000_000, ErrorMessage = "Enter a limit of at least 1, or leave it empty.")]
        public int? UsageLimit { get; set; }

        public decimal Value { get; set; } = 10m;
    }

    #endregion
}
