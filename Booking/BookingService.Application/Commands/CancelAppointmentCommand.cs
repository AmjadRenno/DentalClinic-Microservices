using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Application.Commands
{
    public sealed record CancelAppointmentCommand(Guid AppointmentId);

}
