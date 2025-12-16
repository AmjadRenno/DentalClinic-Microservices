using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace DentalClinic.Cms.Controllers
{
    public class MyAppointmentsPageController : RenderController
    {
        public MyAppointmentsPageController(
            ILogger<RenderController> logger,
            ICompositeViewEngine viewEngine,
            IUmbracoContextAccessor umbracoContextAccessor
        ) : base(logger, viewEngine, umbracoContextAccessor)
        {
        }

        public override IActionResult Index()
        {
            return CurrentTemplate(CurrentPage);
        }
    }
}
