using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Caching.Distributed;
using Suncor.LessonsLearnedMP.Business;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Common;
using Suncor.LessonsLearnedMP.Web.Security;
using Suncor.LessonsLearnedMP.Web.ViewData;
using Suncor.LessonsLearnedMP.Web.ViewData.Shared;

namespace Suncor.LessonsLearnedMP.Web.Controllers
{
    public class LessonController : ControllerBase
    {
        public LessonController(LessonsLearnedMPEntities context, IDistributedCache cache) :base(context, cache)
        {

        }
        [HybridAuthorize(Enumerations.Role.User)]
        [HttpPost]
        public IActionResult GetLessonList(DataTableParametersViewData gridData)
        {
            UserSessionContext userContext = new UserSessionContext(this.HttpContext);

            //Ensure iDisplayLength never causes a divide by zero exception
            gridData.iDisplayLength = (gridData.iDisplayLength == 0) ? 10 : gridData.iDisplayLength;
            
            int pageIndex = (gridData.iDisplayStart / gridData.iDisplayLength) + 1;
            pageIndex = pageIndex <= 0 ? 0 : pageIndex - 1;
            var pageSize = gridData.iDisplayLength;
            int totalCount = 0;

            LessonBusiness lessonBusinessManager = new LessonBusiness(DbContext);

            string gridMessage = "";

            switch (gridData.NavigationPage)
            {
                case Enumerations.NavigationPage.Review:
                case Enumerations.NavigationPage.Edit:
                case Enumerations.NavigationPage.Validate:
                    gridMessage = "Lessons Requiring Action";
                    break;
                case Enumerations.NavigationPage.MyLessons:
                    gridMessage = "My Lessons";
                    //if (gridData.SearchModel.Status.HasValue)//TODO: Add logic for multiple options
                    //{
                    //    gridMessage = "My " + Utility.StringValue((Enumerations.LessonStatus)gridData.SearchModel.Status.Value) + " Lessons";
                    //}
                    if (gridData.SearchModel.SelectedStatus!= null && gridData.SearchModel.SelectedStatus.Count > 0)//TODO: Add logic for multiple options
                    {
                        string StatusList = "";
                        foreach (int StatusID in gridData.SearchModel.SelectedStatus)
                        { 
                            StatusList += Utility.StringValue((Enumerations.LessonStatus)StatusID);
                            StatusList += ", ";
                        }
                        gridMessage = "My " + StatusList + " Lessons";
                    }
                    break;
                case Enumerations.NavigationPage.Search:
                    gridMessage = "Filtered Search Results";
                    break;
                case Enumerations.NavigationPage.Submit:
                    gridMessage = "My Draft Lessons";
                    break;
            }

            List<Lesson> lessons = null;

            if (!gridData.SearchModel.Blank)
            {
                List<SortColumn> sortColumns = null;

                if (gridData.iSortingCols > 0)
                {
                    sortColumns = new List<SortColumn>();

                    if (gridData.iSortingCols > 5)
                    {
                        sortColumns.Add(new SortColumn { Column = (Enumerations.LessonListSortColumn)gridData.iSortCol_5, Direction = gridData.sSortDir_5 == "asc" ? Enumerations.SortDirection.Ascending : Enumerations.SortDirection.Descending, SortOrder = 6 }); 
                    }
                    
                    if (gridData.iSortingCols > 4)
                    {
                        sortColumns.Add(new SortColumn { Column = (Enumerations.LessonListSortColumn)gridData.iSortCol_4, Direction = gridData.sSortDir_4 == "asc" ? Enumerations.SortDirection.Ascending : Enumerations.SortDirection.Descending, SortOrder = 5 }); 
                    }
                    
                    if (gridData.iSortingCols > 3)
                    {
                        sortColumns.Add(new SortColumn { Column = (Enumerations.LessonListSortColumn)gridData.iSortCol_3, Direction = gridData.sSortDir_3 == "asc" ? Enumerations.SortDirection.Ascending : Enumerations.SortDirection.Descending, SortOrder = 4 });
                    }
                  
                    if (gridData.iSortingCols > 2)
                    {
                        sortColumns.Add(new SortColumn { Column = (Enumerations.LessonListSortColumn)gridData.iSortCol_2, Direction = gridData.sSortDir_2 == "asc" ? Enumerations.SortDirection.Ascending : Enumerations.SortDirection.Descending, SortOrder = 3 });
                    }
                    
                    if (gridData.iSortingCols > 1)
                    {
                        sortColumns.Add(new SortColumn { Column = (Enumerations.LessonListSortColumn)gridData.iSortCol_1, Direction = gridData.sSortDir_1 == "asc" ? Enumerations.SortDirection.Ascending : Enumerations.SortDirection.Descending, SortOrder = 2 });
                    }
                        
                    sortColumns.Add(new SortColumn { Column = (Enumerations.LessonListSortColumn)gridData.iSortCol_0, Direction = gridData.sSortDir_0 == "asc" ? Enumerations.SortDirection.Ascending : Enumerations.SortDirection.Descending, SortOrder = 1 });
                }

                lessons = lessonBusinessManager.GetLessonsPaged(sortColumns, userContext.CurrentUser, gridData.SearchModel, false, true, pageIndex, pageSize, out totalCount);
            }

            userContext.LastSearchResults = lessons;

            List<object> result = new List<object>();

            if (lessons != null)
            {
                foreach (var lesson in lessons)
                {
                    Dictionary<string, string> lessonData = new Dictionary<string, string>();
                    lessonData.Add("Enabled", lesson.Enabled.ToString());
                    lessonData.Add("Selected", false.ToString());
                    lessonData.Add("Id", lesson.Id.ToString());
                    lessonData.Add("Status", lesson.Status.Name + (lesson.Enabled == true ? "" : " (Deleted)"));
                    lessonData.Add("Title", HttpUtility.HtmlEncode(lesson.Title));
                    lessonData.Add("Discipline", lesson.Discipline == null ? "" : HttpUtility.HtmlEncode(lesson.Discipline.Name));
                    lessonData.Add("SubmitDate", lesson.SubmittedDate.HasValue ? lesson.SubmittedDate.Value.ToShortDateString() : "");
                    lessonData.Add("Contact", HttpUtility.HtmlEncode(Utility.ContactToDisplayString(lesson.ContactLastName, lesson.ContactFirstName)));
                    string buttonHtml = Utils.RenderPartialViewToString(this, "GridButton", LessonViewModel.ToViewModel(this.HttpContext,lesson));
                    lessonData.Add("Actions", buttonHtml);
                    lessonData.Add("LessonId", lesson.Id.ToString());
                    lessonData.Add("StatusId", lesson.StatusId.ToString());

                    var rawLessonData = (from l in lessonData
                                         select l.Value).ToArray();

                    result.Add(rawLessonData);
                }
            }

            return Json(
                new
                {
                    eEcho = gridData.sEcho,
                    iTotalRecords = totalCount,
                    iTotalDisplayRecords = totalCount,
                    aaData = result,
                    gridMessage = gridMessage
                });
        }

        [HybridAuthorize(Enumerations.Role.User)]
        public IActionResult GetLessonCommentList(int id, DataTableParametersViewData gridData)
        {
            UserSessionContext userContext = new UserSessionContext(this.HttpContext);

            int pageIndex = (gridData.iDisplayStart / gridData.iDisplayLength) + 1;
            pageIndex = pageIndex <= 0 ? 0 : pageIndex - 1;
            var pageSize = gridData.iDisplayLength;
            int totalCount;

            LessonBusiness lessonBusinessManager = new LessonBusiness(DbContext);

            var comments = lessonBusinessManager.GetLessonComments(id, userContext.CurrentUser, userContext.CurrentUser.RoleId == (int)Enumerations.Role.Administrator, out totalCount);

            if (comments == null)
            {
                return Json(null);
            }

            List<object> result = new List<object>();

            foreach (var comment in comments)
            {
                Dictionary<string, string> commentData = new Dictionary<string, string>();
                commentData.Add("Enabled", comment.Enabled.ToString());
                commentData.Add("Id", comment.Id.ToString());
                commentData.Add("Date", comment.CreateDate.ToDisplayDate());
                commentData.Add("User", HttpUtility.HtmlEncode(comment.CreateUser));
                commentData.Add("Type", comment.CommentType.Name + (comment.Enabled == true ? "" : " (Deleted)"));
                commentData.Add("Comment", HttpUtility.HtmlEncode(comment.Content));
                //@Url.Action("Delete", "Lesson", new { id = Model.Id }, "http")
                string buttonHtml = userContext.CurrentUser.RoleId == (int)Enumerations.Role.Administrator ?
                    string.Format("<div><a href='#' class='{0}delete-comment float-left' data-url='{1}' data-commentType='{2}'><span class='float-left web-sprite sprite-{3}'></span>&nbsp;{0}Delete</a><div class='clear'></div></div>",
                    comment.Enabled == true ? "" : "Un-",
                    Url.Action((comment.Enabled == true ? "" : "Un") + "DeleteComment", "Lesson", new { id = comment.Id, lessonId = id }, "http"),
                    comment.CommentType.Name,
                    comment.Enabled == true ? "delete16" : "arrow-undo")
                    : "";
                commentData.Add("Actions", buttonHtml);

                var rawCommentData = (from c in commentData
                                      select c.Value).ToArray();

                result.Add(rawCommentData);
            }

            return Json(
                new
                {
                    eEcho = gridData.sEcho,
                    iTotalRecords = totalCount,
                    iTotalDisplayRecords = totalCount,
                    aaData = result
                });
        }

        public IActionResult Edit(int? id)
        {
            return Index(Enumerations.PageAction.Edit, id);
        }

        public IActionResult MyLessons()
        {
            TempData["MyLessonModel"] = new SearchViewModel { ShowOnlyOwnedLessons = true };
            return Index(Enumerations.PageAction.MyLessons, null);
        }

        [HttpPost]
        public IActionResult MyLessons(SearchViewModel model)
        {
            this.ShowAlert("To return to your full \"My Lessons\" list, click the \"My Lessons\" Tab.", "sprite-book");
            TempData["MyLessonModel"] = model;
            return Index(Enumerations.PageAction.MyLessons, null);
        }

        public IActionResult Submit(int? lessonId)
        {
            return Index(Enumerations.PageAction.Submit, lessonId);
        }

        [HttpPost]
        public IActionResult Edit(LessonViewModel updatedModel)
        {
            return Save(updatedModel);
        }

        [HttpPost]
        public IActionResult Submit(LessonViewModel updatedModel)
        {
            if (updatedModel.SaveAction == Enumerations.SaveAction.SubmitAnother)
            {
                ModelState.Clear();
                return Index(Enumerations.PageAction.Submit, null);
            }

            UserSessionContext userSession = new UserSessionContext(this.HttpContext);
            userSession.DraftDefaults = updatedModel;

            return Save(updatedModel);
        }

        [HttpPost]
        public IActionResult SaveDraft(LessonViewModel updatedModel)
        {
            UserSessionContext userSession = new UserSessionContext(this.HttpContext);
            userSession.DraftDefaults = updatedModel;

            return Save(updatedModel);
        }

        private IActionResult Save(LessonViewModel updatedModel)
        {
            if (ModelState.IsValid)
            {
                if (!updatedModel.IsLessonTypeValidSelected)
                {
                    updatedModel.LessonTypeValidId = null;
                }
                
                UserSessionContext userContext = new UserSessionContext(this.HttpContext);
                ApplicationContext appContext = new ApplicationContext(this.Cache);

                //Populate the Coordinator
                if (!string.IsNullOrWhiteSpace(updatedModel.CoordinatorOwnerSid))
                {
                    if (updatedModel.CoordinatorOwnerSid == Constants.TextDefaults.LLCListPrimaryAdminLabel)
                    {
                        updatedModel.Coordinator = Constants.TextDefaults.LLCListPrimaryAdminLabel;
                    }
                    else
                    {
                        updatedModel.Coordinator = appContext.Coordinators.Where(x => x.Sid == updatedModel.CoordinatorOwnerSid).First().Name;
                    }
                }

                LessonBusiness lessonManager = new LessonBusiness(DbContext);

                string comment = null;
                switch (updatedModel.SaveAction)
                {
                    case Enumerations.SaveAction.AdminToClarification:
                    case Enumerations.SaveAction.BPOToClarification:
                    case Enumerations.SaveAction.ClarificationToBPO:
                    case Enumerations.SaveAction.ClarificationToAdmin:
                        comment = updatedModel.ClarificationComment;
                        break;
                    case Enumerations.SaveAction.AdminToBPO:
                    case Enumerations.SaveAction.BPOToBPO:
                        comment = updatedModel.TransferBpoComment;
                        updatedModel.DisciplineId = updatedModel.TransferBpoDisciplineId;
                        break;
                    case Enumerations.SaveAction.BPOToClose:
                        comment = updatedModel.CloseComment;
                        break;
                }

                var dataModel = updatedModel.ToDataModel();

                var result = lessonManager.SaveLesson(dataModel, userContext.CurrentUser, comment, updatedModel.SaveAction);

                //Send email notification                switch (updatedModel.SaveAction)
                switch (updatedModel.SaveAction)
                {
                    case Enumerations.SaveAction.DraftToNew:
                        SendNotification(result, Enumerations.NotificationEmailType.N1_NewToAdmin);
                        break;
                    case Enumerations.SaveAction.AdminToClarification:
                        SendNotification(result, Enumerations.NotificationEmailType.N3_AdminToClarification);
                        break;
                    case Enumerations.SaveAction.AdminToBPO:
                    case Enumerations.SaveAction.BPOToBPO:
                    case Enumerations.SaveAction.ClosedToBPO: //TODO: Validate this notification should be sent
                        SendNotification(result, Enumerations.NotificationEmailType.N2_AdminToBPO_And_BPOToBPO);
                        break;
                    case Enumerations.SaveAction.BPOToClarification:
                        SendNotification(result, Enumerations.NotificationEmailType.N5_BPOToClarification);
                        break;
                    case Enumerations.SaveAction.BPOToClose:
                        SendNotification(result, Enumerations.NotificationEmailType.N7_BPOToCloseLLC);
                        if (result.OwnerSid != result.CoordinatorOwnerSid)
                        {
                            SendNotification(result, Enumerations.NotificationEmailType.N7_BPOToCloseOwner);
                        }
                        break;
                    case Enumerations.SaveAction.ClarificationToAdmin:
                        SendNotification(result, Enumerations.NotificationEmailType.N4_ClarificationToAdmin);
                        break;
                    case Enumerations.SaveAction.ClarificationToBPO:
                        SendNotification(result, Enumerations.NotificationEmailType.N6_ClarificationToBPO);
                        break;
                    case Enumerations.SaveAction.AssignToUser:
                        SendNotification(result, Enumerations.NotificationEmailType.N10_AssignToUser);
                        break;
                }

                if (updatedModel.SaveAction == Enumerations.SaveAction.DraftToNew)
                {
                    //Issue 88 - Show a different save message when submiting
                    this.ShowAlert("The lesson has successfully been submitted for validation.", "sprite-accept");
                }
                else
                {
                    SetSuccessfulSave();
                }

                if (updatedModel.SaveAction == Enumerations.SaveAction.SaveDraft)
                {
                    return RedirectToActionPermanent("Index", new { pageAction = Enumerations.PageAction.Submit });
                }

                return RedirectToActionPermanent("Index", new { pageAction = updatedModel.ReturnToAction, lessonId = result.Id });
            }

            TempData["Lesson"] = updatedModel;

            var pageAction = Enumerations.PageAction.Edit;

            if (updatedModel.Status == Enumerations.LessonStatus.Draft)
            {
                pageAction = Enumerations.PageAction.Submit;
            }

            return Index(pageAction, updatedModel.Id);
        }

        private void SendNotification(Lesson lesson, Enumerations.NotificationEmailType notificationType)
        {
            List<EmailInfo> email = new List<EmailInfo>();

            if (lesson != null)
            {
                email = GenerateNotifications(new List<Lesson> { lesson }, notificationType);
            }

            LessonBusiness lessonManager = new LessonBusiness(DbContext);
            lessonManager.SendEmails(email);
        }

        private List<EmailInfo> GenerateNotifications(List<Lesson> lessons, Enumerations.NotificationEmailType notificationType)
        {
            List<EmailInfo> emailList = new List<EmailInfo>();

            if (lessons != null && lessons.Count > 0)
            {
                ApplicationContext appContext = new ApplicationContext(this.Cache);

                foreach (var lesson in lessons)
                {
                    //If this key exists in the web.config, re-direct all emails to that address.
                    string overrideEmailAddress = Utility.SafeGetAppConfigSetting<string>("Debug_OverrideEmailAddress", null);

                    EmailTemplateViewData model = new EmailTemplateViewData(LessonViewModel.ToViewModel(this.HttpContext,lesson), notificationType, appContext, overrideEmailAddress);
                    string emailMessageBody = Utils.RenderPartialViewToString(this, "EmailTemplate", model);

                    EmailInfo emailInfo = new EmailInfo
                    {
                        Body = emailMessageBody,
                        MailTo = model.Redirecting ? model.OverrideMailTo : model.MailTo,
                        Subject = model.Subject
                    };

                    //Only send 1 email per email address
                    if (!emailList.Where(x => x.MailTo == emailInfo.MailTo).Any())
                    {
                        emailList.Add(emailInfo);
                    }
                }
            }

            return emailList;
        }
        /// <summary>
        /// Created strictly for sending assingment of lesson to user
        /// </summary>
        /// <param name="lessons"></param>
        /// <param name="notificationType"></param>
        /// <returns></returns>
        private List<EmailInfo> GenerateNotification(List<Lesson> lessons)
        {
            List<EmailInfo> emailList = new List<EmailInfo>();

            if (lessons != null && lessons.Count > 0)
            {
                ApplicationContext appContext = new ApplicationContext(this.Cache);
                UserSessionContext userContext = new UserSessionContext(this.HttpContext);
                
                string overrideEmailAddress = Utility.SafeGetAppConfigSetting<string>("Debug_OverrideEmailAddress", null);
                Lesson template = lessons.FirstOrDefault();
                EmailTemplateViewData model = new EmailTemplateViewData(LessonViewModel.ToViewModel(this.HttpContext,template), Enumerations.NotificationEmailType.N10_AssignToUser, appContext, overrideEmailAddress);

                string htmlBody = "<p>" + model.AssignTo.FirstName + " " + model.AssignTo.LastName + ", you have new lessons waiting to validate from " + userContext.CurrentUser.FirstName + " " + userContext.CurrentUser.LastName + ".</p>";
      
                htmlBody += "<p>Lesson #'s are:<br>";

                UrlHelper urlHelper = new UrlHelper(this.ControllerContext);

                foreach (var lesson in lessons)
                {
                    string url = Suncor.LessonsLearnedMP.Web.Helpers.HtmlHelpers.ToPublicUrl(urlHelper, (new Uri("Lesson/Edit/" + lesson.Id, UriKind.Relative)));
                    htmlBody += " <a href=" + url + ">" + lesson.Id.ToString() + " </a>";
                    htmlBody += "<br>";
                }

                htmlBody += "</p>";


                htmlBody += "<p>Please <a href=" + Suncor.LessonsLearnedMP.Web.Helpers.HtmlHelpers.ToPublicUrl(urlHelper, (new Uri("Lesson/MyLessons/", UriKind.Relative))) + ">login</a> to review lessons assigned to you.</p>";
    
                //string emailMessageBody = Utils.RenderPartialViewToString(this, "EmailTemplate", model);

                EmailInfo emailInfo = new EmailInfo
                {
                    Body = htmlBody,
                    MailTo = model.Redirecting ? model.OverrideMailTo : model.MailTo,
                    Subject = model.Subject
                };

                emailList.Add(emailInfo);
              
            }

            return emailList;
        }

        public IActionResult Index(Enumerations.PageAction pageAction, int? lessonId)
        {
            UserSessionContext userContext = new UserSessionContext(this.HttpContext);

            var lessonModel = new LessonViewModel(this.HttpContext);
            LessonBusiness businessManager = new LessonBusiness(DbContext);

            SearchViewModel userSearch = new SearchViewModel();

            //Set Lesson List search based on page action
            switch (pageAction)
            {
                case Enumerations.PageAction.Submit:
                    //Show only current user's draft lessons
                    userSearch = new SearchViewModel { OwnerSid = userContext.CurrentUser.Sid, Status = (int?)Enumerations.LessonStatus.Draft };
                    break;
                case Enumerations.PageAction.Edit:
                    //Show only editable lessons 
                    userSearch = new SearchViewModel { ShowEditableOnly = true };
                    break;
                case Enumerations.PageAction.Search:
                    //Show the last search
                    userSearch = userContext.LastSearch;
                    break;
                case Enumerations.PageAction.MyLessons:
                    //Show all owned lessons (or filtered lessons of comming from dashboard)
                    userSearch = (SearchViewModel)TempData["MyLessonModel"] ?? new SearchViewModel { ShowOnlyOwnedLessons = true };
                    break;
                default:
                    throw new ArgumentOutOfRangeException("pageAction");
            }

            if (TempData.ContainsKey("Lesson"))
            {
                //Existing lesson (Edit Validation)
                lessonModel = (LessonViewModel)TempData["Lesson"];
            }
            else if (lessonId.HasValue &&
                (pageAction == Enumerations.PageAction.Edit || pageAction == Enumerations.PageAction.Submit))
            {
                //Existing Lesson (Edit Existing)
                var lesson = businessManager.GetLessonById(lessonId.Value);
                lessonModel = LessonViewModel.ToViewModel(this.HttpContext,lesson);

                if (!Utils.IsLessonVisible(lessonModel, userContext.CurrentUser))
                {
                    return RedirectToAction("Search");
                }
            }
            else
            {
                if (pageAction != Enumerations.PageAction.Edit)
                {
                    if (lessonId.HasValue)
                    {
                        //Submit with "Save Changes", leave on submit tab with the recently added lesson
                        var lesson = businessManager.GetLessonById(lessonId.Value);
                        lessonModel = LessonViewModel.ToViewModel(this.HttpContext,lesson);
                    }
                    else
                    {
                        //New Lesson (Submit)
                        lessonModel = userContext.DraftDefaults;
                    }
                }
                else
                {
                    int unused = 0;
                    //Edit / Review, set to first lesson in search list
                    var lesson = businessManager.GetLessonsPaged(userContext.CurrentUser, userSearch, false, true, 0, 1, out unused).FirstOrDefault();
                    lessonModel = LessonViewModel.ToViewModel(this.HttpContext,lesson);
                    if (lesson != null)
                    {
                        lessonId = lesson.Id;
                    }
                    else
                    {
                        ViewBag.NothingToEdit = true;
                    }
                }
            }

            lessonModel.ReturnToAction = pageAction;

            var submittedUsers = businessManager.GetSubmittedByUsers();
            ViewBag.SubmittedByUsers = submittedUsers;

            userContext.LastSystemSearch = userSearch;

            LessonIndexViewModel model = new LessonIndexViewModel(this.HttpContext)
            {
                PageAction = pageAction,
                LessonId = lessonId,
                Lesson = lessonModel,
                SearchModel = userSearch
            };

            return View("Index", model);
        }

        [HttpPost]
        public IActionResult Search(SearchViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserSessionContext userContext = new UserSessionContext(this.HttpContext);

                if (model.Clear)
                {
                    model = new SearchViewModel { Blank = true };
                }
                else
                {
                    model.Clear = false;
                    model.Blank = false;
                    this.ShowAlert("Your search results are being processed and will be displayed in the Lesson List below shortly.  You can clear these results and return to your Lesson List by clicking the arrow on the Search button and selecting \"Clear Search Results\".", "sprite-find");
                }

                if (model.IsLessonTypeValidSelected)
                {
                    model.LessonTypeInvalidId = null;
                }
                else
                {
                    model.LessonTypeValidId = null;                    
                }

                userContext.LastSearch = model;

                ModelState.Clear();
            }

            return Index(Enumerations.PageAction.Search, null);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                UserSessionContext userSession = new UserSessionContext(this.HttpContext);
                LessonBusiness lessonManager = new LessonBusiness(DbContext);
                lessonManager.DeleteLesson(id, userSession.CurrentUser);

                this.ShowAlert("The lesson was successfully deleted.", "sprite-delete16");
            }
            catch(Exception ex)
            {
                this.AddError(ex.Message);
            }

            return RedirectToAction("Index", new { pageAction = Enumerations.PageAction.Search });
        }

        [HttpPost]
        public IActionResult UnDelete(int id)
        {
            try
            {
                UserSessionContext userSession = new UserSessionContext(this.HttpContext);
                LessonBusiness lessonManager = new LessonBusiness(DbContext);
                lessonManager.UnDeleteLesson(id, userSession.CurrentUser);

                this.ShowAlert("The lessons was successfully restored.", "sprite-accept");
            }
            catch (Exception ex)
            {
                this.AddError(ex.Message);
            }

            return RedirectToAction("Index", new { pageAction = Enumerations.PageAction.Search });
        }

        [HttpPost]
        public IActionResult BulkSave(LessonIndexViewModel updatedModel)
        {
            UserSessionContext userSessionContext = new UserSessionContext(this.HttpContext);
            ApplicationContext appContext = new ApplicationContext(this.Cache);
            LessonBusiness lessonManager = new LessonBusiness(DbContext);
            var selectedLessonIds = string.IsNullOrWhiteSpace(updatedModel.BulkSelectedLessons) ? new List<int>() : updatedModel.BulkSelectedLessons.Split(',').Select(x => int.Parse(x)).ToList();
            var selectedLessons = userSessionContext.LastSearchResults.Where(x => selectedLessonIds.Contains(x.Id));
            List<string> errorMessages = new List<string>();
            bool hasSuccesses = false;
            List<EmailInfo> emailsToSend = new List<EmailInfo>();
            List<Lesson> AssignLessonIds = new List<Lesson>();

            if (updatedModel.BulkLesson.SaveAction == Enumerations.SaveAction.ActionLog
                || updatedModel.BulkLesson.SaveAction == Enumerations.SaveAction.CsvLog
                || updatedModel.BulkLesson.SaveAction == Enumerations.SaveAction.CsvLogAll
                || updatedModel.BulkLesson.SaveAction == Enumerations.SaveAction.ActionLogAll)
            {
                Enumerations.LogType logType = Enumerations.LogType.ActionLog;

                if (updatedModel.BulkLesson.SaveAction == Enumerations.SaveAction.CsvLog
                    || updatedModel.BulkLesson.SaveAction == Enumerations.SaveAction.CsvLogAll)
                {
                    logType = userSessionContext.CurrentUser.RoleId == (int)Enumerations.Role.Administrator ? Enumerations.LogType.AdminLog : Enumerations.LogType.GenericLog;
                }

                if (updatedModel.BulkLesson.SaveAction == Enumerations.SaveAction.CsvLogAll
                    || updatedModel.BulkLesson.SaveAction == Enumerations.SaveAction.ActionLogAll)
                {
                    userSessionContext.ExportLog = lessonManager.GenerateLog(userSessionContext.CurrentUser, userSessionContext.LastSystemSearch, logType);
                }
                else
                {
                    userSessionContext.ExportLog = lessonManager.GenerateLog(selectedLessonIds, logType);
                }
            }
            else
            {
                foreach (var selectedLesson in selectedLessons)
                {
                    if (updatedModel.BulkLesson.SaveAction == Enumerations.SaveAction.Delete
                        || updatedModel.BulkLesson.SaveAction == Enumerations.SaveAction.UnDelete)
                    {
                        //Just save, no need to validate
                        lessonManager.SaveLesson(selectedLesson, userSessionContext.CurrentUser, null, updatedModel.BulkLesson.SaveAction);
                        hasSuccesses = true;
                    }
                    else
                    {
                        List<Lesson> successLessons = new List<Lesson>();

                        if (updatedModel.BulkLesson.SaveAction == Enumerations.SaveAction.BPOToClose)
                        {
                            selectedLesson.Resolution = updatedModel.BulkLesson.Resolution;
                            selectedLesson.LessonTypeValidId = updatedModel.BulkLesson.LessonTypeValidId;
                            selectedLesson.LessonTypeInvalidId = updatedModel.BulkLesson.LessonTypeInvalidId;
                        }

                        var model = LessonViewModel.ToViewModel(this.HttpContext,selectedLesson);
                        model.TransferBpoDisciplineId = updatedModel.BulkLesson.TransferBpoDisciplineId;
                        model.SaveAction = updatedModel.BulkLesson.SaveAction;

                        var validatedErrors = model.Validate(null);

                        if (validatedErrors.Count() == 0)
                        {
                            string comment = null;
                            switch (updatedModel.BulkLesson.SaveAction)
                            {
                                case Enumerations.SaveAction.AdminToClarification:
                                case Enumerations.SaveAction.BPOToClarification:
                                case Enumerations.SaveAction.ClarificationToBPO:
                                case Enumerations.SaveAction.ClarificationToAdmin:
                                    comment = updatedModel.BulkLesson.ClarificationComment;
                                    break;
                                case Enumerations.SaveAction.AdminToBPO:
                                case Enumerations.SaveAction.BPOToBPO:
                                    selectedLesson.DisciplineId = updatedModel.BulkLesson.TransferBpoDisciplineId;
                                    comment = updatedModel.BulkLesson.TransferBpoComment;
                                    break;
                                case Enumerations.SaveAction.BPOToClose:
                                    comment = updatedModel.BulkLesson.CloseComment;
                                    break;
                                case Enumerations.SaveAction.AssignToUser:
                                    //selectedLesson.ValidatedBy = updatedModel.BulkLesson.AssignToUserId;
                                    selectedLesson.AssignTo = updatedModel.BulkLesson.AssignToUserId;
                                    break;
                            }

                            var result = lessonManager.SaveLesson(selectedLesson, userSessionContext.CurrentUser, comment, updatedModel.BulkLesson.SaveAction);

                            successLessons.Add(selectedLesson);

                            hasSuccesses = true;
                        }
                        else
                        {
                            foreach (var validationResult in validatedErrors)
                            {
                                errorMessages.Add(model.Id + ": " + validationResult.ErrorMessage);
                            }
                        }

                        //Send email notification
                        switch (updatedModel.BulkLesson.SaveAction)
                        {
                            case Enumerations.SaveAction.DraftToNew:
                                emailsToSend.AddRange(GenerateNotifications(successLessons, Enumerations.NotificationEmailType.N1_NewToAdmin));
                                break;
                            case Enumerations.SaveAction.AdminToClarification:
                                emailsToSend.AddRange(GenerateNotifications(successLessons, Enumerations.NotificationEmailType.N3_AdminToClarification));
                                break;
                            case Enumerations.SaveAction.AdminToBPO:
                            case Enumerations.SaveAction.BPOToBPO:
                            case Enumerations.SaveAction.ClosedToBPO: //TODO: Validate this notification should be sent
                                emailsToSend.AddRange(GenerateNotifications(successLessons, Enumerations.NotificationEmailType.N2_AdminToBPO_And_BPOToBPO));
                                break;
                            case Enumerations.SaveAction.BPOToClarification:
                                emailsToSend.AddRange(GenerateNotifications(successLessons, Enumerations.NotificationEmailType.N5_BPOToClarification));
                                break;
                            case Enumerations.SaveAction.BPOToClose:
                                emailsToSend.AddRange(GenerateNotifications(successLessons, Enumerations.NotificationEmailType.N7_BPOToCloseLLC));
                                break;
                            case Enumerations.SaveAction.ClarificationToAdmin:
                                emailsToSend.AddRange(GenerateNotifications(successLessons, Enumerations.NotificationEmailType.N4_ClarificationToAdmin));
                                break;
                            case Enumerations.SaveAction.ClarificationToBPO:
                                emailsToSend.AddRange(GenerateNotifications(successLessons, Enumerations.NotificationEmailType.N6_ClarificationToBPO));
                                break;
                            case Enumerations.SaveAction.AssignToUser:
                                if (successLessons.Count > 0)
                                {
                                    AssignLessonIds.Add(successLessons.First());
                                }
                                break;
                        }
                    }
                }

                if (AssignLessonIds.Count > 0)
                { 
                    emailsToSend.AddRange(GenerateNotification(AssignLessonIds));
                }

                if (emailsToSend.Count > 0)
                {
                    LessonBusiness businessManager = new LessonBusiness(DbContext);
                    businessManager.SendEmails(emailsToSend);
                }
            }

            ModelState.Clear();

            if (errorMessages.Count > 0)
            {
                string message = hasSuccesses ? "Items have been saved, however the following Lessons could not be processed:" : "No Lessons could be processed.  The following errors occured:";
                errorMessages.Insert(0, message);

                foreach (var error in errorMessages)
                {
                    this.AddError(error);
                }
            }
            else if(hasSuccesses)
            {
                SetSuccessfulSave();
            }

            //Make sure to re-populate correctly if we came from a system initiated search back to MyLessons
            TempData["MyLessonModel"] = userSessionContext.LastSystemSearch;

            return RedirectToActionPermanent("Index", new { pageAction = updatedModel.BulkLesson.ReturnToAction });
        }

        [HybridAuthorize(Enumerations.Role.User)]
        public IActionResult DownloadFile()
        {
            UserSessionContext userSession = new UserSessionContext(this.HttpContext);

            if(userSession.ExportLog != null)
            {
                userSession.ExportLog.Downloaded = true;
                return File(userSession.ExportLog.FileBytes, userSession.ExportLog.ContentType, userSession.ExportLog.FileName);
            }

            return null;
        }

        [HttpPost]
        public IActionResult DeleteComment(int id, int lessonId)
        {
            try
            {
                UserSessionContext userSession = new UserSessionContext(this.HttpContext);
                LessonBusiness lessonManager = new LessonBusiness(DbContext);
                lessonManager.DeleteComment(id, userSession.CurrentUser);

                this.SetSuccessfulSave();
            }
            catch (Exception ex)
            {
                this.AddError(ex.Message);
            }

            return RedirectToAction("Index", new { pageAction = Enumerations.PageAction.Edit, lessonId = lessonId });
        }

        [HttpPost]
        public IActionResult UnDeleteComment(int id, int lessonId)
        {
            try
            {
                UserSessionContext userSession = new UserSessionContext(this.HttpContext);
                LessonBusiness lessonManager = new LessonBusiness(DbContext);
                lessonManager.UnDeleteComment(id, userSession.CurrentUser);

                this.SetSuccessfulSave();
            }
            catch (Exception ex)
            {
                this.AddError(ex.Message);
            }

            return RedirectToAction("Index", new { pageAction = Enumerations.PageAction.Edit, lessonId = lessonId });
        }
    }
}
