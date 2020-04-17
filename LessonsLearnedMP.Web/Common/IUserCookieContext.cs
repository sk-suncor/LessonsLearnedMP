using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Suncor.LessonsLearnedMP.Web.Common
{
    public interface IUserCookieContext
    {
        int SearchistPageSize { get; set; }
        int LongFormViewHeight { get; set; }
        int LessonListHeight { get; set; }
    }
}