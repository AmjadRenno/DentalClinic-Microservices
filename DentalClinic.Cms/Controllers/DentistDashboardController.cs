using DentalClinic.Cms.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Microsoft.AspNetCore.Http;

namespace DentalClinic.Cms.Controllers
{
    public class DentistDashboardController : SurfaceController
    {
        private readonly IAppointmentApiClient _appointments;
        private readonly IHttpContextAccessor _http;

        public DentistDashboardController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IPublishedUrlProvider publishedUrlProvider,
            IAppointmentApiClient appointments,
            IHttpContextAccessor http
        ) : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, publishedUrlProvider)
        {
            _appointments = appointments;
            _http = http;
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(Guid appointmentId)
        {
            var http = _http.HttpContext!;
            var token = http.Session.GetString("jwt");
            var role = http.Session.GetString("CurrentUserRole");

            if (token == null)
            {
                TempData["dashboardMessage"] = "You must be logged in.";
                return Redirect("/login");
            }

            if (!string.Equals(role, "Dentist", StringComparison.OrdinalIgnoreCase))
            {
                TempData["dashboardMessage"] = "You are not allowed to perform this action.";
                return Redirect("/Dentist-Dashboard");
            }

            _appointments.SetAuthToken(token);

            var ok = await _appointments.ConfirmAppointmentAsync(appointmentId);

            TempData["dashboardMessage"] = ok
                ? "Appointment confirmed."
                : "Error confirming appointment.";

            return Redirect("/Dentist-Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(Guid appointmentId)
        {
            var http = _http.HttpContext!;
            var token = http.Session.GetString("jwt");
            var role = http.Session.GetString("CurrentUserRole");

            if (token == null)
            {
                TempData["dashboardMessage"] = "You must be logged in.";
                return Redirect("/login");
            }

            if (!string.Equals(role, "Dentist", StringComparison.OrdinalIgnoreCase))
            {
                TempData["dashboardMessage"] = "You are not allowed to perform this action.";
                return Redirect("/Dentist-Dashboard");
            }

            _appointments.SetAuthToken(token);

            var ok = await _appointments.CancelAppointmentAsync(appointmentId);

            TempData["dashboardMessage"] = ok
                ? "Appointment cancelled."
                : "Error cancelling appointment.";

            return Redirect("/Dentist-Dashboard");
        }
    }
}
