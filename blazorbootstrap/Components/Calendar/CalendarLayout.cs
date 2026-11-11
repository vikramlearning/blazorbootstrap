namespace BlazorBootstrap;

/// <summary>
/// Computes what a <see cref="Calendar{TItem}" /> shows: visible ranges, navigation steps, and titles.
/// It renders nothing and calls no browser code. Month arithmetic uses the culture's own calendar,
/// so a non-Gregorian culture needs no separate code path.
/// </summary>
internal static class CalendarLayout
{
    #region Fields and Constants

    private const int DaysInWeek = 7;

    private const int MinutesInDay = 24 * 60;

    #endregion

    #region Methods

    #region Internal Methods

    /// <summary>
    /// Moves a date by one period of the view: a month, a week, a day, or the agenda period.
    /// Returns the date unchanged when the result is outside the range the calendar supports.
    /// </summary>
    internal static DateOnly AddPeriod(CalendarView view, DateOnly date, int direction, CultureInfo culture, int agendaDays)
    {
        try
        {
            return view switch
            {
                CalendarView.Month => DateOnly.FromDateTime(culture.DateTimeFormat.Calendar.AddMonths(date.ToDateTime(TimeOnly.MinValue), direction)),
                CalendarView.Day => date.AddDays(direction),
                CalendarView.Agenda => date.AddDays(direction * Math.Max(1, agendaDays)),
                _ => date.AddDays(direction * DaysInWeek)
            };
        }
        catch (ArgumentOutOfRangeException)
        {
            return date;
        }
    }

    /// <summary>
    /// Lays out the Agenda view: the days of the period that have events, each with its events in order.
    /// An event that covers several days appears on each of them. Hidden weekdays are left out.
    /// </summary>
    internal static List<AgendaDay> BuildAgenda<TItem>(
        DateOnly start,
        DateOnly end,
        IReadOnlyList<CalendarEntry<TItem>> entries,
        IReadOnlyCollection<DayOfWeek> hiddenDays,
        Func<DateOnly, bool> isAlwaysListed)
    {
        var days = new List<AgendaDay>();

        for (var day = start; day <= end; day = AddDaysClamped(day, 1))
        {
            // A hidden weekday is left out. A day the calendar must show, such as a holiday, is listed even without events.
            if (!hiddenDays.Contains(day.DayOfWeek))
            {
                var keys = GetEntriesOfDay(entries, day).Select(entry => entry.Key).ToList();

                if (keys.Count > 0 || isAlwaysListed(day))
                    days.Add(new AgendaDay(day, keys));
            }

            if (day == DateOnly.MaxValue)
                break;
        }

        return days;
    }

    /// <summary>
    /// Reads the app's items once through the field functions. An item without a start function is skipped.
    /// </summary>
    internal static List<CalendarEntry<TItem>> BuildEntries<TItem>(
        IEnumerable<TItem>? data,
        Func<TItem, DateTime>? startField,
        Func<TItem, DateTime>? endField,
        Func<TItem, string?>? titleField,
        Func<TItem, bool>? allDayField,
        Func<TItem, CalendarColor>? colorField)
    {
        var entries = new List<CalendarEntry<TItem>>();

        if (data is null || startField is null)
            return entries;

        foreach (var item in data)
        {
            if (item is null)
                continue;

            var start = startField(item);

            entries.Add(new CalendarEntry<TItem>(
                entries.Count,
                item,
                titleField?.Invoke(item),
                start,
                endField?.Invoke(item) ?? start,
                allDayField?.Invoke(item) ?? false,
                colorField?.Invoke(item) ?? CalendarColor.None));
        }

        return entries;
    }

    /// <summary>
    /// Lays out the weeks of the Month view. An event becomes one segment per week row it touches, so an event
    /// that crosses week rows keeps its first and last day. Segments are stacked in lanes; a segment in a lane
    /// beyond <paramref name="visibleLanes" /> is counted as hidden on each day it covers.
    /// </summary>
    internal static List<MonthWeek> BuildMonthWeeks<TItem>(
        DateOnly rangeStart,
        DateOnly rangeEnd,
        IReadOnlyList<CalendarEntry<TItem>> entries,
        int visibleLanes,
        IReadOnlyCollection<DayOfWeek> hiddenDays)
    {
        var weeks = new List<MonthWeek>();

        for (var weekStart = rangeStart; weekStart <= rangeEnd; weekStart = AddDaysClamped(weekStart, DaysInWeek))
        {
            var weekEnd = AddDaysClamped(weekStart, DaysInWeek - 1);

            // A hidden weekday has no column; an event that covers it keeps its segments on the days that are shown.
            var week = new MonthWeek(
                weekStart,
                Enumerable.Range(0, DaysInWeek)
                          .Select(offset => AddDaysClamped(weekStart, offset))
                          .Where(day => !hiddenDays.Contains(day.DayOfWeek))
                          .ToArray());

            week.Segments.AddRange(BuildRowSegments(week.Days, entries, visibleLanes, week.HiddenCounts, out _));
            weeks.Add(week);

            if (weekEnd == DateOnly.MaxValue)
                break;
        }

        return weeks;
    }

    /// <summary>
    /// Lays out the Week, Work week, and Day views. All-day and multi-day events go to the all-day row as segments
    /// in lanes. A timed event that starts and ends on one day becomes a block in that day's column; blocks that
    /// overlap in time share the column side by side, so none is hidden.
    /// </summary>
    internal static TimeGrid BuildTimeGrid<TItem>(DateOnly[] days, IReadOnlyList<CalendarEntry<TItem>> entries, int dayStartMinutes, int dayEndMinutes, int slotMinutes)
    {
        var grid = new TimeGrid(days);

        grid.AllDaySegments.AddRange(BuildRowSegments(days, entries.Where(entry => entry.IsBar), int.MaxValue, new int[days.Length], out var laneCount));
        grid.AllDayLaneCount = laneCount;

        for (var index = 0; index < days.Length; index++)
        {
            var day = days[index];
            var blocks = grid.DayBlocks[index];

            // A group of blocks that overlap, directly or through one another, shares one set of columns.
            var group = new List<(int Key, int Start, int End, int Column)>();
            var columnEnds = new List<int>();
            var groupEnd = 0;

            void CloseGroup()
            {
                blocks.AddRange(group.Select(block => new TimeGridBlock(block.Key, block.Start, block.End, block.Column, columnEnds.Count)));
                group.Clear();
                columnEnds.Clear();
            }

            var dayEntries = entries.Where(entry => !entry.IsBar && entry.FirstDay == day)
                                    .OrderBy(entry => entry.Start)
                                    .ThenByDescending(entry => entry.End)
                                    .ThenBy(entry => entry.Key);

            foreach (var entry in dayEntries)
            {
                var start = (int)entry.Start.TimeOfDay.TotalMinutes;
                var end = (int)Math.Min(MinutesInDay, (entry.End - entry.Start.Date).TotalMinutes);

                // An event shorter than a slot, or with no length at all, is drawn one slot tall.
                end = Math.Max(end, start + slotMinutes);

                if (end <= dayStartMinutes || start >= dayEndMinutes)
                    continue;

                start = Math.Max(start, dayStartMinutes);
                end = Math.Min(end, dayEndMinutes);

                if (group.Count > 0 && start >= groupEnd)
                    CloseGroup();

                var column = columnEnds.FindIndex(columnEnd => columnEnd <= start);

                if (column < 0)
                {
                    column = columnEnds.Count;
                    columnEnds.Add(end);
                }
                else
                {
                    columnEnds[column] = end;
                }

                groupEnd = group.Count == 0 ? end : Math.Max(groupEnd, end);
                group.Add((entry.Key, start, end, column));
            }

            CloseGroup();
        }

        return grid;
    }

    /// <summary>
    /// Gets the day number of a date in the culture's calendar.
    /// </summary>
    internal static int GetDayOfMonth(DateOnly date, CultureInfo culture)
    {
        try
        {
            return culture.DateTimeFormat.Calendar.GetDayOfMonth(date.ToDateTime(TimeOnly.MinValue));
        }
        catch (ArgumentOutOfRangeException)
        {
            return date.Day;
        }
    }

    /// <summary>
    /// Gets the events that touch a day: all-day and multi-day events first, then by start time.
    /// </summary>
    internal static List<CalendarEntry<TItem>> GetEntriesOfDay<TItem>(IReadOnlyList<CalendarEntry<TItem>> entries, DateOnly date) =>
        entries.Where(entry => entry.FirstDay <= date && entry.LastDay >= date)
               .OrderByDescending(entry => entry.IsBar)
               .ThenBy(entry => entry.Start)
               .ThenBy(entry => entry.Key)
               .ToList();

    /// <summary>
    /// Gets the first day of the week: the override when given, otherwise the culture's.
    /// </summary>
    internal static DayOfWeek GetFirstDayOfWeek(CultureInfo culture, DayOfWeek? firstDayOfWeek) =>
        firstDayOfWeek ?? culture.DateTimeFormat.FirstDayOfWeek;

    /// <summary>
    /// Gets the toolbar title of the visible period in the culture's formats.
    /// </summary>
    internal static string GetTitle(CalendarView view, DateOnly date, DateOnly start, DateOnly end, CultureInfo culture) =>
        view switch
        {
            CalendarView.Month => date.ToString("Y", culture),
            CalendarView.Day => date.ToString("D", culture),
            _ => start == end
                ? start.ToString("D", culture)
                : $"{start.ToString("d", culture)} – {end.ToString("d", culture)}"
        };

    /// <summary>
    /// Gets the days a Week, Work week, or Day view shows as columns. Hidden weekdays are left out, and Work week
    /// keeps only the working days; when that leaves no day, the view falls back to the days that are not hidden.
    /// </summary>
    internal static DateOnly[] GetViewDays(CalendarView view, DateOnly start, DateOnly end, IReadOnlyList<DayOfWeek> workDays, IReadOnlyCollection<DayOfWeek> hiddenDays)
    {
        var days = new List<DateOnly>();

        for (var day = start; day <= end; day = AddDaysClamped(day, 1))
        {
            days.Add(day);

            if (day == DateOnly.MaxValue)
                break;
        }

        // The Day view shows its one day whatever is hidden.
        if (view == CalendarView.Day)
            return days.ToArray();

        var shown = days.Where(day => !hiddenDays.Contains(day.DayOfWeek)
                                      && (view != CalendarView.WorkWeek || workDays.Contains(day.DayOfWeek)))
                        .ToArray();

        if (shown.Length > 0)
            return shown;

        // Nothing is left: fall back to the days that are not hidden, or to them all.
        var visible = days.Where(day => !hiddenDays.Contains(day.DayOfWeek)).ToArray();

        return visible.Length == 0 ? days.ToArray() : visible;
    }

    /// <summary>
    /// Gets the first and last day a view shows for a date. Month includes the days of the neighboring months that complete its weeks.
    /// </summary>
    internal static (DateOnly Start, DateOnly End) GetVisibleRange(
        CalendarView view,
        DateOnly date,
        CultureInfo culture,
        DayOfWeek firstDayOfWeek,
        IReadOnlyList<DayOfWeek> workDays,
        int agendaDays)
    {
        switch (view)
        {
            case CalendarView.Month:
                var (firstOfMonth, lastOfMonth) = GetMonthBounds(date, culture);
                return (GetWeekStart(firstOfMonth, firstDayOfWeek), AddDaysClamped(GetWeekStart(lastOfMonth, firstDayOfWeek), DaysInWeek - 1));

            case CalendarView.Week:
                var weekStart = GetWeekStart(date, firstDayOfWeek);
                return (weekStart, AddDaysClamped(weekStart, DaysInWeek - 1));

            case CalendarView.WorkWeek:
                var workWeekStart = GetWeekStart(date, firstDayOfWeek);
                var days = Enumerable.Range(0, DaysInWeek)
                                     .Select(offset => AddDaysClamped(workWeekStart, offset))
                                     .Where(day => workDays.Contains(day.DayOfWeek))
                                     .ToList();

                // With no working day configured, the view falls back to the whole week.
                return days.Count == 0
                    ? (workWeekStart, AddDaysClamped(workWeekStart, DaysInWeek - 1))
                    : (days[0], days[^1]);

            case CalendarView.Agenda:
                return (date, AddDaysClamped(date, Math.Max(1, agendaDays) - 1));

            default:
                return (date, date);
        }
    }

    /// <summary>
    /// Gets the number of the week that starts on a day.
    /// </summary>
    internal static int GetWeekNumber(DateOnly weekStart, CultureInfo culture, DayOfWeek firstDayOfWeek, CalendarWeekNumberRule rule)
    {
        if (rule == CalendarWeekNumberRule.Iso8601)
        {
            // ISO weeks run Monday to Sunday and are numbered by their Thursday. A week row that starts on another
            // day takes the number of the ISO week its Thursday falls in, which holds most of its days.
            var thursday = AddDaysClamped(weekStart, ((int)DayOfWeek.Thursday - (int)weekStart.DayOfWeek + DaysInWeek) % DaysInWeek);

            return ISOWeek.GetWeekOfYear(thursday.ToDateTime(TimeOnly.MinValue));
        }

        // The last day of the row decides. .NET splits the week that holds 1 January in two; under the "first day"
        // rule the row is week 1 of the new year, and its last day is the one that says so.
        var lastDay = AddDaysClamped(weekStart, DaysInWeek - 1).ToDateTime(TimeOnly.MinValue);

        try
        {
            return culture.DateTimeFormat.Calendar.GetWeekOfYear(lastDay, culture.DateTimeFormat.CalendarWeekRule, firstDayOfWeek);
        }
        catch (ArgumentOutOfRangeException)
        {
            return ISOWeek.GetWeekOfYear(lastDay);
        }
    }

    /// <summary>
    /// Gets the first day of the week that contains a date.
    /// </summary>
    internal static DateOnly GetWeekStart(DateOnly date, DayOfWeek firstDayOfWeek)
    {
        var offset = ((int)date.DayOfWeek - (int)firstDayOfWeek + DaysInWeek) % DaysInWeek;

        return AddDaysClamped(date, -offset);
    }

    /// <summary>
    /// Tells whether two dates fall in the same month of the culture's calendar.
    /// </summary>
    internal static bool IsSameMonth(DateOnly date, DateOnly other, CultureInfo culture)
    {
        var (first, last) = GetMonthBounds(other, culture);

        return date >= first && date <= last;
    }

    #endregion

    #region Private Methods

    private static DateOnly AddDaysClamped(DateOnly date, int days)
    {
        var dayNumber = (long)date.DayNumber + days;

        if (dayNumber < DateOnly.MinValue.DayNumber) return DateOnly.MinValue;
        if (dayNumber > DateOnly.MaxValue.DayNumber) return DateOnly.MaxValue;

        return DateOnly.FromDayNumber((int)dayNumber);
    }

    /// <summary>
    /// Places events as segments over a row of days, stacked in lanes. The days are in order but need not be
    /// consecutive (a Work week skips days), so an event covers one run of columns. A segment in a lane beyond
    /// <paramref name="visibleLanes" /> is not returned; it is counted in <paramref name="hiddenCounts" /> instead.
    /// </summary>
    private static List<RowSegment> BuildRowSegments<TItem>(
        DateOnly[] days,
        IEnumerable<CalendarEntry<TItem>> entries,
        int visibleLanes,
        int[] hiddenCounts,
        out int laneCount)
    {
        var segments = new List<RowSegment>();
        var lanes = new List<bool[]>();

        if (days.Length == 0)
        {
            laneCount = 0;

            return segments;
        }

        var rowStart = days[0];
        var rowEnd = days[^1];

        // Earlier and longer events take the upper lanes; bars come before timed events on the same day.
        var rowEntries = entries.Where(entry => entry.FirstDay <= rowEnd && entry.LastDay >= rowStart)
                                .OrderBy(entry => entry.FirstDay < rowStart ? rowStart : entry.FirstDay)
                                .ThenByDescending(entry => entry.LastDay.DayNumber - entry.FirstDay.DayNumber)
                                .ThenByDescending(entry => entry.IsBar)
                                .ThenBy(entry => entry.Start)
                                .ThenBy(entry => entry.Key);

        foreach (var entry in rowEntries)
        {
            var column = Array.FindIndex(days, day => day >= entry.FirstDay && day <= entry.LastDay);

            // The event covers only days this row does not show.
            if (column < 0)
                continue;

            var span = Array.FindLastIndex(days, day => day >= entry.FirstDay && day <= entry.LastDay) - column + 1;
            var lane = 0;

            while (lane < lanes.Count && Enumerable.Range(column, span).Any(day => lanes[lane][day]))
                lane++;

            if (lane == lanes.Count)
                lanes.Add(new bool[days.Length]);

            for (var day = column; day < column + span; day++)
                lanes[lane][day] = true;

            if (lane < visibleLanes)
            {
                segments.Add(new RowSegment(entry.Key, column, span, lane, entry.FirstDay < rowStart, entry.LastDay > rowEnd));
            }
            else
            {
                for (var day = column; day < column + span; day++)
                    hiddenCounts[day]++;
            }
        }

        laneCount = lanes.Count;

        return segments;
    }

    private static (DateOnly First, DateOnly Last) GetMonthBounds(DateOnly date, CultureInfo culture)
    {
        var calendar = culture.DateTimeFormat.Calendar;
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        try
        {
            var era = calendar.GetEra(dateTime);
            var year = calendar.GetYear(dateTime);
            var month = calendar.GetMonth(dateTime);
            var first = DateOnly.FromDateTime(calendar.ToDateTime(year, month, 1, 0, 0, 0, 0, era));

            return (first, AddDaysClamped(first, calendar.GetDaysInMonth(year, month, era) - 1));
        }
        catch (ArgumentOutOfRangeException)
        {
            // The date is outside the range the culture's calendar supports; show the Gregorian month instead.
            var first = new DateOnly(date.Year, date.Month, 1);

            return (first, AddDaysClamped(first, DateTime.DaysInMonth(date.Year, date.Month) - 1));
        }
    }

    #endregion

    #endregion

    #region Nested Types

    /// <summary>
    /// One day of the Agenda view that has events.
    /// </summary>
    /// <param name="Date">The day.</param>
    /// <param name="Keys">The keys of the day's events, all-day and multi-day events first, then by start time.</param>
    internal sealed record AgendaDay(DateOnly Date, IReadOnlyList<int> Keys);

    /// <summary>
    /// What a view shows on one day besides its events: a label such as a holiday name, a highlight, and non-working shading.
    /// </summary>
    /// <param name="Label">The label, or <see langword="null" /> when the day has none.</param>
    /// <param name="HighlightClass">The class that tints the day, or an empty text.</param>
    /// <param name="LabelClass">The class that colors the label.</param>
    /// <param name="IsNonWorking">Whether the day is shaded as a non-working day.</param>
    internal sealed record DayInfo(string? Label, string HighlightClass, string LabelClass, bool IsNonWorking)
    {
        internal static DayInfo None { get; } = new(null, "", "text-body-secondary", false);

        internal bool HasHighlight => HighlightClass.Length > 0;
    }

    /// <summary>
    /// One week row of the Month view: its seven days, the event segments drawn in it, and how many events are hidden on each day.
    /// </summary>
    internal sealed class MonthWeek
    {
        internal MonthWeek(DateOnly weekStart, DateOnly[] days)
        {
            WeekStart = weekStart;
            Days = days;
            HiddenCounts = new int[days.Length];
        }

        /// <summary>
        /// Gets the days the row shows. Hidden weekdays are not among them.
        /// </summary>
        internal DateOnly[] Days { get; }

        internal int[] HiddenCounts { get; }

        internal List<RowSegment> Segments { get; } = new();

        /// <summary>
        /// Gets the first day of the row's week, whether or not that day is shown.
        /// </summary>
        internal DateOnly WeekStart { get; }
    }

    /// <summary>
    /// The part of an event drawn across a row of days: a week row of the Month view, or the all-day row of a time-grid view.
    /// </summary>
    /// <param name="Key">The key of the event's <see cref="CalendarEntry{TItem}" />.</param>
    /// <param name="Column">The zero-based day column the segment starts in.</param>
    /// <param name="Span">The number of day columns the segment covers.</param>
    /// <param name="Lane">The zero-based row the segment sits in.</param>
    /// <param name="ContinuesBefore">Whether the event started before the first day of the row.</param>
    /// <param name="ContinuesAfter">Whether the event goes on after the last day of the row.</param>
    internal sealed record RowSegment(int Key, int Column, int Span, int Lane, bool ContinuesBefore, bool ContinuesAfter);

    /// <summary>
    /// The layout of a Week, Work week, or Day view: its day columns, the all-day row, and the timed blocks of each day.
    /// </summary>
    internal sealed class TimeGrid
    {
        internal TimeGrid(DateOnly[] days)
        {
            Days = days;
            DayBlocks = days.Select(_ => new List<TimeGridBlock>()).ToArray();
        }

        internal int AllDayLaneCount { get; set; }

        internal List<RowSegment> AllDaySegments { get; } = new();

        internal List<TimeGridBlock>[] DayBlocks { get; }

        internal DateOnly[] Days { get; }
    }

    /// <summary>
    /// A timed event in a day column of a time-grid view.
    /// </summary>
    /// <param name="Key">The key of the event's <see cref="CalendarEntry{TItem}" />.</param>
    /// <param name="StartMinutes">The minute of the day the block starts at, inside the visible hours.</param>
    /// <param name="EndMinutes">The minute of the day the block ends at, inside the visible hours.</param>
    /// <param name="Column">The zero-based column the block takes among the blocks it overlaps.</param>
    /// <param name="ColumnCount">The number of columns its group of overlapping blocks uses.</param>
    internal sealed record TimeGridBlock(int Key, int StartMinutes, int EndMinutes, int Column, int ColumnCount);

    #endregion
}
