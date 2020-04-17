using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Common;

namespace Suncor.LessonsLearnedMP.Web.ViewData
{
    public class LessonViewModel : IValidatableObject
    {
        private HttpContext _context;
        private IDistributedCache _cache;

        public LessonViewModel(HttpContext context)
        {
            Status = Enumerations.LessonStatus.Draft;
            Title = Constants.TextDefaults.DefaultLessonTitle;
            CreatedDate = Utility.GetCurrentDateTimeAsMST();
            Enabled = true;
            MigrationRecord = false;
            SessionDate = DateTime.Now;
            IsLessonTypeValidSelected = true;
            CloseIsLessonTypeValidSelected = true;
            ThemeIds = new List<int>();

            this._context = context;
            //this._cache = cache;
        }

        public int Id { get; set; }
        public Enumerations.LessonStatus Status { get; set; }
        public string OwnerSid { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string CausalFactors { get; set; }
        public string SuggestedAction { get; set; }
        public string Benefit { get; set; }
        public string SupportingDocuments { get; set; }
        public string Resolution { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string Coordinator { get; set; }
        public string CoordinatorOwnerSid { get; set; }
        public string TransferBpoComment { get; set; }
        public string ClarificationComment { get; set; }
        public string CloseComment { get; set; }
        public int? CloseLessonTypeValidId { get; set; }
        public int? CloseLessonTypeInvalidId { get; set; }
        public bool CloseIsLessonTypeValidSelected { get; set; }
        public string CloseResolution { get; set; }
        public string ThemeDescription { get; set; }

        public int? ImpactBenefitRangeId { get; set; }
        public int? CostImpactId { get; set; }
        public int? LessonTypeValidId { get; set; }
        public int? LessonTypeInvalidId { get; set; }
        public int? RiskRankingId { get; set; }
        public int? DisciplineId { get; set; }
        public int? TransferBpoDisciplineId { get; set; }
        public int? CredibilityChecklistId { get; set; }
        public int? PhaseId { get; set; }
        public int? ClassificationId { get; set; }
        public int? ProjectId { get; set; }
        public int? LocationId { get; set; }
        public List<int> ThemeIds { get; set; }
        public string AssignToUserId { get; set; }

        [Display(Name = "Estimated Completion")]
        public DateTime? EstimatedCompletion { get; set; }
        [Display(Name = "Session Date")]
        public DateTime? SessionDate { get; set; }

        public string CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }

        public bool? Enabled { get; set; }
        public bool MigrationRecord { get; set; }

        public Enumerations.SaveAction SaveAction { get; set; }
        public Enumerations.PageAction ReturnToAction { get; set; }

        public DateTime? LastSentForClarification { get; set; }
        public DateTime? LastSentToBpo { get; set; }
        public string SessionQuarter { get; set; }
        public int LessonAge { get; set; }
        public int TimesSentToBpo { get; set; }
        public int TimesSentForClarification { get; set; }
        public int BpoTransfers { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public string ValidatedBy { get; set; }
        public DateTime? ClosedDate { get; set; }

        public bool IsLessonTypeValidSelected { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            UserSessionContext userSessionContext = new UserSessionContext(_context);

            #region Input Size Validation

            if (!string.IsNullOrWhiteSpace(Title) && Title.Length > Constants.DatabaseStringLengths.Title)
            {
                yield return new ValidationResult(string.Format("Title cannot exceed {0} characters.", Constants.DatabaseStringLengths.Title), new[] { "Title" });
            }

            if (!string.IsNullOrWhiteSpace(Description) && Description.Length > Constants.DatabaseStringLengths.ExtraLongText)
            {
                yield return new ValidationResult(string.Format("Description cannot exceed {0} characters.", Constants.DatabaseStringLengths.ExtraLongText), new[] { "Description" });
            }

            if (!string.IsNullOrWhiteSpace(CausalFactors) && CausalFactors.Length > Constants.DatabaseStringLengths.ExtraLongText)
            {
                yield return new ValidationResult(string.Format("Causal Factors cannot exceed {0} characters.", Constants.DatabaseStringLengths.ExtraLongText), new[] { "CausalFactors" });
            }

            if (!string.IsNullOrWhiteSpace(SuggestedAction) && SuggestedAction.Length > Constants.DatabaseStringLengths.ExtraLongText)
            {
                yield return new ValidationResult(string.Format("Suggested Action cannot exceed {0} characters.", Constants.DatabaseStringLengths.ExtraLongText), new[] { "SuggestedAction" });
            }

            if (!string.IsNullOrWhiteSpace(Benefit) && Benefit.Length > Constants.DatabaseStringLengths.ExtraLongText)
            {
                yield return new ValidationResult(string.Format("Benefit cannot exceed {0} characters.", Constants.DatabaseStringLengths.ExtraLongText), new[] { "Benefit" });
            }

            if (!string.IsNullOrWhiteSpace(ContactName))
            {
                string firstName = ContactName.ContactFirstNameFromDisplayString();
                string lastName =  ContactName.ContactLastNameFromDisplayString();

                if ((!string.IsNullOrWhiteSpace(firstName) && firstName.Length > Constants.DatabaseStringLengths.ShortText)
                    || (!string.IsNullOrWhiteSpace(lastName) && lastName.Length > Constants.DatabaseStringLengths.ShortText))
                {
                    yield return new ValidationResult(string.Format("Contact First or Last Name cannot exceed {0} characters.", Constants.DatabaseStringLengths.ShortText), new[] { "ContactName" });
                }
            }

            if (!string.IsNullOrWhiteSpace(ContactPhone) && ContactPhone.Length > Constants.DatabaseStringLengths.ShortText)
            {
                yield return new ValidationResult(string.Format("Contact Phone cannot exceed {0} characters.", Constants.DatabaseStringLengths.ShortText), new[] { "ContactPhone" });
            }

            if (!string.IsNullOrWhiteSpace(ContactEmail) && ContactEmail.Length > Constants.DatabaseStringLengths.ShortText)
            {
                yield return new ValidationResult(string.Format("Contact Email cannot exceed {0} characters.", Constants.DatabaseStringLengths.ShortText), new[] { "ContactEmail" });
            }

            if (!string.IsNullOrWhiteSpace(SupportingDocuments) && SupportingDocuments.Length > Constants.DatabaseStringLengths.LongText)
            {
                yield return new ValidationResult(string.Format("Supporting Documents cannot exceed {0} characters.", Constants.DatabaseStringLengths.LongText), new[] { "SupportingDocuments" });
            }

            if (!string.IsNullOrEmpty(ThemeDescription) && ThemeDescription.Length > Constants.DatabaseStringLengths.ExtraLongText)
            {
                yield return new ValidationResult(string.Format("Theme Description cannot exceed {0} characters.", Constants.DatabaseStringLengths.ExtraLongText), new[] { "ThemeDescription" });
            }

            if (!string.IsNullOrWhiteSpace(Resolution) && Resolution.Length > Constants.DatabaseStringLengths.ExtraLongText)
            {
                yield return new ValidationResult(string.Format("Resolution cannot exceed {0} characters.", Constants.DatabaseStringLengths.ExtraLongText), new[] { "Resolution" });
            }

            if (!string.IsNullOrWhiteSpace(ClarificationComment) && ClarificationComment.Length > Constants.DatabaseStringLengths.ExtraLongText)
            {
                yield return new ValidationResult(string.Format("Comment cannot exceed {0} characters.", Constants.DatabaseStringLengths.ExtraLongText), new[] { "ClarificationComment" });
            }

            if (!string.IsNullOrWhiteSpace(CloseComment) && CloseComment.Length > Constants.DatabaseStringLengths.ExtraLongText)
            {
                yield return new ValidationResult(string.Format("Comment cannot exceed {0} characters.", Constants.DatabaseStringLengths.ExtraLongText), new[] { "CloseComment" });
            }

            if (!string.IsNullOrWhiteSpace(TransferBpoComment) && TransferBpoComment.Length > Constants.DatabaseStringLengths.ExtraLongText)
            {
                yield return new ValidationResult(string.Format("Comment cannot exceed {0} characters.", Constants.DatabaseStringLengths.ExtraLongText), new[] { "TransferBpoComment" });
            }

            #endregion

            #region Business Rule Validation

            //Don't validate if this is a draft lesson and we're saving it as draft
            //Also don't validate if deleteing / undeleting
            //Also don't validate for Admin users (on save) so they can fix part of the data without fixing other parts
            if (!(Status == Enumerations.LessonStatus.Draft
                && (SaveAction == Enumerations.SaveAction.SaveChanges || SaveAction == Enumerations.SaveAction.SaveDraft))
                && SaveAction != Enumerations.SaveAction.Delete
                && SaveAction != Enumerations.SaveAction.UnDelete
                && userSessionContext.CurrentUser.RoleId != (int)Enumerations.Role.Administrator)
            {
                ApplicationContext appContext = new ApplicationContext(_cache);

                if (!ProjectId.HasValue)
                {
                    yield return new ValidationResult("Project is required.", new[] { "ProjectId" });
                }
                else if (!appContext.Projects.Where(x => x.Id == ProjectId.Value).First().Enabled)
                {
                    yield return new ValidationResult("The selected Project is no longer available.", new[] { "ProjectId" });
                }

                if (string.IsNullOrWhiteSpace(CoordinatorOwnerSid))
                {
                    yield return new ValidationResult("Lesson Coordinator is required.", new[] { "CoordinatorOwnerSid" });
                }
                else if (CoordinatorOwnerSid != Constants.TextDefaults.LLCListPrimaryAdminLabel && !appContext.Coordinators.Where(x => x.Sid == CoordinatorOwnerSid).Any())
                {
                    yield return new ValidationResult("The selected Lesson Coordinator is no longer available.", new[] { "CoordinatorOwnerSid" });
                }
                else if (appContext.Coordinators.Where(x => x.Sid == CoordinatorOwnerSid).Any() && appContext.Coordinators.Where(x => x.Sid == CoordinatorOwnerSid).First().Enabled == false)
                {
                    yield return new ValidationResult("The selected Lesson Coordinator is no longer available.", new[] { "CoordinatorOwnerSid" });
                }

                if (!PhaseId.HasValue)
                {
                    yield return new ValidationResult("Phase is required.", new[] { "PhaseId" });
                }
                else if (!appContext.Phases.Where(x => x.Id == PhaseId.Value).First().Enabled)
                {
                    yield return new ValidationResult("The selected Phase is no longer available.", new[] { "PhaseId" });
                }

                if (!ClassificationId.HasValue)
                {
                    yield return new ValidationResult("Classification is required.", new[] { "ClassificationId" });
                }
                else if (!appContext.Classifications.Where(x => x.Id == ClassificationId.Value).First().Enabled)
                {
                    yield return new ValidationResult("The selected Classification is no longer available.", new[] { "ClassificationId" });
                }

                if (!SessionDate.HasValue)
                {
                    yield return new ValidationResult("Session Date is requred.", new[] { "SessionDate" });
                }

                if (!LocationId.HasValue)
                {
                    yield return new ValidationResult("Location is required.", new[] { "LocationId" });
                }
                else if (!appContext.Locations.Where(x => x.Id == LocationId.Value).First().Enabled)
                {
                    yield return new ValidationResult("The selected Location is no longer available.", new[] { "LocationId" });
                }

                if (string.IsNullOrWhiteSpace(Title))
                {
                    yield return new ValidationResult("Title is required.", new[] { "Title" });
                }
                else if (Title == Constants.TextDefaults.DefaultLessonTitle)
                {
                    yield return new ValidationResult("Title must be changed.", new[] { "Title" });
                }

                if (string.IsNullOrWhiteSpace(Description))
                {
                    yield return new ValidationResult("Description is required.", new[] { "Description" });
                }

                if (string.IsNullOrWhiteSpace(CausalFactors))
                {
                    yield return new ValidationResult("Causal Factors is required.", new[] { "CausalFactors" });
                }

                if (string.IsNullOrWhiteSpace(SuggestedAction))
                {
                    yield return new ValidationResult("Suggested Action is required.", new[] { "SuggestedAction" });
                }

                if (string.IsNullOrWhiteSpace(Benefit))
                {
                    yield return new ValidationResult("Benefit is required.", new[] { "Benefit" });
                }

                if (!ImpactBenefitRangeId.HasValue)
                {
                    yield return new ValidationResult("Benefit Range is required.", new[] { "ImpactBenefitRangeId" });
                }
                else if (!appContext.ImpactBenefitRanges.Where(x => x.Id == ImpactBenefitRangeId).First().Enabled)
                {
                    yield return new ValidationResult("The selected Benefit Range is no longer available.", new[] { "ImpactBenefitRangeId" });
                }

                if (!CostImpactId.HasValue)
                {
                    yield return new ValidationResult("Cost Impact is required.", new[] { "CostImpactId" });
                }
                else if (!appContext.CostImpacts.Where(x => x.Id == CostImpactId).First().Enabled)
                {
                    yield return new ValidationResult("The selected Cost Impact is no longer available.", new[] { "CostImpactId" });
                }

                if (!RiskRankingId.HasValue)
                {
                    yield return new ValidationResult("Risk Ranking is required.", new[] { "RiskRankingId" });
                }
                else if (!appContext.RiskRankings.Where(x => x.Id == RiskRankingId).First().Enabled)
                {
                    yield return new ValidationResult("The selected Risk Ranking is no longer available.", new[] { "RiskRankingId" });
                }

                if (!CredibilityChecklistId.HasValue)
                {
                    yield return new ValidationResult("Credibility Checklist is required.", new[] { "CredibilityChecklistId" });
                }
                else if (!appContext.CredibilityChecklists.Where(x => x.Id == CredibilityChecklistId).First().Enabled)
                {
                    yield return new ValidationResult("The selected Credibility Checklist is no longer available.", new[] { "CredibilityChecklistId" });
                }

                if (!DisciplineId.HasValue)
                {
                    yield return new ValidationResult("Discipline is required.", new[] { "DisciplineId" });
                }
                else if (!appContext.Disciplines.Where(x => x.Id == DisciplineId.Value).First().Enabled)
                {
                    yield return new ValidationResult("The selected Discipline is no longer available.", new[] { "DisciplineId" });
                }
                else if (SaveAction == Enumerations.SaveAction.BPOToBPO)
                {
                    if (!TransferBpoDisciplineId.HasValue)
                    {
                        yield return new ValidationResult("Discipline is required.", new[] { "TransferBpoDisciplineId" });
                    }
                    else if (!appContext.Disciplines.Where(x => x.Id == TransferBpoDisciplineId.Value).First().Enabled)
                    {
                        yield return new ValidationResult("The selected Discipline is no longer available.", new[] { "TransferBpoDisciplineId" });
                    }
                    else if (TransferBpoDisciplineId.Value == DisciplineId.Value)
                    {
                        yield return new ValidationResult("You cannot transfer to the same disipline.", new[] { "TransferBpoDisciplineId" });
                    }
                }

                if (string.IsNullOrWhiteSpace(ContactName))
                {
                    yield return new ValidationResult("Lesson Contact is required.", new[] { "ContactName" });
                }

                if (string.IsNullOrWhiteSpace(ContactPhone))
                {
                    yield return new ValidationResult("Contact Phone is required.", new[] { "ContactPhone" });
                }

                if (string.IsNullOrWhiteSpace(ContactEmail))
                {
                    yield return new ValidationResult("Contact Email is required.", new[] { "ContactEmail" });
                }
                else
                {
                    bool valid = false;
                    try
                    {
                        new MailAddress(ContactEmail);
                        valid = true;
                    }
                    catch
                    {
                        //Email is invalid
                    }

                    if (!valid)
                    {
                        yield return new ValidationResult("Contact Email is not a valid Email address.", new[] { "ContactEmail" });
                    }
                }

                if (!string.IsNullOrWhiteSpace(SupportingDocuments))
                {
                    Uri throwAway;
                    if (!Uri.TryCreate(SupportingDocuments, UriKind.Absolute, out throwAway))
                    {
                        yield return new ValidationResult("Supporting Documents is not a valid URL.", new[] { "SupportingDocuments" });
                    }
                    else if (!SupportingDocuments.ToLower().Contains("/ecmlivelinkprd/"))
                    {
                        yield return new ValidationResult("Supporting Documents is not a valid Livelink URL.", new[] { "SupportingDocuments" });
                    }
                }

                if (ThemeIds == null || ThemeIds.Count == 0)
                {
                    yield return new ValidationResult("At least 1 Lesson Theme is required.", new[] { "ThemeIds" });
                }
                else if (ThemeIds.Contains(appContext.Themes.Where(x => x.System).FirstOrDefault().Id) && string.IsNullOrEmpty(ThemeDescription))
                {
                    yield return new ValidationResult("Theme Description is required.", new[] { "ThemeDescription" });
                }

                if (SaveAction == Enumerations.SaveAction.BPOToClose)
                {
                    if (string.IsNullOrWhiteSpace(Resolution))
                    {
                        yield return new ValidationResult("Resolution is required.", new[] { "Resolution" });
                    }

                    if (LessonTypeValidId == null && LessonTypeInvalidId == null)
                    {
                        yield return new ValidationResult("Lesson Type is required.", new[] { "LessonTypeValidId" });
                    }
                    else
                    {
                        if (LessonTypeValidId != null)
                        {
                            if (appContext.LessonTypesValid.Where(x => x.Id == LessonTypeValidId.Value && !x.Enabled).Any())
                            {
                                yield return new ValidationResult("The selected Lesson Type is no longer available.", new[] { "LessonTypeValidId" });
                            }
                        }
                        else
                        {
                            if (appContext.LessonTypesInvalid.Where(x => x.Id == LessonTypeInvalidId.Value && !x.Enabled).Any())
                            {
                                yield return new ValidationResult("The selected Lesson Type is no longer available.", new[] { "LessonTypeValidId" });
                            }
                        }
                    }
                }
            }

            #endregion
        }

        public static IEnumerable<LessonViewModel> ToViewModel(HttpContext context, IEnumerable<Lesson> dataModelList)
        {
            var lessons = from a in dataModelList
                          select ToViewModel(context, a);
            return lessons.ToList();
        }

        public static LessonViewModel ToViewModel(HttpContext context, Lesson dataModel)
        {
            if (dataModel != null)
            {
                return new LessonViewModel(context)
                {
                    Benefit = dataModel.Benefit,
                    CausalFactors = dataModel.CasualFactors,
                    ClassificationId = dataModel.ClassificationId,
                    ContactName = Utility.ContactToDisplayString(dataModel.ContactLastName, dataModel.ContactFirstName),
                    ContactEmail = dataModel.ContactEmail,
                    ContactPhone = dataModel.ContactPhone,
                    Coordinator = dataModel.Coordinator,
                    CoordinatorOwnerSid = dataModel.CoordinatorOwnerSid,
                    CostImpactId = dataModel.CostImpactId,
                    CreatedDate = dataModel.CreateDate,
                    CreatedUser = dataModel.CreateUser,
                    CredibilityChecklistId = dataModel.CredibilityChecklistId,
                    Description = dataModel.Description,
                    DisciplineId = dataModel.DisciplineId,
                    TransferBpoDisciplineId = dataModel.DisciplineId,
                    Enabled = dataModel.Enabled,
                    EstimatedCompletion = dataModel.EstimatedCompletion,
                    Id = dataModel.Id,
                    ImpactBenefitRangeId = dataModel.ImpactBenefitRangeId,
                    LessonTypeValidId = dataModel.LessonTypeValidId,
                    LessonTypeInvalidId = dataModel.LessonTypeInvalidId,
                    CloseLessonTypeValidId = dataModel.LessonTypeValidId,
                    CloseLessonTypeInvalidId = dataModel.LessonTypeInvalidId,
                    LocationId = dataModel.LocationId,
                    MigrationRecord = dataModel.MigrationRecord,
                    PhaseId = dataModel.PhaseId,
                    ProjectId = dataModel.ProjectId,
                    Resolution = dataModel.Resolution,
                    CloseResolution = dataModel.Resolution,
                    RiskRankingId = dataModel.RiskRankingId,
                    SessionDate = dataModel.SessionDate,
                    Status = (Enumerations.LessonStatus)dataModel.StatusId,
                    SuggestedAction = dataModel.SuggestedAction,
                    SupportingDocuments = dataModel.SupportingDocuments,
                    Title = dataModel.Title,
                    UpdateDate = dataModel.UpdateDate,
                    UpdateUser = dataModel.UpdateUser,
                    ValidatedBy = dataModel.ValidatedBy,
                    LastSentForClarification = dataModel.LastSentForClarification,
                    LastSentToBpo = dataModel.LastSentToBpo,
                    SessionQuarter = dataModel.SessionQuarter.HasValue ? "Q" + dataModel.SessionQuarter.ToString() : "",
                    LessonAge = dataModel.LessonAge,
                    TimesSentToBpo = dataModel.TimesSentToBpo,
                    TimesSentForClarification = dataModel.TimesSentForClarification,
                    BpoTransfers = dataModel.BpoTransfers,
                    OwnerSid = dataModel.OwnerSid,
                    IsLessonTypeValidSelected = dataModel.LessonTypeValidId.HasValue || (dataModel.LessonTypeInvalidId == null && dataModel.LessonTypeValidId == null),
                    CloseIsLessonTypeValidSelected = dataModel.LessonTypeValidId.HasValue || (dataModel.LessonTypeInvalidId == null && dataModel.LessonTypeValidId == null),
                    ThemeIds = dataModel.LessonThemes.Select(x => x.ThemeId).ToList(),
                    ThemeDescription = dataModel.ThemeDescription,
                    SubmittedBy = dataModel.SubmittedBy,
                    SubmittedDate = dataModel.SubmittedDate,
                    ClosedDate = dataModel.ClosedDate,
                    AssignToUserId = dataModel.AssignTo
                };
            }

            return new LessonViewModel(context);
        }

        public Lesson ToDataModel()
        {
            var themes = new List<LessonTheme>();
            foreach (var themeId in ThemeIds)
            {
                themes.Add(new LessonTheme { ThemeId = themeId });
            }

            return new Lesson
            {
                Benefit = Benefit,
                CasualFactors = CausalFactors,
                ClassificationId = ClassificationId,
                ContactEmail = ContactEmail,
                ContactFirstName = ContactName.ContactFirstNameFromDisplayString(),
                ContactLastName = ContactName.ContactLastNameFromDisplayString(),
                ContactPhone = ContactPhone,
                Coordinator = Coordinator,
                CoordinatorOwnerSid = CoordinatorOwnerSid,
                CostImpactId = CostImpactId,
                CreateDate = CreatedDate,
                CreateUser = CreatedUser,
                CredibilityChecklistId = CredibilityChecklistId,
                Description = Description,
                DisciplineId = DisciplineId,
                Enabled = Enabled,
                EstimatedCompletion = EstimatedCompletion,
                Id = Id,
                ImpactBenefitRangeId = ImpactBenefitRangeId,
                LessonTypeValidId = LessonTypeValidId,
                LessonTypeInvalidId = LessonTypeInvalidId,
                LocationId = LocationId,
                MigrationRecord = MigrationRecord,
                PhaseId = PhaseId,
                ProjectId = ProjectId,
                Resolution = Resolution,
                RiskRankingId = RiskRankingId,
                SessionDate = SessionDate,
                StatusId = (int)Status,
                SuggestedAction = SuggestedAction,
                SupportingDocuments = SupportingDocuments,
                Title = Title,
                UpdateDate = UpdateDate,
                UpdateUser = UpdateUser,
                OwnerSid = OwnerSid,
                LessonThemes = themes,
                ThemeDescription = ThemeDescription,
                AssignTo = AssignToUserId
            };
        }
    }
}