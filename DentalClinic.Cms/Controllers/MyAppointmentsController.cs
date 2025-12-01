using DentalClinic.Cms.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;

namespace DentalClinic.Cms.Controllers
{
    public class MyAppointmentsController : SurfaceController
    {
        private readonly IAppointmentApiClient _appointments;
        private readonly IHttpContextAccessor _http;

        public MyAppointmentsController(
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
        public async Task<IActionResult> Cancel(Guid appointmentId)
        {
            var token = _http.HttpContext!.Session.GetString("jwt");

            if (token == null)
            {
                TempData["message"] = "You must be logged in.";
                return Redirect("/login");
            }

            _appointments.SetAuthToken(token);

            var ok = await _appointments.CancelAppointmentAsync(appointmentId);

            TempData["message"] = ok ? "Appointment cancelled." : "Error cancelling appointment.";

            return Redirect("/MyAppointments");
        }
    }
}
