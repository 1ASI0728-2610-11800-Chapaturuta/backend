using Frock_backend.Driver.Domain.Repositories;

namespace Frock_backend.Driver.Interfaces.ACL.Services;

public class DriverContextFacade(
    IDriverRepository driverRepository,
    ITariffRepository tariffRepository,
    IRouteDurationRepository routeDurationRepository) : IDriverContextFacade
{
    /**
     * <summary>
     *     Checks whether a driver exists given its identifier.
     * </summary>
     * <param name="driverId">The driver identifier.</param>
     * <returns>True when the driver exists, false otherwise.</returns>
     */
    public async Task<bool> ExistsDriverAsync(int driverId)
    {
        var driver = await driverRepository.FindByIdAsync(driverId);
        return driver != null && !driver.IsDeleted;
    }

    /**
     * <summary>
     *     Returns an approximate fare for the given driver/route pair using the driver's
     *     active tariff and configured route duration. The approximation is BaseFare + MinFare.
     * </summary>
     * <param name="driverId">The driver identifier.</param>
     * <param name="routeId">The route identifier.</param>
     * <returns>The approximate fare, or null if no active tariff or route duration is configured.</returns>
     */
    public async Task<decimal?> GetEstimatedFareAsync(int driverId, int routeId)
    {
        var tariff = await tariffRepository.FindActiveByDriverIdAsync(driverId);
        if (tariff == null) return null;

        var routeDuration = await routeDurationRepository.FindByTariffAndRouteAsync(tariff.Id, routeId);
        if (routeDuration == null) return null;

        return tariff.BaseFare + tariff.MinFare;
    }

    /**
     * <summary>
     *     Resolves the driver identifier from an IAM user identifier.
     * </summary>
     * <param name="userId">The IAM user identifier.</param>
     * <returns>The driver identifier, or null if no driver matches the user.</returns>
     */
    public async Task<int?> FetchDriverIdByUserIdAsync(int userId)
    {
        var driver = await driverRepository.FindByFkIdUserAsync(userId);
        return driver?.Id;
    }

    /**
     * <summary>
     *     Resolves the IAM user identifier that owns the given driver.
     * </summary>
     * <param name="driverId">The driver identifier.</param>
     * <returns>The IAM user identifier, or null if no driver matches.</returns>
     */
    public async Task<int?> FetchUserIdByDriverIdAsync(int driverId)
    {
        var driver = await driverRepository.FindByIdAsync(driverId);
        return driver != null && !driver.IsDeleted ? driver.FkIdUser : null;
    }

    /**
     * <summary>
     *     Resolves the full name (first + last) of the driver with the given identifier.
     * </summary>
     * <param name="driverId">The driver identifier.</param>
     * <returns>The driver's full name, or null if no existing driver matches.</returns>
     */
    public async Task<string?> FetchDriverNameByDriverIdAsync(int driverId)
    {
        var driver = await driverRepository.FindByIdAsync(driverId);
        if (driver == null || driver.IsDeleted) return null;
        return $"{driver.FirstName} {driver.LastName}".Trim();
    }

    /**
     * <summary>
     *     Returns the registered vehicle's seat capacity for the given driver.
     * </summary>
     * <param name="driverId">The driver identifier.</param>
     * <returns>The vehicle capacity, or null when no existing driver matches.</returns>
     */
    public async Task<int?> FetchVehicleCapacityByDriverIdAsync(int driverId)
    {
        var driver = await driverRepository.FindByIdAsync(driverId);
        if (driver == null || driver.IsDeleted) return null;
        return driver.Vehicle.Capacity;
    }

    /**
     * <summary>
     *     Bulk-resolves driver full names keyed by driver identifier in a single query.
     * </summary>
     * <param name="driverIds">The driver identifiers to resolve.</param>
     * <returns>A dictionary mapping driver id to full name for every existing driver.</returns>
     */
    public async Task<IReadOnlyDictionary<int, string>> FetchDriverNamesByDriverIdsAsync(IEnumerable<int> driverIds)
    {
        var drivers = await driverRepository.FindByIdsAsync(driverIds);
        return drivers.ToDictionary(d => d.Id, d => $"{d.FirstName} {d.LastName}".Trim());
    }
}
