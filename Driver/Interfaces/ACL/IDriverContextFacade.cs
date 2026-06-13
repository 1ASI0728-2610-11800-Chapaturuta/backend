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
}
