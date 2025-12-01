namespace DentalClinic.SharedKernel.DomainEvents;

public record AppointmentConfirmedEvent(Guid AppointmentId, Guid PatientId);
