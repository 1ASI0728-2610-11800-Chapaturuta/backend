namespace Frock_backend.Driver.Interfaces.ACL;

public interface IDriverContextFacade
{
    /// <summary>
    ///     Checks whether a driver exists (and is not soft-deleted) given its identifier.
    /// </summary>
    Task<bool> ExistsDriverAsync(int driverId);

    /// <summary>
    ///     Computes an approximate fare for a driver/route pair using the driver's active tariff
    ///     and the configured route duration. Returns null when the tariff or duration are missing.
    /// </summary>
    Task<decimal?> GetEstimatedFareAsync(int driverId, int routeId);

    /// <summary>
    ///     Resolves the driver identifier from an IAM user identifier.
    /// </summary>
    Task<int?> FetchDriverIdByUserIdAsync(int userId);

    /// <summary>
    ///     Resolves the IAM user identifier that owns the given driver.
    /// </summary>
    Task<int?> FetchUserIdByDriverIdAsync(int driverId);

    /// <summary>
    ///     Resolves the full name of a driver given its identifier. Returns null when no driver matches.
    /// </summary>
    Task<string?> FetchDriverNameByDriverIdAsync(int driverId);

    /// <summary>
    ///     Returns the seat capacity of the driver's registered vehicle. Null when no driver matches.
    ///     This is the single source of truth for how many seats a published trip offers.
    /// </summary>
    Task<int?> FetchVehicleCapacityByDriverIdAsync(int driverId);

    /// <summary>
    ///     Bulk-resolves driver full names keyed by driver identifier, avoiding N+1 lookups.
    ///     Only existing (non soft-deleted) drivers are present in the result.
    /// </summary>
    Task<IReadOnlyDictionary<int, string>> FetchDriverNamesByDriverIdsAsync(IEnumerable<int> driverIds);
}
