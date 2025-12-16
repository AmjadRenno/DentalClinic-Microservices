using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using DentalClinic.Cms.Services;
using DentalClinic.Cms.Models;
using Umbraco.Cms.Core.Routing;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace DentalClinic.Cms.Controllers
{
    public class RegisterController : SurfaceController
    {
        private readonly IAuthApiClient _authApi;

        public RegisterController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IPublishedUrlProvider publishedUrlProvider,
            IAuthApiClient authApi
        ) : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, publishedUrlProvider)
        {
            _authApi = authApi;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRegister(RegisterFormModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["errors"] = string.Join(" | ", errors);
                return RedirectToCurrentUmbracoPage();
            }

            // 1) Register in AuthService via Gateway
            var registered = await _authApi.RegisterAsync(model);

            if (!registered)
            {
                TempData["errors"] = "Registration failed (email already exists?).";
                return RedirectToCurrentUmbracoPage();
            }

            // 2) Automatic login to get JWT + Role
            var loginResult = await _authApi.LoginAsync(model.Email, model.Password);

            if (loginResult != null && !string.IsNullOrEmpty(loginResult.token))
            {
                HttpContext.Session.SetString("jwt", loginResult.token);
                HttpContext.Session.SetString("CurrentUserEmail", model.Email);
                HttpContext.Session.SetString("CurrentUserName", model.FullName);
                HttpContext.Session.SetString("CurrentUserRole", loginResult.role ?? "Patient");

                TempData["message"] = "Registration completed. You are now logged in.";
            }
            else
            {
                TempData["message"] = "Registration completed, but auto login failed.";
            }


            return RedirectToCurrentUmbracoPage();
        }
    }
}
