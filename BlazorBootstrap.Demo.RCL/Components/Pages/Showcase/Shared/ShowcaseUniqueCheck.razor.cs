using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// An asynchronous uniqueness check for one form field (rule WR-002): when the field changes, it waits briefly, shows
/// "Checking…", asks <see cref="IsTakenAsync" />, and adds a validation message when the value is taken. The message stays
/// until the value changes, so the form can't be submitted with a taken value. Before saving, a page calls
/// <see cref="CheckNowAsync" /> so a check still running finishes first:
/// <code>if (!await emailCheck.CheckNowAsync() || !editContext.Validate()) return;</code>
/// </summary>
public partial class ShowcaseUniqueCheck : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(350);

    private CancellationTokenSource? cancellation;

    private FieldIdentifier field;

    private bool isChecking;

    private ValidationMessageStore? store;

    private EditContext? subscribed;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnParametersSet()
    {
        if (ReferenceEquals(EditContext, subscribed))
            return;

        Unsubscribe();
        subscribed = EditContext ?? throw new InvalidOperationException($"{nameof(ShowcaseUniqueCheck)} must be inside an EditForm.");
        field = FieldIdentifier.Create(For);
        store = new ValidationMessageStore(subscribed);
        subscribed.OnFieldChanged += OnFieldChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Runs the check now (or waits for the one running) and returns whether the value is free.
    /// </summary>
    public async Task<bool> CheckNowAsync()
    {
        cancellation?.Cancel();
        return await RunAsync(CancellationToken.None, wait: false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        cancellation?.Cancel();
        Unsubscribe();
    }

    #endregion

    #region Private Methods

    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        if (!args.FieldIdentifier.Equals(field))
            return;

        // A new value clears the old answer at once; the new answer comes after the delay.
        store?.Clear(field);
        subscribed?.NotifyValidationStateChanged();

        cancellation?.Cancel();
        cancellation = new CancellationTokenSource();
        _ = RunAsync(cancellation.Token, wait: true);
    }

    private async Task<bool> RunAsync(CancellationToken token, bool wait)
    {
        var value = For.Compile().Invoke()?.Trim() ?? string.Empty;

        try
        {
            if (wait)
                await Task.Delay(Delay, token);

            if (value.Length == 0)
                return true;

            isChecking = true;
            await InvokeAsync(StateHasChanged);

            var taken = await IsTakenAsync(value);

            if (token.IsCancellationRequested)
                return !taken;

            store?.Clear(field);

            if (taken)
                store?.Add(field, TakenMessage(value));

            subscribed?.NotifyValidationStateChanged();

            return !taken;
        }
        catch (TaskCanceledException)
        {
            return true;
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                isChecking = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private void Unsubscribe()
    {
        if (subscribed is null)
            return;

        subscribed.OnFieldChanged -= OnFieldChanged;
        store?.Clear();
        subscribed = null;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the text shown while the check runs.</summary>
    [Parameter] public string CheckingText { get; set; } = "Checking…";

    [CascadingParameter] private EditContext? EditContext { get; set; }

    /// <summary>Gets or sets the field to check, for example <c>() => form.Email</c>.</summary>
    [Parameter, EditorRequired] public Expression<Func<string?>> For { get; set; } = default!;

    /// <summary>Gets or sets the check: returns whether the trimmed value is already taken (excluding the record being edited).</summary>
    [Parameter, EditorRequired] public Func<string, Task<bool>> IsTakenAsync { get; set; } = default!;

    /// <summary>Gets or sets the message for a taken value, for example <c>value => $"{value} already belongs to another employee."</c></summary>
    [Parameter, EditorRequired] public Func<string, string> TakenMessage { get; set; } = default!;

    #endregion
}
