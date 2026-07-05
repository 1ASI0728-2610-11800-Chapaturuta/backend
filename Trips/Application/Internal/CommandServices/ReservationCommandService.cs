using Frock_backend.IAM.Domain.Repositories;
using Frock_backend.Payments.Interfaces.ACL;
using Frock_backend.routes.Domain.Repository;
using Frock_backend.routes.Domain.Service;
using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Commands;
using Frock_backend.Trips.Domain.Model.ValueObjects;
using Frock_backend.Trips.Domain.Repositories;
using Frock_backend.Trips.Domain.Services;
using Frock_backend.shared.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Frock_backend.Trips.Application.Internal.CommandServices;

public class ReservationCommandService(
    IReservationRepository reservationRepository,
    ITripRepository tripRepository,
    IRouteRepository routeRepository,
    IUserRepository userRepository,
    IPaymentsContextFacade paymentsContextFacade,
    IUnitOfWork unitOfWork,
    IOptions<ReservationHoldOptions> holdOptions) : IReservationCommandService
{
    private readonly int _paymentHoldMinutes = holdOptions.Value.PaymentHoldMinutes;

    public async Task<Reservation?> Handle(CreateReservationCommand command)
    {
        // Defensive validation: missing references would otherwise surface as a
        // MySQL FK-constraint failure (HTTP 500). KeyNotFoundException → 404.
        if (await userRepository.FindByIdAsync(command.FkIdUser) is null)
            throw new KeyNotFoundException($"User with id {command.FkIdUser} not found");

        var trip = await tripRepository.FindByIdAsync(command.FkIdTrip);
        if (trip == null)
            throw new InvalidOperationException($"Trip with id {command.FkIdTrip} not found");

        // The trip's date/time was fixed when it was created/published; re-validate it against the
        // route's current attention hours (Schedules) so a reservation can't be made against a trip
        // that now falls outside them. FindByRouteId eager-loads Schedules (FindByIdAsync doesn't).
        var route = await routeRepository.FindByRouteId(trip.FkIdRoute);
        if (route != null && !RouteScheduleRules.IsOpenAt(route, trip.StartTime, out _))
            throw new InvalidOperationException(
                "No puedes reservar este viaje: está fuera del horario de atención de la ruta.");

        // Free up seats held by unpaid reservations before committing new ones, otherwise abandoned
        // (never-paid) holds would drain availability forever. Two cases are released here:
        //   1) any pending hold whose payment window has elapsed (global cleanup), and
        //   2) this same user's prior pending hold on this trip, so retrying the payment flow
        //      replaces the old hold instead of stacking a second one.
        await ReleaseStaleHoldsAsync(trip, command.FkIdUser);

        trip.ReserveSeats(command.Seats);

        var totalPrice = (trip.Price ?? 0m) * command.Seats;

        var reservation = new Reservation(
            command.FkIdUser,
            command.FkIdTrip,
            command.DocumentType,
            command.DocumentNumber,
            command.Seats);

        await reservationRepository.AddAsync(reservation);
        await unitOfWork.CompleteAsync();

        var paymentId = await paymentsContextFacade.RegisterPendingPaymentAsync(
            command.FkIdUser,
            totalPrice,
            command.PaymentMethod,
            "Reservation",
            reservation.Id);

        reservation.AttachPayment(paymentId);
        reservationRepository.Update(reservation);
        await unitOfWork.CompleteAsync();

        return reservation;
    }

    public async Task<Reservation?> Handle(ConfirmReservationCommand command)
    {
        var reservation = await reservationRepository.FindByIdAsync(command.ReservationId);
        if (reservation == null)
            throw new InvalidOperationException($"Reservation with id {command.ReservationId} not found");

        if (!reservation.FkIdPayment.HasValue)
            throw new InvalidOperationException("Cannot confirm a reservation without an attached payment");

        reservation.Confirm(reservation.FkIdPayment.Value);

        reservationRepository.Update(reservation);
        await unitOfWork.CompleteAsync();
        return reservation;
    }

    public async Task<Reservation?> Handle(CancelReservationCommand command)
    {
        var reservation = await reservationRepository.FindByIdAsync(command.ReservationId);
        if (reservation == null)
            throw new InvalidOperationException($"Reservation with id {command.ReservationId} not found");

        var trip = await tripRepository.FindByIdAsync(reservation.FkIdTrip);
        if (trip == null)
            throw new InvalidOperationException($"Trip with id {reservation.FkIdTrip} not found");

        var wasConfirmed = reservation.Status == ReservationStatus.Confirmed;

        trip.ReleaseSeats(reservation.Seats);
        reservation.Cancel();

        reservationRepository.Update(reservation);
        await unitOfWork.CompleteAsync();

        if (reservation.FkIdPayment.HasValue && wasConfirmed)
        {
            var refundAmount = (trip.Price ?? 0m) * reservation.Seats;
            await paymentsContextFacade.RegisterRefundAsync(
                reservation.FkIdPayment.Value,
                refundAmount,
                "Reservation cancelled by user");
        }

        return reservation;
    }

    /// <summary>
    ///     Releases seats held by unpaid reservations on the trip and marks those reservations Expired,
    ///     failing their orphan pending payments. Targets expired holds (payment window elapsed) and the
    ///     requesting user's existing pending hold on this trip so a payment retry supersedes it.
    /// </summary>
    private async Task ReleaseStaleHoldsAsync(Trip trip, int requestingUserId)
    {
        var now = DateTime.UtcNow;
        var reservations = await reservationRepository.FindByTripIdAsync(trip.Id);

        var stale = reservations
            .Where(r => r.Status == ReservationStatus.Pending
                        && (r.IsPaymentExpired(now, _paymentHoldMinutes) || r.FkIdUser == requestingUserId))
            .ToList();

        if (stale.Count == 0) return;

        foreach (var hold in stale)
        {
            trip.ReleaseSeats(hold.Seats);
            hold.Expire();
            reservationRepository.Update(hold);

            if (hold.FkIdPayment.HasValue)
                await paymentsContextFacade.FailPaymentAsync(hold.FkIdPayment.Value);
        }

        await unitOfWork.CompleteAsync();
    }
}
