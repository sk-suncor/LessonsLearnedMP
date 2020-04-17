using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suncor.LessonsLearnedMP.Data
{
    [Serializable]
    [Table("Lesson")]
    public partial class Lesson
    {
        public Lesson()
        {
            Comments = new HashSet<Comment>();
            LessonHistories = new HashSet<LessonHistory>();
            LessonThemes = new HashSet<LessonTheme>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CasualFactors { get; set; }
        public string Benefit { get; set; }
        public string SuggestedAction { get; set; }
        public string Resolution { get; set; }
        public DateTime? SessionDate { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
        public string ContactLastName { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Coordinator { get; set; }
        public string SupportingDocuments { get; set; }
        public int StatusId { get; set; }
        public int? ProjectId { get; set; }
        public int? PhaseId { get; set; }
        public int? DisciplineId { get; set; }
        public int? LocationId { get; set; }
        public int? CostImpactId { get; set; }
        public int? ClassificationId { get; set; }
        public int? ImpactBenefitRangeId { get; set; }
        public int? LessonTypeValidId { get; set; }
        public int? LessonTypeInvalidId { get; set; }
        public int? RiskRankingId { get; set; }
        public int? CredibilityChecklistId { get; set; }
        public bool? Enabled { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool MigrationRecord { get; set; }
        public string OwnerSid { get; set; }
        public string CoordinatorOwnerSid { get; set; }
        public string ThemeDescription { get; set; }

        public virtual ReferenceValue Classification { get; set; }
        public virtual ReferenceValue CostImpact { get; set; }
        public virtual ReferenceValue CredibilityChecklist { get; set; }
        public virtual ReferenceValue Discipline { get; set; }
        public virtual ReferenceValue ImpactBenefitRange { get; set; }
        public virtual ReferenceValue LessonTypeInvalid { get; set; }
        public virtual ReferenceValue LessonTypeValid { get; set; }
        public virtual ReferenceValue Location { get; set; }
        public virtual ReferenceValue Phase { get; set; }
        public virtual ReferenceValue Project { get; set; }
        public virtual ReferenceValue RiskRanking { get; set; }
        public virtual ReferenceValue Status { get; set; }
       
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<LessonHistory> LessonHistories { get; set; }
        public virtual ICollection<LessonTheme> LessonThemes { get; set; }
    }
}
