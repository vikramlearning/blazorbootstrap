using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Budgets: this month's spending against each category's limit (amounts, the share used, and the status in words as well
/// as color), totals at the top, and dialogs to add a budget for a spending category that has none, change a limit, or
/// delete a budget after a confirmation that names it. The add and edit dialog is an <see cref="EditForm" /> (rule WR-002):
/// the category is required and unique (one budget per category each month, checked as you choose, with a pending state),
/// and the limit is $1 to $100,000 in dollars and cents; a failed add shows a summary that links to each field. Dialogs open from code, so focus is returned by hand: to the button
/// that opened the dialog, to the new budget's "Edit limit" after adding, or to "Add budget" after deleting. Every result
/// is reported in a status message; without budgets the page shows an empty state.
/// </summary>
public partial class Budgets : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button addButton = default!;

    private ShowcaseUniqueCheck? categoryCheck;

    /// <summary>The state's answers that belong to a field (a category that has a budget), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    /// <summary>Whether the form dialog adds a budget (<see langword="true" />) or edits a limit.</summary>
    private bool adding;

    /// <summary>The category whose new budget gets focus after the add dialog closes.</summary>
    private string? addedCategoryId;

    private readonly Dictionary<string, Button> deleteButtons = new();

    private string? deleteError;

    /// <summary>Whether the budget was deleted, so focus goes to "Add budget" rather than back to its "Delete" button.</summary>
    private bool deleted;

    private Modal deleteModal = default!;

    private string? deletingCategoryId;

    private decimal deletingLimit;

    private string deletingName = string.Empty;

    private readonly Dictionary<string, Button> editButtons = new();

    private EditContext editContext = default!;

    private string? editingCategoryId;

    private string editingName = string.Empty;

    private decimal editingSpent;

    private LimitForm form = new();

    private string? formError;

    private Modal formModal = default!;

    private string? statusMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        NewContext();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static (string Text, BadgeColor Color, ProgressColor Bar) Status(double percent) => percent switch
    {
        > 100 => ("Over budget", BadgeColor.Danger, ProgressColor.Danger),
        >= 80 => ("Near the limit", BadgeColor.Warning, ProgressColor.Warning),
        _ => ("On track", BadgeColor.Success, ProgressColor.Success),
    };

    private async Task AddAsync()
    {
        adding = true;
        addedCategoryId = null;
        editingCategoryId = null;
        formError = null;
        form = new LimitForm();
        NewContext();

        await formModal.ShowAsync();
    }

    private async Task ConfirmDeleteAsync(FintechBudget budget)
    {
        deletingCategoryId = budget.CategoryId;
        deletingName = State.FindCategory(budget.CategoryId).Name;
        deletingLimit = budget.Limit;
        deleteError = null;
        deleted = false;

        await deleteModal.ShowAsync();
    }

    private async Task DeleteAsync()
    {
        if (deletingCategoryId is not { } categoryId)
            return;

        var result = State.DeleteBudget(categoryId);

        switch (result)
        {
            case FintechResult.Ok:
                deleted = true;
                statusMessage = $"The {deletingName} budget is deleted.";
                break;
            case FintechResult.NotFound:
                deleted = true;
                statusMessage = $"The {deletingName} budget was already deleted.";
                break;
            default:
                deleteError = ResultMessage(result, deletingName);
                return;
        }

        await deleteModal.HideAsync();
    }

    private async Task EditAsync(FintechBudget budget)
    {
        adding = false;
        addedCategoryId = null;
        editingCategoryId = budget.CategoryId;
        editingName = State.FindCategory(budget.CategoryId).Name;
        editingSpent = State.SpentThisMonth(budget.CategoryId);
        formError = null;
        form = new LimitForm { CategoryId = budget.CategoryId, Limit = budget.Limit };
        NewContext();

        await formModal.ShowAsync();
    }

    /// <summary>Focus goes back to the budget's "Delete" button when it was kept, otherwise to "Add budget".</summary>
    private async Task OnDeleteHiddenAsync()
    {
        var categoryId = deletingCategoryId;
        deletingCategoryId = null;

        if (categoryId is null)
            return;

        if (deleted)
        {
            deleteButtons.Remove(categoryId);
            editButtons.Remove(categoryId);
            StateHasChanged();
            await Task.Yield();
            await FintechDisplay.FocusAsync(addButton?.Element);
        }
        else if (deleteButtons.TryGetValue(categoryId, out var button))
        {
            await FintechDisplay.FocusAsync(button.Element);
        }
    }

    /// <summary>Focus goes to the new budget's "Edit limit" after adding, back to "Add budget" otherwise, or to the edited budget's "Edit limit".</summary>
    private async Task OnFormHiddenAsync()
    {
        var target = adding ? addedCategoryId : editingCategoryId;
        var wasAdding = adding;
        editingCategoryId = null;
        addedCategoryId = null;

        StateHasChanged();
        await Task.Yield();

        if (target is not null && editButtons.TryGetValue(target, out var button))
            await FintechDisplay.FocusAsync(button.Element);
        else if (wasAdding)
            await FintechDisplay.FocusAsync(addButton?.Element);
    }

    private string CategoryTakenMessage(string categoryId) =>
        $"{State.FindCategory(categoryId).Name} already has a budget for {MonthName}. Close this and change its limit instead.";

    private Task<bool> IsCategoryTakenAsync(string categoryId) => Task.FromResult(State.HasBudget(categoryId));

    /// <summary>A new form gets a new context; a changed field clears the state's message on it.</summary>
    private void NewContext()
    {
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += (_, args) => serverErrors.Clear(args.FieldIdentifier);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private static string ResultMessage(FintechResult result, string name) => result switch
    {
        FintechResult.NotAllowed => "Your session needs your password and one-time code again. Sign in, then try again.",
        FintechResult.NotFound => $"The {name} budget no longer exists. It may have been deleted.",
        FintechResult.Invalid => "Enter a limit from $1 to $100,000, in dollars and cents.",
        FintechResult.Duplicate => $"{name} already has a budget. Close this and change its limit instead.",
        FintechResult.NoChange => $"The {name} limit is already that amount.",
        _ => "Something went wrong. Try again.",
    };

    /// <summary>Saves once the form is valid: the category check finishes first, then every rule runs; the state checks again.</summary>
    private async Task SaveAsync()
    {
        formError = null;
        serverErrors?.Clear();
        var categoryFree = !adding || categoryCheck is null || await categoryCheck.CheckNowAsync();

        if (!await editContext.ValidateAsync() || !categoryFree)
            return;

        if (adding)
        {
            var name = State.Categories.FirstOrDefault(category => category.Id == form.CategoryId)?.Name ?? "This category";
            var result = State.AddBudget(form.CategoryId, form.Limit);

            if (result == FintechResult.Duplicate)
            {
                serverErrors?.Add(FieldIdentifier.Create(() => form.CategoryId), CategoryTakenMessage(form.CategoryId));
                editContext.NotifyValidationStateChanged();

                return;
            }

            if (result != FintechResult.Ok)
            {
                formError = result == FintechResult.Invalid && form.Limit is >= 1 and <= 100_000
                    ? "Enter the limit in dollars and cents, with no more than two decimals."
                    : ResultMessage(result, name);

                return;
            }

            addedCategoryId = form.CategoryId;
            statusMessage = $"Added a {name} budget of {FintechDisplay.Money(form.Limit)} a month.";
        }
        else if (editingCategoryId is { } categoryId)
        {
            var result = State.UpdateBudget(categoryId, form.Limit);

            switch (result)
            {
                case FintechResult.Ok:
                    statusMessage = $"The {editingName} budget is now {FintechDisplay.Money(form.Limit)} a month.";
                    break;
                case FintechResult.NoChange:
                    statusMessage = $"The {editingName} limit was already {FintechDisplay.Money(form.Limit)}, so nothing changed.";
                    break;
                default:
                    formError = result == FintechResult.Invalid && form.Limit is >= 1 and <= 100_000
                        ? "Enter the limit in dollars and cents, with no more than two decimals."
                        : ResultMessage(result, editingName);

                    return;
            }
        }

        await formModal.HideAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private int DaysLeft => DateTime.DaysInMonth(State.Today.Year, State.Today.Month) - State.Today.Day + 1;

    private string DaysLeftText => DaysLeft == 1 ? "1 day left" : $"{DaysLeft.ToString(CultureInfo.InvariantCulture)} days left";

    /// <summary>Gets the spending categories that have no budget yet.</summary>
    private IEnumerable<FintechCategory> FreeCategories =>
        State.Categories.Where(category => category.IsSpending && State.Budgets.All(budget => budget.CategoryId != category.Id));

    private string MonthName => State.Today.ToString("MMMM", CultureInfo.InvariantCulture);

    [Inject] private FintechState State { get; set; } = default!;

    private decimal TotalLimit => State.Budgets.Sum(budget => budget.Limit);

    private decimal TotalSpent => State.Budgets.Sum(budget => State.SpentThisMonth(budget.CategoryId));

    #endregion

    #region Nested Types

    /// <summary>The dialog's fields: a spending category (required; unique, checked by the page) and a monthly limit of $1 to $100,000 in cents.</summary>
    private sealed class LimitForm : IValidatableObject
    {
        [Required(ErrorMessage = "Choose a category.")]
        public string CategoryId { get; set; } = string.Empty;

        [Range(typeof(decimal), "1", "100000", ParseLimitsInInvariantCulture = true, ErrorMessage = "Enter a limit from $1 to $100,000.")]
        public decimal Limit { get; set; }

        /// <summary>Runs once the fields are filled in: the limit is in dollars and cents.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (decimal.Round(Limit, 2) != Limit)
                yield return new ValidationResult("Enter the limit in dollars and cents, with no more than two decimals.", new[] { nameof(Limit) });
        }
    }

    #endregion
}
