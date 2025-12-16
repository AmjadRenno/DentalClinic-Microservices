using System;
using System.Collections.Generic;

namespace DentalClinic.Cms.Models
{
    public class DentistDashboardAppointmentRow
    {
        public int AppointmentId { get; set; }
        public DateTime Start { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PatientName { get; set; }
    }

    public class DentistDashboardViewModel
    {
        public string PageTitle { get; set; } = string.Empty;
        public string IntroText { get; set; } = string.Empty;

        public List<DentistDashboardAppointmentRow> Appointments { get; set; }
            = new List<DentistDashboardAppointmentRow>();
    }
}
