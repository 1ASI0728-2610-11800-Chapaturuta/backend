using System.Globalization;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Time;

namespace Frock_backend.routes.Domain.Service;

/// <summary>
///     Domain rules for whether a route is within its configured attention hours (its
///     <see cref="Frock_backend.routes.Domain.Model.Entities.Schedule" /> list) at a given UTC instant.
///     Schedule day names are stored as free-form strings that may be in English or Spanish
///     (with or without accents), so matching is case-insensitive and accent-tolerant.
/// </summary>
public static class RouteScheduleRules
{
    // Accepted day names (English + Spanish, accented and unaccented) per .NET DayOfWeek value.
    private static readonly Dictionary<DayOfWeek, string[]> AcceptedDayNames = new()
    {
        [DayOfWeek.Monday] = new[] { "Monday", "Lunes" },
        [DayOfWeek.Tuesday] = new[] { "Tuesday", "Martes" },
        [DayOfWeek.Wednesday] = new[] { "Wednesday", "Miercoles", "Miércoles" },
        [DayOfWeek.Thursday] = new[] { "Thursday", "Jueves" },
        [DayOfWeek.Friday] = new[] { "Friday", "Viernes" },
        [DayOfWeek.Saturday] = new[] { "Saturday", "Sabado", "Sábado" },
        [DayOfWeek.Sunday] = new[] { "Sunday", "Domingo" }
    };

    /// <summary>
    ///     Checks whether <paramref name="route" /> is open (within its attention hours) at the given
    ///     UTC instant. A route with zero enabled schedules is treated as always open, since that means
    ///     no hours have been configured for it yet.
    /// </summary>
    /// <param name="reason">Spanish, user-facing explanation set only when the route is closed.</param>
    public static bool IsOpenAt(RouteAggregate route, DateTime utc, out string? reason)
    {
        reason = null;

        var enabledSchedules = route.Schedules.Where(s => s.Enabled).ToList();
        if (enabledSchedules.Count == 0)
            return true;

        var limaLocal = LimaTime.ToLima(utc);
        var acceptedNames = AcceptedDayNames[limaLocal.DayOfWeek];
        var limaTimeOfDay = limaLocal.TimeOfDay;

        var isOpen = enabledSchedules.Any(schedule =>
            acceptedNames.Any(name => string.Equals(name, schedule.DayOfWeek?.Trim(), StringComparison.OrdinalIgnoreCase))
            && TryParseTime(schedule.StartTime, out var start)
            && TryParseTime(schedule.EndTime, out var end)
            && limaTimeOfDay >= start
            && limaTimeOfDay <= end);

        if (isOpen) return true;

        reason = "El viaje está fuera del horario de atención de la ruta " +
                  $"({limaLocal.DayOfWeek} {limaLocal:HH\\:mm}, hora de Lima).";
        return false;
    }

    // Tolerant of both "H:mm" and "HH:mm".
    private static bool TryParseTime(string? value, out TimeSpan time)
    {
        return TimeSpan.TryParse(value?.Trim(), CultureInfo.InvariantCulture, out time);
    }
}
