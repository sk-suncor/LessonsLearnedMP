using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Web.Controllers
{
    public class HomeController : ControllerBase
    {
        [CoverageExclude]
        public HomeController(LessonsLearnedMPEntities context, IDistributedCache cache) :base(context, cache)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AuthenticationError()
        {
            ModelState.AddModelError("_FAULT", "You are not authorized to view the requested page.  Please contact your System Administrator.");
            return View("Index");
        }

        public IActionResult Help(int id)
        {
            return View("HelpTopic", (Enumerations.HelpTopic)id);
        }
    }
}
