using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// The calendar of one of the signed-in host's listings (<see cref="StayParam" />, or the first): one month at a time with
/// Previous and Next, today marked. Every night says in words whether it is booked (with the guest and a link to the
/// reservation), requested (likewise), blocked (with the host's note), free, or past; color only repeats that. Wide screens
/// get a month table with a caption and weekday headers; narrow ones a list of runs of nights. Below it, a form blocks
/// nights (<see cref="TravelState.AddBlock" />; taken nights are named when they aren't free) and a list of the listing's
/// blocks removes one after a confirmation (<see cref="TravelState.RemoveBlock" />). Every result maps to a message.
/// </summary>
public partial class HostCalendar : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The longest block note (the state's limit).</summary>
    private const int MaxNoteLength = 80;

    /// <summary>How many months before this one the calendar goes back, for recent stays.</summary>
    private const int MonthsBack = 6;

    private static readonly string[] weekdays = CultureInfo.InvariantCulture.DateTimeFormat.DayNames;

    private readonly Dictionary<string, ElementReference> removeButtons = new();

    private DateOnly blockFirst;

    private DateOnly blockLast;

    private string blockNote = string.Empty;

    private string? datesError;

    private string? dialogError;

    private DateInput<DateOnly>? firstInput;

    /// <summary>The query value the listing was picked from, so a new query picks again but a re-render doesn't.</summary>
    private int? loadedParam;

    private bool loaded;

    private ElementReference nextButton;

    private string? noteError;

    private ElementReference noteInput;

    /// <summary>The block in the remove dialog.</summary>
    private TravelBlock? pendingBlock;

    private ElementReference previousButton;

    private bool removeDone;

    private Modal removeModal = default!;

    /// <summary>The listing shown, picked from the query or the select.</summary>
    private int selectedStayId;

    /// <summary>The first day of the month shown; clamped to the allowed months when read (<see cref="FirstMonth" />).</summary>
    private DateOnly shownMonth;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    /// <summary>Says why the listing shown isn't the one in the query, or <see langword="null" />.</summary>
    private string? stayNotice;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        shownMonth = StartOfMonth(State.Today);
        blockFirst = State.Today;
        blockLast = State.Today;
    }

    protected override void OnParametersSet()
    {
        if (loaded && loadedParam == StayParam)
            return;

        loaded = true;
        loadedParam = StayParam;
        var listings = State.HostStays();
        var asked = StayParam is int id ? listings.FirstOrDefault(stay => stay.Id == id) : null;

        selectedStayId = asked?.Id ?? listings.FirstOrDefault()?.Id ?? 0;
        stayNotice = StayParam is not null && asked is null && listings.Count > 0
            ? FormattableString.Invariant($"Listing {StayParam} isn't one of yours, so the calendar shows {listings[0].Name}.")
            : null;
        ResetBlockForm();
    }

    #endregion

    #region Public Methods

    /// <summary>Returns the route of the calendar of one of the host's listings.</summary>
    public static string CalendarUrl(int stayId) => FormattableString.Invariant($"{HostLayout.Base}/calendar?stay={stayId}");

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string BlockKey(TravelBlock block) => FormattableString.Invariant($"{block.StayId}-{block.Start.DayNumber}-{block.End.DayNumber}");

    /// <summary>Returns the nights a block covers, like "Dec 8 – Dec 11, 2026" (its end is the first free day, so the last night is the day before).</summary>
    private static string BlockRange(TravelBlock block) => RangeText(block.Start, block.End.AddDays(-1), withYear: true);

    private static string CounterText(string value, int max) => string.Create(CultureInfo.InvariantCulture, $"{value.Trim().Length} of {max} characters");

    private bool HoldsDates(TravelBooking booking) => State.HoldsDates(booking);

    private static string KindClass(NightKind kind) => kind switch
    {
        NightKind.Booked => "ds-hc-booked",
        NightKind.Requested => "ds-hc-requested",
        NightKind.Blocked => "ds-hc-blocked",
        NightKind.Past => "ds-hc-past",
        _ => "ds-hc-free",
    };

    private static string KindText(NightKind kind) => kind switch
    {
        NightKind.Booked => "Booked",
        NightKind.Requested => "Requested",
        NightKind.Blocked => "Blocked",
        NightKind.Past => "Past",
        _ => "Free",
    };

    /// <summary>Formats a date with its weekday, like "Tue, Nov 24".</summary>
    private static string LongDay(DateOnly date) => date.ToString("ddd, MMM d", CultureInfo.InvariantCulture);

    private static string MonthName(DateOnly month) => month.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>Formats the nights from <paramref name="first" /> to <paramref name="last" />, like "Dec 8 – Dec 11" or "Dec 8".</summary>
    private static string RangeText(DateOnly first, DateOnly last, bool withYear = false)
    {
        var end = withYear ? last.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) : TravelDisplay.Day(last);

        return first == last ? end : $"{TravelDisplay.Day(first)} – {end}";
    }

    /// <summary>Returns the reservation page of a booking in the host back office.</summary>
    private static string ReservationUrl(string reference) => $"{HostLayout.Base}/reservations/{Uri.EscapeDataString(reference)}";

    /// <summary>Returns a guest's first name and last initial, like "Jordan A.", to fit a calendar cell.</summary>
    private static string ShortName(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length < 2 ? name : $"{parts[0]} {char.ToUpperInvariant(parts[^1][0])}.";
    }

    private static DateOnly StartOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    /// <summary>Returns the weeks of a month, Sunday first, with <see langword="null" /> for the days outside it.</summary>
    private static IEnumerable<DateOnly?[]> Weeks(DateOnly month)
    {
        var week = new DateOnly?[7];
        var column = (int)month.DayOfWeek;

        for (var date = month; date.Month == month.Month; date = date.AddDays(1))
        {
            week[column++] = date;

            if (column == 7)
            {
                yield return week;
                week = new DateOnly?[7];
                column = 0;
            }
        }

        if (column > 0)
            yield return week;
    }

    /// <summary>Checks the dates and note, then blocks the nights; taken nights are named when they aren't free.</summary>
    private async Task AddBlockAsync()
    {
        if (Stay is not { } stay)
            return;

        var end = blockLast.AddDays(1);
        var nights = end.DayNumber - blockFirst.DayNumber;
        var note = blockNote.Trim();

        datesError = blockFirst < State.Today ? "The first night can't be in the past."
            : blockLast < blockFirst ? "The last night must be on or after the first night."
            : nights > TravelState.MaxBlockNights ? FormattableString.Invariant($"Block up to {TravelState.MaxBlockNights} nights at a time (now {nights}). Split longer breaks into several blocks.")
            : end > State.Today.AddDays(366) ? "Block nights up to a year ahead."
            : null;
        noteError = note.Length > MaxNoteLength ? FormattableString.Invariant($"Shorten the note to {MaxNoteLength} characters or fewer (now {note.Length}).") : null;

        if (datesError is not null)
        {
            await TravelDisplay.FocusAsync(firstInput?.Element);

            return;
        }

        if (noteError is not null)
        {
            await TravelDisplay.FocusAsync(noteInput);

            return;
        }

        var result = State.AddBlock(stay.Id, blockFirst, end, note);

        switch (result)
        {
            case TravelResult.Ok:
                SetStatus($"Blocked {TravelDisplay.Nights(nights)} on {stay.Name}: {RangeText(blockFirst, blockLast, withYear: true)}. Guests can't book them.");
                shownMonth = StartOfMonth(blockFirst);
                ResetBlockForm();
                await TravelDisplay.FocusAsync(statusRegion);
                break;

            case TravelResult.Unavailable:
                datesError = $"Some of those nights aren't free: {TakenText(stay.Id, blockFirst, end)}. Nothing was blocked; choose free nights.";
                await TravelDisplay.FocusAsync(firstInput?.Element);
                break;

            case TravelResult.Invalid:
                datesError = FormattableString.Invariant($"Those nights can't be blocked: start from today, block up to {TravelState.MaxBlockNights} nights within the next year, and keep the note to {MaxNoteLength} characters.");
                await TravelDisplay.FocusAsync(firstInput?.Element);
                break;

            case TravelResult.NotFound:
                datesError = "This listing isn't one of yours any more, so nothing was blocked.";
                break;

            case TravelResult.NotAllowed:
                datesError = "Sign in as the host of this listing to block nights. Nothing was blocked.";
                break;

            default:
                datesError = "The nights couldn't be blocked. Try again.";
                break;
        }
    }

    private async Task AskRemoveAsync(TravelBlock block)
    {
        pendingBlock = block;
        dialogError = null;
        removeDone = false;

        await removeModal.ShowAsync();
    }

    /// <summary>Says how many nights the form would block, like "Blocks 3 nights: Dec 8 – Dec 10, 2026."</summary>
    private string BlockSummary(TravelStay stay)
    {
        if (blockLast < blockFirst)
            return "Choose a last night on or after the first night.";

        var nights = blockLast.DayNumber - blockFirst.DayNumber + 1;

        return $"Blocks {TravelDisplay.Nights(nights)} on {stay.Name}: {RangeText(blockFirst, blockLast, withYear: true)}. Both nights are included.";
    }

    private async Task ConfirmRemoveAsync()
    {
        if (pendingBlock is not { } block)
            return;

        var result = State.RemoveBlock(block.StayId, block.Start, block.End);

        switch (result)
        {
            case TravelResult.Ok:
                removeDone = true;
                SetStatus($"The block on {BlockRange(block)} is removed. Those nights are free again.");
                await removeModal.HideAsync();
                break;

            case TravelResult.NotFound:
                removeDone = true;
                SetStatus($"The block on {BlockRange(block)} was already removed.");
                await removeModal.HideAsync();
                break;

            case TravelResult.NotAllowed:
                dialogError = "Sign in as the host of this listing to remove blocks. Nothing changed.";
                break;

            default:
                dialogError = "The block couldn't be removed. Try again.";
                break;
        }
    }

    private async Task NextAsync()
    {
        if (!CanGoForward)
            return;

        shownMonth = FirstMonth.AddMonths(1);

        // A button that becomes disabled loses focus, so focus moves to the other one.
        if (!CanGoForward)
        {
            await TravelDisplay.FocusAsync(previousButton);
        }
    }

    /// <summary>Returns every night of a month of a listing: who has it, the block on it, or whether it's free or past.</summary>
    private IReadOnlyList<Night> NightsOf(int stayId, DateOnly month)
    {
        var end = month.AddMonths(1);
        var bookings = State.HostBookings().Where(booking => booking.StayId == stayId && booking.CheckIn < end && booking.CheckOut > month && HoldsDates(booking)).ToList();
        var blocks = State.BlocksFor(stayId).Where(block => block.Start < end && block.End > month).ToList();
        var nights = new List<Night>(31);

        for (var date = month; date < end; date = date.AddDays(1))
        {
            var night = date;
            var booking = bookings.FirstOrDefault(candidate => candidate.CheckIn <= night && night < candidate.CheckOut);
            var block = booking is null ? blocks.FirstOrDefault(candidate => candidate.Start <= night && night < candidate.End) : null;
            var kind = booking is not null ? (booking.Status == TravelBookingStatus.Requested ? NightKind.Requested : NightKind.Booked)
                : block is not null ? NightKind.Blocked
                : night < State.Today ? NightKind.Past
                : NightKind.Free;

            nights.Add(new Night(night, kind, booking, block));
        }

        return nights;
    }

    private void OnFirstChanged(DateOnly value)
    {
        var length = Math.Max(0, blockLast.DayNumber - blockFirst.DayNumber);

        blockFirst = value;

        // Moving the first night keeps the block's length, so the last night never ends up before it.
        if (blockLast < value)
            blockLast = value.AddDays(length);

        datesError = null;
    }

    private void OnLastChanged(DateOnly value)
    {
        if (value >= blockFirst)
            blockLast = value;

        datesError = null;
    }

    private void OnNoteInput(ChangeEventArgs args)
    {
        blockNote = args.Value?.ToString() ?? string.Empty;

        if (noteError is not null && blockNote.Trim().Length <= MaxNoteLength)
            noteError = null;
    }

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: after a removal it goes to the status message, otherwise back to the Remove button.</summary>
    private async Task OnRemoveHiddenAsync()
    {
        var block = pendingBlock;
        pendingBlock = null;
        dialogError = null;

        if (removeDone)
        {
            removeDone = false;
            await TravelDisplay.FocusAsync(statusRegion);
        }
        else if (block is not null && removeButtons.TryGetValue(BlockKey(block), out var button))
        {
            await TravelDisplay.FocusAsync(button);
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Picking a listing doesn't navigate, so focus stays on the select; the month shown stays too.</summary>
    private void OnStayChanged(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var id) && State.HostStays().Any(stay => stay.Id == id))
        {
            selectedStayId = id;
            stayNotice = null;
            ResetBlockForm();
        }
    }

    private async Task PreviousAsync()
    {
        if (!CanGoBack)
            return;

        shownMonth = FirstMonth.AddMonths(-1);

        // A button that becomes disabled loses focus, so focus moves to the other one.
        if (!CanGoBack)
        {
            await TravelDisplay.FocusAsync(nextButton);
        }
    }

    private void ResetBlockForm()
    {
        blockFirst = State.Today;
        blockLast = State.Today;
        blockNote = string.Empty;
        datesError = null;
        noteError = null;
    }

    /// <summary>Returns the runs of a month's nights with the same status and booking or block, for the narrow-screen list.</summary>
    private static IEnumerable<Run> Runs(IReadOnlyList<Night> nights)
    {
        Run? current = null;

        foreach (var night in nights)
        {
            if (current is { } run && run.Kind == night.Kind && ReferenceEquals(run.Booking, night.Booking) && run.Block == night.Block)
            {
                current = run with { Last = night.Date };
                continue;
            }

            if (current is not null)
                yield return current;

            current = new Run(night.Date, night.Date, night.Kind, night.Booking, night.Block);
        }

        if (current is not null)
            yield return current;
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>The button leaves the page once this month shows, so focus moves to a month button.</summary>
    private async Task ShowThisMonthAsync()
    {
        shownMonth = StartOfMonth(State.Today);
        await TravelDisplay.FocusAsync(CanGoBack ? previousButton : nextButton);
    }

    /// <summary>Names the bookings, requests, and blocks that take some of the nights from start up to (not including) end.</summary>
    private string TakenText(int stayId, DateOnly start, DateOnly end)
    {
        var parts = new List<string>();

        foreach (var booking in State.HostBookings().Where(booking => booking.StayId == stayId && HoldsDates(booking) && booking.CheckIn < end && start < booking.CheckOut))
        {
            var first = booking.CheckIn > start ? booking.CheckIn : start;
            var last = (booking.CheckOut < end ? booking.CheckOut : end).AddDays(-1);
            var what = booking.Status == TravelBookingStatus.Requested ? "requested by" : "booked by";
            parts.Add($"{RangeText(first, last)} {what} {booking.GuestName} ({booking.Reference})");
        }

        foreach (var block in State.BlocksFor(stayId).Where(block => block.Start < end && start < block.End))
        {
            var first = block.Start > start ? block.Start : start;
            var last = (block.End < end ? block.End : end).AddDays(-1);
            parts.Add($"{RangeText(first, last)} already blocked ({block.Note})");
        }

        return parts.Count == 0 ? "another booking or block just took them" : string.Join("; ", parts);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private bool CanGoBack => FirstMonth > EarliestMonth;

    private bool CanGoForward => FirstMonth < LatestMonth;

    private DateOnly EarliestMonth => StartOfMonth(State.Today).AddMonths(-MonthsBack);

    /// <summary>The month shown, kept between <see cref="EarliestMonth" /> and <see cref="LatestMonth" />.</summary>
    private DateOnly FirstMonth => shownMonth < EarliestMonth ? EarliestMonth : shownMonth > LatestMonth ? LatestMonth : shownMonth;

    /// <summary>The last month: it holds the last possible check-out (a year ahead plus the longest stay).</summary>
    private DateOnly LatestMonth => StartOfMonth(State.Today.AddDays(366 + TravelSeedData.MaxNights));

    [Inject] private TravelState State { get; set; } = default!;

    /// <summary>The listing shown: the one picked, or the host's first; <see langword="null" /> without listings.</summary>
    private TravelStay? Stay
    {
        get
        {
            var listings = State.HostStays();

            return listings.FirstOrDefault(stay => stay.Id == selectedStayId) ?? listings.FirstOrDefault();
        }
    }

    /// <summary>Gets or sets the listing to show, from the query string (<c>?stay=</c>).</summary>
    [SupplyParameterFromQuery(Name = "stay")] public int? StayParam { get; set; }

    #endregion

    #region Nested Types

    /// <summary>What a night of the calendar holds.</summary>
    private enum NightKind
    {
        Free,
        Past,
        Booked,
        Requested,
        Blocked,
    }

    /// <summary>One night of a listing's calendar.</summary>
    private sealed record Night(DateOnly Date, NightKind Kind, TravelBooking? Booking, TravelBlock? Block);

    /// <summary>Consecutive nights with the same status and booking or block.</summary>
    private sealed record Run(DateOnly First, DateOnly Last, NightKind Kind, TravelBooking? Booking, TravelBlock? Block);

    #endregion
}
