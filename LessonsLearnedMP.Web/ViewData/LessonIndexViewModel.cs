using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Web.ViewData
{
    public class LessonIndexViewModel
    {
        public LessonIndexViewModel(HttpContext context)
        {
            Lesson = new LessonViewModel(context);
            SearchModel = new SearchViewModel();
            BulkLesson = new LessonViewModel(context);
        }

        public Enumerations.PageAction PageAction { get; set; }
        public int? LessonId { get; set; }
        public LessonViewModel Lesson { get; set; }
        public SearchViewModel SearchModel { get; set; }
        public LessonViewModel BulkLesson { get; set; }
        public string BulkSelectedLessons { get; set; }
    }
}