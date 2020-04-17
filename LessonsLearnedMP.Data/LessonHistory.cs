using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suncor.LessonsLearnedMP.Data
{
    [Serializable]
    [Table("LessonHistory")]
    public partial class LessonHistory
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public int PreviousStatusId { get; set; }
        public int NewStatusId { get; set; }
        public int PreviousDisciplineId { get; set; }
        public int NewDisciplineId { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual Lesson Lesson { get; set; }
        public virtual ReferenceValue NewDiscipline { get; set; }
        public virtual ReferenceValue NewStatus { get; set; }
        public virtual ReferenceValue PreviousDiscipline { get; set; }
        public virtual ReferenceValue PreviousStatus { get; set; }
    }
}
