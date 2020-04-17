using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Suncor.LessonsLearnedMP.Business;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.ViewData;

namespace Suncor.LessonsLearnedMP.Web.Common
{
    public interface IUserSessionContext
    {
        bool SessionExpired { get; set; }
        RoleUser CurrentUser { get; set; }
        List<Lesson> LastSearchResults { get; set; }
        ExportLog ExportLog { get; set; }
        SearchViewModel LastSearch { get; set; }
        LessonViewModel DraftDefaults { get; set; }
    }
}