using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suncor.LessonsLearnedMP.Web.Helpers
{
	public class LeftRightListBoxComponent : ViewComponent
	{
		public LeftRightListBoxComponent()
		{
		}
		public async Task<IViewComponentResult> InvokeAsync<TModel>(string viewName, TModel model)
		{
			return View(viewName, model);
		}

	}
}
