using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// Adds a listing for the signed-in host or edits one of theirs (a <see cref="TravelStayDraft" />): name, destination,
/// neighborhood, type, description, nightly rate and cleaning fee (with what a guest pays for <see cref="ExampleNights" />
/// nights, taxes included, worked out as they type), guests and bedrooms, amenities, cancellation policy, instant book or
/// request to book, the address (which guests only see on a confirmed trip), and photos: the chosen ones in order with
/// move and remove buttons, and the photo library to pick from (up to <see cref="TravelStay.MaxPhotos" />). Each rule
/// <see cref="TravelState.SaveStay" /> enforces is checked here first and shown at its field; the results it can still
/// return (Duplicate name, Invalid, NotFound, NotAllowed) map to messages. A save goes back to the listings, which
/// report it. Another host's or an unknown listing shows a not-found screen.
/// </summary>
public partial class HostListingEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The nights the guest price example is worked out for.</summary>
    private const int ExampleNights = 3;

    private const int MaxAddressLength = 120;

    private const int MaxBedrooms = 8;

    private const decimal MaxCleaning = 500m;

    private const int MaxNeighborhoodLength = 40;

    private const decimal MaxRate = 2000m;

    private const decimal MinRate = 20m;

    private static readonly IReadOnlyList<string> amenityCatalog = TravelSeedData.AmenityCatalog();

    private static readonly IReadOnlyList<string> photoLibrary = TravelSeedData.PhotoLibrary();

    private readonly ElementReference[] downButtons = new ElementReference[TravelStay.MaxPhotos];

    private readonly ElementReference[] removeButtons = new ElementReference[TravelStay.MaxPhotos];

    private readonly ElementReference[] upButtons = new ElementReference[TravelStay.MaxPhotos];

    private string? addressError;

    private ElementReference addressInput;

    private string? bedroomsError;

    private ElementReference bedroomsSelect;

    private string? cleaningError;

    private ElementReference cleaningInput;

    private string cleaningText = string.Empty;

    private string? descriptionError;

    private ElementReference descriptionInput;

    private TravelStayDraft draft = new();

    private int formErrorCount;

    private string? formError;

    private ElementReference formErrorRegion;

    private ElementReference guestsSelect;

    private ElementReference libraryToggle;

    /// <summary>The route ID the form was filled from (0 for a new listing); <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    private string? nameError;

    private ElementReference nameInput;

    private string? neighborhoodError;

    private ElementReference neighborhoodInput;

    /// <summary>Where focus goes after the next render, when a photo moved or was removed: a button of the photo at an index.</summary>
    private (PhotoButton Button, int Index)? pendingFocus;

    private string? photoAnnouncement;

    private ElementReference photosHeading;

    private string? photosError;

    private string? rateError;

    private ElementReference rateInput;

    private string rateText = string.Empty;

    private bool showLibrary;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The buttons' references are set during the render, so the target is looked up only now.
        if (pendingFocus is { } target)
        {
            pendingFocus = null;
            var index = Math.Clamp(target.Index, 0, TravelStay.MaxPhotos - 1);

            await TravelDisplay.FocusAsync(draft.Photos.Count == 0 ? libraryToggle : target.Button switch
            {
                PhotoButton.Up => upButtons[index],
                PhotoButton.Down => downButtons[index],
                _ => removeButtons[index],
            });
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        var routeId = Id ?? 0;

        if (loadedId == routeId)
            return;

        loadedId = routeId;
        Load(Item);
    }

    #endregion

    #region Public Methods

    /// <summary>Returns the route of the editor of one of the host's listings.</summary>
    public static string EditUrl(int stayId) => FormattableString.Invariant($"{ListUrl}/{stayId}");

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>
    /// Clears <see cref="Id" /> first: the new-listing route has no ID, and the router reuses this page when moving from a
    /// listing to "Add listing", so the previous listing's ID would otherwise stay.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Id = null;

        return base.SetParametersAsync(parameters);
    }

    #endregion

    #region Private Methods

    private static string? AriaInvalid(string? error) => error is null ? null : "true";

    /// <summary>Returns the accent of the destination a library photo comes from, for its placeholder.</summary>
    private string AccentOf(string key)
    {
        var dash = key.IndexOf('-', StringComparison.Ordinal);

        return dash > 0 && int.TryParse(key[..dash], NumberStyles.None, CultureInfo.InvariantCulture, out var stayId) && State.FindStay(stayId) is { } stay
            ? State.FindDestination(stay.DestinationId).Accent
            : State.Destinations[0].Accent;
    }

    private static string AmenitySlug(string amenity) => new(amenity.ToLowerInvariant().Select(character => char.IsAsciiLetterOrDigit(character) ? character : '-').ToArray());

    private static string CounterText(string? value, int max)
    {
        var length = value?.Trim().Length ?? 0;

        return length > max
            ? string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters: {length - max} over the limit")
            : string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters");
    }

    private static string? Invalid(string? error) => error is null ? null : "is-invalid";

    private static bool Over(string? value, int max) => (value?.Trim().Length ?? 0) > max;

    /// <summary>Reads a money amount from a native number input (always "1234.5" style), or <see langword="null" />.</summary>
    private static decimal? ParseMoney(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) ? value : null;

    private static string? CleaningProblem(string text) =>
        string.IsNullOrWhiteSpace(text) ? "Enter a cleaning fee; use 0 for none."
        : ParseMoney(text) is not { } fee ? "Enter the cleaning fee as a number, like 40."
        : fee > MaxCleaning ? $"Use a cleaning fee of {TravelDisplay.Money(MaxCleaning)} or less."
        : decimal.Round(fee, 2) != fee ? "Use whole cents: at most two decimal places."
        : null;

    private static string? RateProblem(string text) =>
        string.IsNullOrWhiteSpace(text) ? "Enter a nightly rate."
        : ParseMoney(text) is not { } rate ? "Enter the nightly rate as a number, like 120."
        : rate is < MinRate or > MaxRate ? $"Use a nightly rate from {TravelDisplay.Money(MinRate)} to {TravelDisplay.Money(MaxRate)}."
        : decimal.Round(rate, 2) != rate ? "Use whole cents: at most two decimal places."
        : null;

    private static string? TextProblem(string? value, int max, string missing, string field)
    {
        var length = value?.Trim().Length ?? 0;

        return length == 0 ? missing
            : length > max ? string.Create(CultureInfo.InvariantCulture, $"Shorten the {field} to {max} characters or fewer (now {length}).")
            : null;
    }

    private string? BedroomsProblem() =>
        draft.Bedrooms > draft.MaxGuests
            ? string.Create(CultureInfo.InvariantCulture, $"A place can't have more bedrooms than guests: choose up to {(draft.MaxGuests == 1 ? "1 bedroom" : $"{draft.MaxGuests} bedrooms")}, or allow more guests.")
            : null;

    /// <summary>Fills the form from a listing, or with a new listing's defaults (in the destination of the host's other listings).</summary>
    private void Load(TravelStay? stay)
    {
        draft = stay is null
            ? new TravelStayDraft { DestinationId = State.HostStays().FirstOrDefault()?.DestinationId ?? State.Destinations[0].Id }
            : TravelStayDraft.From(stay);
        rateText = draft.NightlyRate.ToString(CultureInfo.InvariantCulture);
        cleaningText = draft.CleaningFee.ToString(CultureInfo.InvariantCulture);
        nameError = neighborhoodError = descriptionError = rateError = cleaningError = bedroomsError = addressError = photosError = null;
        formError = null;
        formErrorCount = 0;
        photoAnnouncement = null;
        showLibrary = stay is null;
    }

    private void MovePhoto(int index, int step)
    {
        var target = index + step;

        if (index < 0 || index >= draft.Photos.Count || target < 0 || target >= draft.Photos.Count)
            return;

        (draft.Photos[index], draft.Photos[target]) = (draft.Photos[target], draft.Photos[index]);
        photoAnnouncement = string.Create(CultureInfo.InvariantCulture,
            $"Photo {draft.Photos[target]} is now photo {target + 1} of {draft.Photos.Count}{(target == 0 ? ", the cover" : string.Empty)}.");

        // Focus follows the photo: the same button at its new place, or the other move button at the end of the list.
        var keepsDirection = step < 0 ? target > 0 : target < draft.Photos.Count - 1;
        pendingFocus = ((step < 0) == keepsDirection ? PhotoButton.Up : PhotoButton.Down, target);
    }

    private void OnAddressInput(ChangeEventArgs args)
    {
        draft.Address = args.Value?.ToString() ?? string.Empty;

        if (addressError is not null)
            addressError = TextProblem(draft.Address, MaxAddressLength, "Enter the street address.", "address");
    }

    private void OnAmenityChanged(string amenity, ChangeEventArgs args)
    {
        draft.Amenities.Remove(amenity);

        if (args.Value is true)
            draft.Amenities = amenityCatalog.Where(item => item == amenity || draft.Amenities.Contains(item)).ToList();
    }

    private void OnBedroomsChanged(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var bedrooms) && bedrooms is >= 0 and <= MaxBedrooms)
            draft.Bedrooms = bedrooms;

        if (bedroomsError is not null)
            bedroomsError = BedroomsProblem();
    }

    private void OnCleaningInput(ChangeEventArgs args)
    {
        cleaningText = args.Value?.ToString() ?? string.Empty;

        if (cleaningError is not null)
            cleaningError = CleaningProblem(cleaningText);
    }

    private void OnDescriptionInput(ChangeEventArgs args)
    {
        draft.Description = args.Value?.ToString() ?? string.Empty;

        if (descriptionError is not null)
            descriptionError = TextProblem(draft.Description, TravelState.MaxDescriptionLength, "Write a description of the place.", "description");
    }

    private void OnDestinationChanged(ChangeEventArgs args)
    {
        if (State.Destinations.FirstOrDefault(destination => destination.Id == args.Value?.ToString()) is { } destination)
            draft.DestinationId = destination.Id;
    }

    private void OnGuestsChanged(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var guests) && guests is >= 1 and <= TravelSeedData.MaxGuests)
            draft.MaxGuests = guests;

        if (bedroomsError is not null)
            bedroomsError = BedroomsProblem();
    }

    private void OnInstantBookChanged(ChangeEventArgs args) => draft.InstantBook = args.Value is true;

    /// <summary>A ticked photo goes to the end of the list; an unticked one leaves it. The limit is checked again here.</summary>
    private void OnLibraryChanged(string key, ChangeEventArgs args)
    {
        if (args.Value is true)
        {
            if (!draft.Photos.Contains(key) && draft.Photos.Count < TravelStay.MaxPhotos)
                draft.Photos.Add(key);
        }
        else
        {
            draft.Photos.Remove(key);
        }

        if (photosError is not null && draft.Photos.Count > 0)
            photosError = null;
    }

    private void OnNameInput(ChangeEventArgs args)
    {
        draft.Name = args.Value?.ToString() ?? string.Empty;

        if (nameError is not null)
            nameError = TextProblem(draft.Name, TravelState.MaxStayNameLength, "Give the listing a name.", "name");
    }

    private void OnNeighborhoodInput(ChangeEventArgs args)
    {
        draft.Neighborhood = args.Value?.ToString() ?? string.Empty;

        if (neighborhoodError is not null)
            neighborhoodError = TextProblem(draft.Neighborhood, MaxNeighborhoodLength, "Enter the neighborhood guests will stay in.", "neighborhood");
    }

    private void OnPolicyChanged(TravelCancellationPolicy policy) => draft.Policy = policy;

    private void OnRateInput(ChangeEventArgs args)
    {
        rateText = args.Value?.ToString() ?? string.Empty;

        if (rateError is not null)
            rateError = RateProblem(rateText);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnTypeChanged(ChangeEventArgs args)
    {
        if (Enum.TryParse<TravelStayType>(args.Value?.ToString(), out var type) && Enum.IsDefined(type))
            draft.Type = type;
    }

    /// <summary>Whether the rate typed differs from the listing's current rate.</summary>
    private bool RateChanged(TravelStay stay) => ParseMoney(rateText) is { } rate && rate != stay.NightlyRate;

    private void RemovePhoto(int index)
    {
        if (index < 0 || index >= draft.Photos.Count)
            return;

        var key = draft.Photos[index];
        draft.Photos.RemoveAt(index);
        photoAnnouncement = draft.Photos.Count == 0
            ? $"Photo {key} removed. No photos are left: add at least one."
            : string.Create(CultureInfo.InvariantCulture, $"Photo {key} removed. {draft.Photos.Count} left.");

        // Focus goes to the photo that took its place, the one before it, or the "Add photos" button.
        pendingFocus = (PhotoButton.Remove, Math.Min(index, draft.Photos.Count - 1));
    }

    /// <summary>
    /// Checks every field, then saves. The first field with a problem gets focus; the form-level alert lists how many need
    /// attention. A save goes back to the listings with the result in their status message.
    /// </summary>
    private async Task SaveAsync()
    {
        formError = null;
        nameError = TextProblem(draft.Name, TravelState.MaxStayNameLength, "Give the listing a name.", "name");
        neighborhoodError = TextProblem(draft.Neighborhood, MaxNeighborhoodLength, "Enter the neighborhood guests will stay in.", "neighborhood");
        descriptionError = TextProblem(draft.Description, TravelState.MaxDescriptionLength, "Write a description of the place.", "description");
        rateError = RateProblem(rateText);
        cleaningError = CleaningProblem(cleaningText);
        bedroomsError = BedroomsProblem();
        addressError = TextProblem(draft.Address, MaxAddressLength, "Enter the street address.", "address");
        photosError = draft.Photos.Count == 0 ? "Add at least one photo from the photo library."
            : draft.Photos.Count > TravelStay.MaxPhotos ? string.Create(CultureInfo.InvariantCulture, $"Use up to {TravelStay.MaxPhotos} photos.")
            : null;

        var problems = new (string? Error, string Field, ElementReference Target)[]
        {
            (nameError, "name", nameInput),
            (neighborhoodError, "neighborhood", neighborhoodInput),
            (descriptionError, "description", descriptionInput),
            (rateError, "nightly rate", rateInput),
            (cleaningError, "cleaning fee", cleaningInput),
            (bedroomsError, "bedrooms", bedroomsSelect),
            (addressError, "address", addressInput),
            (photosError, "photos", photosHeading),
        }.Where(problem => problem.Error is not null).ToList();

        if (problems.Count > 0)
        {
            ShowFormError(problems.Count == 1
                ? $"Check the {problems[0].Field}, then save again."
                : $"Check these {problems.Count} fields, then save again: {string.Join(", ", problems.Select(problem => problem.Field))}.");
            await TravelDisplay.FocusAsync(problems[0].Target);

            return;
        }

        draft.NightlyRate = ParseMoney(rateText)!.Value;
        draft.CleaningFee = ParseMoney(cleaningText)!.Value;
        draft.Name = draft.Name.Trim();

        var before = Item;
        var result = State.SaveStay(before?.Id ?? 0, draft, out var saved);

        switch (result)
        {
            case TravelResult.Ok when saved is not null:
                Navigation.NavigateTo(before is null ? $"{ListUrl}?saved={saved.Id.ToString(CultureInfo.InvariantCulture)}&added=true" : $"{ListUrl}?saved={saved.Id.ToString(CultureInfo.InvariantCulture)}");
                break;

            case TravelResult.Duplicate:
                nameError = "Another listing on the site already has this name. Use a different one, so guests can tell them apart.";
                ShowFormError("Check the name, then save again.");
                await TravelDisplay.FocusAsync(nameInput);
                break;

            case TravelResult.Invalid:
                ShowFormError(before is null
                    ? FormattableString.Invariant($"The listing couldn't be added: a value is missing or out of range, or the site already holds its limit of {TravelState.MaxStays} listings. Check every field and try again.")
                    : "The listing couldn't be saved: a value is missing or out of range. Check every field and try again.");
                await TravelDisplay.FocusAsync(formErrorRegion);
                break;

            case TravelResult.NotFound:
                ShowFormError("This listing couldn't be saved: it's no longer one of yours. It may have been deleted.");
                await TravelDisplay.FocusAsync(formErrorRegion);
                break;

            case TravelResult.NotAllowed:
                ShowFormError("Sign in as a host to save listings. Nothing was saved.");
                await TravelDisplay.FocusAsync(formErrorRegion);
                break;

            default:
                ShowFormError("The listing couldn't be saved. Try again.");
                await TravelDisplay.FocusAsync(formErrorRegion);
                break;
        }
    }

    /// <summary>Shows a form-level error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void ShowFormError(string message)
    {
        formErrorCount++;
        formError = formErrorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrorCount})");
    }

    private void ToggleLibrary() => showLibrary = !showLibrary;

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Listings", Href = ListUrl },
        new() { Text = Item?.Name ?? "Add listing", IsCurrentPage = true },
    };

    /// <summary>The price a guest sees for <see cref="ExampleNights" /> nights at the rate and fee typed, or <see langword="null" /> while either is invalid.</summary>
    private TravelQuote? Example
    {
        get
        {
            if (RateProblem(rateText) is not null || CleaningProblem(cleaningText) is not null)
                return null;

            var rate = ParseMoney(rateText)!.Value;
            var subtotal = rate * ExampleNights;
            var taxes = Math.Round(subtotal * TravelSeedData.TaxRate, 2, MidpointRounding.AwayFromZero);

            return new TravelQuote(ExampleNights, rate, subtotal, ParseMoney(cleaningText)!.Value, taxes);
        }
    }

    /// <summary>Gets or sets the listing ID from the route; <see langword="null" /> on the new-listing route.</summary>
    [Parameter] public int? Id { get; set; }

    /// <summary>The host's listing being edited, or <see langword="null" /> for a new listing or one that isn't theirs.</summary>
    private TravelStay? Item => Id is int id ? State.HostStays().FirstOrDefault(stay => stay.Id == id) : null;

    /// <summary>Gets the route of the host's listings.</summary>
    public static string ListUrl => $"{HostLayout.Base}/listings";

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets the route of the new-listing form.</summary>
    public static string NewUrl => $"{ListUrl}/new";

    private string PhotoCountText => draft.Photos.Count == 1
        ? FormattableString.Invariant($"1 of {TravelStay.MaxPhotos} photos")
        : FormattableString.Invariant($"{draft.Photos.Count} of {TravelStay.MaxPhotos} photos");

    [Inject] private TravelState State { get; set; } = default!;

    private static string TaxPercent => TravelSeedData.TaxRate.ToString("0%", CultureInfo.InvariantCulture);

    #endregion

    #region Nested Types

    /// <summary>The buttons of a chosen photo, for moving focus after a move or a removal.</summary>
    private enum PhotoButton
    {
        Up,
        Down,
        Remove,
    }

    #endregion
}
