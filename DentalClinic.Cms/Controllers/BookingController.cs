using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Core.Routing;
using Microsoft.AspNetCore.Http;

using DentalClinic.Cms.Models;
using DentalClinic.Cms.Services;
using System.Linq;
using System.Threading.Tasks;

namespace DentalClinic.Cms.Controllers
{
    public class BookingController : SurfaceController
    {
        private readonly IAppointmentApiClient _appointmentApi;
        private readonly IAuthApiClient _authApi;
        private readonly IDentistApiClient _dentistApi;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IPublishedUrlProvider publishedUrlProvider,
            IAppointmentApiClient appointmentApi,
            IAuthApiClient authApi,
            IDentistApiClient dentistApi,
            IHttpContextAccessor httpContextAccessor
        ) : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, publishedUrlProvider)
        {
            _appointmentApi = appointmentApi;
            _authApi = authApi;
            _dentistApi = dentistApi;
            _httpContextAccessor = httpContextAccessor;
        }

        // -----------------------------------------------------
        // POST — Submitting the booking
        // -----------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> SubmitBooking(BookingFormModel model)
        {
            // 1) Validation على المدخلات
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["errors"] = string.Join(" | ", errors);
                return CurrentUmbracoPage();
            }

            // 2) التأكد من أن المستخدم مسجّل الدخول
            var token = _httpContextAccessor.HttpContext?.Session.GetString("jwt");

            if (string.IsNullOrEmpty(token))
            {
                TempData["errors"] = "You must be logged in before booking an appointment.";
                return CurrentUmbracoPage();
            }

            _appointmentApi.SetAuthToken(token);

            // 3) استدعاء الـ Gateway لإنشاء الموعد
            var appointmentId = await _appointmentApi.CreateAppointmentAsync(model);

            if (appointmentId == null)
            {
                TempData["errors"] = "Failed to create booking through Gateway.";
                return CurrentUmbracoPage();
            }

            // 4) تحويل المستخدم إلى صفحة الدفع مع المبلغ
            decimal amount = 250m;
            TempData["message"] = "Booking created. Redirecting to payment...";

            return Redirect($"/Payment?bookingId={appointmentId}&amount={amount}");
        }
    }
}
