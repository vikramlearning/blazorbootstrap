using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// The card fields of a payment form. It sits inside the page's <see cref="EditForm" /> and validates the card with
/// <see cref="ShowcaseCards.Validate" /> on submit and as each field changes, so the page's own form blocks submission
/// until the card is valid. The page then calls <see cref="ShowcaseCards.Charge" />, which keeps only the brand and the
/// last four digits.
/// </summary>
public partial class ShowcaseCardForm : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext? subscribedContext;

    private ValidationMessageStore? messages;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnParametersSet()
    {
        if (ReferenceEquals(EditContext, subscribedContext))
            return;

        Unsubscribe();
        subscribedContext = EditContext;
        messages = new ValidationMessageStore(EditContext);
        EditContext.OnValidationRequested += OnValidationRequested;
        EditContext.OnFieldChanged += OnFieldChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops validating with the page's form.</summary>
    public void Dispose() => Unsubscribe();

    #endregion

    #region Private Methods

    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        if (!ReferenceEquals(args.FieldIdentifier.Model, Card))
            return;

        var errors = ShowcaseCards.Validate(Card, Today);
        Recheck(args.FieldIdentifier, errors);

        // The brand decides the security-code length, so a shown code message is re-checked when the number changes.
        var cvc = new FieldIdentifier(Card, nameof(ShowcaseCardInput.Cvc));

        if (args.FieldIdentifier.FieldName == nameof(ShowcaseCardInput.Number) && EditContext.GetValidationMessages(cvc).Any())
            Recheck(cvc, errors);

        EditContext.NotifyValidationStateChanged();
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs args)
    {
        messages!.Clear();

        foreach (var (field, message) in ShowcaseCards.Validate(Card, Today))
            messages.Add(new FieldIdentifier(Card, field), message);
    }

    private void Recheck(FieldIdentifier field, Dictionary<string, string> errors)
    {
        messages!.Clear(field);

        if (errors.TryGetValue(field.FieldName, out var message))
            messages.Add(field, message);
    }

    private void Unsubscribe()
    {
        if (subscribedContext is null)
            return;

        subscribedContext.OnValidationRequested -= OnValidationRequested;
        subscribedContext.OnFieldChanged -= OnFieldChanged;
        messages?.Clear();
        subscribedContext = null;
    }

    private void Use(ShowcaseTestCard test)
    {
        Card.Number = test.Display;
        EditContext.NotifyFieldChanged(new FieldIdentifier(Card, nameof(ShowcaseCardInput.Number)));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string BrandText => ShowcaseCards.Digits(Card.Number).Length >= 2 && ShowcaseCards.Brand(Card.Number) is { } brand ? brand : "Card";

    /// <summary>Gets or sets the card being typed; the page owns it and clears it after the charge.</summary>
    [Parameter, EditorRequired] public ShowcaseCardInput Card { get; set; } = default!;

    [CascadingParameter] private EditContext EditContext { get; set; } = default!;

    /// <summary>Gets or sets the prefix of the field IDs, unique on the page.</summary>
    [Parameter] public string IdPrefix { get; set; } = "sc";

    /// <summary>Gets or sets the site's current date, for the expiry check.</summary>
    [Parameter, EditorRequired] public DateTime Today { get; set; }

    #endregion
}
