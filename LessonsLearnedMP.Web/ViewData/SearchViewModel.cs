using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Suncor.LessonsLearnedMP.Business;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Web.ViewData
{
    [Serializable]
    public class SearchViewModel : LessonFilters
    {
        public SearchViewModel()
        {
            IsLessonTypeValidSelected = true;
        }

        public bool AdvancedSearch { get; set; }
        public bool IsLessonTypeValidSelected { get; set; }
        public bool Clear { get; set; }
        public List<RoleUser> SubmittedByUsers { get; set; }
        public bool Blank { get; set; }
    }
}