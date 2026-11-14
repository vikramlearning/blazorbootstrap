namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// The clock of one showcase site. It starts at the site's fixed seed time, so every fresh load shows the same content,
/// and then moves forward with real elapsed time, so records created later get later times and countdowns change. The
/// elapsed time is kept in the tab's snapshot, so a reload continues from where the tab was.
/// </summary>
public sealed class ShowcaseClock
{
    #region Fields and Constants

    /// <summary>The longest elapsed time a restored snapshot may carry; anything larger is treated as invalid.</summary>
    public static readonly TimeSpan MaxElapsed = TimeSpan.FromDays(365);

    private readonly TimeProvider timeProvider;

    private TimeSpan carried;

    private long resumedAt;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts a clock at the site's seed time.</summary>
    /// <param name="start">The site's fixed seed time.</param>
    /// <param name="timeProvider">The source of real time; the system clock when omitted.</param>
    public ShowcaseClock(DateTime start, TimeProvider? timeProvider = null)
    {
        Start = start;
        this.timeProvider = timeProvider ?? TimeProvider.System;
        resumedAt = this.timeProvider.GetTimestamp();
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Continues the clock from an elapsed time read back from a snapshot.</summary>
    /// <returns><see langword="false" /> when the value is negative, not finite, or larger than <see cref="MaxElapsed" />.</returns>
    public bool TryRestore(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0 || elapsedSeconds > MaxElapsed.TotalSeconds)
            return false;

        carried = TimeSpan.FromSeconds(elapsedSeconds);
        resumedAt = timeProvider.GetTimestamp();

        return true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the real time elapsed since the site was seeded, including time carried over from earlier loads of the tab.</summary>
    public TimeSpan Elapsed => carried + timeProvider.GetElapsedTime(resumedAt);

    /// <summary>Gets the site's current time.</summary>
    public DateTime Now => Start + Elapsed;

    /// <summary>Gets the site's fixed seed time.</summary>
    public DateTime Start { get; }

    /// <summary>Gets the site's current date.</summary>
    public DateTime Today => Now.Date;

    #endregion
}
