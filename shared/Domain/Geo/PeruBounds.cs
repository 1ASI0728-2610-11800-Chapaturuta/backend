namespace Frock_backend.shared.Domain.Geo;

/// <summary>
///     Bounding box of Peru, used to validate that geographic coordinates
///     (e.g. a Stop location) fall within the country.
/// </summary>
public static class PeruBounds
{
    public const double MinLat = -18.35;
    public const double MaxLat = -0.03;
    public const double MinLng = -81.33;
    public const double MaxLng = -68.65;

    /// <summary>
    ///     Returns true only when both coordinates are present and inside Peru's bounding box.
    /// </summary>
    public static bool Contains(double? latitude, double? longitude)
    {
        if (latitude is null || longitude is null) return false;
        return latitude >= MinLat && latitude <= MaxLat
            && longitude >= MinLng && longitude <= MaxLng;
    }
}
