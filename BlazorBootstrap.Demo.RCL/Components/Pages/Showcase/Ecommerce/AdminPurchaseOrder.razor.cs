using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// One purchase order at <c>admin/purchase-orders/{id}</c> (AC-7g): its header, lines, approval, supplier, and timeline,
/// with the actions its status allows. A draft (or a submitted order, audited as a change after submitting) edits its
/// lines, expected date, and note (rule WR-002: at least one line, quantities 1 to 10,000, unit costs above 0, each product
/// once, an expected date from today on; a linked summary on a failed save), then is submitted; an approver approves it
/// after a confirmation (the rule depends on the total, see <see cref="EcommerceState.CanApprove" />) or returns it to
/// draft with a reason; an approved or partly received order records goods received (0 to outstanding per product, at
/// least one above 0), which go into stock at once; an open order nothing arrived on can be cancelled with a reason. Every
/// step is announced in an always-rendered status message that focus moves to when the button is gone.
/// </summary>
public partial class AdminPurchaseOrder : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button? approveButton;

    private bool approved;

    private Modal approveModal = default!;

    private Button? cancelButton;

    private EditContext editContext = default!;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private bool focusStatus;

    private LinesForm form = new();

    /// <summary>The order and status the forms were last loaded for, so they reload when the order moves on.</summary>
    private (int Id, EcommercePurchaseOrderStatus Status, int Version)? loaded;

    private ValidationMessageStore editErrors = default!;

    private bool reasonDone;

    private EditContext reasonContext = default!;

    private ReasonForm reasonForm = new();

    private InputTextArea? reasonInput;

    private ReasonMode reasonMode;

    private Modal reasonModal = default!;

    private EditContext receiveContext = default!;

    private ValidationMessageStore receiveErrors = default!;

    private ReceiveForm receiveForm = new();

    private Button? returnButton;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private Button? submitButton;

    /// <summary>Bumped after a save or a receipt, so the forms reload from the stored order.</summary>
    private int version;

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

    protected override void OnInitialized()
    {
        reasonContext = new EditContext(reasonForm);
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        if (loaded?.Id != Id)
        {
            statusMessage = null;
            errorMessage = null;
        }

        LoadForms();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;

        if (editContext is not null)
            editContext.OnFieldChanged -= OnEditFieldChanged;

        if (receiveContext is not null)
            receiveContext.OnFieldChanged -= OnReceiveFieldChanged;
    }

    #endregion

    #region Private Methods

    private static bool IsEditable(EcommercePurchaseOrder order) => order.Status is EcommercePurchaseOrderStatus.Draft or EcommercePurchaseOrderStatus.Submitted;

    private static string StepTitle(EcommercePurchaseOrderEventKind kind) => kind switch
    {
        EcommercePurchaseOrderEventKind.Created => "Created",
        EcommercePurchaseOrderEventKind.Edited => "Changed",
        EcommercePurchaseOrderEventKind.Submitted => "Submitted for approval",
        EcommercePurchaseOrderEventKind.Returned => "Returned to draft",
        EcommercePurchaseOrderEventKind.Approved => "Approved and sent",
        EcommercePurchaseOrderEventKind.Received => "Goods received",
        _ => "Cancelled",
    };

    /// <summary>Products that can go on a new line: active, not on the order yet, this supplier's preferred products first.</summary>
    private IEnumerable<EcommerceProduct> AddableProducts(EcommercePurchaseOrder order) => State.Products
        .Where(product => form.Lines.All(line => line.ProductId != product.Id))
        .OrderBy(product => State.ReplenishmentFor(product.Id).SupplierId == order.SupplierId ? 0 : 1)
        .ThenBy(product => State.IsLowStock(product) ? 0 : 1)
        .ThenBy(product => product.Name, StringComparer.OrdinalIgnoreCase);

    /// <summary>Adds the chosen product as a line with its reorder quantity and unit cost (the preferred supplier's, or 45% of the price).</summary>
    private void AddLine()
    {
        editErrors.Clear(FieldIdentifier.Create(() => form.AddProductId));

        if (Order is not { } order || State.FindProduct(form.AddProductId) is not { } product || form.Lines.Any(line => line.ProductId == product.Id))
        {
            editErrors.Add(FieldIdentifier.Create(() => form.AddProductId), "Choose a product to add; each product goes on one line.");
            editContext.NotifyValidationStateChanged();

            return;
        }

        if (form.Lines.Count >= EcommerceState.MaxPurchaseOrderLines)
        {
            editErrors.Add(FieldIdentifier.Create(() => form.AddProductId), "A purchase order holds up to 50 lines. Raise another one for the rest.");
            editContext.NotifyValidationStateChanged();

            return;
        }

        var setting = State.ReplenishmentFor(product.Id);
        form.Lines.Add(new LineForm
        {
            ProductId = product.Id,
            Name = product.Name,
            Hint = HintFor(product, order),
            Quantity = setting.ReorderQuantity,
            UnitCost = setting.SupplierId == order.SupplierId && setting.UnitCost > 0 ? setting.UnitCost : Math.Round(product.Price * .45m, 2),
        });
        form.AddProductId = 0;
        editContext.NotifyValidationStateChanged();
    }

    private void Announce(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
        errorMessage = null;
    }

    /// <summary>The rule that applies to this order's approval, in words.</summary>
    private string ApprovalRule(EcommercePurchaseOrder order)
    {
        var limit = EcommerceDisplay.Money(EcommerceSeedData.ApprovalThreshold);
        var total = EcommerceDisplay.Money(State.TotalUsd(order));

        if (!State.NeedsSecondApprover(order))
            return $"At {total}, it's within the {limit} limit: anyone in the back office approves it, its creator included.";

        var creatorRole = State.Identity.FindAccount(order.CreatedBy)?.RoleId;
        var approver = creatorRole == EcommerceSeedData.PurchasingRole ? "the retailer" : "the purchasing manager";

        return $"At {total}, it's over the {limit} limit: {approver} approves it, not the person who raised it.";
    }

    private async Task AskApproveAsync()
    {
        approved = false;
        await approveModal.ShowAsync();
    }

    private async Task AskReasonAsync(ReasonMode mode)
    {
        reasonMode = mode;
        reasonDone = false;
        reasonForm = new ReasonForm();
        reasonContext = new EditContext(reasonForm);
        await reasonModal.ShowAsync();
    }

    private async Task ConfirmApproveAsync()
    {
        if (Order is { } order)
        {
            var result = State.ApprovePurchaseOrder(order.Id);

            if (result == EcommercePurchaseResult.Ok && State.FindPurchaseOrder(order.Id) is { } saved)
            {
                approved = true;
                Announce($"{order.Number} is approved and sent to {State.FindSupplier(order.SupplierId)?.Name}; expected {EcommerceDisplay.Date(saved.ExpectedOn)}. "
                    + "Record the goods below when they arrive.");
            }
            else
            {
                ShowError(result switch
                {
                    EcommercePurchaseResult.NotAllowed => $"You can't approve {order.Number}: {ApprovalRule(order)}",
                    EcommercePurchaseResult.WrongState => $"{order.Number} isn't waiting for approval any more. Nothing changed.",
                    _ => $"{order.Number} couldn't be approved. Nothing changed.",
                });
            }
        }

        await approveModal.HideAsync();
    }

    private async Task ConfirmReasonAsync()
    {
        if (Order is { } order)
        {
            var result = reasonMode == ReasonMode.Cancel ? State.CancelPurchaseOrder(order.Id, reasonForm.Reason) : State.ReturnPurchaseOrder(order.Id, reasonForm.Reason);

            if (result == EcommercePurchaseResult.Ok)
            {
                reasonDone = true;
                Announce(reasonMode == ReasonMode.Cancel
                    ? $"{order.Number} is cancelled. Its products show on the reorder list again if they're still low."
                    : $"{order.Number} is back in draft. Its creator was told why.");
            }
            else
            {
                ShowError(result == EcommercePurchaseResult.WrongState
                    ? $"{order.Number} can't be {(reasonMode == ReasonMode.Cancel ? "cancelled: goods arrived on it, or it's closed" : "returned: it isn't waiting for approval")}. Nothing changed."
                    : $"That didn't work, and nothing changed.");
            }
        }

        await reasonModal.HideAsync();
    }

    /// <summary>Fills each "Received now" with what's outstanding, for a delivery that arrived in full.</summary>
    private void FillOutstanding()
    {
        foreach (var line in receiveForm.Lines)
            line.Quantity = line.Outstanding;

        receiveErrors.Clear();
        receiveContext.NotifyValidationStateChanged();
    }

    private string HintFor(EcommerceProduct product, EcommercePurchaseOrder order)
    {
        var setting = State.ReplenishmentFor(product.Id);
        var onOrder = State.OnOrderFor(product.Id) - (order.Lines.FirstOrDefault(line => line.ProductId == product.Id)?.Outstanding ?? 0);

        return string.Create(CultureInfo.InvariantCulture,
            $"{product.Sku} · {product.Stock} in stock · reorder at {setting.ReorderPoint}{(onOrder > 0 ? $" · {onOrder} on other orders" : null)}{(setting.SupplierId == order.SupplierId ? null : " · another supplier's product")}");
    }

    /// <summary>Loads the forms from the stored order when it is first shown, after it moved on, or after a save.</summary>
    private void LoadForms()
    {
        if (Order is not { } order)
            return;

        var key = (order.Id, order.Status, version);

        if (loaded == key)
            return;

        loaded = key;

        if (editContext is not null)
            editContext.OnFieldChanged -= OnEditFieldChanged;

        form = new LinesForm
        {
            ExpectedOn = order.ExpectedOn.Date,
            Note = order.Note,
            Today = State.Clock.Today,
            Lines = order.Lines.Select(line => new LineForm
            {
                ProductId = line.ProductId,
                Name = line.Name,
                Hint = State.FindAnyProduct(line.ProductId) is { } product ? HintFor(product, order) : line.SupplierSku ?? string.Empty,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
            }).ToList(),
        };
        editContext = new EditContext(form);
        editErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnEditFieldChanged;

        if (receiveContext is not null)
            receiveContext.OnFieldChanged -= OnReceiveFieldChanged;

        receiveForm = new ReceiveForm
        {
            Lines = order.Lines.Where(line => line.Outstanding > 0)
                .Select(line => new ReceiveLine { ProductId = line.ProductId, Name = line.Name, Outstanding = line.Outstanding })
                .ToList(),
        };
        receiveContext = new EditContext(receiveForm);
        receiveErrors = new ValidationMessageStore(receiveContext);
        receiveContext.OnFieldChanged += OnReceiveFieldChanged;
    }

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: to the status after approving, otherwise back to "Approve and send".</summary>
    private async Task OnApproveHiddenAsync()
    {
        if (approved || errorMessage is not null)
            await EcommerceDisplay.FocusAsync(statusRegion);
        else
            await EcommerceDisplay.FocusAsync(approveButton?.Element);

        approved = false;
    }

    private void OnEditFieldChanged(object? sender, FieldChangedEventArgs args) => editErrors.Clear(args.FieldIdentifier);

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: to the status after the step, otherwise back to its button.</summary>
    private async Task OnReasonHiddenAsync()
    {
        if (reasonDone || errorMessage is not null)
            await EcommerceDisplay.FocusAsync(statusRegion);
        else
            await EcommerceDisplay.FocusAsync((reasonMode == ReasonMode.Cancel ? cancelButton : returnButton)?.Element);

        reasonDone = false;
    }

    private Task OnReasonShownAsync() => EcommerceDisplay.FocusAsync(reasonInput?.Element);

    private void OnReceiveFieldChanged(object? sender, FieldChangedEventArgs args) => receiveErrors.Clear(args.FieldIdentifier);

    private void OnStateChanged() => InvokeAsync(() =>
    {
        LoadForms();
        StateHasChanged();
    });

    /// <summary>
    /// Records the goods received once the form is valid (rule WR-002): each quantity 0 to outstanding, at least one above 0.
    /// The state checks again and refuses an over-receipt.
    /// </summary>
    private async Task Receive()
    {
        if (Order is not { } order)
            return;

        receiveErrors.Clear();
        await receiveContext.ValidateAsync();

        foreach (var line in receiveForm.Lines)
        {
            if (line.Quantity is < 0)
                receiveErrors.Add(FieldIdentifier.Create(() => line.Quantity), $"Enter 0 or more for {line.Name}.");
            else if (line.Quantity > line.Outstanding)
                receiveErrors.Add(FieldIdentifier.Create(() => line.Quantity), $"Only {line.Outstanding} of {line.Name} are outstanding; more than that can't be received on this order.");
        }

        if (receiveForm.Lines.All(line => (line.Quantity ?? 0) == 0) && receiveForm.Lines.FirstOrDefault() is { } first)
            receiveErrors.Add(FieldIdentifier.Create(() => first.Quantity), "Enter how many arrived of at least one product.");

        receiveContext.NotifyValidationStateChanged();

        if (receiveContext.GetValidationMessages().Any())
            return;

        var quantities = receiveForm.Lines.Where(line => (line.Quantity ?? 0) > 0).ToDictionary(line => line.ProductId, line => line.Quantity!.Value);
        var result = State.ReceivePurchaseOrder(order.Id, quantities, receiveForm.Note);

        if (result != EcommercePurchaseResult.Ok)
        {
            ShowError(result == EcommercePurchaseResult.WrongState
                ? $"{order.Number} isn't open for receiving any more. Nothing was received."
                : "Those quantities couldn't be received: more than is outstanding, or nothing at all. Nothing was received.");

            return;
        }

        var saved = State.FindPurchaseOrder(order.Id)!;
        version++;
        LoadForms();
        Announce($"Received {EcommerceDisplay.Plural(quantities.Values.Sum(), "unit")} into stock on {order.Number}: "
            + string.Join(", ", quantities.Select(pair => $"{pair.Value} × {order.Lines.First(line => line.ProductId == pair.Key).Name}")) + ". "
            + (saved.Status == EcommercePurchaseOrderStatus.Received ? "Everything has arrived, so the order is closed." : $"{saved.Units - saved.UnitsReceived} units are still to come."));
        focusStatus = true;
    }

    private void RemoveLine(LineForm line)
    {
        form.Lines.Remove(line);
        editErrors.Clear();
        editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Saves the lines once the form is valid: the lines' rules run with the form's own (rule WR-002) and a failed save shows
    /// the summary. The state checks again; its refusal shows in the alert.
    /// </summary>
    private async Task Save()
    {
        if (Order is not { } order)
            return;

        editErrors.Clear();
        await editContext.ValidateAsync();
        ValidateLines();
        editContext.NotifyValidationStateChanged();

        if (editContext.GetValidationMessages().Any())
            return;

        var result = State.SavePurchaseOrder(order.Id, form.Lines.Select(line => (line.ProductId, line.Quantity!.Value, line.UnitCost!.Value)).ToList(), form.ExpectedOn!.Value, form.Note);

        switch (result)
        {
            case EcommercePurchaseResult.Ok:
                version++;
                LoadForms();
                Announce(order.Status == EcommercePurchaseOrderStatus.Submitted
                    ? $"Saved {order.Number}. It's still waiting for approval; the change is in the audit log."
                    : $"Saved {order.Number}: {EcommerceDisplay.Plural(form.Lines.Count, "line")}, {State.MoneyIn(State.FindPurchaseOrder(order.Id)!)}. Submit it for approval when it's ready.");
                break;
            case EcommercePurchaseResult.NoChange:
                editContext.MarkAsUnmodified();
                Announce($"Nothing changed: {order.Number} is already saved like this.");
                break;
            case EcommercePurchaseResult.WrongState:
                ShowError($"{order.Number} can't change any more: it was approved or closed meanwhile.");
                break;
            default:
                ShowError("The lines couldn't be saved: a product was archived meanwhile, or a value is out of range. Nothing was saved.");
                break;
        }

        focusStatus = true;
    }

    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
        focusStatus = true;
    }

    /// <summary>Whether the editor shows exactly what is saved: the same lines in the same order, expected date, and note.</summary>
    private bool FormMatchesSaved(EcommercePurchaseOrder order)
    {
        static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

        return form.ExpectedOn?.Date == order.ExpectedOn.Date
            && Clean(form.Note) == Clean(order.Note)
            && form.Lines.Count == order.Lines.Count
            && form.Lines.Zip(order.Lines).All(pair => pair.First.ProductId == pair.Second.ProductId
                && pair.First.Quantity == pair.Second.Quantity
                && pair.First.UnitCost == pair.Second.UnitCost);
    }

    /// <summary>Submits the draft; unsaved lines must be saved first, so a draft is submitted as it's stored.</summary>
    private void Submit()
    {
        if (Order is not { } order)
            return;

        // Compare values, not the edit context's modified flags: inputs can flag a field on their first render.
        if (!FormMatchesSaved(order))
        {
            ShowError($"Save the lines first: {order.Number} is submitted as it's saved.");

            return;
        }

        var result = State.SubmitPurchaseOrder(order.Id);

        switch (result)
        {
            case EcommercePurchaseResult.Ok:
                var saved = State.FindPurchaseOrder(order.Id)!;
                Announce(State.CanApprove(saved)
                    ? $"{order.Number} is submitted. It's within the approval limit, so you can approve it now."
                    : $"{order.Number} is submitted. Over {EcommerceDisplay.Money(EcommerceSeedData.ApprovalThreshold)}, it needs a second approver, who was told.");
                focusStatus = true;
                break;
            case EcommercePurchaseResult.Invalid:
                ShowError(order.Lines.Count == 0
                    ? $"{order.Number} has no lines yet. Add at least one and save it before submitting."
                    : $"{order.Number}'s expected date has passed. Choose today or later and save it before submitting.");
                break;
            default:
                ShowError($"{order.Number} isn't a draft any more. Nothing changed.");
                break;
        }
    }

    /// <summary>The lines' rules: at least one, each product once, a quantity of 1 to 10,000, and a unit cost of $0.01 to $100,000.</summary>
    private void ValidateLines()
    {
        if (form.Lines.Count == 0)
            editErrors.Add(FieldIdentifier.Create(() => form.AddProductId), "Add at least one product to order.");

        var seen = new HashSet<int>();

        foreach (var line in form.Lines)
        {
            if (!seen.Add(line.ProductId))
                editErrors.Add(FieldIdentifier.Create(() => line.Quantity), $"{line.Name} is on another line already. Remove one of them.");
            else if (line.Quantity is not { } quantity)
                editErrors.Add(FieldIdentifier.Create(() => line.Quantity), $"Enter how many of {line.Name} to order, from 1 to 10,000.");
            else if (quantity is < 1 or > EcommerceState.MaxPurchaseOrderQuantity)
                editErrors.Add(FieldIdentifier.Create(() => line.Quantity), $"Order 1 to 10,000 of {line.Name}.");

            if (line.UnitCost is not { } cost || cost is < .01m or > 100_000m)
                editErrors.Add(FieldIdentifier.Create(() => line.UnitCost), $"Enter a unit cost above 0, up to 100,000, for {line.Name}.");
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the purchase order's ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    private static string ListUrl => $"{EcommerceAdminLayout.Base}/purchase-orders";

    private EcommercePurchaseOrder? Order => State.FindPurchaseOrder(Id);

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Which reason the dialog asks for.</summary>
    private enum ReasonMode
    {
        Return,
        Cancel,
    }

    /// <summary>
    /// The line editor's fields: the lines (checked by <see cref="ValidateLines" />), the product to add, the expected date
    /// (required, from today to a year ahead), and the note.
    /// </summary>
    private sealed class LinesForm : IValidatableObject
    {
        public int AddProductId { get; set; }

        [Required(ErrorMessage = "Choose the date the goods should arrive.")]
        public DateTime? ExpectedOn { get; set; }

        public List<LineForm> Lines { get; init; } = new();

        [StringLength(EcommerceState.MaxPurchaseOrderNoteLength, ErrorMessage = "Use 300 characters or fewer for the note.")]
        public string? Note { get; set; }

        /// <summary>The site's today, for the date rule.</summary>
        public DateTime Today { get; init; }

        /// <summary>Runs once the fields are filled in: the expected date can't be in the past or more than a year away.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ExpectedOn is { } expected && expected.Date < Today)
                yield return new ValidationResult("Choose today or a later expected date.", new[] { nameof(ExpectedOn) });
            else if (ExpectedOn is { } later && later.Date > Today.AddYears(1))
                yield return new ValidationResult("Choose an expected date within the next year.", new[] { nameof(ExpectedOn) });
        }
    }

    /// <summary>One line being edited.</summary>
    private sealed class LineForm
    {
        public string Hint { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public int ProductId { get; init; }

        public int? Quantity { get; set; }

        public decimal? UnitCost { get; set; }
    }

    /// <summary>The cancel or return dialog's one field.</summary>
    private sealed class ReasonForm
    {
        private string reason = string.Empty;

        [Required(ErrorMessage = "Give a reason of 5 to 200 characters.")]
        [StringLength(EcommerceState.MaxDecisionNoteLength, MinimumLength = EcommerceState.MinReasonLength, ErrorMessage = "Use 5 to 200 characters for the reason.")]
        public string Reason { get => reason; set => reason = value?.Trim() ?? string.Empty; }
    }

    /// <summary>The receiving form: one quantity per outstanding line (checked on submit) and a note.</summary>
    private sealed class ReceiveForm
    {
        public List<ReceiveLine> Lines { get; init; } = new();

        [StringLength(EcommerceState.MaxPurchaseOrderNoteLength, ErrorMessage = "Use 300 characters or fewer for the note.")]
        public string? Note { get; set; }
    }

    /// <summary>One line being received.</summary>
    private sealed class ReceiveLine
    {
        public string Name { get; init; } = string.Empty;

        public int Outstanding { get; init; }

        public int ProductId { get; init; }

        public int? Quantity { get; set; }
    }

    #endregion
}
