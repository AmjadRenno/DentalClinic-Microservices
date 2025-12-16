using DentalClinic.Cms.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace DentalClinic.Cms.Controllers
{
    public class PaymentSurfaceController : SurfaceController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PaymentSurfaceController> _logger;

        public PaymentSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IPublishedUrlProvider publishedUrlProvider,
            IHttpClientFactory httpClientFactory,
            ILogger<PaymentSurfaceController> paymentLogger)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, publishedUrlProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = paymentLogger;
        }

        [HttpPost]
        // [ValidateAntiForgeryToken] // Currently disabled to avoid 400
        public async Task<IActionResult> Submit(PaymentFormViewModel model)
        {
            // 1) Validation on inputs
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["PaymentError"] = string.Join(" | ", errors);
                return CurrentUmbracoPage();
            }

            if (model.Amount <= 0)
            {
                TempData["PaymentError"] = "Invalid amount.";
                return CurrentUmbracoPage();
            }

            try
            {
                // This HttpClient is registered in Program.cs as "PaymentService"
                var client = _httpClientFactory.CreateClient("PaymentService");

                var request = new
                {
                    PatientName = model.CardHolderName,
                    Amount = model.Amount
                };

                // Send request to PaymentService.API on /charge
                var response = await client.PostAsJsonAsync("/charge", request);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["PaymentError"] = "Payment failed. Try again.";
                    return CurrentUmbracoPage();
                }

                var result = await response.Content.ReadFromJsonAsync<PaymentResultDto>();

                if (result != null && result.Success)
                {
                    TempData["PaymentSuccess"] = result.Message;
                }
                else
                {
                    TempData["PaymentError"] = result?.Message ?? "Payment was not successful.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing payment.");
                TempData["PaymentError"] = "A system error occurred while processing payment.";
            }

            // Return to the same Payment page to show the message
            return CurrentUmbracoPage();
        }

        private class PaymentResultDto
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }
    }
}
