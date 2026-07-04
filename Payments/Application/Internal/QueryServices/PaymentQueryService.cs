using Frock_backend.IAM.Interfaces.ACL;
using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Queries;
using Frock_backend.Payments.Domain.Repositories;
using Frock_backend.Payments.Domain.Services;
using Frock_backend.Trips.Domain.Repositories;

namespace Frock_backend.Payments.Application.Internal.QueryServices;

public class PaymentQueryService(
    IPaymentRepository paymentRepository,
    ITripRepository tripRepository,
    IReservationRepository reservationRepository,
    IIamContextFacade iamContextFacade) : IPaymentQueryService
{
    public async Task<Payment?> Handle(GetPaymentByIdQuery query)
    {
        return await paymentRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Payment>> Handle(GetPaymentsByUserIdQuery query)
    {
        return await paymentRepository.FindByUserIdAsync(query.FkIdUser);
    }

    public async Task<IEnumerable<Payment>> Handle(GetPaymentsByReferenceQuery query)
    {
        return await paymentRepository.FindByReferenceAsync(query.ReferenceType, query.ReferenceId);
    }

    public async Task<IEnumerable<ReceivedPaymentView>> Handle(GetPaymentsReceivedByDriverQuery query)
    {
        // A driver receives money through two channels: payments tied directly to trips they drive
        // (ReferenceType "Trip"), and payments tied to reservations made on those trips
        // (ReferenceType "Reservation"). Resolve both id sets first, then pull matching payments.
        var trips = await tripRepository.FindByDriverIdAsync(query.DriverId);
        var tripIds = trips.Select(t => t.Id).ToList();
        if (tripIds.Count == 0) return Enumerable.Empty<ReceivedPaymentView>();

        var reservations = await reservationRepository.FindByTripIdsAsync(tripIds);
        var reservationIds = reservations.Select(r => r.Id).ToList();

        var tripPayments = await paymentRepository.FindByReferenceTypeAndReferenceIdsAsync("Trip", tripIds);
        var reservationPayments = reservationIds.Count == 0
            ? new List<Payment>()
            : await paymentRepository.FindByReferenceTypeAndReferenceIdsAsync("Reservation", reservationIds);

        var payments = tripPayments
            .Concat(reservationPayments)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
        if (payments.Count == 0) return Enumerable.Empty<ReceivedPaymentView>();

        // Resolve payer display names in bulk (no N+1) via the IAM ACL facade.
        var payerIds = payments.Select(p => p.FkIdUser).ToHashSet();
        var payerNames = await iamContextFacade.FetchUsernamesByUserIdsAsync(payerIds);

        return payments.Select(p =>
        {
            var payerName = payerNames.TryGetValue(p.FkIdUser, out var name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : $"Usuario #{p.FkIdUser}";
            return new ReceivedPaymentView(p, payerName);
        }).ToList();
    }
}
