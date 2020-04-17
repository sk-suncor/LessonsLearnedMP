using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Web.ViewData.Shared
{
    public class LeftRightListViewdata
    {
        public IEnumerable<SelectListItem> LeftList { get; set; }
        public IEnumerable<SelectListItem> RightList { get; set; }
        public string LeftName { get; set; }
        public string LeftId { get; set; }
        public string RightName { get; set; }
        public string RightId { get; set; }
        public object HtmlAttributes { get; set; }
        public Enumerations.Role RequiredPrivilege { get; set; }

        public string LeftLabel { get; set; }
        public string RightLabel { get; set; }
        public string LeftTitle { get; set; }
        public string RightTitle { get; set; }
        public bool AllowSorting { get; set; }
    }
}