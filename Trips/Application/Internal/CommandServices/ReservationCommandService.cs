using Frock_backend.IAM.Domain.Repositories;
using Frock_backend.Payments.Interfaces.ACL;
using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Commands;
using Frock_backend.Trips.Domain.Model.ValueObjects;
using Frock_backend.Trips.Domain.Repositories;
using Frock_backend.Trips.Domain.Services;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Trips.Application.Internal.CommandServices;

public class ReservationCommandService(
    IReservationRepository reservationRepository,
    ITripRepository tripRepository,
    IUserRepository userRepository,
    IPaymentsContextFacade paymentsContextFacade,
    IUnitOfWork unitOfWork) : IReservationCommandService
{
    public async Task<Reservation?> Handle(CreateReservationCommand command)
    {
        // Defensive validation: missing references would otherwise surface as a
        // MySQL FK-constraint failure (HTTP 500). KeyNotFoundException → 404.
        if (await userRepository.FindByIdAsync(command.FkIdUser) is null)
            throw new KeyNotFoundException($"User with id {command.FkIdUser} not found");

        var trip = await tripRepository.FindByIdAsync(command.FkIdTrip);
        if (trip == null)
            throw new InvalidOperationException($"Trip with id {command.FkIdTrip} not found");

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
}
