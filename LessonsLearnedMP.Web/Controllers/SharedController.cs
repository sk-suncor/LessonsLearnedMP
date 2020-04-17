using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
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
    public class SharedController : ControllerBase
    {
        [CoverageExclude]
        public SharedController(LessonsLearnedMPEntities context, IDistributedCache cache) :base(context, cache)
        {
        }

        [HybridAuthorize(Enumerations.Role.User)]
        public IActionResult LeftRightList(LeftRightListViewdata list)
        {
            return PartialView(list);
        }

        [HttpPost]
        [CoverageExclude]
        public void LogJavascriptError(string message, string url, string lineNumber, string browser, string browserVersion)
        {
            string error = string.Format("Client Side Scripting Error:{0}{1} Version {2}{0}URL: {3}{0}Line #: {4}{0}Error Message: {5}",
                Environment.NewLine,
                browser,
                browserVersion,
                url,
                lineNumber,
                message);

            Logger.Error(Url, error);
        }

        [HttpPost]
        [HybridAuthorize(Enumerations.Role.User)]
        public JsonResult SetSearchListPageSize(int size)
        {
            new UserCookieContext(this.ControllerContext.HttpContext).SearchistPageSize = size;
            return Json(null);
        }

        [HttpPost]
        [HybridAuthorize(Enumerations.Role.User)]
        public JsonResult SetLongFormViewHeight(int height)
        {
            new UserCookieContext(this.ControllerContext.HttpContext).LongFormViewHeight = height;
            return Json(null);
        }

        [HttpPost]
        [HybridAuthorize(Enumerations.Role.User)]
        public JsonResult SetLessonListHeight(int height)
        {
            new UserCookieContext(this.ControllerContext.HttpContext).LessonListHeight = height;
            return Json(null);
        }

        [HttpPost]
        [HybridAuthorize(Enumerations.Role.User)]
        public JsonResult AutoCompleteContactList(string term)
        {
            var beginTag = "::";
            var endTag = ";;";

            ApplicationContext appContext = new ApplicationContext(this.Cache);

            List<string> searchTerms = new List<string>();

            //split the terms by spaces, so each term is tested
            if(!string.IsNullOrWhiteSpace(term))
            {
                searchTerms = new List<string>(term.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                searchTerms.Remove(beginTag);
                searchTerms.Remove(endTag);
            }

            if (appContext.AllUsers != null)
            {
                var query = from u in appContext.AllUsers
                            where u.Enabled
                            let label = Utils.FormatUserNameForDisplay(u)
                            where searchTerms.Count == 0
                            || searchTerms.All(s => label.Contains(s, StringComparison.OrdinalIgnoreCase)) //test to ensure ALL terms supplied exist in the label
                            select new ContactListViewModel
                            {
                                Label = label,
                                Name = Utils.FormatUserNameForDisplay(u, false),
                                Email = u.Email,
                                Phone = u.Phone
                            };

                //Return the first x results
                var results = query.Take(Utils.AutoCompleteMaxListSize).ToList();

                //Wrap the label in some html so we can highlight the matched terms
                if (searchTerms.Count > 0)
                {
                    var termList = searchTerms.ToList();
                    foreach (var result in results)
                    {
                        termList.ForEach(x =>
                            {
                                //First replace matches with begin and end tag special characters
                                var regex = new Regex(x, RegexOptions.IgnoreCase);
                                var evaluator = new MatchEvaluator(match => Wrap(match, result.Label, beginTag + "{0}" + endTag));
                                result.Label = regex.Replace(result.Label, evaluator);
                            });

                        //now repace the special tags with the real html tags
                        //This process will prevent matching the class names in the tags themselves
                        var regexSecond = new Regex(beginTag, RegexOptions.IgnoreCase);
                        result.Label = regexSecond.Replace(result.Label, "<span class='autocomplete-highlight'>");
                        regexSecond = new Regex(endTag, RegexOptions.IgnoreCase);
                        result.Label = regexSecond.Replace(result.Label, "</span>");
                    }
                }
                    
                return Json(results);
            }

            return Json(null);
        }

        public static string Wrap(Match m, string original, string format)
        {
            // it is a match, apply the format
            return string.Format(format, m.Value);
        }

        [HybridAuthorizeAttribute(Enumerations.Role.User)]
        public IActionResult ActionButton(Enumerations.PageAction pageAction, Enumerations.LessonStatus lessonStatus, bool readOnly, bool lessonEnabled)
        {
            UserSessionContext userSessionContext = new UserSessionContext(this.HttpContext);
            List<SaveActionOption> options = new List<SaveActionOption>();

            string buttonText = "";

            switch (pageAction)
            {
                case Enumerations.PageAction.Submit:
                    if (!readOnly && lessonStatus == Enumerations.LessonStatus.Draft)
                    {
                        options.Add(new SaveActionOption { ButtonType = ButtonType.MainDropDown, ButtonText = "Save", IconClass = "sprite-disk", SaveAction = Enumerations.SaveAction.SaveChanges });
                        options.Add(new SaveActionOption { ButtonType = ButtonType.SecondaryAction, ButtonText = "Submit", IconClass = "sprite-new", SaveAction = Enumerations.SaveAction.DraftToNew });
                        options.Add(new SaveActionOption { ButtonText = "Save Draft & Add Another Lesson", IconClass = "sprite-report-disk", SaveAction = Enumerations.SaveAction.SaveDraft });
                    }
                    else
                    {
                        options.Add(new SaveActionOption { ButtonType = ButtonType.MainDropDown, ButtonText = "Submit Another", IconClass = "sprite-add", SaveAction = Enumerations.SaveAction.SubmitAnother });
                    }
                    break;
                case Enumerations.PageAction.Edit:
                    if (readOnly && !lessonEnabled && userSessionContext.CurrentUser.RoleId == (int)Enumerations.Role.Administrator)
                    {
                        options.Add(new SaveActionOption { ButtonType = ButtonType.MainDropDown, ButtonText = "Un-Delete", IconClass = "sprite-arrow-undo", SaveAction = Enumerations.SaveAction.UnDelete });
                    }

                    if (!readOnly)
                    {
                        switch (lessonStatus)
                        {
                            case Enumerations.LessonStatus.MIGRATION:
                                options.Add(new SaveActionOption { ButtonType = ButtonType.MainDropDown, ButtonText = "Save", IconClass = "sprite-disk", SaveAction = Enumerations.SaveAction.SaveChanges });
                                options.Add(new SaveActionOption { ButtonText = "Transfer to Admin", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.NewToAdmin });
                                break;
                            case Enumerations.LessonStatus.Draft:
                                options.Add(new SaveActionOption { ButtonType = ButtonType.MainDropDown, ButtonText = "Save", IconClass = "sprite-disk", SaveAction = Enumerations.SaveAction.SaveChanges });
                                options.Add(new SaveActionOption { ButtonText = "Submit", IconClass = "sprite-new", SaveAction = Enumerations.SaveAction.DraftToNew });
                                break;
                            case Enumerations.LessonStatus.New:
                                options.Add(new SaveActionOption { ButtonType = ButtonType.MainDropDown, ButtonText = "Transfer to Admin", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.NewToAdmin });
                                break;
                            case Enumerations.LessonStatus.AdminReview:
                                options.Add(new SaveActionOption { ButtonType = ButtonType.MainDropDown, ButtonText = "Save", IconClass = "sprite-disk", SaveAction = Enumerations.SaveAction.SaveChanges });
                                options.Add(new SaveActionOption { ButtonText = "Transfer to BPO", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.AdminToBPO });
                                options.Add(new SaveActionOption { ButtonText = "Request Clarification", IconClass = "sprite-note-error", SaveAction = Enumerations.SaveAction.AdminToClarification });
                                break;
                            case Enumerations.LessonStatus.Clarification:
                                options.Add(new SaveActionOption { ButtonType = ButtonType.MainDropDown, ButtonText = "Save", IconClass = "sprite-disk", SaveAction = Enumerations.SaveAction.SaveChanges });
                                options.Add(new SaveActionOption { ButtonText = "Clarify and Transfer to Admin", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.ClarificationToAdmin });
                                options.Add(new SaveActionOption { ButtonText = "Clarify and Transfer to BPO", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.ClarificationToBPO });
                                break;
                            case Enumerations.LessonStatus.BPOReview:
                                options.Add(new SaveActionOption { ButtonType = ButtonType.MainDropDown, ButtonText = "Save", IconClass = "sprite-disk", SaveAction = Enumerations.SaveAction.SaveChanges });
                                options.Add(new SaveActionOption { ButtonText = "Close", IconClass = "sprite-accept", SaveAction = Enumerations.SaveAction.BPOToClose });
                                options.Add(new SaveActionOption { ButtonText = "Transfer to Another BPO", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.BPOToBPO });
                                options.Add(new SaveActionOption { ButtonText = "Request Clarification", IconClass = "sprite-note-error", SaveAction = Enumerations.SaveAction.BPOToClarification });
                                options.Add(new SaveActionOption { ButtonText = "Assign to User", IconClass = "sprite-note-error", SaveAction = Enumerations.SaveAction.AssignToUser });
                                break;
                            case Enumerations.LessonStatus.Closed:
                                if (userSessionContext.CurrentUser.RoleId == (int)Enumerations.Role.Administrator && lessonEnabled)
                                {
                                    options.Add(new SaveActionOption { ButtonType = ButtonType.MainDropDown, ButtonText = "Save", IconClass = "sprite-disk", SaveAction = Enumerations.SaveAction.SaveChanges });
                                    options.Add(new SaveActionOption { ButtonText = "Return to BPO Review", IconClass = "sprite-arrow-180", SaveAction = Enumerations.SaveAction.ClosedToBPO });
                                }
                                break;
                        }

                        if (lessonEnabled && userSessionContext.CurrentUser.RoleId == (int)Enumerations.Role.Administrator)
                        {
                            options.Add(new SaveActionOption { ButtonText = "Delete", IconClass = "sprite-delete16", SaveAction = Enumerations.SaveAction.Delete });
                        }
                    }
                    break;
                case Enumerations.PageAction.Search:
                    options.Add(new SaveActionOption { ButtonType = ButtonType.MainDropDown, ButtonText = "Search", IconClass = "sprite-find" });
                    options.Add(new SaveActionOption { ButtonText = "Clear Search Results", IconClass = "sprite-page-find" });
                    break;
            }

            return PartialView(options);
        }

        [HttpPost]
        [HybridAuthorizeAttribute(Enumerations.Role.User)]
        public IActionResult ActionButtonBulk(List<int> selectedLessonIds)
        {
            List<SaveActionOption> options = new List<SaveActionOption>();
            UserSessionContext userSessionContext = new UserSessionContext(this.HttpContext);

            if (selectedLessonIds != null && selectedLessonIds.Count > 0)
            {
                bool adminRole = userSessionContext.CurrentUser.RoleId == (int)Enumerations.Role.Administrator;

                List<Lesson> selectedLessons = new List<Lesson>();

                if (userSessionContext.LastSearchResults != null)
                {
                    selectedLessons = userSessionContext.LastSearchResults.Where(x => selectedLessonIds.Contains(x.Id)).ToList();
                }
                else
                {
                    LessonBusiness lessonManager = new LessonBusiness(DbContext);
                    selectedLessons = lessonManager.GetLessons(selectedLessonIds, userSessionContext.CurrentUser);
                }

                if (selectedLessons.Select(x => x.DisciplineId).Distinct().Count() == 1)
                {
                    ViewBag.DisciplineId = selectedLessons.Select(x => x.DisciplineId).Distinct().First();
                }

                int editableCount = selectedLessons.Where(x => Utils.IsLessonEditable(this.Cache, LessonViewModel.ToViewModel(HttpContext, x), userSessionContext.CurrentUser)).Count();
                bool allEditable = editableCount == selectedLessons.Count();
                bool allReadOnly = editableCount == 0;
                bool allEnabled = !selectedLessons.Where(x => x.Enabled!=true).Any();

                bool deleteAdded = false;
                if (allEnabled && adminRole)
                {
                    deleteAdded = true;
                    options.Add(new SaveActionOption { ButtonText = "Delete Selected", IconClass = "sprite-delete16", SaveAction = Enumerations.SaveAction.Delete, SortOrder = 999 });
                }
                else if(allReadOnly && adminRole)
                {
                    options.Add(new SaveActionOption { ButtonText = "Un-Delete Selected", IconClass = "sprite-arrow-undo", SaveAction = Enumerations.SaveAction.UnDelete });
                }

                //Only show buttons if the selected items have the same status
                if (selectedLessons.Select(x => x.StatusId).Distinct().Count() == 1)
                {
                    var selectedLessonsStatus = (Enumerations.LessonStatus)selectedLessons.Select(x => x.StatusId).Distinct().First();
                    if (allEditable)
                    {
                        switch (selectedLessonsStatus)
                        {
                            case Enumerations.LessonStatus.MIGRATION:
                                options.Add(new SaveActionOption { ButtonText = "Transfer Selected to Admin", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.NewToAdmin });
                                break;
                            case Enumerations.LessonStatus.Draft:
                                options.Add(new SaveActionOption { ButtonText = "Submit Selected", IconClass = "sprite-new", SaveAction = Enumerations.SaveAction.DraftToNew });

                                if (!deleteAdded)
                                {
                                    options.Add(new SaveActionOption { ButtonText = "Delete Selected", IconClass = "sprite-delete16", SaveAction = Enumerations.SaveAction.Delete, SortOrder = 999 });
                                }
                                break;
                            case Enumerations.LessonStatus.New:
                                options.Add(new SaveActionOption { ButtonText = "Transfer Selected to Admin", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.NewToAdmin });
                                break;
                            case Enumerations.LessonStatus.AdminReview:

                                options.Add(new SaveActionOption { ButtonText = "Transfer Selected to BPO", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.AdminToBPO, SortOrder = 1 });
                                options.Add(new SaveActionOption { ButtonText = "Request Clarification for Selected", IconClass = "sprite-note-error", SaveAction = Enumerations.SaveAction.AdminToClarification, SortOrder = 2 });
                                break;
                            case Enumerations.LessonStatus.Clarification:
                                options.Add(new SaveActionOption { ButtonText = "Transfer Selected to Admin", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.ClarificationToAdmin, SortOrder = 1 });
                                options.Add(new SaveActionOption { ButtonText = "Transfer Selected to BPO", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.ClarificationToBPO, SortOrder = 2 });
                                break;
                            case Enumerations.LessonStatus.BPOReview:

                                if (selectedLessons.Select(x => x.DisciplineId).Distinct().Count() == 1)
                                {
                                    options.Add(new SaveActionOption { ButtonText = "Close Selected", IconClass = "sprite-accept", SaveAction = Enumerations.SaveAction.BPOToClose, SortOrder = 1 });
                                }
                                
                                options.Add(new SaveActionOption { ButtonText = "Transfer Selected to Another BPO", IconClass = "sprite-arrow", SaveAction = Enumerations.SaveAction.BPOToBPO, SortOrder = 2 });
                                options.Add(new SaveActionOption { ButtonText = "Request Clarification for Selected", IconClass = "sprite-note-error", SaveAction = Enumerations.SaveAction.BPOToClarification, SortOrder = 3 });
                                options.Add(new SaveActionOption { ButtonText = "Assign to User", IconClass = "sprite-note-error", SaveAction = Enumerations.SaveAction.AssignToUser, SortOrder = 4 });
                                break;
                        }
                    }
                }

                options.Add(new SaveActionOption { ButtonText = "Action Log for Selected", IconClass = "sprite-script-edit", SaveAction = Enumerations.SaveAction.ActionLog, SortOrder = 98 });
                options.Add(new SaveActionOption { ButtonText = "Excel File for Selected", IconClass = "sprite-table-edit", SaveAction = Enumerations.SaveAction.CsvLog, SortOrder = 99 });
            }

            if (userSessionContext.LastSearchResults != null && userSessionContext.LastSearchResults.Count > 0)
            {
                options.Add(new SaveActionOption { ButtonText = "Action Log for All Results", IconClass = "sprite-script-edit", SaveAction = Enumerations.SaveAction.ActionLogAll, SortOrder = 98 });
                options.Add(new SaveActionOption { ButtonText = "Excel File for All Results", IconClass = "sprite-table-edit", SaveAction = Enumerations.SaveAction.CsvLogAll, SortOrder = 99 });
            }

            if (options.Count > 0)
            {
                options = options.OrderBy(x => x.SortOrder).ToList();

                //Add "Menu" button (does not do anything, just causes all other options to be grouped in the drop down list)
                options.Insert(0, new SaveActionOption { ButtonType = Web.ViewData.ButtonType.MainDropDown, ButtonText = "Menu", SaveAction = Enumerations.SaveAction.None });
            }

            return PartialView("ActionButton", options);
        }

        [HttpPost]
        [HybridAuthorizeAttribute(Enumerations.Role.User)]
        public IActionResult ChangeRole(int roleId)
        {
            if (bool.Parse(Utility.SafeGetAppConfigSetting("Debug_AllUserPermissionChange", "false")))
            {
                UserSessionContext userSession = new UserSessionContext(this.HttpContext);
                var currentuser = userSession.CurrentUser;

                //tem code
               
                currentuser.RoleId = roleId;

                userSession.CurrentUser = currentuser;
            }

            return RedirectToActionPermanent("Index", "Home");
        }

        [HybridAuthorize(Enumerations.Role.User)]
        public IActionResult HelpTopic(Enumerations.HelpTopic helpTopic)
        {
            return PartialView(helpTopic);
        }
    }
}
