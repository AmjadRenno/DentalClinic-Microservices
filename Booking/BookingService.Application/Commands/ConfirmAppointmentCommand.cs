using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Application.Commands
{
    public sealed record ConfirmAppointmentCommand(Guid AppointmentId);
}
