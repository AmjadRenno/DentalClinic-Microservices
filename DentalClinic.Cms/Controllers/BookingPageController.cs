using DentalClinic.Cms.Models;
using DentalClinic.Cms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using System.Linq;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace DentalClinic.Cms.Controllers
{
    public class BookingPageController : RenderController
    {
        private readonly IDentistApiClient _dentistApi;

        public BookingPageController(
            ILogger<RenderController> logger,
            ICompositeViewEngine viewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IDentistApiClient dentistApi
        ) : base(logger, viewEngine, umbracoContextAccessor)
        {
            _dentistApi = dentistApi;
        }

        public override IActionResult Index()
        {
            // Async call — safe sync wrapper
            var apiDentists = _dentistApi
                .GetDentistsAsync()
                .GetAwaiter()
                .GetResult();

            var dentists = apiDentists.Select(d => new DentistViewModel
            {
                Id = d.Id,
                FullName = d.FullName
            }).ToList();

            var vm = new BookingViewModel
            {
                Dentists = dentists
            };

            return CurrentTemplate(vm);
        }
    }
}
