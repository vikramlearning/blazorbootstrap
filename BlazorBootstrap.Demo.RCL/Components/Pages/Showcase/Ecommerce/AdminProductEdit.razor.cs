using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The product editor at <c>admin/products/new</c> and <c>admin/products/{id}</c>: name, category, SKU (its form, and unique
/// in the store, checked while typing), the photo's description (required when the product has a library photo),
/// description, highlights, colors, price and compare-at price (above the price), stock, and the featured flag, validated
/// with DataAnnotations (rule WR-002: messages after a field is touched or on submit, a linked summary on a failed submit)
/// and saved through <see cref="EcommerceState" />, whose refusals map back to their field. An existing product also shows
/// its sales and rating, a "View in store" link, Archive or Restore, and Delete when nothing refers to it (each destructive
/// step behind a confirmation dialog). An unknown ID shows a not-found panel.
/// </summary>
public partial class AdminProductEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int ColorsMaxLength = 200;

    private const int HighlightsMaxLength = 1000;

    private const int MaxColorLength = 20;

    private const int MaxColors = 6;

    private const int MaxHighlightLength = 80;

    private const int MaxHighlights = 6;

    private Button? archiveButton;

    private bool archiveConfirmed;

    private Modal archiveModal = default!;

    private Button? deleteButton;

    private bool deleteConfirmed;

    private Modal deleteModal = default!;

    private EditContext editContext = default!;

    private bool focusStatus;

    private ProductForm form = new();

    private string? loadedKey;

    /// <summary>A message for the page a just-added product opens on (the same component instance, with a new route).</summary>
    private string? pendingStatus;

    private int saves;

    /// <summary>The state's refusals, shown on their field until it changes.</summary>
    private ValidationMessageStore? serverErrors;

    private ShowcaseUniqueCheck? skuCheck;

    private EditContext replenishmentContext = new(new ReplenishmentForm());

    private ReplenishmentForm replenishmentForm = new();

    private string? replenishmentMessage;

    private int replenishmentSaves;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatus)
        {
            focusStatus = false;
            await EcommerceDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        var key = Id?.ToString(CultureInfo.InvariantCulture) ?? "new";

        // Only a different product (or the switch from "new" to the product just added) starts a fresh form.
        if (loadedKey == key)
            return;

        loadedKey = key;
        saves = 0;
        statusMessage = pendingStatus;
        focusStatus = pendingStatus is not null;
        pendingStatus = null;
        SetForm(Product is { } product ? FromProduct(product) : new ProductForm { Sku = State.SuggestSku(null) });
        replenishmentSaves = 0;
        replenishmentMessage = null;
        LoadReplenishment();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;

        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;
    }

    #endregion

    #region Private Methods

    private static IReadOnlyList<string> ColorsOf(string? text) =>
        (text ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static ProductForm FromProduct(EcommerceProduct product) => new()
    {
        Name = product.Name,
        CategoryId = product.CategoryId,
        Sku = product.Sku,
        HasPhoto = product.HasPhoto,
        ImageAlt = product.ImageAlt,
        Description = product.Description,
        Highlights = string.Join("\n", product.Highlights),
        Colors = string.Join(", ", product.Colors),
        Price = product.Price,
        CompareAtPrice = product.CompareAtPrice,
        Stock = product.Stock,
        IsFeatured = product.IsFeatured,
    };

    private static IReadOnlyList<string> HighlightsOf(string? text) =>
        (text ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>Builds a validation result for the property being validated, so the message shows next to its field.</summary>
    private static ValidationResult? Result(string? error, ValidationContext context) => error is null
        ? ValidationResult.Success
        : new ValidationResult(error, context.MemberName is { } member ? new[] { member } : null);

    private async Task AskDeleteAsync()
    {
        deleteConfirmed = false;
        await deleteModal.ShowAsync();
    }

    private async Task ConfirmArchiveAsync()
    {
        archiveConfirmed = true;
        await archiveModal.HideAsync();
    }

    private async Task ConfirmDeleteAsync()
    {
        deleteConfirmed = true;
        await deleteModal.HideAsync();
    }

    /// <summary>
    /// Archives once the dialog has closed. The dialog opens from code, so Bootstrap doesn't return focus: it goes back to
    /// the Archive button, which now reads Restore.
    /// </summary>
    private async Task OnArchiveHiddenAsync()
    {
        if (archiveConfirmed && Product is { } product)
        {
            archiveConfirmed = false;

            if (State.SetArchived(product.Id, true))
                statusMessage = $"{product.Name} is archived: it's hidden from the store and removed from carts.";
        }

        await EcommerceDisplay.FocusAsync(archiveButton?.Element);
    }

    /// <summary>
    /// Deletes once the dialog has closed, then opens the product list; a kept product gets focus back on Delete.
    /// </summary>
    private async Task OnDeleteHiddenAsync()
    {
        if (deleteConfirmed && Product is { } product)
        {
            deleteConfirmed = false;

            if (State.DeleteProduct(product.Id))
            {
                Navigation.NavigateTo(ProductsUrl);

                return;
            }

            statusMessage = $"{product.Name} can't be deleted any more: it's now in an order or a cart. Archive it instead.";
            await EcommerceDisplay.FocusAsync(statusRegion);

            return;
        }

        await EcommerceDisplay.FocusAsync(deleteButton?.Element);
    }

    /// <summary>A refusal from the state clears once its field changes; a new SKU is shown in capitals as it is saved.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);

        if (args.FieldIdentifier.FieldName == nameof(ProductForm.Sku) && form.Sku is { } sku && sku != sku.ToUpperInvariant())
            form.Sku = sku.ToUpperInvariant();

        // A new product's suggested SKU follows the category until someone types their own.
        if (args.FieldIdentifier.FieldName == nameof(ProductForm.CategoryId) && Id is null
            && (form.Sku == State.SuggestSku(null) || State.Categories.Any(category => form.Sku == State.SuggestSku(category.Id))))
            form.Sku = State.SuggestSku(form.CategoryId);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>
    /// Saves once the form is valid: the SKU check finishes first, then every rule runs, and a failed submit shows the
    /// summary (rule WR-002). The state checks the SKU again; a refusal shows on the SKU field.
    /// </summary>
    private async Task SaveAsync()
    {
        serverErrors?.Clear();
        editContext.NotifyValidationStateChanged();

        var skuFree = skuCheck is null || await skuCheck.CheckNowAsync();

        if (!await editContext.ValidateAsync() || !skuFree)
            return;

        var draft = ToDraft();

        if (Product is not { } existing)
        {
            if (Id is not null)
                return;

            if (State.CreateProduct(draft) is not { } created)
            {
                ServerError(() => form.Sku, State.IsSkuTaken(draft.Sku) ? SkuTakenMessage(draft.Sku) : "This SKU can't be used. Use 4 to 20 letters, digits, and single dashes, starting with a letter.");

                return;
            }

            pendingStatus = $"{created.Name} is added with SKU {created.Sku}.";
            Navigation.NavigateTo($"{ProductsUrl}/{created.Id}");

            return;
        }

        if (!State.UpdateProduct(existing.Id, draft))
        {
            ServerError(() => form.Sku, State.IsSkuTaken(draft.Sku, existing.Id) ? SkuTakenMessage(draft.Sku) : "This SKU can't be used. Use 4 to 20 letters, digits, and single dashes, starting with a letter.");

            return;
        }

        // Show what was stored (trimmed, de-duplicated colors, rounded prices) without replacing the form's EditContext,
        // so focus stays on the Save button.
        if (State.FindAnyProduct(existing.Id) is { } saved)
        {
            var stored = FromProduct(saved);
            form.Name = stored.Name;
            form.CategoryId = stored.CategoryId;
            form.Sku = stored.Sku;
            form.ImageAlt = stored.ImageAlt;
            form.Description = stored.Description;
            form.Highlights = stored.Highlights;
            form.Colors = stored.Colors;
            form.Price = stored.Price;
            form.CompareAtPrice = stored.CompareAtPrice;
            form.Stock = stored.Stock;
            form.IsFeatured = stored.IsFeatured;
            editContext.MarkAsUnmodified();
        }

        saves++;
        statusMessage = saves == 1 ? "Changes saved." : $"Changes saved ({saves} times this visit).";
    }

    /// <summary>Loads the replenishment form from the product's stored settings (the store-wide defaults when it has none).</summary>
    private void LoadReplenishment()
    {
        var setting = State.ReplenishmentFor(Id ?? 0);

        replenishmentForm = new ReplenishmentForm
        {
            ReorderPoint = setting.ReorderPoint,
            ReorderQuantity = setting.ReorderQuantity,
            SupplierId = setting.SupplierId ?? string.Empty,
            SupplierSku = setting.SupplierSku,
            UnitCost = setting.SupplierId is null ? null : setting.UnitCost,
        };
        replenishmentContext = new EditContext(replenishmentForm);
    }

    /// <summary>Saves the replenishment settings once the form is valid (rule WR-002); the change is announced and audited.</summary>
    private void SaveReplenishment()
    {
        if (Product is not { } product)
            return;

        var supplierId = string.IsNullOrEmpty(replenishmentForm.SupplierId) ? null : replenishmentForm.SupplierId;
        var result = State.SaveReplenishment(product.Id, replenishmentForm.ReorderPoint!.Value, replenishmentForm.ReorderQuantity!.Value, supplierId,
            replenishmentForm.SupplierSku, replenishmentForm.UnitCost ?? 0m);

        replenishmentSaves++;
        replenishmentMessage = (result switch
        {
            EcommercePurchaseResult.Ok => State.IsLowStock(State.FindAnyProduct(product.Id)!)
                ? $"Replenishment saved. {product.Name} is at or below its reorder point now, so it's on the reorder list."
                : "Replenishment saved.",
            EcommercePurchaseResult.NoChange => "Nothing changed: the replenishment settings are already like this.",
            EcommercePurchaseResult.NotAllowed => "Only the back office can change replenishment settings. Nothing was saved.",
            _ => "These settings couldn't be saved: the supplier is gone, or a value is out of range. Nothing was saved.",
        }) + (replenishmentSaves > 1 ? string.Create(CultureInfo.InvariantCulture, $" ({replenishmentSaves})") : null);

        if (result == EcommercePurchaseResult.Ok)
            LoadReplenishment();
    }

    private void ServerError(System.Linq.Expressions.Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    private void SetForm(ProductForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    private Task<bool> IsSkuTakenAsync(string sku) => Task.FromResult(State.IsSkuTaken(sku, Id ?? 0));

    private static string SkuTakenMessage(string sku) => $"{sku.Trim().ToUpperInvariant()} is already another product's SKU. Use a SKU no other product has.";

    private EcommerceProductDraft ToDraft()
    {
        var price = Math.Round(form.Price ?? 0m, 2, MidpointRounding.AwayFromZero);
        decimal? compareAt = form.CompareAtPrice is { } was ? Math.Round(was, 2, MidpointRounding.AwayFromZero) : null;

        return new EcommerceProductDraft
        {
            Name = form.Name.Trim(),
            CategoryId = form.CategoryId,
            Sku = form.Sku.Trim().ToUpperInvariant(),
            ImageAlt = form.HasPhoto ? form.ImageAlt?.Trim() : null,
            Description = form.Description.Trim(),
            Highlights = string.Join("\n", HighlightsOf(form.Highlights)),
            Colors = string.Join(", ", ColorsOf(form.Colors)),
            Price = price,
            CompareAtPrice = compareAt,
            Stock = form.Stock,
            IsFeatured = form.IsFeatured,
        };
    }

    /// <summary>Restores without a dialog (nothing is lost); archiving asks first.</summary>
    private async Task ToggleArchiveAsync()
    {
        if (Product is not { } product)
            return;

        if (!product.IsArchived)
        {
            archiveConfirmed = false;
            await archiveModal.ShowAsync();

            return;
        }

        if (State.SetArchived(product.Id, false))
            statusMessage = $"{product.Name} is restored: it's back in the store.";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private bool CanDelete => Product is { } product && State.CanDeleteProduct(product.Id);

    private string CannotDeleteText => Product is { } product && State.Orders.Any(order => order.Lines.Any(line => line.ProductId == product.Id))
        ? "Products with orders can be archived, not deleted."
        : "Products in a shopper's cart can be archived, not deleted.";

    private string? CategoryAccent => State.FindCategory(Product?.CategoryId ?? form.CategoryId)?.Accent;

    private IconName CategoryIcon => State.FindCategory(Product?.CategoryId ?? form.CategoryId)?.IconName ?? IconName.Image;

    private string CategoryName => Product is { } product ? State.FindCategory(product.CategoryId)?.Name ?? product.CategoryId : string.Empty;

    /// <summary>Gets or sets the product ID from the route; <see langword="null" /> on <c>admin/products/new</c>.</summary>
    [Parameter] public int? Id { get; set; }

    private bool IsMissing => Id is not null && Product is null;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private string OrderCountText
    {
        get
        {
            var count = SalesOrders.Count();

            return count == 1 ? "1 order" : $"{count} orders";
        }
    }

    private EcommerceProduct? Product => Id is int id ? State.FindAnyProduct(id) : null;

    private string ProductsUrl => $"{EcommerceAdminLayout.Base}/products";

    private double Rating => Product is { } product ? State.AverageRating(product.Id) : 0;

    private int ReviewCount => Product is { } product ? State.ReviewsFor(product.Id).Count : 0;

    /// <summary>An example SKU for the help text: the product's own, or the one suggested for the chosen category.</summary>
    private string SkuExample => Product?.Sku ?? State.SuggestSku(form.CategoryId);

    /// <summary>Orders that include the product, cancelled ones left out (their items went back into stock).</summary>
    private IEnumerable<EcommerceOrder> SalesOrders => Product is { } product
        ? State.Orders.Where(order => order.Status != EcommerceOrderStatus.Cancelled && order.Lines.Any(line => line.ProductId == product.Id))
        : Enumerable.Empty<EcommerceOrder>();

    [Inject] private EcommerceState State { get; set; } = default!;

    private int UnitsSold => Product is { } product
        ? SalesOrders.SelectMany(order => order.Lines).Where(line => line.ProductId == product.Id).Sum(line => line.Quantity)
        : 0;

    #endregion

    #region Nested Types

    /// <summary>
    /// The replenishment form, with the limits of <see cref="EcommerceState.SaveReplenishment" />: a reorder point of 0 to
    /// 1,000, a reorder quantity of 1 to 5,000 and not below the reorder point (a cross-field rule), and with a preferred
    /// supplier a unit cost above 0; a supplier SKU of letters, digits, and dashes.
    /// </summary>
    private sealed class ReplenishmentForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter the reorder point, from 0 to 1,000.")]
        [Range(0, EcommerceState.MaxReorderPoint, ErrorMessage = "Enter a reorder point from 0 to 1,000.")]
        public int? ReorderPoint { get; set; }

        [Required(ErrorMessage = "Enter the reorder quantity, from 1 to 5,000.")]
        [Range(1, EcommerceState.MaxReorderQuantity, ErrorMessage = "Enter a reorder quantity from 1 to 5,000.")]
        public int? ReorderQuantity { get; set; }

        public string SupplierId { get; set; } = string.Empty;

        [StringLength(EcommerceState.MaxSupplierSkuLength, ErrorMessage = "Use 30 characters or fewer for the supplier SKU.")]
        [RegularExpression("^[A-Za-z0-9]+(-[A-Za-z0-9]+)*$", ErrorMessage = "Use letters, digits, and single dashes in the supplier SKU.")]
        public string? SupplierSku { get; set; }

        [Range(0.01, 100_000, ErrorMessage = "Enter a unit cost from 0.01 to 100,000.")]
        public decimal? UnitCost { get; set; }

        /// <summary>Runs once the fields are filled in: the reorder quantity covers the reorder point, and a supplier needs a unit cost.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ReorderPoint is { } point && ReorderQuantity is { } quantity && quantity < point)
                yield return new ValidationResult($"Order at least as many as the reorder point ({point}), so one purchase order lifts stock above it.", new[] { nameof(ReorderQuantity) });

            if (SupplierId.Length > 0 && UnitCost is null)
                yield return new ValidationResult("Enter what the supplier charges per unit.", new[] { nameof(UnitCost) });
        }
    }

    /// <summary>
    /// The editor's form; mapped to an <see cref="EcommerceProductDraft" /> on save. The cross-field rules: a compare-at price
    /// above the price, and a photo description when the product has a library photo.
    /// </summary>
    private sealed class ProductForm : IValidatableObject
    {
        [Required(ErrorMessage = "Choose a category.")]
        public string CategoryId { get; set; } = string.Empty;

        /// <summary>Whether the product has a library photo, so its description is required (set by the page, not by the user).</summary>
        public bool HasPhoto { get; set; }

        [StringLength(EcommerceState.MaxImageAltLength, ErrorMessage = "Use 160 characters or fewer for the photo description.")]
        public string? ImageAlt { get; set; }

        [Required(ErrorMessage = "Enter the SKU, like KG-KIT-0101.")]
        [StringLength(EcommerceState.MaxSkuLength, MinimumLength = 4, ErrorMessage = "Use 4 to 20 characters for the SKU.")]
        [RegularExpression("^[A-Za-z][A-Za-z0-9]*(-[A-Za-z0-9]+)*$", ErrorMessage = "Start the SKU with a letter and use letters, digits, and single dashes between them, like KG-KIT-0101.")]
        public string Sku { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: a product with a photo needs its description.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HasPhoto && string.IsNullOrWhiteSpace(ImageAlt))
                yield return new ValidationResult("Describe the product's photo for people who can't see it, like “Oak cutting board with a juice groove”.", new[] { nameof(ImageAlt) });
        }

        [Required(ErrorMessage = "Enter at least one color.")]
        [StringLength(ColorsMaxLength, ErrorMessage = "Use 200 characters or fewer.")]
        [ColorList]
        public string Colors { get; set; } = string.Empty;

        [Range(0.01, 100_000, ErrorMessage = "Enter a compare-at price from $0.01 to $100,000, or leave it empty.")]
        [CompareAtAbovePrice]
        public decimal? CompareAtPrice { get; set; }

        [Required(ErrorMessage = "Enter a description.")]
        [TrimmedLength(10, 1000, ErrorMessage = "Use 10 to 1,000 characters.")]
        public string Description { get; set; } = string.Empty;

        [StringLength(HighlightsMaxLength, ErrorMessage = "Use 1,000 characters or fewer.")]
        [HighlightList]
        public string Highlights { get; set; } = string.Empty;

        public bool IsFeatured { get; set; }

        [Required(ErrorMessage = "Enter the product name.")]
        [TrimmedLength(2, 80, ErrorMessage = "Use 2 to 80 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter a price.")]
        [Range(0.01, 100_000, ErrorMessage = "Enter a price from $0.01 to $100,000.")]
        public decimal? Price { get; set; }

        [Range(0, 9999, ErrorMessage = "Enter a stock level from 0 to 9,999.")]
        public int Stock { get; set; }
    }

    /// <summary>Checks the comma-separated colors: 1 to 6, each up to 20 characters.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    private sealed class ColorListAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var colors = ColorsOf(value as string);
            var error = colors.Count == 0 ? "Enter at least one color."
                : colors.Count > MaxColors ? $"Use up to {MaxColors} colors (you entered {colors.Count})."
                : colors.FirstOrDefault(color => color.Length > MaxColorLength) is { } longColor ? $"Keep each color to {MaxColorLength} characters (\"{longColor[..MaxColorLength]}…\" is longer)."
                : null;

            return Result(error, validationContext);
        }
    }

    /// <summary>Checks that a compare-at price, when set, is more than the price.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    private sealed class CompareAtAbovePriceAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var error = value is decimal compareAt && validationContext.ObjectInstance is ProductForm { Price: { } price } && compareAt <= price
                ? "The compare-at price must be more than the price. Leave it empty when the product isn't on sale."
                : null;

            return Result(error, validationContext);
        }
    }

    /// <summary>Checks the highlights: up to 6 non-empty lines, each up to 80 characters.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    private sealed class HighlightListAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var lines = HighlightsOf(value as string);
            var longLine = lines.Select((line, index) => (line, index)).FirstOrDefault(entry => entry.line.Length > MaxHighlightLength);
            var error = lines.Count > MaxHighlights ? $"Use up to {MaxHighlights} highlights (you entered {lines.Count})."
                : longLine.line is not null ? $"Keep each highlight to {MaxHighlightLength} characters (highlight {longLine.index + 1} has {longLine.line.Length})."
                : null;

            return Result(error, validationContext);
        }
    }

    /// <summary>Checks the length of a text once leading and trailing spaces are trimmed.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    private sealed class TrimmedLengthAttribute : ValidationAttribute
    {
        private readonly int maximum;

        private readonly int minimum;

        public TrimmedLengthAttribute(int minimum, int maximum)
        {
            this.minimum = minimum;
            this.maximum = maximum;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // An empty value is left to [Required].
            if (value is not string text || text.Trim().Length == 0)
                return ValidationResult.Success;

            var length = text.Trim().Length;

            return Result(length < minimum || length > maximum ? ErrorMessage ?? $"Use {minimum} to {maximum} characters." : null, validationContext);
        }
    }

    #endregion
}
