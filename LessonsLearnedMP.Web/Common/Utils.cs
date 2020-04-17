using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Caching.Distributed;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Helpers;
using Suncor.LessonsLearnedMP.Web.ViewData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Suncor.LessonsLearnedMP.Web.Common
{
    public static class Utils
    {
        public static bool CurrentUserHasAccess(Enumerations.Role requiredPrivilege)
        {
            return new UserSessionContext(null).UserHasAccess(requiredPrivilege);
        }

        public static bool UserHasAccess(this UserSessionContext user, Enumerations.Role requiredPrivilege)
        {
            if (requiredPrivilege == Enumerations.Role.User)
            {
                return true;
            }

            if (user.CurrentUser == null)
            {
                return false;
            }

            return user.CurrentUser.RoleId == (int)requiredPrivilege;
        }

        //NOTE: This logic should mirror LessonsLearnedMP.Business/LessonBusiness/GetLessonsPaged (filters.ShowEditableOnly = true)
        public static bool IsLessonEditable(IDistributedCache cache, LessonViewModel lesson, RoleUser user)
        {
            if (lesson.Enabled == true)
            {
                var appContext = new ApplicationContext(cache);

                switch ((Enumerations.Role)user.RoleId)
                {
                    case Enumerations.Role.User:
                        switch (lesson.Status)
                        {
                            case Enumerations.LessonStatus.New:
                            case Enumerations.LessonStatus.AdminReview:
                            case Enumerations.LessonStatus.Clarification:
                            case Enumerations.LessonStatus.BPOReview:
                            case Enumerations.LessonStatus.Closed:
                            case Enumerations.LessonStatus.MIGRATION:
                                return false;
                            case Enumerations.LessonStatus.Draft:
                                //Owner can edit their own drafts
                                return lesson.OwnerSid == user.Sid;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    case Enumerations.Role.Coordinator:
                        switch (lesson.Status)
                        {
                            case Enumerations.LessonStatus.Clarification:
                                //Coordinators can edit Clarifications that they own or are assigned to
                                return lesson.OwnerSid == user.Sid
                                    || lesson.CoordinatorOwnerSid == user.Sid;
                            case Enumerations.LessonStatus.BPOReview:
                                //DS:Issue 79 - This is incorrect.  Removed this rule. Coordinators can edit BPO Review that they are assigned to
                                //return lesson.CoordinatorOwnerSid == user.Sid;
                                return false;
                            case Enumerations.LessonStatus.New:
                            case Enumerations.LessonStatus.AdminReview:
                            case Enumerations.LessonStatus.Closed:
                            case Enumerations.LessonStatus.MIGRATION:
                                return false;
                            case Enumerations.LessonStatus.Draft:
                                //Owner can edit their own drafts
                                return lesson.OwnerSid == user.Sid;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    case Enumerations.Role.BPO:
                        switch (lesson.Status)
                        {
                            case Enumerations.LessonStatus.Clarification:
                                //BPO user can edit Clarifications they own, and lessons assigned to their discipline
                                return appContext.DisciplineUsers.Where(x => x.DisciplineId == lesson.DisciplineId && x.Sid == user.Sid).Any()
                                    || lesson.OwnerSid == user.Sid;
                            case Enumerations.LessonStatus.BPOReview:
                                //BPO user can edit BPO Review lessons assigned to their discipline
                                return appContext.DisciplineUsers.Where(x => x.DisciplineId == lesson.DisciplineId && x.Sid == user.Sid).Any();
                            case Enumerations.LessonStatus.New:
                            case Enumerations.LessonStatus.AdminReview:
                            case Enumerations.LessonStatus.Closed:
                            case Enumerations.LessonStatus.MIGRATION:
                                return false;
                            case Enumerations.LessonStatus.Draft:
                                //Owner can edit their own drafts
                                return lesson.OwnerSid == user.Sid;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    //Admin can do anything
                    case Enumerations.Role.Administrator:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return false;
        }

        //Remove lessons from the list the user has no business seeing
        //NOTE: This logic should mirror LessonsLearnedMP.Business/LessonBusiness/GetLessonsPaged
        public static bool IsLessonVisible(LessonViewModel lesson, RoleUser user)
        {
            if (lesson.Enabled!=true)
            {
                return user.RoleId == (int)Enumerations.Role.Administrator;
            }

            if((lesson.Status == Enumerations.LessonStatus.Closed && lesson.LessonTypeInvalidId == null)
                || (lesson.Status != Enumerations.LessonStatus.Closed && lesson.OwnerSid == user.Sid))
            {
                //all user can see valid closed lessons
                //ALl users can see their own open lessons
                return true;
            }

            switch ((Enumerations.Role)user.RoleId)
            {
                case Enumerations.Role.User:
                    //User can see Draft Lessons they own
                    //User can see New Lessons they own
                    //User can see Admin Review Lessons they own
                    //User can see Clarification Lessons they own
                    //User can see BPO Review Lessons they own
                    //User can see Closed Lessons (Valid only)
                    return false;
                case Enumerations.Role.Coordinator:
                    //User can see Draft Lessons they own
                    //User can see New Lessons they own
                    //User can see Admin Review Lessons they own
                    //User can see Clarification Lessons they own
                    //User can see Clarification Lessons they are assigned to
                    //User can see BPO Review Lessons they own
                    //User can see BPO Review Lessons they are assigned to
                    //User can see Closed Lessons (Valid only)
                    return (lesson.Status == Enumerations.LessonStatus.Clarification && lesson.CoordinatorOwnerSid == user.Sid)
                        || (lesson.Status == Enumerations.LessonStatus.BPOReview && lesson.CoordinatorOwnerSid == user.Sid);
                case Enumerations.Role.BPO:
                    //User can see Draft Lessons they own
                    //User can see New Lessons they own
                    //User can see Admin Review Lessons they own
                    //User can see Clarification Lessons they own
                    //User can see Clarification Lessons for their discipline
                    //User can see BPO Review Lessons they own
                    //User can see BPO Review Lessons for their discipline
                    //User can see Closed Lessons for their discipline (Valid or Invalid)
                    //User can see Closed Lessons for other disciplines (Valid only)
                    return (user.DisciplineId != null && lesson.DisciplineId == user.DisciplineId);
                case Enumerations.Role.Administrator:
                    //No action, users can see everything
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void ClearValid(this ModelStateDictionary modelState, bool keepFirstRow)
        {
            var errors = modelState.Where(ms => (ms.Value.Errors != null && ms.Value.Errors.Count > 0) || (keepFirstRow && ms.Key.StartsWith("GridData[0]"))).ToList();

            modelState.Clear();

            foreach (var error in errors)
            {
                foreach(var v in error.Value.Errors)
                {
                    modelState.AddModelError(error.Key, v.ErrorMessage);

                }
            }
        }

        public static int AutoCompleteMaxListSize
        {
            get { return 25; }
        }

        public static int PagingSize
        {
            get
            {
                return int.Parse(Utility.SafeGetAppConfigSetting<string>("PagingSize", "25"));
            }
        }

        // Using code from here:
        // http://blog.approache.com/2010/11/render-any-aspnet-mvc-IActionResult-to.html
        public class ResponseCapture : IDisposable
        {
            private readonly HttpResponse response;
         
            private readonly Stream originalStream;
            private MemoryStream localStream;
            public ResponseCapture(HttpResponse response)
            {
                this.response = response;
                originalStream = response.Body;

                localStream = new MemoryStream();
                response.Body = localStream;
            }
            public override string ToString()
            {
                localStream.Flush();

                return Encoding.UTF8.GetString(localStream.ToArray());
            }

            public void Dispose()
            {
                if (localStream != null)
                {
                    localStream.Dispose();

                    localStream = null;

                    response.Body = originalStream;
                }
            }
        }

        public static string Capture(this ActionResult result, ControllerContext controllerContext)
        {
            using (var it = new ResponseCapture(controllerContext.HttpContext.Response))
            {
                result.ExecuteResult(controllerContext);
                return it.ToString();
            }
        }

         public static string RenderPartialViewToString(Controller controller, string viewName, object model)
         {
             controller.ViewData.Model = model;
             try
             {
                 using (StringWriter sw = new StringWriter())
                 {
                     var engine = controller.GetCompositeViewEngine();

                     ViewEngineResult viewResult = engine.FindView(controller.ControllerContext, viewName, false);

                     ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw, null);

                     viewResult.View.RenderAsync(viewContext).Wait();

                     return sw.GetStringBuilder().ToString();
                 }
             }
             catch (Exception ex)
             {
                 return ex.ToString();
             }
         }
        
        public static dynamic ToDisplayList(this List<ReferenceValue> list, bool administratorAccess = false, bool formatAmpersand = true)
        {
            return ToDisplayList(list, null, administratorAccess, formatAmpersand);
        }

        public static dynamic ToDisplayList(this List<ReferenceValue> list, int? currentValue, bool administratorAccess = false, bool formatAmpersand = true)
        {
            return (from l in list
                    where (l.Enabled || administratorAccess)
                    || (currentValue.HasValue && l.Id == currentValue.Value)
                    select new 
                    {
                        Id = l.Id,
                        Name = Utility.FormatReferenceValueNameForDisplay(l.Name, l.Enabled, formatAmpersand),
                    }).ToList();
        }

        public static IEnumerable<SelectListItem> ToSelectList(this List<ReferenceValue> list, List<int> selectedValues, bool administratorAccess = false)
        {
            if (selectedValues == null)
            {
                selectedValues = new List<int>();
            }

            return (from l in list
                    where (l.Enabled || administratorAccess)
                    || selectedValues.Contains(l.Id)
                    select new SelectListItem
                    {
                        Selected = selectedValues.Contains(l.Id),
                        Text = Utility.FormatReferenceValueNameForDisplay(l.Name, l.Enabled),
                        Value = l.Id.ToString()
                    }).ToList();
        }

        public static IEnumerable<SelectListItem> ToSelectList(this List<ReferenceValue> list, int? currentValue, bool administratorAccess = false)
        {
            return (from l in list
                    where (l.Enabled || administratorAccess)
                    || (currentValue.HasValue && l.Id == currentValue.Value)
                    select new SelectListItem
                    {
                        Selected = l.Id == currentValue,
                        Text = Utility.FormatReferenceValueNameForDisplay(l.Name, l.Enabled),
                        Value = l.Id.ToString()
                    }).ToList();
        }

        public static dynamic SubmittedUsersToDisplayList(this List<RoleUser> list)
        {
            return (from l in list
                    select new 
                    {
                        Name = Utility.FormatUserNameForDisplay(l.LastName, l.FirstName, null, true, false)
                    }).ToList();
        }

        public static dynamic ToDisplayList(this List<RoleUser> list, bool administratorAccess = false, bool addPrimaryAdminUser = false)
        {
            var result = (from l in list
                          where administratorAccess || l.Enabled
                          select new 
                          {
                              Name = FormatUserNameForDisplay(l)
                          }).ToList();

            if (addPrimaryAdminUser)
            {
                result.Insert(0, new  { Name = Constants.TextDefaults.LLCListPrimaryAdminLabel });
            }

            return result;
        }

        public static string FormatUserNameForDisplay(RoleUser person, bool showEmail = true)
        {
            if (person != null)
            {
                return Utility.FormatUserNameForDisplay(person.LastName, person.FirstName, person.Email, person.Enabled, showEmail);
            }

            return "[*Unknown*]";
        }
    }
}
