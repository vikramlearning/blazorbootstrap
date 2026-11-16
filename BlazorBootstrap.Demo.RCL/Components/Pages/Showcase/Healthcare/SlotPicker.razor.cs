using System.Globalization;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Open times with one doctor, from <see cref="HcState.Slots" />: the next <see cref="PageDays" /> days that have any, as
/// toggle buttons (<c>aria-pressed</c>) with how many times each has, "Show later days" to page further up to
/// <see cref="HcState.BookingDays" />, and the chosen day's times as <c>hc-slot</c> buttons whose accessible names give the
/// full day and time ("Tuesday, December 1 at 10:00"). Picking a time raises <see cref="OnSelect" />; the page decides what
/// it means (the booking page chooses it, the appointment page moves the visit there).
/// </summary>
public partial class SlotPicker : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many days with open times show at first, and how many more each "Show later days" adds.</summary>
    public const int PageDays = 14;

    /// <summary>The day buttons by date, so focus can move to the first day "Show later days" added.</summary>
    private readonly Dictionary<DateTime, ElementReference> dayButtons = new();

    /// <summary>The day whose button takes focus after the next render.</summary>
    private DateTime? focusDay;

    private string? loadedDoctorId;

    private DateTime? loadedSelected;

    /// <summary>The day whose times are shown, or <see langword="null" /> for the first day with open times.</summary>
    private DateTime? selectedDay;

    private int shownDays = PageDays;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusDay is { } day)
        {
            focusDay = null;

            if (dayButtons.TryGetValue(day, out var button))
                await HcDisplay.FocusAsync(button);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // Another doctor starts over; a new selected time from the page shows its day.
        if (loadedDoctorId != DoctorId)
        {
            loadedDoctorId = DoctorId;
            shownDays = PageDays;
            selectedDay = null;
            dayButtons.Clear();
        }

        if (loadedSelected != Selected)
        {
            loadedSelected = Selected;

            if (Selected is { } selected)
                selectedDay = selected.Date;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Formats a day like "Tuesday, December 1".</summary>
    internal static string LongDay(DateTime date) => date.ToString("dddd, MMMM d", CultureInfo.InvariantCulture);

    /// <summary>Returns a <c>tel:</c> link for a clinic phone number like "(555) 010-2200".</summary>
    internal static string TelUrl(string phone) => "tel:" + new string(phone.Where(char.IsAsciiDigit).ToArray());

    /// <summary>Formats an open time's full name, like "Tuesday, December 1 at 10:00".</summary>
    internal static string SlotName(DateTime start) => $"{LongDay(start)} at {HcDisplay.Time(start)}";

    #endregion

    #region Private Methods

    private static string TimesText(int count, bool open = false) =>
        string.Create(CultureInfo.InvariantCulture, $"{count} {(open ? "open " : null)}{(count == 1 ? "time" : "times")}");

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Returns every day in the booking window with at least one open time, soonest first.</summary>
    private List<OpenDay> OpenDays()
    {
        var list = new List<OpenDay>();

        if (State.FindDoctor(DoctorId) is null)
            return list;

        for (var offset = 0; offset <= HcState.BookingDays; offset++)
        {
            var date = State.Clock.Today.AddDays(offset);
            var slots = State.Slots(DoctorId, date);

            if (slots.Count > 0)
                list.Add(new OpenDay(date, slots));
        }

        return list;
    }

    private Task PickAsync(HcSlot slot) => OnSelect.InvokeAsync(slot);

    private void SelectDay(DateTime date) => selectedDay = date;

    /// <summary>Shows the next days with open times and moves focus to the first of them.</summary>
    private void ShowLaterDays()
    {
        var days = OpenDays();
        var selectedIndex = selectedDay is { } wanted ? days.FindIndex(day => day.Date == wanted) : -1;
        var visible = Math.Max(shownDays, selectedIndex + 1);

        if (visible >= days.Count)
            return;

        shownDays = visible + PageDays;
        focusDay = days[visible].Date;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the doctor whose open times are shown.</summary>
    [Parameter, EditorRequired] public string DoctorId { get; set; } = default!;

    /// <summary>Gets or sets the prefix of the element IDs, unique on the page.</summary>
    [Parameter] public string IdPrefix { get; set; } = "hc-sp";

    /// <summary>Gets or sets the callback raised with the open time the patient picked.</summary>
    [Parameter] public EventCallback<HcSlot> OnSelect { get; set; }

    /// <summary>Gets or sets the time currently chosen (shown pressed, and its day opened), or <see langword="null" />.</summary>
    [Parameter] public DateTime? Selected { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A day with its open times.</summary>
    private sealed record OpenDay(DateTime Date, IReadOnlyList<HcSlot> Slots);

    #endregion
}
