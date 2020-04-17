using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Suncor.LessonsLearnedMP.Business;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Common;
using Suncor.LessonsLearnedMP.Web.Security;

namespace Suncor.LessonsLearnedMP.Web.Controllers
{
    public class DashboardController : ControllerBase
    {
        public DashboardController(LessonsLearnedMPEntities context, IDistributedCache cache) : base(context, cache)
        {

        }
        [HybridAuthorize(Enumerations.Role.Administrator, Enumerations.Role.BPO, Enumerations.Role.Coordinator)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [HybridAuthorize(Enumerations.Role.Administrator, Enumerations.Role.BPO, Enumerations.Role.Coordinator)]
        public IActionResult GetOpenLessonsChartData()
        {
            LessonBusiness lessonManager = new LessonBusiness(DbContext);
            UserSessionContext userSessionContext = new UserSessionContext(this.HttpContext);

            //Filter by lessons user owns.  For Admin, show all.
            var filter = new LessonFilters { ShowOnlyOwnedLessons = true };

            int unused = 0;
            var userLessons = lessonManager.GetLessonsPaged(userSessionContext.CurrentUser, filter, false, 0, 0, out unused)
                .Where(x => x.StatusId != (int)Enumerations.LessonStatus.MIGRATION && x.StatusId != (int)Enumerations.LessonStatus.Closed).ToList();

            var data = from l in userLessons
                       group l by l.Status into lessonsByStatus
                       select new
                       {
                           Label = lessonsByStatus.Key.Name,
                           Percent = Math.Round((((double)lessonsByStatus.Count()) / userLessons.Count() * 100), 1),
                           Count = lessonsByStatus.Count(),
                           StatusId = lessonsByStatus.Key.Id,
                           Sort = lessonsByStatus.Key.SortOrder
                       };

            return Json(data.OrderBy(x => x.Sort));
        }

        [HttpPost]
        [HybridAuthorize(Enumerations.Role.Administrator, Enumerations.Role.BPO, Enumerations.Role.Coordinator)]
        public IActionResult GetClosedLessonsChartData(int year, bool isValidSelected)
        {
            LessonBusiness lessonManager = new LessonBusiness(DbContext);
            UserSessionContext userSessionContext = new UserSessionContext(this.HttpContext);
            ApplicationContext appContext = new ApplicationContext(this.Cache);

            int unused = 0;
            var userLessons = lessonManager.GetLessonsPaged(userSessionContext.CurrentUser,
                new LessonFilters 
                {
                    //Status = (int)Enumerations.LessonStatus.Closed
                    SelectedStatus = new List<int>(new int[] {(int)Enumerations.LessonStatus.Closed} ),
                    ShowOnlyOwnedLessons = userSessionContext.CurrentUser.RoleId != (int)Enumerations.Role.Administrator
                },
                false, true, 0, 0, out unused);

            //Meaning "Pre-2010" was selected from the list, gatehr all lessons closed previous to Jan 1 2010
            if (year == 0)
            {
                userLessons = userLessons.Where(x => x.ClosedDate.HasValue && x.ClosedDate.Value.Year < 2010).ToList();
            }
            else
            {
                //Another valid year was selected
                userLessons = userLessons.Where(x => x.ClosedDate.HasValue && x.ClosedDate.Value.Year == year).ToList();
            }

            List<ReferenceValue> distinctLessonTypes = new List<ReferenceValue>();

            if (isValidSelected)
            {
                distinctLessonTypes = appContext.LessonTypesValid.Where(x => x.Enabled || userSessionContext.CurrentUser.RoleId == (int)Enumerations.Role.Administrator).Distinct().OrderBy(x => x.SortOrder).ToList();
            }
            else
            {
                distinctLessonTypes = appContext.LessonTypesInvalid.Where(x => x.Enabled || userSessionContext.CurrentUser.RoleId == (int)Enumerations.Role.Administrator).Distinct().OrderBy(x => x.SortOrder).ToList();
            }

            List<ClosedLessonType> closedLessonTypeData = new List<ClosedLessonType>();

            //Create a detail for each distinct lesson type
            foreach (var lessonType in distinctLessonTypes)
            {
                ClosedLessonType closedLessonType = new ClosedLessonType();
                closedLessonType.LessonTypeId = lessonType.Id;
                closedLessonType.LessonTypeName = lessonType.Name;

                //Get the data for each Month
                for (int i = 1; i <= 12; i++)
                {
                    ClosedLessonTypeDetail month = new ClosedLessonTypeDetail
                        {
                            Count = userLessons.Where(x =>
                                (isValidSelected ? x.LessonTypeValidId : x.LessonTypeInvalidId) == lessonType.Id
                                && x.ClosedDate.Value.Month == i).Count()
                        };

                    closedLessonType.Detail.Add(month);
                }

                closedLessonTypeData.Add(closedLessonType);
            }

            return Json(closedLessonTypeData);
        }
    }

    public class ClosedLessonType
    {
        public ClosedLessonType()
        {
            Detail = new List<ClosedLessonTypeDetail>();
        }

        public int LessonTypeId { get; set; }
        public string LessonTypeName { get; set; }
        public List<ClosedLessonTypeDetail> Detail { get; set; }
    }

    public class ClosedLessonTypeDetail
    {
        public int Count { get; set; }
    }
}
