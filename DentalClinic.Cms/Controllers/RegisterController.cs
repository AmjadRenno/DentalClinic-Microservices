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
    public class RegisterController : SurfaceController // RegisterController DentalClinic.Cms
    {
        private readonly IUserRegistrationApiClient _userApi;
        private readonly IAuthApiClient _authApi;

        public RegisterController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IPublishedUrlProvider publishedUrlProvider,
            IUserRegistrationApiClient userApi,
            IAuthApiClient authApi
        ) : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, publishedUrlProvider)
        {
            _userApi = userApi;
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

            // 1) تسجيل في AuthService عبر Gateway
            var registered = await _userApi.RegisterUserAsync(model);

            if (!registered)
            {
                TempData["errors"] = "Registration failed (email already exists?).";
                return RedirectToCurrentUmbracoPage();
            }

            // 2) Login أوتوماتيكي للحصول على JWT + Role
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
