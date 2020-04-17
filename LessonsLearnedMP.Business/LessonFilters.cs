using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Business
{
    [Serializable]
    public class LessonFilters
    {
        //Simple
        public string Keyword { get; set; }
        [Display(Name = "Lesson Number")]
        public int? LessonId { get; set; }

        public int? Status { get; set; }
        public List <int> SelectedStatus { get; set; }

        public string SubmittedUser { get; set; }
        public List<string> SelectedSubmittedUser { get; set; }

        [Display(Name = "Date Submitted (From)")]
        public DateTime? SubmittedDateBegin { get; set; }
        [Display(Name = "Date Submitted (To)")]
        public DateTime? SubmittedDateEnd { get; set; }

        public int? ProjectId { get; set; }
        public List<int> SelectedProjectId { get; set; }

        public string Coordinator { get; set; }
        public List<string> SelectedCoordinator { get; set; }

        public int? PhaseId { get; set; }
        public List<int> SelectedPhaseId { get; set; }

        public int? ClassificationId { get; set; }
        public List<int> SelectedClassificationId { get; set; }

        [Display(Name = "Session Date (From)")]
        public DateTime? SessionDateBegin { get; set; }
        [Display(Name = "Session Date (To)")]
        public DateTime? SessionDateEnd { get; set; }

        public int? LocationId { get; set; }
        public List<int> SelectedLocationId { get; set; }

        public int? ImpactBenefitRangeId { get; set; }
        public List<int> SelectedImpactBenefitRangeId { get; set; }

        public int? CostImpactId { get; set; }
        public List<int> SelectedCostImpactId { get; set; }

        public int? RiskRankingId { get; set; }
        public List<int> SelectedRiskRankingId { get; set; }

        public int? CredibilityChecklistId { get; set; }
        public List<int> SelectedCredibilityChecklistId { get; set; }

        public int? DisciplineId { get; set; }
        public List<int> SelectedDisciplineId { get; set; }

        public int? LessonTypeValidId { get; set; }
        public List<int> SelectedLessonTypeValidId { get; set; }

        public int? LessonTypeInvalidId { get; set; }
        public List<int> SelectedLessonTypeInvalidId { get; set; }

        public string OwnerSid { get; set; }
        public string CoordinatorOwnerSid { get; set; }


        //Advanced
        public string Title { get; set; }
        public string Description { get; set; }
        public string CausalFactors { get; set; }
        public string SuggestedAction { get; set; }
        [Display(Name = "Lesson Age (From)")]
        public int? LessonAgeBegin { get; set; }
        [Display(Name = "Lesson Age (To)")]
        public int? LessonAgeEnd { get; set; }
        [Display(Name = "Response Turnaround (From)")]
        public int? ResponseTurnaroundBegin { get; set; }
        [Display(Name = "Response Turnaround (To)")]
        public int? ResponseTurnaroundEnd { get; set; }
        [Display(Name = "Last Sent For Clarification (From)")]
        public DateTime? LastClarificationDateBegin { get; set; }
        [Display(Name = "Last Sent For Clarification (To)")]
        public DateTime? LastClarificationDateEnd { get; set; }
        [Display(Name = "No. of Times Sent For Clarification (From)")]
        public int? TimesSentForClarificationBegin { get; set; }
        [Display(Name = "No. of Times Sent For Clarification (To)")]
        public int? TimesSentForClarificationEnd { get; set; }
        [Display(Name = "Last Sent to BPO (From)")]
        public DateTime? LastSentToBpoDateBegin { get; set; }
        [Display(Name = "Last Sent to BPO (To)")]
        public DateTime? LastSentToBpoDateEnd { get; set; }
        [Display(Name = "No. of Times Sent to BPO (From)")]
        public int? TimesSentToBpoBegin { get; set; }
        [Display(Name = "No. of Times Sent to BPO (To)")]
        public int? TimesSentToBpoEnd { get; set; }
        [Display(Name = "No. of BPO Transfers (From)")]
        public int? TimesBpoTransferedBegin { get; set; }
        [Display(Name = "No. of BPO Transfers (To)")]
        public int? TimesBpoTransferedEnd { get; set; }
        [Display(Name = "Date Closed (From)")]
        public DateTime? ClosedDateBegin { get; set; }
        [Display(Name = "Date Closed (To)")]
        public DateTime? ClosedDateEnd { get; set; }

        public int? ClosedQuarter { get; set; }
        public List<int> SelectedClosedQuarter { get; set; }

        public int? ThemeId { get; set; }
        public List<int> SelectedThemeId { get; set; }

        public string ThemeDescription { get; set; }

        //Dashboard (not available on search screen
        public bool ShowOnlyOwnedLessons { get; set; }
        public bool ShowEditableOnly { get; set; }
    }
}
