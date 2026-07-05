namespace Frock_backend.shared.Domain.Time;

/// <summary>
///     Converts UTC timestamps to Lima local time. Lima is UTC-5 year-round (no DST observed
///     in Peru), so this is a fixed-offset conversion — no timezone database lookup required.
/// </summary>
public static class LimaTime
{
    private const int UtcOffsetHours = -5;

    /// <summary>
    ///     Converts a UTC <see cref="DateTime" /> to Lima local time. The input's <see cref="DateTime.Kind" />
    ///     is ignored — it is always treated as UTC, since callers in this codebase work in UTC even when
    ///     the value's Kind is Unspecified (e.g. after model binding or EF round-trips).
    /// </summary>
    public static DateTime ToLima(DateTime utc)
    {
        return utc.AddHours(UtcOffsetHours);
    }
}
