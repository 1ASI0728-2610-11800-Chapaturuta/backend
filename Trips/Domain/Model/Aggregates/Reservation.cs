using Frock_backend.Trips.Domain.Model.ValueObjects;

namespace Frock_backend.Trips.Domain.Model.Aggregates;

public class Reservation
{
    public int Id { get; }
    public int FkIdUser { get; set; }
    public int FkIdTrip { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; }
    public int Seats { get; set; }
    public ReservationStatus Status { get; set; }
    public int? FkIdPayment { get; set; }
    public DateTime ReservedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    protected Reservation()
    {
        DocumentNumber = string.Empty;
        Status = ReservationStatus.Pending;
        ReservedAt = DateTime.UtcNow;
    }

    public Reservation(int fkIdUser, int fkIdTrip, DocumentType documentType, string documentNumber, int seats)
    {
        if (seats <= 0)
            throw new ArgumentException("Seats must be greater than zero", nameof(seats));
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new ArgumentException("Document number cannot be empty", nameof(documentNumber));

        FkIdUser = fkIdUser;
        FkIdTrip = fkIdTrip;
        DocumentType = documentType;
        DocumentNumber = documentNumber;
        Seats = seats;
        Status = ReservationStatus.Pending;
        ReservedAt = DateTime.UtcNow;
    }

    public void AttachPayment(int paymentId)
    {
        FkIdPayment = paymentId;
    }

    public void Confirm(int paymentId)
    {
        FkIdPayment = paymentId;
        Status = ReservationStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = ReservationStatus.Cancelled;
    }

    public void Complete()
    {
        Status = ReservationStatus.Completed;
    }

    public void Refund()
    {
        Status = ReservationStatus.Refunded;
    }
}
