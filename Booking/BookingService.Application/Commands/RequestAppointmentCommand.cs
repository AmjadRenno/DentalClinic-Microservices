using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Application.Commands
{
    public sealed record RequestAppointmentCommand(
    Guid AppointmentId,
    Guid PatientId,
    Guid DentistId,
    DateTime Start,
    DateTime End);
}
