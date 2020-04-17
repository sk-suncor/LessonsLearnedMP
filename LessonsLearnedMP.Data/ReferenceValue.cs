using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suncor.LessonsLearnedMP.Data
{
    [Serializable]
    [Table("ReferenceValue")]
    public partial class ReferenceValue
    {
        public ReferenceValue()
        {
            Comment = new HashSet<Comment>();
            LessonClassification = new HashSet<Lesson>();
            LessonCostImpact = new HashSet<Lesson>();
            LessonCredibilityChecklist = new HashSet<Lesson>();
            LessonDiscipline = new HashSet<Lesson>();
            LessonHistoryNewDiscipline = new HashSet<LessonHistory>();
            LessonHistoryNewStatus = new HashSet<LessonHistory>();
            LessonHistoryPreviousDiscipline = new HashSet<LessonHistory>();
            LessonHistoryPreviousStatus = new HashSet<LessonHistory>();
            LessonImpactBenefitRange = new HashSet<Lesson>();
            LessonLessonTypeInvalid = new HashSet<Lesson>();
            LessonLessonTypeValid = new HashSet<Lesson>();
            LessonLocation = new HashSet<Lesson>();
            LessonPhase = new HashSet<Lesson>();
            LessonProject = new HashSet<Lesson>();
            LessonRiskRanking = new HashSet<Lesson>();
            LessonStatus = new HashSet<Lesson>();
            LessonTheme = new HashSet<LessonTheme>();
            RoleUserDiscipline = new HashSet<RoleUser>();
            RoleUserRole = new HashSet<RoleUser>();
        }

        public int Id { get; set; }
        public int ReferenceTypeId { get; set; }
        public string Name { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int SortOrder { get; set; }
        public bool Enabled { get; set; }
        public bool System { get; set; }

        public virtual ReferenceType ReferenceType { get; set; }
        public virtual ICollection<Comment> Comment { get; set; }
        public virtual ICollection<Lesson> LessonClassification { get; set; }
        public virtual ICollection<Lesson> LessonCostImpact { get; set; }
        public virtual ICollection<Lesson> LessonCredibilityChecklist { get; set; }
        public virtual ICollection<Lesson> LessonDiscipline { get; set; }
        public virtual ICollection<LessonHistory> LessonHistoryNewDiscipline { get; set; }
        public virtual ICollection<LessonHistory> LessonHistoryNewStatus { get; set; }
        public virtual ICollection<LessonHistory> LessonHistoryPreviousDiscipline { get; set; }
        public virtual ICollection<LessonHistory> LessonHistoryPreviousStatus { get; set; }
        public virtual ICollection<Lesson> LessonImpactBenefitRange { get; set; }
        public virtual ICollection<Lesson> LessonLessonTypeInvalid { get; set; }
        public virtual ICollection<Lesson> LessonLessonTypeValid { get; set; }
        public virtual ICollection<Lesson> LessonLocation { get; set; }
        public virtual ICollection<Lesson> LessonPhase { get; set; }
        public virtual ICollection<Lesson> LessonProject { get; set; }
        public virtual ICollection<Lesson> LessonRiskRanking { get; set; }
        public virtual ICollection<Lesson> LessonStatus { get; set; }
        public virtual ICollection<LessonTheme> LessonTheme { get; set; }
        public virtual ICollection<RoleUser> RoleUserDiscipline { get; set; }
        public virtual ICollection<RoleUser> RoleUserRole { get; set; }
    }
}
