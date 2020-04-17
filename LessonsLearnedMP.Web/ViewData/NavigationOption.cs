using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Web.ViewData
{
    public class NavigationOption
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public string IconClass { get; set; }
        public string Label { get; set; }
        public bool Selected { get; set; }
    }
}