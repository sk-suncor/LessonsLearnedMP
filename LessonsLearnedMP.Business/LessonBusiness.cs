using LessonsLearnedMP.Business.v2;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using System;
using System.Collections.Generic;
using System.Data.Objects.Core;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Security.Principal;
using System.Threading.Tasks;
using Comment = Suncor.LessonsLearnedMP.Data.Comment;
using Exception = System.Exception;

namespace Suncor.LessonsLearnedMP.Business
{
    public class LessonBusiness
    {
        private LessonsLearnedMPEntities _context;

        public LessonBusiness(LessonsLearnedMPEntities context)
        {
            _context = context;
        }

        /// <summary>
        /// Admin users have the option to view disabled lessons
        /// </summary>
        public List<Lesson> GetLessonsPaged(
            RoleUser currentUser,
            LessonFilters filters,
            bool includeDisabled,
            bool includeHistory,
            int pageIndex,
            int pageSize,
            out int totalCount)
        {
            return GetLessonsPaged(null, currentUser, filters, includeDisabled, includeHistory, pageIndex, pageSize, out totalCount);
        }

        /// <summary>
        /// Admin users have the option to view disabled lessons
        /// </summary>
        public List<Lesson> GetLessonsPaged(
            RoleUser currentUser,
            LessonFilters filters,
            bool includeDisabled,
            int pageIndex,
            int pageSize,
            out int totalCount)
        {
            return GetLessonsPaged(null, currentUser, filters, includeDisabled, false, pageIndex, pageSize, out totalCount);
        }

        /// <summary>
        /// Admin users have the option to view disabled lessons
        /// </summary>
        public List<Lesson> GetLessonsPaged(
            List<SortColumn> sortColumns,
            RoleUser currentUser,
            LessonFilters filters,
            bool includeDisabled,
            bool includeHistory,
            int pageIndex,
            int pageSize,
            out int totalCount)
        {//TODO:LL RFS Issue 2
            totalCount = 0;
            if (filters == null)
            {
                return null;
            }

            int statusAdminReview = (int)Enumerations.LessonStatus.AdminReview;
            int statusBpoReview = (int)Enumerations.LessonStatus.BPOReview;
            int statusClarification = (int)Enumerations.LessonStatus.Clarification;
            int statusClosed = (int)Enumerations.LessonStatus.Closed;
            int statusDraft = (int)Enumerations.LessonStatus.Draft;
            int statusMigration = (int)Enumerations.LessonStatus.MIGRATION;
            int statusNew = (int)Enumerations.LessonStatus.New;

            var ContextParam = _context.Lessons;
            var query = ContextParam.Where(x => x.Enabled == true);

            if (currentUser.RoleId == (int)Enumerations.Role.Administrator)
            {
                //TODO: For now, include all lessons for Admin users.  No UI currently for selecting filtering disabled
                query = _context.Lessons.AsQueryable();
            }
            else
            {
                //Remove migration lessons from the list
                query = query.Where(x => x.StatusId != statusMigration);
            }

           

            #region new filters-----------------------------------------------------------------------------------------------------

            var currentDate = Utility.GetCurrentDateTimeAsMST();

            if (filters.LessonId.HasValue)
            {
                query = query.Where(x => x.Id == filters.LessonId.Value);
            }
            else
            {
                //Only Include "Closed" lessons when filtering by Closed Date, Response Turnaround, Closed Date, Closed Quarter
                if (filters.ClosedDateBegin.HasValue
                    || filters.ClosedDateEnd.HasValue
                    || filters.ResponseTurnaroundBegin.HasValue
                    || filters.ResponseTurnaroundEnd.HasValue
                    || filters.ClosedDateBegin.HasValue
                    || filters.ClosedDateEnd.HasValue
                    || filters.ClosedQuarter.HasValue)
                {
                    query = query.Where(x => x.StatusId == statusClosed);
                }
                else if (filters.LessonAgeBegin.HasValue || filters.LessonAgeEnd.HasValue)
                {
                    //Only Include "Open" lessons when filtering by age
                    query = query.Where(x =>
                        x.StatusId == statusNew
                        || x.StatusId == statusAdminReview
                        || x.StatusId == statusClarification
                        || x.StatusId == statusBpoReview);
                }

                if (!string.IsNullOrWhiteSpace(filters.OwnerSid))
                {
                    query = query.Where(x => x.OwnerSid == filters.OwnerSid);
                }

                if (!string.IsNullOrWhiteSpace(filters.Keyword))
                {
                    query = query.Where(x =>
                        x.Title.Contains(filters.Keyword) ||
                        x.Description.Contains(filters.Keyword) ||
                        x.CasualFactors.Contains(filters.Keyword) ||
                        x.SuggestedAction.Contains(filters.Keyword));
                }

                if (filters.SelectedStatus != null && filters.SelectedStatus.Count > 0)
                {
                    int[] filterStatusId = filters.SelectedStatus.ToArray();
                    query = query.Where(x => filterStatusId.Contains(x.StatusId));
                }
                else if (filters.Status.HasValue)
                {
                    query = query.Where(x => x.StatusId == filters.Status.Value);
                }

                //only include "Submitted" lessons when searching by Submitted User
                if (filters.SelectedSubmittedUser != null && filters.SelectedSubmittedUser.Count > 0)
                {
                    var temp = _context.LessonHistories.Where(y =>
                            y.NewStatusId == statusNew
                            && filters.SelectedSubmittedUser.Contains(y.CreateUser));

                    query = query.Where(x => x.StatusId != statusDraft &&
                        x.LessonHistories.Where(y =>
                            y.NewStatusId == statusNew
                            && filters.SelectedSubmittedUser.Contains(y.CreateUser))
                            .Any());
                }

                //only include "Submitted" lessons when searching by Submitted Date
                if (filters.SubmittedDateBegin.HasValue || filters.SubmittedDateEnd.HasValue)
                {
                    query = query.Where(x => x.StatusId != statusDraft);

                    if (filters.SubmittedDateBegin.HasValue)
                    {
                        query = from x in query
                                let submittedDate = x.LessonHistories.Where(y =>
                                    y.NewStatusId == statusNew)
                                    .Select(y => y.CreateDate)
                                    .DefaultIfEmpty(DateTime.MinValue)
                                    .FirstOrDefault()
                                where submittedDate != DateTime.MinValue && EntityFunctions.TruncateTime(submittedDate) >= EntityFunctions.TruncateTime(filters.SubmittedDateBegin.Value)
                                select x;
                    }

                    if (filters.SubmittedDateEnd.HasValue)
                    {
                        query = from x in query
                                let submittedDate = x.LessonHistories.Where(y =>
                                    y.NewStatusId == statusNew)
                                    .Select(y => y.CreateDate)
                                    .DefaultIfEmpty(DateTime.MinValue)
                                    .FirstOrDefault()
                                where submittedDate != DateTime.MinValue && EntityFunctions.TruncateTime(submittedDate) <= EntityFunctions.TruncateTime(filters.SubmittedDateEnd.Value)
                                select x;
                    }
                }

                if (filters.SelectedProjectId != null && filters.SelectedProjectId.Count > 0)
                {
                    int[] filterProjectId = filters.SelectedProjectId.ToArray();
                    query = query.Where(x => filterProjectId.Contains(x.ProjectId.Value));
                }
                else if (filters.ProjectId.HasValue)
                {
                    query = query.Where(x => x.ProjectId == filters.ProjectId.Value);
                }

                if (filters.SelectedCoordinator != null && filters.SelectedCoordinator.Count > 0)
                { 
                    query = query.Where(x => filters.SelectedCoordinator.Contains(x.Coordinator));
                }
                else if (!string.IsNullOrWhiteSpace(filters.Coordinator))
                {
                    query = query.Where(x => x.Coordinator == filters.Coordinator);
                }

                if (filters.SelectedPhaseId != null && filters.SelectedPhaseId.Count > 0)
                {
                    int[] filterPhaseId = filters.SelectedPhaseId.ToArray();
                    query = query.Where(x => filterPhaseId.Contains(x.PhaseId.Value));
                }
                else if (filters.PhaseId.HasValue)
                {
                    query = query.Where(x => x.PhaseId == filters.PhaseId.Value);
                }


                if (filters.SelectedClassificationId != null && filters.SelectedClassificationId.Count > 0)
                {
                    int[] filterClassificationId = filters.SelectedClassificationId.ToArray();
                    query = query.Where(x => filterClassificationId.Contains(x.ClassificationId.Value));
                }
                else if (filters.ClassificationId.HasValue)
                {
                    query = query.Where(x => x.ClassificationId == filters.ClassificationId.Value);
                }
  

                if (filters.SessionDateBegin.HasValue)
                {
                    query = query.Where(x => EntityFunctions.TruncateTime(x.SessionDate) >= EntityFunctions.TruncateTime(filters.SessionDateBegin.Value));
                }

                if (filters.SessionDateEnd.HasValue)
                {
                    query = query.Where(x => EntityFunctions.TruncateTime(x.SessionDate) <= EntityFunctions.TruncateTime(filters.SessionDateEnd.Value));
                }

            
                if (filters.SelectedLocationId != null && filters.SelectedLocationId.Count > 0)
                {
                    int[] filterLocationId = filters.SelectedLocationId.ToArray();
                    query = query.Where(x => filterLocationId.Contains(x.LocationId.Value));
                }
                else if (filters.LocationId.HasValue)
                {
                    query = query.Where(x => x.LocationId == filters.LocationId.Value);
                }
              
                if (filters.SelectedImpactBenefitRangeId != null && filters.SelectedImpactBenefitRangeId.Count > 0)
                {
                    int[] filterImpactBenefitRangeId = filters.SelectedImpactBenefitRangeId.ToArray();
                    query = query.Where(x => filterImpactBenefitRangeId.Contains(x.ImpactBenefitRangeId.Value));
                }
                else if (filters.ImpactBenefitRangeId.HasValue)
                {
                    query = query.Where(x => x.ImpactBenefitRangeId == filters.ImpactBenefitRangeId.Value);
                }

                if (filters.SelectedCostImpactId != null && filters.SelectedCostImpactId.Count > 0)
                {
                    int[] filterCostImpactId = filters.SelectedCostImpactId.ToArray();
                    query = query.Where(x => filterCostImpactId.Contains(x.CostImpactId.Value));
                }
                else if (filters.CostImpactId.HasValue)
                {
                    query = query.Where(x => x.CostImpactId == filters.CostImpactId.Value);
                }

                if (filters.SelectedRiskRankingId != null && filters.SelectedRiskRankingId.Count > 0)
                {
                    int[] filterRiskRankingId = filters.SelectedRiskRankingId.ToArray();
                    query = query.Where(x => filterRiskRankingId.Contains(x.RiskRankingId.Value));
                }
                else if (filters.RiskRankingId.HasValue)
                {
                    query = query.Where(x => x.RiskRankingId == filters.RiskRankingId.Value);
                }

                if (filters.SelectedCredibilityChecklistId != null && filters.SelectedCredibilityChecklistId.Count > 0)
                {
                    int[] filterCredibilityChecklistId = filters.SelectedCredibilityChecklistId.ToArray();
                    query = query.Where(x => filterCredibilityChecklistId.Contains(x.CredibilityChecklistId.Value));
                }
                else if (filters.CredibilityChecklistId.HasValue)
                {
                    query = query.Where(x => x.CredibilityChecklistId == filters.CredibilityChecklistId.Value);
                }

                if (filters.SelectedDisciplineId != null && filters.SelectedDisciplineId.Count > 0)
                {
                    int[] filterDisciplineId = filters.SelectedDisciplineId.ToArray();
                    query = query.Where(x => filterDisciplineId.Contains(x.DisciplineId.Value));
                }
                else if (filters.DisciplineId.HasValue)
                {
                    query = query.Where(x => x.DisciplineId == filters.DisciplineId.Value);
                }

                if (filters.SelectedLessonTypeValidId != null && filters.SelectedLessonTypeValidId.Count > 0)
                {
                    int[] filterLessonTypeValidId = filters.SelectedLessonTypeValidId.ToArray();
                    query = query.Where(x => filterLessonTypeValidId.Contains(x.LessonTypeValidId.Value));
                }
                else if (filters.LessonTypeValidId.HasValue)
                {
                    query = query.Where(x => x.LessonTypeValidId == filters.LessonTypeValidId.Value);
                }

                if (filters.SelectedLessonTypeInvalidId != null && filters.SelectedLessonTypeInvalidId.Count > 0)
                {
                    int[] filterLessonTypeInvalidId = filters.SelectedLessonTypeInvalidId.ToArray();
                    query = query.Where(x => filterLessonTypeInvalidId.Contains(x.LessonTypeInvalidId.Value));
                }
                else if (filters.LessonTypeInvalidId.HasValue)
                {
                    query = query.Where(x => x.LessonTypeInvalidId == filters.LessonTypeInvalidId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filters.Title))
                {
                    query = query.Where(x => x.Title.Contains(filters.Title));
                }

                if (!string.IsNullOrWhiteSpace(filters.Description))
                {
                    query = query.Where(x => x.Description.Contains(filters.Description));
                }

                if (!string.IsNullOrWhiteSpace(filters.CausalFactors))
                {
                    query = query.Where(x => x.CasualFactors.Contains(filters.CausalFactors));
                }

                if (!string.IsNullOrWhiteSpace(filters.SuggestedAction))
                {
                    query = query.Where(x => x.SuggestedAction.Contains(filters.SuggestedAction));
                }

                if (filters.SelectedThemeId != null && filters.SelectedThemeId.Count > 0)
                {
                    int[] filterThemeId = filters.SelectedThemeId.ToArray();
                    query = query.Where(x => x.LessonThemes.Where(y => filterThemeId.Contains(y.ThemeId)).Any());
                }
                else if (filters.ThemeId.HasValue)
                {
                    query = query.Where(x => x.LessonThemes.Where(y => y.ThemeId == filters.ThemeId.Value).Any());
                }

                if (!string.IsNullOrWhiteSpace(filters.ThemeDescription))
                {
                    query = query.Where(x => x.ThemeDescription.Contains(filters.ThemeDescription));
                }

                //Verify Lesson Age is calculating correctly
                //var bob = query.Select(x => new
                //{
                //    x.Id,
                //    x.CreateDate,
                //    ClosedDate = x.LessonHistories.Where(y =>
                //        //Date record was closed
                //            y.NewStatusId == statusClosed)
                //            .Select(y => y.CreateDate)
                //            .DefaultIfEmpty(currentDate)
                //            .Max(),
                //    SubmitDate = x.LessonHistories.Where(y =>
                //        //First (only) Date the record hits "New" status
                //            y.NewStatusId == statusNew)
                //            .Select(y => y.CreateDate)
                //            .DefaultIfEmpty(x.CreateDate)
                //            .FirstOrDefault(),
                //LessonAge = EntityFunctions.DiffDays(
                //    x.LessonHistories.Where(y =>
                //        //First (only) Date the record hits "New" status
                //        y.NewStatusId == statusNew)
                //        .Select(y => y.CreateDate)
                //        .DefaultIfEmpty(x.CreateDate)
                //        .FirstOrDefault(),
                //    x.LessonHistories.Where(y =>
                //        //Date record was closed
                //        y.NewStatusId == statusClosed)
                //        .Select(y => y.CreateDate)
                //        .DefaultIfEmpty(currentDate)
                //        .Max()
                //        )
                //}).ToList();

                if (filters.LessonAgeBegin.HasValue)
                {
                    //Business Rule: Lesson Age = Number of days between submitted date and closed date

                    query = query.Where(x => EntityFunctions.DiffDays(
                        x.LessonHistories.Where(y =>
                            //First (only) Date the record hits "New" status
                            y.NewStatusId == statusNew)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(x.CreateDate)
                            .FirstOrDefault(),
                        x.LessonHistories.Where(y =>
                            //Date record was closed
                            y.NewStatusId == statusClosed)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(currentDate)
                            .Max()
                            ) >= filters.LessonAgeBegin.Value);
                }

                if (filters.LessonAgeEnd.HasValue)
                {
                    //Business Rule: Lesson Age = Number of days between submitted date and closed date
                    query = query.Where(x => EntityFunctions.DiffDays(
                        x.LessonHistories.Where(y =>
                            //First (only) Date the record hits "New" status
                            y.NewStatusId == statusNew)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(x.CreateDate)
                            .FirstOrDefault(),
                        x.LessonHistories.Where(y =>
                            //Date record was closed
                            y.NewStatusId == statusClosed)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(currentDate)
                            .Max()
                       ) <= filters.LessonAgeEnd.Value);
                }

                if (filters.ResponseTurnaroundBegin.HasValue)
                {
                    //Business Rule: Response Turnaround = Number of days from the LAST time record is in "BPO Review" to it's closed date
                    query = query.Where(x => EntityFunctions.DiffDays(
                        x.LessonHistories.Where(y =>
                            //Last Date the record hit BPO Review
                            y.NewStatusId == statusBpoReview)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max(),
                        x.LessonHistories.Where(y =>
                            //Date record was closed
                            y.NewStatusId == statusClosed)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max()) >= filters.ResponseTurnaroundBegin.Value);
                }

                if (filters.ResponseTurnaroundEnd.HasValue)
                {
                    //Business Rule: Response Turnaround = Number of days from the LAST time record is in "BPO Review" to it's closed date
                    query = query.Where(x => EntityFunctions.DiffDays(
                        x.LessonHistories.Where(y =>
                            //Last Date the record hit BPO Review
                            y.NewStatusId == statusBpoReview)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max(),
                        x.LessonHistories.Where(y =>
                            //Date record was closed
                            y.NewStatusId == statusClosed)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max()) <= filters.ResponseTurnaroundEnd.Value);
                }

                if (filters.LastClarificationDateBegin.HasValue)
                {
                    query = query.Where(x => EntityFunctions.TruncateTime(
                        x.LessonHistories.Where(y =>
                            y.NewStatusId == statusClarification)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max()) >= EntityFunctions.TruncateTime(filters.LastClarificationDateBegin.Value));
                }

                if (filters.LastClarificationDateEnd.HasValue)
                {
                    query = query.Where(x => EntityFunctions.TruncateTime(
                        x.LessonHistories.Where(y =>
                            y.NewStatusId == statusClarification)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max()) <= EntityFunctions.TruncateTime(filters.LastClarificationDateEnd.Value));
                }

                if (filters.TimesSentForClarificationBegin.HasValue)
                {
                    query = query.Where(x =>
                        x.LessonHistories.Where(y =>
                            y.NewStatusId == statusClarification)
                            .Count() >= filters.TimesSentForClarificationBegin.Value);
                }

                if (filters.TimesSentForClarificationEnd.HasValue)
                {
                    query = query.Where(x =>
                        x.LessonHistories.Where(y =>
                            y.NewStatusId == statusClarification)
                            .Count() <= filters.TimesSentForClarificationEnd.Value);
                }

                if (filters.LastSentToBpoDateBegin.HasValue)
                {
                    query = query.Where(x => EntityFunctions.TruncateTime(
                        x.LessonHistories.Where(y =>
                            y.NewStatusId == statusBpoReview)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max()) >= EntityFunctions.TruncateTime(filters.LastSentToBpoDateBegin.Value));
                }

                if (filters.LastSentToBpoDateEnd.HasValue)
                {
                    query = query.Where(x => EntityFunctions.TruncateTime(
                        x.LessonHistories.Where(y =>
                            y.NewStatusId == statusBpoReview)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max()) <= EntityFunctions.TruncateTime(filters.LastSentToBpoDateEnd.Value));
                }

                if (filters.TimesSentToBpoBegin.HasValue)
                {
                    query = query.Where(x =>
                        x.LessonHistories.Where(y =>
                            y.NewStatusId == statusBpoReview)
                            .Count() >= filters.TimesSentToBpoBegin.Value);
                }

                if (filters.TimesSentToBpoEnd.HasValue)
                {
                    query = query.Where(x =>
                        x.LessonHistories.Where(y =>
                            y.NewStatusId == statusBpoReview)
                            .Count() <= filters.TimesSentToBpoEnd.Value);
                }

                if (filters.TimesBpoTransferedBegin.HasValue)
                {
                    query = query.Where(x =>
                        x.LessonHistories.Where(y =>
                            y.PreviousStatusId == statusBpoReview)
                            .Count() >= filters.TimesBpoTransferedBegin.Value);
                }

                if (filters.TimesBpoTransferedEnd.HasValue)
                {
                    query = query.Where(x =>
                        x.LessonHistories.Where(y =>
                            y.PreviousStatusId == statusBpoReview)
                            .Count() <= filters.TimesBpoTransferedEnd.Value);
                }

                if (filters.ClosedDateBegin.HasValue)
                {
                    query = query.Where(x => EntityFunctions.TruncateTime(
                        x.LessonHistories.Where(y =>
                            y.NewStatusId == statusClosed)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max()) >= EntityFunctions.TruncateTime(filters.ClosedDateBegin));
                }

                if (filters.ClosedDateEnd.HasValue)
                {
                    query = query.Where(x => EntityFunctions.TruncateTime(
                        x.LessonHistories.Where(y =>
                            y.NewStatusId == statusClosed)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max()) <= EntityFunctions.TruncateTime(filters.ClosedDateEnd));
                }

                if (filters.SelectedClosedQuarter != null && filters.SelectedClosedQuarter.Count > 0)
                {
                    query = query.Where(x => filters.SelectedClosedQuarter.Contains(
                        (x.LessonHistories.Where(y =>
                            y.NewStatusId == statusClosed)
                            .Select(y => y.CreateDate)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max().Month - 1) / 3 + 1));
                }

                if (filters.ShowOnlyOwnedLessons)
                {
                    switch ((Enumerations.Role)currentUser.RoleId)
                    {
                        case Enumerations.Role.User:
                            query = query.Where(x => x.OwnerSid == currentUser.Sid);
                            break;
                        case Enumerations.Role.Coordinator:
                            query = query.Where(x => x.OwnerSid == currentUser.Sid || x.CoordinatorOwnerSid == currentUser.Sid);
                            break;
                        case Enumerations.Role.BPO:
                            query = query.Where(x => x.OwnerSid == currentUser.Sid || (currentUser.DisciplineId != null && x.DisciplineId == currentUser.DisciplineId));
                            break;
                        case Enumerations.Role.Administrator:
                            //No action, users can see everything
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                //NOTE: This logic should mirror LessonsLearnedMP.Web/Common/Utils.cs IsLessonEditable()
                else if (filters.ShowEditableOnly)
                {
                    query = query.Where(x => x.Enabled == true);

                    switch ((Enumerations.Role)currentUser.RoleId)
                    {
                        case Enumerations.Role.User:
                            //Owner can edit their own drafts
                            query = query.Where(x => x.StatusId == statusDraft && x.OwnerSid == currentUser.Sid);
                            break;
                        case Enumerations.Role.Coordinator:
                            //Coordinators can edit Clarifications that they own or are assigned to
                            //DS:Issue 79 - This is incorrect.  Removed this rule. |  Coordinators can edit BPO Review that they are assigned to
                            //Owner can edit their own drafts
                            query = query.Where(x => (x.StatusId == statusClarification && (x.OwnerSid == currentUser.Sid || x.CoordinatorOwnerSid == currentUser.Sid))
                                //|| (x.StatusId == statusBpoReview && x.CoordinatorOwnerSid == currentUser.Sid)
                                || (x.StatusId == statusDraft && x.OwnerSid == currentUser.Sid));
                            break;
                        case Enumerations.Role.BPO:
                            //BPO user can edit Clarifications they own, and lessons assigned to their discipline
                            //Owner can edit their own drafts
                            query = query.Where(x => (x.StatusId == statusBpoReview && currentUser.DisciplineId != null && x.DisciplineId == currentUser.DisciplineId)
                                || (x.StatusId == statusDraft && x.OwnerSid == currentUser.Sid));
                            break;
                        case Enumerations.Role.Administrator:
                            //Admin can edit anything
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            //Remove lessons from the list the user has no business seeing
            //NOTE: This logic should mirror LessonsLearnedMP.Web/Common/Utils.cs IsLessonVisible()
            switch ((Enumerations.Role)currentUser.RoleId)
            {
                case Enumerations.Role.User:
                    //User can see Draft Lessons they own
                    //User can see New Lessons they own
                    //User can see Admin Review Lessons they own
                    //User can see Clarification Lessons they own
                    //User can see BPO Review Lessons they own
                    //User can see Closed Lessons (Valid only)
                    query = query.Where(x =>
                        (x.StatusId == statusClosed && x.LessonTypeInvalidId == null)
                        || (x.StatusId != statusClosed && x.OwnerSid == currentUser.Sid)
                    );
                    break;
                case Enumerations.Role.Coordinator:
                    //User can see Draft Lessons they own
                    //User can see New Lessons they own
                    //User can see Admin Review Lessons they own
                    //User can see Clarification Lessons they own
                    //User can see Clarification Lessons they are assigned to
                    //User can see BPO Review Lessons they own
                    //User can see BPO Review Lessons they are assigned to
                    //User can see Closed Lessons (Valid only)
                    query = query.Where(x =>
                        (x.StatusId == statusClosed && x.LessonTypeInvalidId == null)
                        || (x.StatusId != statusClosed && x.OwnerSid == currentUser.Sid)
                        || (x.StatusId == statusClarification && x.CoordinatorOwnerSid == currentUser.Sid)
                        || (x.StatusId == statusBpoReview && x.CoordinatorOwnerSid == currentUser.Sid)
                    );
                    break;
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
                    query = query.Where(x =>
                        (x.StatusId == statusClosed && x.LessonTypeInvalidId == null)
                        || (x.StatusId != statusClosed && x.OwnerSid == currentUser.Sid)
                        //|| (currentUser.DisciplineId != null && x.DisciplineId == currentUser.DisciplineId)      
                        || (currentUser.DisciplineId != null && x.DisciplineId == currentUser.DisciplineId && x.StatusId != statusNew && x.StatusId != statusDraft && x.StatusId != statusAdminReview)         
                    );
                    break;
                case Enumerations.Role.Administrator:
                    //No action, users can see everything
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
    
            #endregion

            #region Sort / Order by

            if (sortColumns == null)
            {
                //Default Sort
                sortColumns = new List<SortColumn> { new SortColumn { Column = Enumerations.LessonListSortColumn.Number, Direction = Enumerations.SortDirection.Descending, SortOrder = 1 } };
            }

            sortColumns = sortColumns.OrderBy(x => x.SortOrder).ToList();

            foreach (var sort in sortColumns)
            {
                switch (sort.Column)
                {
                    case Enumerations.LessonListSortColumn.Number:
                        query = ApplySorting(query, x => x.Id, sort.Direction);
                        break;
                    case Enumerations.LessonListSortColumn.Status:
                        query = ApplySorting(query, x => x.Status.Name, sort.Direction);
                        break;
                    case Enumerations.LessonListSortColumn.Title:
                        query = ApplySorting(query, x => x.Title, sort.Direction);
                        break;
                    case Enumerations.LessonListSortColumn.Discipline:
                        query = ApplySorting(query, x => x.Discipline.Name, sort.Direction);
                        break;
                    case Enumerations.LessonListSortColumn.SubmitDate:
                        query = ApplySorting(query, x =>
                            x.LessonHistories.Where(y => y.NewStatusId == statusNew).Select(y => x.CreateDate).FirstOrDefault(),
                            sort.Direction);
                        break;
                    case Enumerations.LessonListSortColumn.Contact:
                        query = ApplySorting(query, x => x.ContactLastName, sort.Direction);
                        query = ApplySorting(query, x => x.ContactFirstName, sort.Direction);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            #endregion

            totalCount = query.Count();

            if (pageSize == 0)
            {
                pageSize = totalCount;
            }

            query = query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Include(x => x.Classification)
             .Include(x => x.CostImpact)
             .Include(x => x.CredibilityChecklist)
             .Include(x => x.Discipline)
             .Include(x => x.ImpactBenefitRange)
             .Include(x => x.LessonTypeValid)
             .Include(x => x.LessonTypeInvalid)
             .Include(x => x.Location)
             .Include(x => x.Phase)
             .Include(x => x.Project)
             .Include(x => x.RiskRanking)
             .Include(x => x.Status)
               .Include(x => x.Comments)
             .Include(x => x.LessonThemes)
             .Include(x => x.LessonThemes).ThenInclude(x => x.Theme);
             //.Include(x => x.LessonThemes.Select(y => y.Theme));

            if (includeHistory)
            {
                query = query.Include(x => x.LessonHistories);
            }

            var result = query.ToList();
            return result;
        }

        private static IQueryable<T> ApplySorting<T, TU>(IQueryable<T> query, Expression<Func<T, TU>> predicate, Enumerations.SortDirection direction)
        {
            if (direction == Enumerations.SortDirection.Ascending)
            {
                return query.OrderBy(x => 1).ThenBy<T, TU>(predicate);
            }

            return query.OrderBy(x => 1).ThenByDescending<T, TU>(predicate);
        }

        public List<Lesson> GetLessons(List<int> selectedLessonIds, RoleUser currentUser)
        {
            List<Lesson> result = new List<Lesson>();

            foreach (var lessonId in selectedLessonIds)
            {
                int unused = 0;
                //Use GetLessonsPaged as it contains all the filtering business logic for user privilege
                result.AddRange(GetLessonsPaged(currentUser, new LessonFilters { LessonId = lessonId }, false, 0, 0, out unused));
            }

            return result;
        }

        /// <summary>
        /// Admin users have the option to view disabled lessons
        /// </summary>
        public List<Comment> GetLessonComments(
            int lessonId,
            RoleUser currentUser,
            bool includeDisabled,
            out int totalCount)
        {
            return GetLessonComments(lessonId, Enumerations.LessonCommentListSortColumn.None, Enumerations.SortDirection.Descending, Enumerations.LessonCommentListFilterColumn.None, currentUser, includeDisabled, out totalCount);
        }

        /// <summary>
        /// Admin users have the option to view disabled lessons
        /// </summary>
        public List<Comment> GetLessonComments(
            int lessonId,
            Enumerations.LessonCommentListSortColumn orderByColumn,
            Enumerations.SortDirection orderByDirection,
            Enumerations.LessonCommentListFilterColumn filterColumn,
            RoleUser currentUser,
            bool includeDisabled,
            out int totalCount)
        {
            var query = _context.Comments.Where(x => x.LessonId == lessonId);

            if (!includeDisabled || currentUser.RoleId != (int)Enumerations.Role.Administrator)
            {
                query = query.Where(x => x.Enabled == true);
            }

            //Order by clauses
            if (orderByDirection == Enumerations.SortDirection.Ascending)
            {
                switch (orderByColumn)
                {
                    default:
                        query = query.OrderBy(x => x.Id);
                        break;
                }
            }
            else
            {
                switch (orderByColumn)
                {
                    default:
                        query = query.OrderByDescending(x => x.Id);
                        break;
                }
            }

            totalCount = query.Count();

            var result = query.Include(x => x.CommentType).ToList();
            return result;
        }

        public Lesson GetLessonById(int id)
        {
            var query = _context.Lessons.Where(x => x.Id == id);

            query = query
                .Include(x => x.Classification)
                .Include(x => x.CostImpact)
                .Include(x => x.CredibilityChecklist)
                .Include(x => x.Discipline)
                .Include(x => x.ImpactBenefitRange)
                .Include(x => x.LessonTypeValid)
                .Include(x => x.LessonTypeInvalid)
                .Include(x => x.Location)
                .Include(x => x.Phase)
                .Include(x => x.Project)
                .Include(x => x.RiskRanking)
                .Include(x => x.Status)
                .Include(x => x.LessonHistories)
                .Include(x => x.LessonThemes);

            return query.FirstOrDefault();
        }

        public List<Lesson> GetLessonByIdList(List<int> ids)
        {
            var query = _context.Lessons.Where(x => ids.Contains(x.Id));

            query = query
                .Include(x => x.Classification)
                .Include(x => x.CostImpact)
                .Include(x => x.CredibilityChecklist)
                .Include(x => x.Discipline)
                .Include(x => x.ImpactBenefitRange)
                .Include(x => x.LessonTypeValid)
                .Include(x => x.LessonTypeInvalid)
                .Include(x => x.Location)
                .Include(x => x.Phase)
                .Include(x => x.Project)
                .Include(x => x.RiskRanking)
                .Include(x => x.Status)
                .Include(x => x.LessonHistories)
                .Include(x => x.LessonThemes);

            return query.ToList();
        }

        public List<ReferenceValue> GetReferenceValues(Enumerations.ReferenceType referenceType)
        {
            var query = _context.ReferenceValues
                .Where(x => x.ReferenceTypeId == (int)referenceType || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System)
                .OrderBy(x => x.SortOrder);

            return query.ToList();
        }

        public List<ReferenceValue> GetAllReferenceValues()
        {
            var query = _context.ReferenceValues
                .OrderBy(x => x.SortOrder);

            return query.ToList();
        }

        public List<ReferenceType> GetReferenceTypes()
        {
            var query = _context.ReferenceTypes
                .OrderBy(x => x.Name);

            return query.ToList();
        }

        public List<RoleUser> GetSubmittedByUsers()
        {
            int statusNew = (int)Enumerations.LessonStatus.New;

            //3 styles of created user
            //Steinke,Devin
            //Devin Steinke
            //MIGRATION
            var querynewuser = _context.LessonHistories.Where(x => x.NewStatusId == statusNew).Select(g=>g.CreateUser).ToList();
            List<RoleUser> submittedUsers = querynewuser
                .GroupBy(u=>u)
                .Select(g => g.FirstOrDefault())
                .ToList()
                .Select(x => new RoleUser
            {
                FirstName = x
            }).ToList();

            return submittedUsers.OrderBy(x => x.FirstName).ToList();
        }

        public List<RoleUser> GetAllUsers()
        {
            var query = _context.RoleUsers.AsQueryable();
            return query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
        }

        public List<RoleUser> GetUsersByRole(Enumerations.Role role)
        {
            int roleId = (int)role;
            var activeUsers = _context.RoleUsers.Where(x => x.RoleId == roleId);
            return activeUsers.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
        }

        public RoleUser GetCurrentUser()
        {
            Logger.Debug("GetCurrentUser()", "Begin");
            var identity = WindowsIdentity.GetCurrent();
            RoleUser user = null;

            Logger.Debug("GetCurrentUser()", "WindowsIdentity.GetCurrent();  -- identity is null?" + (identity == null).ToString());

            if (identity != null && identity.User != null)
            {
                Logger.Debug("GetCurrentUser()", "identity: " + identity.Name + ", Authenticated? " + identity.IsAuthenticated.ToString());
                user = _context.RoleUsers.Where(x => x.Sid == identity.User.Value).FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(Utility.SafeGetAppConfigSetting("Debug_OverrideSid", "")))
            {
                var sid = Utility.SafeGetAppConfigSetting("Debug_OverrideSid", "");
                user = _context.RoleUsers.Where(x => x.Sid == sid).FirstOrDefault();
            }

            if (user == null)
            {
                Logger.Error("GetCurrentUser()", "WindowsIdentity.GetCurrent(); returned null");
            }

            return user;
        }

        public Lesson SaveLesson(Lesson updatedModel, RoleUser currentUser, string comment, Enumerations.SaveAction saveAction)
        {
            updatedModel = updatedModel.Clone();
            DateTime updateDate = Utility.GetCurrentDateTimeAsMST();
            string updateUser = Utility.FormatUserNameForDisplay(currentUser.LastName, currentUser.FirstName, currentUser.Email, true, false);

            if (updatedModel == null)
            {
                throw new ArgumentNullException();
            }

            if (saveAction == Enumerations.SaveAction.Delete)
            {
                return DeleteLesson(updatedModel.Id, currentUser);
            }

            if (saveAction == Enumerations.SaveAction.UnDelete)
            {
                return UnDeleteLesson(updatedModel.Id, currentUser);
            }

            if (saveAction == Enumerations.SaveAction.AssignToUser)
            {
                comment = currentUser.Name + " assigned lesson to " + updatedModel.AssignTo;
            }

            if (updatedModel.Id > 0)
            {
                //Updating existing

                var currentModel = _context.Lessons.Where(x => x.Id == updatedModel.Id)
                    .Include(x => x.Classification)
                    .Include(x => x.CostImpact)
                    .Include(x => x.CredibilityChecklist)
                    .Include(x => x.Discipline)
                    .Include(x => x.ImpactBenefitRange)
                    .Include(x => x.LessonTypeInvalid)
                    .Include(x => x.LessonTypeValid)
                    .Include(x => x.Location)
                    .Include(x => x.Phase)
                    .Include(x => x.Project)
                    .Include(x => x.RiskRanking)
                    .Include(x => x.Status)
                    .Include(x => x.Comments)
                    .Include(x => x.LessonHistories)
                    .Include(x => x.LessonThemes)
                    .First();

                currentModel.Benefit = updatedModel.Benefit;
                currentModel.CasualFactors = updatedModel.CasualFactors;
                currentModel.ClassificationId = updatedModel.ClassificationId;
                currentModel.ContactEmail = updatedModel.ContactEmail;
                currentModel.ContactFirstName = updatedModel.ContactFirstName;
                currentModel.ContactLastName = updatedModel.ContactLastName;
                currentModel.ContactPhone = updatedModel.ContactPhone;
                currentModel.Coordinator = updatedModel.Coordinator;
                currentModel.CoordinatorOwnerSid = updatedModel.CoordinatorOwnerSid;
                currentModel.CostImpactId = updatedModel.CostImpactId;
                currentModel.CredibilityChecklistId = updatedModel.CredibilityChecklistId;
                currentModel.Description = updatedModel.Description;
                currentModel.Enabled = updatedModel.Enabled;
                currentModel.EstimatedCompletion = updatedModel.EstimatedCompletion;
                currentModel.ImpactBenefitRangeId = updatedModel.ImpactBenefitRangeId;

                if (updatedModel.LessonTypeValidId.HasValue)
                {
                    currentModel.LessonTypeValidId = updatedModel.LessonTypeValidId;
                    currentModel.LessonTypeInvalidId = null;
                }
                else
                {
                    currentModel.LessonTypeValidId = null;
                    currentModel.LessonTypeInvalidId = updatedModel.LessonTypeInvalidId;
                }

                currentModel.LocationId = updatedModel.LocationId;
                currentModel.MigrationRecord = updatedModel.MigrationRecord;
                currentModel.PhaseId = updatedModel.PhaseId;
                currentModel.ProjectId = updatedModel.ProjectId;
                currentModel.Resolution = updatedModel.Resolution;
                currentModel.RiskRankingId = updatedModel.RiskRankingId;
                currentModel.SessionDate = updatedModel.SessionDate;
                currentModel.SuggestedAction = updatedModel.SuggestedAction;
                currentModel.SupportingDocuments = updatedModel.SupportingDocuments;
                currentModel.Title = updatedModel.Title;
                currentModel.UpdateDate = updateDate;
                currentModel.UpdateUser = updateUser;
                currentModel.ThemeDescription = updatedModel.ThemeDescription;
                int? previousDiscipline = currentModel.DisciplineId;
                currentModel.DisciplineId = updatedModel.DisciplineId;

                
                if (saveAction != Enumerations.SaveAction.SaveChanges && saveAction != Enumerations.SaveAction.SaveDraft)
                {
                    //Save a new LessonHistory
                    LessonHistory history = new LessonHistory();
                    history.CreateDate = updateDate;
                    history.CreateUser = updateUser;

                    if (saveAction == Enumerations.SaveAction.NewToAdmin && currentModel.StatusId == (int)Enumerations.LessonStatus.MIGRATION)
                    {
                        history.PreviousStatusId = (int)Enumerations.LessonStatus.New;
                    }
                    else
                    {
                        history.PreviousStatusId = currentModel.StatusId;
                    }

                    history.PreviousDisciplineId = previousDiscipline.HasValue ? previousDiscipline.Value : (int)Enumerations.LessonStatus.MIGRATION;
                    history.NewDisciplineId = currentModel.DisciplineId.HasValue ? currentModel.DisciplineId.Value : (int)Enumerations.LessonStatus.MIGRATION;

                    switch (saveAction)
                    {
                        case Enumerations.SaveAction.DraftToNew:
                            currentModel.StatusId = (int)Enumerations.LessonStatus.New;
                            break;
                        case Enumerations.SaveAction.ClarificationToAdmin:
                        case Enumerations.SaveAction.NewToAdmin:
                            currentModel.StatusId = (int)Enumerations.LessonStatus.AdminReview;
                            break;
                        case Enumerations.SaveAction.BPOToClarification:
                        case Enumerations.SaveAction.AdminToClarification:
                            currentModel.StatusId = (int)Enumerations.LessonStatus.Clarification;
                            break;
                        case Enumerations.SaveAction.BPOToBPO:
                        case Enumerations.SaveAction.ClarificationToBPO:
                        case Enumerations.SaveAction.AdminToBPO:
                        case Enumerations.SaveAction.ClosedToBPO:
                        case Enumerations.SaveAction.AssignToUser:
                            currentModel.StatusId = (int)Enumerations.LessonStatus.BPOReview;
                            break;
                        case Enumerations.SaveAction.BPOToClose:
                            currentModel.StatusId = (int)Enumerations.LessonStatus.Closed;
                            break;
                      
                        default:
                            throw new ArgumentOutOfRangeException("saveAction");
                    }

                    history.NewStatusId = currentModel.StatusId;

                    currentModel.LessonHistories.Add(history);
                }

                if (!string.IsNullOrEmpty(comment))
                {
                    Enumerations.CommentType commentType;

                    switch (saveAction)
                    {
                        case Enumerations.SaveAction.AdminToClarification:
                        case Enumerations.SaveAction.BPOToClarification:
                            commentType = Enumerations.CommentType.ClarificationRequired;
                            break;
                        case Enumerations.SaveAction.BPOToBPO:
                        case Enumerations.SaveAction.AdminToBPO:
                            commentType = Enumerations.CommentType.TransferToBPO;
                            break;
                        case Enumerations.SaveAction.BPOToClose:
                        case Enumerations.SaveAction.AssignToUser:
                            commentType = Enumerations.CommentType.BPOValidation;
                            break;
                        case Enumerations.SaveAction.ClarificationToBPO:
                        case Enumerations.SaveAction.ClarificationToAdmin:
                            commentType = Enumerations.CommentType.Clarification;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("saveAction");
                    }

                    currentModel.Comments.Add(new Comment
                    {
                        CommentTypeId = (int)commentType,
                        Content = comment,
                        CreateDate = updateDate,
                        CreateUser = updateUser,
                        Enabled = true
                    });
                }

                //Replace the existing Lesson Theme's with the new list
                currentModel.LessonThemes.ToList().ForEach(x => _context.Remove(x));

                foreach (var newTheme in updatedModel.LessonThemes)
                {
                    currentModel.LessonThemes.Add(new LessonTheme { ThemeId = newTheme.ThemeId });
                }
            }
            else
            {
                //Adding New
                updatedModel.CreateDate = updateDate;
                updatedModel.CreateUser = updateUser;
                updatedModel.OwnerSid = currentUser.Sid;
                updatedModel.StatusId = (int)Enumerations.LessonStatus.Draft;

                if (saveAction == Enumerations.SaveAction.DraftToNew)
                {
                    updatedModel.StatusId = (int)Enumerations.LessonStatus.New;

                    //Save a new LessonHistory
                    LessonHistory history = new LessonHistory();
                    history.CreateDate = updateDate;
                    history.CreateUser = updateUser;
                    history.PreviousStatusId = (int)Enumerations.LessonStatus.Draft;
                    history.NewStatusId = (int)Enumerations.LessonStatus.New;
                    history.PreviousDisciplineId = (int)updatedModel.DisciplineId;
                    history.NewDisciplineId = (int)updatedModel.DisciplineId;

                    updatedModel.LessonHistories.Add(history);
                }

                _context.Add(updatedModel);
            }

            bool saved = _context.SaveChanges() > 0;

            if (saved)
            {
                return updatedModel;
            }

            return null;
        }

        private static IEnumerable<RoleUser> GetActiveDirectoryUsers(string groupName, bool recursive = false)
        {
            List<RoleUser> people = new List<RoleUser>();

            try
            {
                Logger.Debug(string.Format("GetActiveDirectoryUsers({0})", groupName), "Enter Try");
                Logger.Debug(string.Format("GetActiveDirectoryUsers({0})", groupName), "Domain from web config: " + Utility.SafeGetAppConfigSetting("Domain", "NETWORK"));

                using (PrincipalContext domainContext = new PrincipalContext(
                    ContextType.Domain,
                    Utility.SafeGetAppConfigSetting("Domain", "NETWORK"),
                    Utility.SafeGetAppConfigSetting("ApplicationPool_ServiceAccount_UserName", ""),
                    Utility.SafeGetAppConfigSetting("ApplicationPool_ServiceAccount_Password", "")))
                {
                    Logger.Debug(string.Format("GetActiveDirectoryUsers({0})", groupName), "Enter First Using");

                    using (GroupPrincipal group = GroupPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, groupName))
                    {
                        Logger.Debug(string.Format("GetActiveDirectoryUsers({0})", groupName), "Enter Second Using");

                        if (group != null)
                        {
                            Logger.Debug(string.Format("GetActiveDirectoryUsers({0})", groupName), "Group is not null");

                            var userCount = 0;

                            List<string> distinguishedNameFilters = Utility.GetDistinguishedNameFilters();

                            bool debug = false;

                            #if DEBUG
                                debug = true;
                            #endif

                            //Parallel.ForEach(group.GetMembers(recursive), principal =>
                            foreach (var principal in group.GetMembers(recursive))
                            {
                                Logger.Debug(string.Format("GetActiveDirectoryUsers({0})", groupName), "Inside Loop");

                                UserPrincipal user = null;

                                try
                                {
                                    user = UserPrincipal.FindByIdentity(domainContext, principal.SamAccountName);
                                }
                                catch (System.Exception ex)
                                {
                                    Logger.Error("UserPrincipal.FindByIdentity(domainContext, principal.SamAccountName)", "SamAccountName: " + principal.SamAccountName + ", Error: " + ex.ToString());
                                }

                                if (user != null
                                    && user.Sid != null
                                    && !string.IsNullOrWhiteSpace(user.GivenName)
                                    && !string.IsNullOrWhiteSpace(user.Surname)
                                    && user.Enabled.HasValue
                                    && user.Enabled.Value
                                    && !distinguishedNameFilters.Where(x => user.DistinguishedName.Contains(x)).Any()
                                    )
                                {
                                    userCount++;

                                    Logger.Debug(string.Format("GetActiveDirectoryUsers({0})", groupName), string.Format("User {0} {1} | DN: {2}", user.GivenName, user.Surname, user.DistinguishedName));

                                    Enumerations.Role role = Enumerations.Role.User;

                                    var pricipalGroups = principal.GetGroups().Select(x => x.Name);

                                    if (pricipalGroups.Contains(Utility.SafeGetAppConfigSetting("Group_Admin", "SG-LLRD-MP-ADMIN")))
                                    {
                                        role = Enumerations.Role.Administrator;
                                    }
                                    else if (pricipalGroups.Contains(Utility.SafeGetAppConfigSetting("Group_LLCoordinator", "SG-LLRD-MP-COORDINATOR")))
                                    {
                                        role = Enumerations.Role.Coordinator;
                                    }
                                    else if (pricipalGroups.Contains(Utility.SafeGetAppConfigSetting("Group_BPO", "SG-LLRD-MP-BPO")))
                                    {
                                        role = Enumerations.Role.BPO;
                                    }

                                    people.Add(new RoleUser
                                    {
                                        LastName = user.Surname,
                                        FirstName = user.GivenName,
                                        Email = user.EmailAddress,
                                        Phone = user.VoiceTelephoneNumber,
                                        Enabled = true,
                                        Description = user.Description,
                                        RoleId = (int)role,
                                        Sid = user.Sid.Value
                                    });
                                }
                            }
                            //);

                            Logger.Info(string.Format("GetActiveDirectoryUsers(string groupName={0}, bool recursive={1}", groupName, recursive), string.Format("Found {0} users", userCount));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetActiveDirectoryUsers(" + groupName + ")", ex.ToString());

                if (bool.Parse(Utility.SafeGetAppConfigSetting("Debug_PopulateFakeUsers", "false")))
                {
                    people.Add(new RoleUser
                    {
                        LastName = "User",
                        FirstName = "Debug",
                        Email = "tempuser@suncor.com",
                        Phone = "780-555-1234",
                        Enabled = true,
                        Sid = "S-1-5-21-861617734-1335137780-1834409622-8391"
                    });

                    people.Add(new RoleUser
                    {
                        LastName = "User 2",
                        FirstName = "Debug",
                        Email = "tempuser2@suncor.com",
                        Phone = "780-666-2345",
                        Enabled = true,
                        Sid = "S-1-5-21-861617734-1335137780-1834409622-8392"
                    });

                    people.Add(new RoleUser
                    {
                        LastName = "User 3",
                        FirstName = "Debug",
                        Email = "tempuser3@suncor.com",
                        Phone = "780-777-3456",
                        Enabled = true,
                        Sid = "S-1-5-21-861617734-1335137780-1834409622-8393"
                    });
                }
            }

            return people;
        }

        private static IEnumerable<RoleUser> GetAllActiveDirectoryUsers()
        {
            List<RoleUser> people = new List<RoleUser>();

            try
            {
                using (PrincipalContext domainContext = new PrincipalContext(
                    ContextType.Domain,
                    Utility.SafeGetAppConfigSetting("Domain", "NETWORK"),
                    Utility.SafeGetAppConfigSetting("ApplicationPool_ServiceAccount_UserName", ""),
                    Utility.SafeGetAppConfigSetting("ApplicationPool_ServiceAccount_Password", "")))
                {
                    Dictionary<string, Enumerations.Role> userRoles = new Dictionary<string, Enumerations.Role>();

                    //Get the admin users
                    using (GroupPrincipal group = GroupPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, Utility.SafeGetAppConfigSetting("Group_Admin", "SG-LLRD-MP-ADMIN")))
                    {
                        if (group != null)
                        {
                            group.GetMembers(false).ToList().ForEach(x =>
                                {
                                    if (x.Sid != null && !userRoles.Keys.Contains(x.Sid.Value))
                                    {
                                        userRoles.Add(x.Sid.Value, Enumerations.Role.Administrator);
                                    }
                                });
                        }
                    }

                    //Get the BPO users
                    using (GroupPrincipal group = GroupPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, Utility.SafeGetAppConfigSetting("Group_BPO", "SG-LLRD-MP-BPO")))
                    {
                        if (group != null)
                        {
                            group.GetMembers(false).ToList().ForEach(x =>
                            {
                                if (x.Sid != null && !userRoles.Keys.Contains(x.Sid.Value))
                                {
                                    userRoles.Add(x.Sid.Value, Enumerations.Role.BPO);
                                }
                            });
                        }
                    }

                    //Get the Coordinator users
                    using (GroupPrincipal group = GroupPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, Utility.SafeGetAppConfigSetting("Group_LLCoordinator", "SG-LLRD-MP-COORDINATOR")))
                    {
                        if (group != null)
                        {
                            group.GetMembers(false).ToList().ForEach(x =>
                            {
                                if (x.Sid != null && !userRoles.Keys.Contains(x.Sid.Value))
                                {
                                    userRoles.Add(x.Sid.Value, Enumerations.Role.Coordinator);
                                }
                            });
                        }
                    }

                    using (GroupPrincipal group = GroupPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, "Domain Users"))
                    {
                        if (group != null)
                        {
                            Logger.Debug(string.Format("GetActiveDirectoryUsers({0})", "Domain Users"), "Group is not null");

                            var userCount = 0;

                            List<string> distinguishedNameFilters = Utility.GetDistinguishedNameFilters();

                            Parallel.ForEach(group.GetMembers(true), principal =>
                            //foreach (var principal in group.GetMembers(true))
                            {
                                Logger.Debug("GetAllActiveDirectoryUsers()", "Inside Loop");

                                UserPrincipal user = null;

                                try
                                {
                                    user = UserPrincipal.FindByIdentity(domainContext, IdentityType.Sid, principal.Sid.Value);
                                }
                                catch (System.Exception ex)
                                {
                                    Logger.Error("UserPrincipal.FindByIdentity(domainContext, principal.SamAccountName)", "SamAccountName: " + principal.SamAccountName + ", Error: " + ex.ToString());
                                }

                                if (user != null
                                    && user.Sid != null
                                    && !string.IsNullOrWhiteSpace(user.GivenName)
                                    && !string.IsNullOrWhiteSpace(user.Surname)
                                    && user.Enabled.HasValue
                                    && user.Enabled.Value
                                    && !distinguishedNameFilters.Where(y => user.DistinguishedName.Contains(y)).Any()
                                    )
                                {
                                    userCount++;

                                    Logger.Debug("GetAllActiveDirectoryUsers()", string.Format("User {0} {1} | DN: {2}", user.GivenName, user.Surname, user.DistinguishedName));

                                    Enumerations.Role role = Enumerations.Role.User;

                                    if (userRoles.Keys.Contains(user.Sid.Value))
                                    {
                                        Logger.Debug("GetAllActiveDirectoryUsers()", string.Format("User {0} {1} | Role: {2}", user.GivenName, user.Surname, userRoles[user.Sid.Value]));
                                        role = userRoles[user.Sid.Value];
                                    }

                                    people.Add(new RoleUser
                                    {
                                        LastName = user.Surname,
                                        FirstName = user.GivenName,
                                        Email = user.EmailAddress,
                                        Phone = user.VoiceTelephoneNumber,
                                        Enabled = true,
                                        Description = user.Description,
                                        RoleId = (int)role,
                                        Sid = user.Sid.Value,
                                        DistinguishedName = user.DistinguishedName,
                                        AutoUpdate = true
                                    });
                                }
                            }
                            );

                            Logger.Info("GetAllActiveDirectoryUsers", string.Format("Found {0} users", userCount));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetAllActiveDirectoryUsers", ex.ToString());

                if (bool.Parse(Utility.SafeGetAppConfigSetting("Debug_PopulateFakeUsers", "false")))
                {
                    people.Add(new RoleUser
                    {
                        LastName = "User",
                        FirstName = "Debug",
                        Email = "tempuser@suncor.com",
                        Phone = "780-555-1234",
                        Enabled = true,
                        Sid = "S-1-5-21-861617734-1335137780-1834409622-8391"
                    });

                    people.Add(new RoleUser
                    {
                        LastName = "User 2",
                        FirstName = "Debug",
                        Email = "tempuser2@suncor.com",
                        Phone = "780-666-2345",
                        Enabled = true,
                        Sid = "S-1-5-21-861617734-1335137780-1834409622-8392"
                    });

                    people.Add(new RoleUser
                    {
                        LastName = "User 3",
                        FirstName = "Debug",
                        Email = "tempuser3@suncor.com",
                        Phone = "780-777-3456",
                        Enabled = true,
                        Sid = "S-1-5-21-861617734-1335137780-1834409622-8393"
                    });
                }
            }

            return people;
        }

        public Lesson DeleteLesson(int id, RoleUser currentUser)
        {
            Lesson lesson = GetLessonById(id);

            // Validate user can delete
            bool canDelete = false;

            if (currentUser.RoleId == (int)Enumerations.Role.Administrator)
            {
                canDelete = true;
            }

            if (currentUser.Sid == lesson.OwnerSid
                && lesson.StatusId == (int)Enumerations.LessonStatus.Draft)
            {
                canDelete = true;
            }

            if (canDelete)
            {
                if (lesson.StatusId == (int)Enumerations.LessonStatus.Draft)
                {
                    foreach (var lessonTheme in lesson.LessonThemes.ToList())
                    {
                        _context.Remove(lessonTheme);
                    } 

                    //Remove drafts from database when deleting
                    _context.Remove(lesson);
                }
                else
                {
                    lesson.Enabled = false;
                }

                _context.SaveChanges();
            }
            else
            {
                throw new AccessViolationException("Access Denied");
            }

            return new Lesson();
        }

        public Lesson UnDeleteLesson(int id, RoleUser currentUser)
        {
            if(currentUser.RoleId != (int)Enumerations.Role.Administrator)
            {
                throw new AccessViolationException("Access Denied");
            }

            Lesson lesson = GetLessonById(id);

            if (lesson.Enabled!=true)
            {
                lesson.Enabled = true;
                _context.SaveChanges();
            }

            return lesson;
        }

        public void SaveReferenceList(List<string> updatedEnabledValues, List<string> updatedDisabledValues, Enumerations.ReferenceType referenceType, RoleUser currentUser)
        {
            var modifiedDate = Utility.GetCurrentDateTimeAsMST();
            string updateUser = Utility.FormatUserNameForDisplay(currentUser.LastName, currentUser.FirstName, currentUser.Email, true, false);
            int sortOrder = 1;

            List<ReferenceValue> currentList = GetReferenceValues(referenceType);

            if (updatedEnabledValues != null)
            {
                foreach (string enabled in updatedEnabledValues)
                {
                    if (enabled != string.Empty)
                    {
                        ReferenceValue modified = currentList.Find(x => x.Name == enabled);

                        //This is a new item
                        if (modified == null)
                        {
                            var newValue = new ReferenceValue
                            {
                                Enabled = true,
                                Name = enabled,
                                ReferenceTypeId = (int)referenceType,
                                SortOrder = sortOrder,
                                System = false,
                                CreateUser = updateUser,
                                CreateDate = modifiedDate
                            };

                            _context.Add(newValue);
                        }
                        else
                        {
                            //This is an existing item.  update enabled and sort order only
                            modified.Enabled = true;
                            modified.SortOrder = sortOrder;
                            modified.UpdateUser = updateUser;
                            modified.UpdateDate = modifiedDate;
                        }

                        sortOrder++;
                    }
                }
            }

            //Add an arbitrary amount to the sort order for disabled items
            sortOrder += 500;

            if (updatedDisabledValues != null)
            {
                foreach (string disabled in updatedDisabledValues)
                {
                    if (disabled != string.Empty)
                    {
                        ReferenceValue modified = currentList.Find(x => x.Name == disabled);

                        //This is a new item
                        if (modified == null)
                        {
                            var newValue = new ReferenceValue
                            {
                                Enabled = false,
                                Name = disabled,
                                ReferenceTypeId = (int)referenceType,
                                SortOrder = sortOrder,
                                System = false,
                                CreateUser = updateUser,
                                CreateDate = modifiedDate
                            };

                            _context.Add(newValue);
                        }
                        else
                        {
                            //This is an existing item.  update enabled and sort order only
                            modified.Enabled = false;
                            modified.SortOrder = sortOrder;
                            modified.UpdateUser = updateUser;
                            modified.UpdateDate = modifiedDate;
                        }

                        sortOrder++;
                    }
                }
            }

            _context.SaveChanges();
        }

        public void SaveDisciplineUserList(List<string> updatedSidValues, string primarySid, int editingDisciplineReferenceValue, RoleUser currentUser)
        {
            //clear the assigned discipline for all current users of the selected discipline
            var currentUsers = _context.RoleUsers.Where(x => x.DisciplineId == editingDisciplineReferenceValue).ToList();
            currentUsers.ForEach(x =>
                {
                    x.DisciplineId = null;
                    x.Primary = false;
                });

            //Update the user tables with the values from the UI
            _context.RoleUsers.Where(x => updatedSidValues.Contains(x.Sid)).ToList().ForEach(x =>
                {
                    x.Primary = x.Sid == primarySid;
                    x.DisciplineId = editingDisciplineReferenceValue;
                });

            _context.SaveChanges();
        }

        public void DeleteComment(int id, RoleUser currentUser)
        {
            Comment comment = _context.Comments.Where(x => x.Id == id).FirstOrDefault();

            // Validate user can delete
            bool canDelete = false;

            if (currentUser.RoleId == (int)Enumerations.Role.Administrator)
            {
                canDelete = true;
            }

            if (canDelete)
            {
                comment.Enabled = false;
                _context.SaveChanges();
            }
            else
            {
                throw new AccessViolationException("Access Denied");
            }
        }

        public void UnDeleteComment(int id, RoleUser currentUser)
        {
            if (currentUser.RoleId != (int)Enumerations.Role.Administrator)
            {
                throw new AccessViolationException("Access Denied");
            }

            Comment comment = _context.Comments.Where(x => x.Id == id).FirstOrDefault();

            if (comment.Enabled!=true)
            {
                comment.Enabled = true;
                _context.SaveChanges();
            }
        }

        public void SavePrimaryAdminUser(string primaryAdminUserSid, RoleUser currentUser)
        {
            int adminRoleId = (int)Enumerations.Role.Administrator;

            //Remove the flag from the current admin user
            _context.RoleUsers.Where(x => x.RoleId == adminRoleId).ToList().ForEach(x => x.Primary = false);

            //Add the flag to the new administrator
            _context.RoleUsers.Where(x => x.Sid == primaryAdminUserSid).First().Primary = true;

            _context.SaveChanges();
        }

        public List<Lesson> GetLessonsForNotification(Enumerations.NotificationEmailType notificationEmailType, int notificationDays)
        {
            List<Lesson> result = new List<Lesson>();

            int bpoReviewStatusId = (int)Enumerations.LessonStatus.BPOReview;
            int draftStatusId = (int)Enumerations.LessonStatus.BPOReview;
            DateTime rightNow = Utility.GetCurrentDateTimeAsMST();

            var query = _context.Lessons.Where(x => x.Enabled == true);

            if (notificationEmailType == Enumerations.NotificationEmailType.N8_BPOUnvalidatedReminder)
            {
                //Get the lessons where the last time the lesson was sent to the BPO is greater than notificationDays
                //The lesson must be in BPOReview Status

                query = query.Where(x => x.StatusId == bpoReviewStatusId);

                query = query.Where(x => EntityFunctions.DiffDays(
                    x.LessonHistories.Where(y =>
                        //Last Date the record hit BPO Review
                        y.NewStatusId == bpoReviewStatusId)
                        .Select(y => x.CreateDate)
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max(),
                    rightNow) >= notificationDays);
            }
            else if (notificationEmailType == Enumerations.NotificationEmailType.N9_LLCDraftReminder)
            {
                //Get the lessons created by LL Coordinators and are in draft status for longer than notificationDays

                var lessonsLearnedCoordinators = GetUsersByRole(Enumerations.Role.Coordinator);
                var coordinatorSids = lessonsLearnedCoordinators.Select(x => x.Sid);

                query = query.Where(x => x.StatusId == draftStatusId
                    && coordinatorSids.Contains(x.OwnerSid));

                //Lesson history is created when the lesson hits New status.  We can assume CreateDate is enough to measure days old
                query = query.Where(x => EntityFunctions.DiffDays(x.CreateDate, rightNow) >= notificationDays);
            }

            result = query
                .Include(x => x.Classification)
                .Include(x => x.CostImpact)
                .Include(x => x.CredibilityChecklist)
                .Include(x => x.Discipline)
                .Include(x => x.ImpactBenefitRange)
                .Include(x => x.LessonTypeValid)
                .Include(x => x.LessonTypeInvalid)
                .Include(x => x.Location)
                .Include(x => x.Phase)
                .Include(x => x.Project)
                .Include(x => x.RiskRanking)
                .Include(x => x.Status)
                .ToList();

            //get the first lesson for each distinct email address (so we only send 1 email per email address)
            if (notificationEmailType == Enumerations.NotificationEmailType.N8_BPOUnvalidatedReminder)
            {
                //Get each distinct discipline and return the first lesson for each
                result = result.GroupBy(x => x.DisciplineId, (x, y) => new { DisciplineId = x, Lesson = y.FirstOrDefault() }).Select(x => x.Lesson).ToList();
            }
            else if (notificationEmailType == Enumerations.NotificationEmailType.N9_LLCDraftReminder)
            {
                //Get each distinct CoordinatorOwnerSid and return the first lesson for each
                result = result.GroupBy(x => x.CoordinatorOwnerSid, (x, y) => new { CoordinatorOwnerSid = x, Lesson = y.FirstOrDefault() }).Select(x => x.Lesson).ToList();
            }

            return result;
        }

        public void SendEmails(List<EmailInfo> emailList)
        {
            //Remove duplicate emails grouped by mailto address
            //This scenario should only happen if override email address is set
            if (emailList != null && emailList.Count > 1)
            {
                if (emailList[0].MailTo == Utility.SafeGetAppConfigSetting("Debug_OverrideEmailAddress", ""))
                {
                    emailList = emailList.GroupBy(x => x.MailTo).Select(y => y.First()).ToList();
                }
            }
            //emailList = emailList.GroupBy(x => x.MailTo).Select(y => y.First()).ToList();

            //Create a delegate that we can throw it onto a worker thread (fire and forget)
            //This method should return instantly
            var sendEmailsDelegate = new System.Action(() => 
                {
                    foreach (var emailInfo in emailList)
                    {
                        SendEmail(emailInfo.MailTo, emailInfo.Subject, emailInfo.Body);
                    }
                });

            sendEmailsDelegate.BeginInvoke(iar =>
            {
                try
                {
                    sendEmailsDelegate.EndInvoke(iar);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("SendEmails (sendEmailsDelegate)", ex.ToString());
                }
            }, null);
        }

        private static void SendEmail(string mailTo, string subject, string body)
        {
            //Credentials from the web.config (mailSettings node)
            SmtpClient client = new SmtpClient();

            MailMessage message = new MailMessage();

            try
            {
                message.From = new MailAddress(Utility.SafeGetAppConfigSetting("MailFromNoReply", "noreply@lessonslearned.suncor.com"));
                message.To.Add(new MailAddress(mailTo));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                client.Send(message);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("SendEmail", "An error occured while sending an email to {0}.  The error is: {1}", mailTo, ex.ToString());
            }
        }

        public ExportLog GenerateLog(List<int> selectedLessonIds, Enumerations.LogType logType)
        {
            ExportLog result = new ExportLog { LogType = logType };

            int bpoValidationCommentTypeId = (int)Enumerations.CommentType.BPOValidation;
            int closedStatusId = (int)Enumerations.LessonStatus.Closed;

            var lessons = _context.Lessons
                .Where(x => selectedLessonIds.Contains(x.Id))
                .Include(x => x.Classification)
                .Include(x => x.CostImpact)
                .Include(x => x.CredibilityChecklist)
                .Include(x => x.Discipline)
                .Include(x => x.ImpactBenefitRange)
                .Include(x => x.LessonTypeValid)
                .Include(x => x.LessonTypeInvalid)
                .Include(x => x.Location)
                .Include(x => x.Phase)
                .Include(x => x.Project)
                .Include(x => x.RiskRanking)
                .Include(x => x.Status)
                .Include(x => x.LessonHistories)
                .Include(x => x.Comments)
                .Include(x => x.LessonThemes)
                .Include(x => x.LessonThemes.Select(y => y.Theme))
                .OrderByDescending(x => x.Id)
                .ToList();

            return GenerateLog(lessons, logType);
        }

        public ExportLog GenerateLog(RoleUser currentUser, LessonFilters filters, Enumerations.LogType logType)
        {
            ExportLog result = new ExportLog { LogType = logType };

            int bpoValidationCommentTypeId = (int)Enumerations.CommentType.BPOValidation;
            int closedStatusId = (int)Enumerations.LessonStatus.Closed;

            int unused = 0;

            //HACK: Remove Duplicates
            var lessonsRaw = GetLessonsPaged(currentUser, filters, false, true, 0, 0, out unused);
            lessonsRaw = lessonsRaw.GroupBy(x => x.Id).Select(y => y.First()).ToList();

            return GenerateLog(lessonsRaw, logType);
        }

        private ExportLog GenerateLog(List<Lesson> lessonList, Enumerations.LogType logType)
        {
            ExportLog result = new ExportLog { LogType = logType };

            int bpoValidationCommentTypeId = (int)Enumerations.CommentType.BPOValidation;
            int closedStatusId = (int)Enumerations.LessonStatus.Closed;
            int unused = 0;

            var lessons = lessonList
                .Select(x => new
                {
                    Id = x.Id,
                    ProjectName = x.Project == null ? "" : x.Project.Name,
                    SessionDate = x.SessionDate == null ? "" : x.SessionDate.Value.ToShortDateString(),
                    Location = x.Location == null ? "" : x.Location.Name,
                    Coordinator = x.Coordinator,
                    Phase = x.Phase == null ? "" : x.Phase.Name,
                    Title = x.Title,
                    Description = x.Description,
                    CausalFactors = x.CasualFactors,
                    Benefit = x.Benefit,
                    CostImpact = x.CostImpact == null ? "" : x.CostImpact.Name,
                    SuggestedAction = x.SuggestedAction,
                    Discipline = x.Discipline == null ? "" : x.Discipline.Name,
                    Contact = (x.ContactFirstName ?? "") + " " + (x.ContactLastName ?? "") + " | " + (x.ContactEmail ?? "") + " | " + (x.ContactPhone ?? ""),
                    LessonType = x.LessonTypeInvalidId == null ? (x.LessonTypeValid == null ? "" : x.LessonTypeValid.Name) : (x.LessonTypeInvalid == null ? "" : x.LessonTypeInvalid.Name),
                    Status = x.Status == null ? "" : x.Status.Name,
                    RiskRanking = x.RiskRanking == null ? "" : x.RiskRanking.Name,
                    ValidationComment = (
                        x.Comments.Where(y => y.CommentTypeId == bpoValidationCommentTypeId).Count() > 0
                        ? x.Comments.Where(y => y.CommentTypeId == bpoValidationCommentTypeId).OrderByDescending(y => y.CreateDate).FirstOrDefault().Content
                        : ""
                    ),
                    DateClosed = x.ClosedDate.ToDisplayDate(),
                    SupportingDocuments = x.SupportingDocuments,
                    Resolution = x.Resolution,
                    EstimatedCompletion = x.EstimatedCompletion == null ? "" : x.EstimatedCompletion.Value.ToShortDateString(),
                    ValidatedUser = x.ValidatedBy,
                    Classification = x.Classification == null ? "" : x.Classification.Name,
                    ImpactBenefit = x.ImpactBenefitRange == null ? "" : x.ImpactBenefitRange.Name,
                    CredibilityChecklist = x.CredibilityChecklist == null ? "" : x.CredibilityChecklist.Name,
                    Enabled = x.Enabled,
                    CreatedUser = x.CreateUser,
                    CreatedDate = x.CreateDate.ToDisplayDate(),
                    UpdateUser = x.UpdateUser,
                    UpdateDate = x.UpdateDate.ToDisplayDate(),
                    MigrationRecord = x.MigrationRecord,
                    SubmittedBy = x.SubmittedBy,
                    SubmittedDate = x.SubmittedDate.ToDisplayDate(),
                    ValidOrInvalid = x.LessonTypeValidId != null ? "Valid" : (x.LessonTypeInvalidId != null ? "Invalid" : ""),
                    LessonAge = x.LessonAge,
                    TimesSentForClarification = x.TimesSentForClarification,
                    LastSentForClarification = x.LastSentForClarification.ToDisplayDate(),
                    TimesSentToBpo = x.TimesSentToBpo,
                    LastSentToBpo = x.LastSentToBpo.ToDisplayDate(),
                    Themes = x.LessonThemes.Count > 0 ? x.LessonThemes.Select(y => y.Theme.Name).Aggregate((a, b) => a + ", " + b) : "",
                    ThemesOtherPresent = x.LessonThemes.Where(y => y.Theme.System).Any() ? "Yes" : "No",
                    ThemeOther = x.ThemeDescription,
                    ThemeCount = x.LessonThemes.Count,
                    SessionQuarter = x.SessionQuarter.HasValue ? x.SessionQuarter.Value.ToString() : "",
                    ClosedQuarter = x.ClosedQuarter.HasValue ? x.ClosedQuarter.Value.ToString() : "",
                    DateAdminToBpo = x.DateAdminToBpo.ToDisplayDate(),
                    BpoTransferCount = x.BpoTransfers,
                    DateLastSentToBpo = x.DateLastSentToBpo.ToDisplayDate(),
                })
                .ToList();

            #region ActionLog

            if (logType == Enumerations.LogType.ActionLog)
            {
                MemoryStream templateFileStream = null;

                result.FileName = "ActionLog.xls";
                templateFileStream = new MemoryStream(Resource.ActionLogTemplate);

                HSSFWorkbook workbook = new HSSFWorkbook(templateFileStream);

                var templateSheet = workbook.GetSheetAt(0);

                int rowIndex = 5;
                foreach (var lesson in lessons)
                {
                    int cellIndex = 0;

                    templateSheet.CreateRow(rowIndex);

                    var cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Id);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ProjectName);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SessionDate);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Location);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Coordinator);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Phase);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Title);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Description);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.CausalFactors);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Benefit);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.CostImpact);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SuggestedAction);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Discipline);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Contact);

                    //Themes
                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Themes);

                    //Other Comments
                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ValidationComment);

                    //Blank column
                    templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ValidOrInvalid + ": " + lesson.LessonType);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Status);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.RiskRanking);

                    //BPO Resolution
                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Resolution);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.DateClosed);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SupportingDocuments);

                    //Blank column
                    templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);

                    //Action owner

                    //Action Taken

                    //Benifits Realized/Knowledge Gained

                    //Action Complete Y/N

                    //Action Completed Date

                    rowIndex++;
                }

                var stream = new MemoryStream();
                workbook.Write(stream);

                result.FileBytes = stream.ToArray();
            }

            #endregion ActionLog

            #region AdminLog

            if ( logType == Enumerations.LogType.AdminLog)
            {
                MemoryStream templateFileStream = null;

                result.FileName = "AdministratorLog.xls";
                templateFileStream = new MemoryStream(Resource.AdminLogTemplate);

                HSSFWorkbook workbook = new HSSFWorkbook(templateFileStream);

                var templateSheet = workbook.GetSheetAt(0);

                int rowIndex = 2;
                foreach (var lesson in lessons)
                {
                    int cellIndex = 0;

                    templateSheet.CreateRow(rowIndex);

                    var cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Status);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Enabled!=true);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.MigrationRecord);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Id);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SubmittedBy);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SubmittedDate);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ProjectName);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Title);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Description);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Location);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.RiskRanking);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SuggestedAction);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.CostImpact);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Benefit);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ImpactBenefit);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.CausalFactors);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.CredibilityChecklist);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ThemeCount);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Themes);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ThemesOtherPresent);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ThemeOther);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Phase);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Classification);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.CreatedUser);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.CreatedDate);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.UpdateUser);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.UpdateDate);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Contact);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Coordinator);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SessionDate);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SessionQuarter);

                    //Comments..... um, not sure what to put here so I'm leaving it blank
                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SupportingDocuments);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Discipline);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ValidatedUser);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ValidOrInvalid);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.LessonType);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Resolution);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.EstimatedCompletion);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ValidationComment);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.DateClosed);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ClosedQuarter);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.LessonAge);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.DateAdminToBpo);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.BpoTransferCount);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.TimesSentForClarification);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.LastSentForClarification);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.BpoTransferCount);
                    
                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.DateLastSentToBpo);

                    rowIndex++;
                }

                var stream = new MemoryStream();
                workbook.Write(stream);

                result.FileBytes = stream.ToArray();
            }

            #endregion

            #region GenericLog

            if (logType == Enumerations.LogType.GenericLog)
            {
                MemoryStream templateFileStream = null;

                result.FileName = "LessonsLearned.xls";
                templateFileStream = new MemoryStream(Resource.GenericLogTemplate);
                
                HSSFWorkbook workbook = new HSSFWorkbook(templateFileStream);

                var templateSheet = workbook.GetSheetAt(0);

                int rowIndex = 2;
                foreach (var lesson in lessons)
                {
                    int cellIndex = 0;

                    templateSheet.CreateRow(rowIndex);

                    var cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Status);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Id);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Title);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Description);
                    
                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.RiskRanking);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SuggestedAction);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.CostImpact);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Benefit);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ImpactBenefit);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.CausalFactors);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.CredibilityChecklist);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ThemeCount);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Themes);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ThemesOtherPresent);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ThemeOther);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Discipline);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ProjectName);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Phase);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Classification);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SubmittedBy);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SubmittedDate);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Contact);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Coordinator);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SessionDate);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Location);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.SupportingDocuments);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ValidOrInvalid);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.LessonType);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.Resolution);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.EstimatedCompletion);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ValidationComment);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ValidatedUser);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.DateClosed);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.ClosedQuarter);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.LessonAge);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.TimesSentForClarification);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.LastSentForClarification);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.BpoTransferCount);

                    cell = templateSheet.GetRow(rowIndex).CreateCell(cellIndex++);
                    cell.SetCellValue(lesson.DateLastSentToBpo);

                    rowIndex++;
                }

                var stream = new MemoryStream();
                workbook.Write(stream);

                result.FileBytes = stream.ToArray();
            }

            #endregion

            return result;
        }

        public void RefreshRoleUsersFromActiveDirectory()
        {
            Logger.Debug("RefreshRoleUsersFromActiveDirectory()", "Begin");
            DateTime updateDate = Utility.GetCurrentDateTimeAsMST();

            var allUpdatedUsers = GetAllActiveDirectoryUsers();
            var allCurrentUsers = GetAllUsers();

            var recordCount = 0;

            List<string> userSids = allCurrentUsers.Select(x => x.Sid).ToList();

            foreach (var roleUser in allUpdatedUsers)
            {
                if (roleUser == null)
                {
                    //Yeah I know, this should never happen.... but it has, so here we are.
                    continue;
                }

                recordCount++;

                //Find the user in our local copy
                var currentUser = allCurrentUsers.Where(x => x.Sid == roleUser.Sid).FirstOrDefault();

                if (currentUser != null)
                {
                    if (currentUser.AutoUpdate == true)
                    {
                        //Existing user, update properties
                        currentUser.Description = roleUser.Description;
                        currentUser.Email = roleUser.Email;
                        currentUser.Enabled = roleUser.Enabled;
                        currentUser.FirstName = roleUser.FirstName;
                        currentUser.LastName = roleUser.LastName;
                        currentUser.Phone = roleUser.Phone;
                        currentUser.Phone = roleUser.Phone;
                        currentUser.RoleId = roleUser.RoleId;
                        currentUser.DistinguishedName = roleUser.DistinguishedName;
                        currentUser.AutoUpdate = roleUser.AutoUpdate;
                    }
                }
                else
                {
                    if (userSids.Contains(roleUser.Sid))
                    {
                        Logger.Warn("RefreshRoleUsersFromActiveDirectory()", "User already exists: " + roleUser.Sid + " " + roleUser.FirstName + " " + roleUser.LastName);
                    }
                    else
                    {
                        userSids.Add(roleUser.Sid);

                        //New User, add it
                        roleUser.CreateUser = "Service";
                        roleUser.CreateDate = updateDate;
                        
                        _context.Add(roleUser);
                    }
                }

                //Save changes every 1000 records so we don't timeout the sql server
                if (recordCount >= 1000)
                {
                    Logger.Debug("RefreshRoleUsersFromActiveDirectory()", "Process 1000 users, saving...");
                    recordCount = 0;
                    _context.SaveChanges();

                    Logger.Debug("RefreshRoleUsersFromActiveDirectory()", "Saved.");
                }
            }

            recordCount = 0;
            _context.SaveChanges();

            var activeDirecorySids = allUpdatedUsers.Select(x => x.Sid).ToList();

            //Get all the users that no longer appear in the Active Directory list and disable them
            foreach (var user in allCurrentUsers.Where(x => !activeDirecorySids.Contains(x.Sid)))
            {
                if (user.AutoUpdate == true)
                {
                    recordCount++;

                    user.Enabled = false;

                    //Save changes every 1000 records so we don't timeout the sql server
                    if (recordCount >= 1000)
                    {
                        Logger.Debug("RefreshRoleUsersFromActiveDirectory()", "Process 1000 users, saving...");
                        recordCount = 0;
                        _context.SaveChanges();

                        Logger.Debug("RefreshRoleUsersFromActiveDirectory()", "Saved.");
                    }
                }
            }

            _context.SaveChanges();
        }

        #region NPOI Helper Methods

        private void CreateLinkCell(ICell cell, string url, string caption = "")
        {
            if (string.IsNullOrEmpty(caption))
            {
                caption = url;
            }

            ////cell style for hyperlinks
            ////by default hyperlinks are blue and underlined
            var linkStyle = cell.Sheet.Workbook.CreateCellStyle();
            var linkFont = cell.Sheet.Workbook.CreateFont();
            linkFont.Underline = FontUnderlineType.Single;
            linkFont.Color = NPOI.HSSF.Util.HSSFColor.Blue.Index;
            linkStyle.SetFont(linkFont);

            cell.SetCellValue(caption);
            HSSFHyperlink link = new HSSFHyperlink(HyperlinkType.Url);
            link.Address = (url);
            cell.Hyperlink = (link);
            cell.CellStyle = (linkStyle);
        }

        /// <summary>
        /// Adds an image to the excel sheet at the specified row/column
        /// </summary>
        /// <param name="image">png only</param>
        /// <param name="position">x, y, width, height are represented in cells</param>
        private void AddPicture(ISheet sheet, System.Drawing.Bitmap image, System.Drawing.Rectangle position)
        {
            HSSFPatriarch patriarch = (HSSFPatriarch)sheet.CreateDrawingPatriarch();

            //create the anchor
            HSSFClientAnchor anchor;
            anchor = new HSSFClientAnchor(0, 0, 0, 255, position.Left, position.Top, position.Right, position.Bottom);
            anchor.AnchorType = AnchorType.MoveDontResize;

            //load the picture and get the picture index in the workbook
            var picStream = new MemoryStream();
            image.Save(picStream, System.Drawing.Imaging.ImageFormat.Png);
            picStream.Position = 0;
            byte[] buffer = new byte[picStream.Length];
            picStream.Read(buffer, 0, (int)picStream.Length);
            var pictureIndex = sheet.Workbook.AddPicture(buffer, PictureType.PNG);

            HSSFPicture picture = (HSSFPicture)patriarch.CreatePicture(anchor, pictureIndex);
            //Reset the image to the original size.
            picture.Resize();
            picture.LineStyle = HSSFPicture.LINESTYLE_SOLID;
        }

        private void SetCellComment(ICell cell, string comment)
        {
            // Create the drawing patriarch. This is the top level container for all shapes including cell comments.
            HSSFPatriarch patr = (HSSFPatriarch)cell.Sheet.CreateDrawingPatriarch();

            //anchor defines size and position of the comment in worksheet
            var comment1 = patr.CreateCellComment(new HSSFClientAnchor(0, 0, 0, 0, cell.RowIndex, cell.ColumnIndex, 6, 5));

            // set text in the comment
            comment1.String = (new HSSFRichTextString(comment));

            cell.CellComment = (comment1);
        }

        private ICellStyle GetHeaderCellStyle(IWorkbook workbook)
        {
            var headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Gold.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;

            var font = workbook.CreateFont();
            font.Boldweight = (short)FontBoldWeight.Bold;
            font.FontHeightInPoints = 12;

            headerStyle.SetFont(font);
            return headerStyle;
        }

        private ICellStyle GetDetailCellStyle(IWorkbook workbook)
        {
            var headerStyle = workbook.CreateCellStyle();

            var font = workbook.CreateFont();
            font.IsItalic = true;

            headerStyle.SetFont(font);
            return headerStyle;
        }

        #endregion
    }
}
