using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Budgets: this month's spending against each category's limit, with the status in words and color, and a dialog to
/// change a limit. The dialog opens from code, so focus returns to the "Edit" button that opened it.
/// </summary>
public partial class Budgets : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<string, Button> editButtons = new();

    private EditContext editContext = default!;

    private string? editingCategoryId;

    private string editingName = string.Empty;

    private decimal editingSpent;

    private Modal editModal = default!;

    private LimitForm form = new();

    private string? savedMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        editContext = new EditContext(form);
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

    private async Task EditAsync(FintechBudget budget)
    {
        editingCategoryId = budget.CategoryId;
        editingName = State.FindCategory(budget.CategoryId).Name;
        editingSpent = State.SpentThisMonth(budget.CategoryId);
        form = new LimitForm { Limit = budget.Limit };
        editContext = new EditContext(form);

        await editModal.ShowAsync();
    }

    /// <summary>The dialog opens from code, so Bootstrap does not return focus; it goes back to the budget's "Edit" button.</summary>
    private async Task OnEditHiddenAsync()
    {
        var categoryId = editingCategoryId;
        editingCategoryId = null;

        if (categoryId is not null && editButtons.TryGetValue(categoryId, out var button))
            await FintechDisplay.FocusAsync(button.Element);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task SaveAsync()
    {
        if (editingCategoryId is null)
            return;

        State.UpdateBudget(editingCategoryId, form.Limit);
        savedMessage = $"The {editingName} budget is now {FintechDisplay.Money(Math.Round(form.Limit, 2))} a month.";

        await editModal.HideAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private int DaysLeft => DateTime.DaysInMonth(FintechSeedData.Today.Year, FintechSeedData.Today.Month) - FintechSeedData.Today.Day + 1;

    [Inject] private FintechState State { get; set; } = default!;

    private decimal TotalLimit => State.Budgets.Sum(budget => budget.Limit);

    private decimal TotalSpent => State.Budgets.Sum(budget => State.SpentThisMonth(budget.CategoryId));

    #endregion

    #region Nested Types

    private sealed class LimitForm
    {
        [Range(typeof(decimal), "1", "100000", ParseLimitsInInvariantCulture = true, ErrorMessage = "Enter a limit from $1 to $100,000.")]
        public decimal Limit { get; set; }
    }

    #endregion
}
