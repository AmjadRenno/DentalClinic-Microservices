using System;
using DentalClinic.SharedKernel;
using BookingService.Domain.ValueObjects;

namespace BookingService.Domain.Entities
{
    public enum AppointmentStatus
    {
        Requested,
        Confirmed,
        Cancelled,
        Completed
    }

    public sealed class Appointment : AggregateRoot
    {
        public PatientId PatientId { get; private init; }
        public DentistId DentistId { get; private init; }
        public TimeSlot Slot { get; private set; }
        public AppointmentStatus Status { get; private set; }

        public Appointment(
            Guid id,
            PatientId patientId,
            DentistId dentistId,
            TimeSlot slot)
            : base(id)
        {
            PatientId = patientId;
            DentistId = dentistId;
            Slot = slot;
            Status = AppointmentStatus.Requested;
        }

        // Constructor فارغ خاص بـ EF Core
        private Appointment() : base(Guid.Empty) { }

        public void Confirm()
        {
            if (Status != AppointmentStatus.Requested)
                throw new InvalidOperationException("Only requested appointments can be confirmed.");

            Status = AppointmentStatus.Confirmed;
        }

        public void Cancel()
        {
            if (Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Completed appointments cannot be cancelled.");

            Status = AppointmentStatus.Cancelled;
        }

        public void Reschedule(TimeSlot newSlot)
        {
            if (Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Cancelled appointments cannot be rescheduled.");

            Slot = newSlot;
        }

        public void Complete()
        {
            if (Status != AppointmentStatus.Confirmed)
                throw new InvalidOperationException("Only confirmed appointments can be completed.");

            Status = AppointmentStatus.Completed;
        }
    }
}
