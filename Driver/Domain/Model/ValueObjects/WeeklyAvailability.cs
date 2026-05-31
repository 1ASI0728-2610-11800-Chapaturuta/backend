namespace Frock_backend.Driver.Domain.Model.ValueObjects;

public class WeeklyAvailability
{
    public HashSet<DayOfWeek> Days { get; private set; }

    public WeeklyAvailability()
    {
        Days = new HashSet<DayOfWeek>();
    }

    public WeeklyAvailability(IEnumerable<DayOfWeek> days)
    {
        Days = days == null ? new HashSet<DayOfWeek>() : new HashSet<DayOfWeek>(days);
    }

    public bool IsAvailableOn(DayOfWeek day) => Days.Contains(day);

    public void Enable(DayOfWeek day) => Days.Add(day);

    public void Disable(DayOfWeek day) => Days.Remove(day);

    /// <summary>
    ///     Serializes the available days as a comma-separated list of DayOfWeek names.
    /// </summary>
    public string ToCsv() => string.Join(",", Days.OrderBy(d => d).Select(d => d.ToString()));

    /// <summary>
    ///     Rebuilds the value object from a comma-separated list of DayOfWeek names.
    /// </summary>
    public static WeeklyAvailability FromCsv(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return new WeeklyAvailability();

        var parsed = csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => Enum.TryParse<DayOfWeek>(token, ignoreCase: true, out var d) ? (DayOfWeek?)d : null)
            .Where(d => d.HasValue)
            .Select(d => d!.Value);

        return new WeeklyAvailability(parsed);
    }
}
