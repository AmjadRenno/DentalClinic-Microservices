using DentalClinic.Cms.Models;
using DentalClinic.Cms.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace DentalClinic.Cms.Controllers
{
    public class LoginController : SurfaceController
    {
        private readonly IAuthApiClient _authClient;
        private readonly IHttpContextAccessor _http;

        public LoginController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IPublishedUrlProvider publishedUrlProvider,
            IAuthApiClient authClient,
            IHttpContextAccessor http
        ) : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, publishedUrlProvider)
        {
            _authClient = authClient;
            _http = http;
        }

        // 🔹 تسجيل الدخول
        [HttpPost]
        // لا نستخدم ValidateAntiForgeryToken في الـ MVP لتجنّب 400
        public async Task<IActionResult> SubmitLogin(LoginFormModel model)
        {
            // 1) التحقق من صحة المدخلات
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["errors"] = string.Join(" | ", errors);
                return RedirectToCurrentUmbracoPage();
            }

            // 2) استدعاء AuthService عبر IAuthApiClient
            var loginResult = await _authClient.LoginAsync(model.Email, model.Password);

            if (loginResult == null || string.IsNullOrEmpty(loginResult.token))
            {
                TempData["errors"] = "Invalid email or password.";
                return RedirectToCurrentUmbracoPage();
            }

            var http = _http.HttpContext!;

            // 3) حفظ التوكن في Session
            http.Session.SetString("jwt", loginResult.token);
            http.Session.SetString("CurrentUserEmail", model.Email);

            // 4) حفظ الدور في Session (Patient أو Dentist)
            var roleValue = loginResult.role ?? "Patient";
            http.Session.SetString("CurrentUserRole", roleValue);

            TempData["message"] = "You are now logged in.";

            return Redirect("/");
        }

        // 🔹 تسجيل الخروج
        [HttpPost]
        // برضه بدون ValidateAntiForgeryToken في الـ MVP
        public IActionResult Logout()
        {
            var http = _http.HttpContext;

            if (http != null)
            {
                http.Session.Remove("jwt");
                http.Session.Remove("CurrentUserEmail");
                http.Session.Remove("CurrentUserName");
                http.Session.Remove("CurrentUserRole");
                // أو http.Session.Clear();
            }

            TempData["message"] = "You have been logged out.";

            return Redirect("/");
        }
    }
}
