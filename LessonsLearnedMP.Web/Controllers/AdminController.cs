using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Suncor.LessonsLearnedMP.Business;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Common;
using Suncor.LessonsLearnedMP.Web.Security;
using Suncor.LessonsLearnedMP.Web.ViewData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suncor.LessonsLearnedMP.Web.Controllers
{
    public class AdminController : ControllerBase
    {
        [CoverageExclude]
        public AdminController(LessonsLearnedMPEntities context, IDistributedCache cache) :base(context, cache)
        {
        }

        [HybridAuthorizeAttribute(Enumerations.Role.Administrator)]
        public IActionResult Index()
        {
            ApplicationContext appContext = new ApplicationContext(this.Cache);

            AdminViewData adminViewModel = new AdminViewData(this.Cache);

            int disciplineId = appContext.Disciplines.Where(x => x.Enabled).Select(x => x.Id).First();

            if (TempData.ContainsKey("AdminData"))
            {
                //Existing Admin Data (Edit Validation)
                adminViewModel = (AdminViewData)TempData["AdminData"];
            }
            else
            {
                adminViewModel.ListName = Utility.StringValue(Enumerations.ReferenceType.Project);
                adminViewModel.EditReferenceType = (int)Enumerations.ReferenceType.Project;
                adminViewModel.EditingReferenceValueList = appContext.Projects;
                adminViewModel.ReferenceValueEnabled = appContext.Projects.Where(x => x.Enabled).Select(x => x.Name).ToList();
                adminViewModel.ReferenceValueDisabled = appContext.Projects.Where(x => !x.Enabled).Select(x => x.Name).ToList();

                adminViewModel.EditingDisciplineUsersReferenceValue = disciplineId;
                adminViewModel.PrimaryDisciplineUser = appContext.DisciplineUsers.Where(x => x.DisciplineId == disciplineId && x.Primary).Select(x => x.Sid).FirstOrDefault();
                adminViewModel.PrimaryAdminUser = appContext.Admins.Where(x => x.Primary).Select(x => x.Sid).FirstOrDefault();
            }

            adminViewModel.DisciplineUsers = appContext.DisciplineUsers.Where(x => x.DisciplineId == disciplineId).Select(x => x.Sid).ToList();

            return View("Index", adminViewModel);
        }

        [HybridAuthorizeAttribute(Enumerations.Role.Administrator)]
        [HttpPost]
        public IActionResult Save(AdminViewData updatedModel)
        {
            ApplicationContext appContext = new ApplicationContext(this.Cache);

            if (ModelState.IsValid)
            {
                UserSessionContext userContext = new UserSessionContext(this.HttpContext);
                LessonBusiness businessManager = new LessonBusiness(DbContext);

                businessManager.SaveReferenceList(updatedModel.ReferenceValueEnabled, updatedModel.ReferenceValueDisabled, (Enumerations.ReferenceType)updatedModel.EditReferenceType, userContext.CurrentUser);
                businessManager.SaveDisciplineUserList(updatedModel.DisciplineUsers, updatedModel.PrimaryDisciplineUser, updatedModel.EditingDisciplineUsersReferenceValue, userContext.CurrentUser);
                businessManager.SavePrimaryAdminUser(updatedModel.PrimaryAdminUser, userContext.CurrentUser);

                //Update the cache
                appContext.AllUsers = businessManager.GetAllUsers();
                appContext.AllReferenceValues = businessManager.GetAllReferenceValues();

                this.SetSuccessfulSave();
                
                return RedirectToActionPermanent("Index");
            }

            switch ((Enumerations.ReferenceType)updatedModel.EditReferenceType)
            {
                case Enumerations.ReferenceType.Project:
                    updatedModel.EditingReferenceValueList = appContext.Projects;
                    break;
                case Enumerations.ReferenceType.Phase:
                    updatedModel.EditingReferenceValueList = appContext.Phases;
                    break;
                case Enumerations.ReferenceType.Classification:
                    updatedModel.EditingReferenceValueList = appContext.Classifications;
                    break;
                case Enumerations.ReferenceType.Location:
                    updatedModel.EditingReferenceValueList = appContext.Locations;
                    break;
                case Enumerations.ReferenceType.ImpactBenefitRange:
                    updatedModel.EditingReferenceValueList = appContext.ImpactBenefitRanges;
                    break;
                case Enumerations.ReferenceType.CostImpact:
                    updatedModel.EditingReferenceValueList = appContext.CostImpacts;
                    break;
                case Enumerations.ReferenceType.RiskRanking:
                    updatedModel.EditingReferenceValueList = appContext.RiskRankings;
                    break;
                case Enumerations.ReferenceType.Discipline:
                    updatedModel.EditingReferenceValueList = appContext.Disciplines;
                    break;
                case Enumerations.ReferenceType.CredibilityChecklist:
                    updatedModel.EditingReferenceValueList = appContext.CredibilityChecklists;
                    break;
                case Enumerations.ReferenceType.LessonTypeValid:
                    updatedModel.EditingReferenceValueList = appContext.LessonTypesValid;
                    break;
                case Enumerations.ReferenceType.LessonTypeInvalid:
                    updatedModel.EditingReferenceValueList = appContext.LessonTypesInvalid;
                    break;
                case Enumerations.ReferenceType.Theme:
                    updatedModel.EditingReferenceValueList = appContext.Themes;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TempData["AdminData"] = updatedModel;

            return Index();
        }

        [HybridAuthorizeAttribute(Enumerations.Role.Administrator)]
        public IActionResult EditReferenceValues(Enumerations.ReferenceType referenceType)
        {
            ApplicationContext appContext = new ApplicationContext(this.Cache);

            List<ReferenceValue> referenceList = new List<ReferenceValue>();

            switch (referenceType)
            {
                case Enumerations.ReferenceType.Project:
                    referenceList = appContext.Projects;
                    break;
                case Enumerations.ReferenceType.Phase:
                    referenceList = appContext.Phases;
                    break;
                case Enumerations.ReferenceType.Classification:
                    referenceList = appContext.Classifications;
                    break;
                case Enumerations.ReferenceType.Location:
                    referenceList = appContext.Locations;
                    break;
                case Enumerations.ReferenceType.ImpactBenefitRange:
                    referenceList = appContext.ImpactBenefitRanges;
                    break;
                case Enumerations.ReferenceType.CostImpact:
                    referenceList = appContext.CostImpacts;
                    break;
                case Enumerations.ReferenceType.RiskRanking:
                    referenceList = appContext.RiskRankings;
                    break;
                case Enumerations.ReferenceType.Discipline:
                    referenceList = appContext.Disciplines;
                    break;
                case Enumerations.ReferenceType.CredibilityChecklist:
                    referenceList = appContext.CredibilityChecklists;
                    break;
                case Enumerations.ReferenceType.LessonTypeValid:
                    referenceList = appContext.LessonTypesValid;
                    break;
                case Enumerations.ReferenceType.LessonTypeInvalid:
                    referenceList = appContext.LessonTypesInvalid;
                    break;
                case Enumerations.ReferenceType.Theme:
                    referenceList = appContext.Themes;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("referenceType");
            }

            var model = new AdminViewData(this.Cache)
                {
                    ListName = Utility.StringValue(referenceType),
                    EditReferenceType = (int)referenceType,
                    EditingReferenceValueList = referenceList,
                    ReferenceValueEnabled = referenceList.Where(x => x.Enabled).Select(x => x.Name).ToList(),
                    ReferenceValueDisabled = referenceList.Where(x => !x.Enabled).Select(x => x.Name).ToList()
                };

            return PartialView(model);
        }

        [HybridAuthorizeAttribute(Enumerations.Role.Administrator)]
        public IActionResult EditBpoUsers(int disciplineId)
        {
            ApplicationContext appContext = new ApplicationContext(this.Cache);

            var model = new AdminViewData(this.Cache)
                {
                    EditingDisciplineUsersReferenceValue = disciplineId,
                    DisciplineUsers = appContext.DisciplineUsers.Where(x => x.DisciplineId == disciplineId).Select(x => x.Sid).ToList(),
                    PrimaryDisciplineUser = appContext.DisciplineUsers.Where(x => x.DisciplineId == disciplineId && x.Primary).Select(x => x.Sid).FirstOrDefault()
                };

            return PartialView(model);
        }

        [HybridAuthorizeAttribute(Enumerations.Role.Administrator)]
        [HttpPost]
        public IActionResult SendNotifications(AdminViewData updatedModel)
        {
            ApplicationContext appContext = new ApplicationContext(this.Cache);

            if (ViewData.ModelState["NotificationDays"].Errors.Count == 0)
            {
                UserSessionContext userContext = new UserSessionContext(this.HttpContext);
                LessonBusiness businessManager = new LessonBusiness(DbContext);
                var notificationType = (Enumerations.NotificationEmailType)updatedModel.EmailNotificationType;

                List<Lesson> lessons = businessManager.GetLessonsForNotification(notificationType, updatedModel.NotificationDays);

                if (lessons != null && lessons.Count > 0)
                {
                    List<EmailInfo> emailList = new List<EmailInfo>();

                    foreach (var lesson in lessons)
                    {
                        //If this key exists in the web.config, re-direct all eamils to that address.
                        string overrideEmailAddress = Utility.SafeGetAppConfigSetting<string>("Debug_OverrideEmailAddress", null);

                        EmailTemplateViewData model = new EmailTemplateViewData(LessonViewModel.ToViewModel(this.HttpContext,lesson), notificationType, appContext, overrideEmailAddress);
                        string emailMessageBody = Utils.RenderPartialViewToString(this, "EmailTemplate", model);

                        EmailInfo emailInfo = new EmailInfo
                        {
                            Body = emailMessageBody,
                            MailTo = model.Redirecting ? model.OverrideMailTo : model.MailTo,
                            Subject = model.Subject
                        };

                        emailList.Add(emailInfo);
                    }

                    businessManager.SendEmails(emailList);
                }

                this.SetEmailsSent();

                return RedirectPermanent("Index");
            }

            ModelState.Clear();

            AddError("X Days is invalid");

            return Index();
        }
    }
}
