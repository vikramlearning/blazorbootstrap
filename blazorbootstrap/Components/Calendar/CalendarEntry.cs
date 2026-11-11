namespace BlazorBootstrap;

/// <summary>
/// One event of a <see cref="Calendar{TItem}" />, read once from the app's item through the field functions.
/// </summary>
/// <typeparam name="TItem">The type of the events in the calendar.</typeparam>
internal sealed class CalendarEntry<TItem>
{
    #region Fields and Constants

    /// <summary>
    /// The form of a date in the markup the browser script reads.
    /// </summary>
    internal const string ScriptDateFormat = "yyyy-MM-dd";

    /// <summary>
    /// The form of a date and time exchanged with the browser script: always Gregorian, with no time zone.
    /// </summary>
    internal const string ScriptDateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss";

    #endregion

    #region Constructors and Finalizers

    internal CalendarEntry(int key, TItem item, string? title, DateTime start, DateTime end, bool isAllDay, CalendarColor color)
    {
        Key = key;
        Item = item;
        Title = title;
        Start = start;
        IsAllDay = isAllDay;
        Color = color;

        // An end that is not after its start still shows, as a minimum-length event.
        End = end > start ? end : start;
        FirstDay = DateOnly.FromDateTime(Start);

        // An end at midnight belongs to the day before: 1 Nov 00:00 to 3 Nov 00:00 covers two days, not three.
        var lastDay = End > Start && End.TimeOfDay == TimeSpan.Zero
            ? DateOnly.FromDateTime(End.AddTicks(-1))
            : DateOnly.FromDateTime(End);

        LastDay = lastDay < FirstDay ? FirstDay : lastDay;
    }

    #endregion

    #region Properties, Indexers

    internal CalendarColor Color { get; }

    internal DateTime End { get; }

    /// <summary>
    /// Gets the end in the culture-free form the browser script reads and sends back.
    /// </summary>
    internal string EndText => End.ToString(ScriptDateTimeFormat, CultureInfo.InvariantCulture);

    internal DateOnly FirstDay { get; }

    internal string FirstDayText => FirstDay.ToString(ScriptDateFormat, CultureInfo.InvariantCulture);

    internal bool IsAllDay { get; }

    /// <summary>
    /// Gets whether the event is drawn as a bar across its days: an all-day event or one that covers more than one day.
    /// </summary>
    internal bool IsBar => IsAllDay || LastDay > FirstDay;

    internal TItem Item { get; }

    /// <summary>
    /// Gets the position of the event in the current render's list. The browser script sends it back after a drag.
    /// </summary>
    internal int Key { get; }

    internal DateOnly LastDay { get; }

    internal string LastDayText => LastDay.ToString(ScriptDateFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the end a resize starts from. An all-day event stored without a length at midnight (one date used as both
    /// start and end) still covers its day, so its end counts as the next midnight. Without this a resized event
    /// would lose that day, because an end at midnight belongs to the day before.
    /// </summary>
    internal DateTime ResizeEnd =>
        IsAllDay && End == Start && End.TimeOfDay == TimeSpan.Zero && End <= DateTime.MaxValue.AddDays(-1) ? End.AddDays(1) : End;

    internal DateTime Start { get; }

    /// <summary>
    /// Gets the start in the culture-free form the browser script reads and sends back.
    /// </summary>
    internal string StartText => Start.ToString(ScriptDateTimeFormat, CultureInfo.InvariantCulture);

    internal string? Title { get; }

    #endregion
}
