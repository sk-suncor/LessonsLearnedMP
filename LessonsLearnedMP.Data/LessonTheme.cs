using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suncor.LessonsLearnedMP.Data
{
    [Serializable]
    [Table("LessonTheme")]
    public partial class LessonTheme
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public int ThemeId { get; set; }

        public virtual Lesson Lesson { get; set; }
        public virtual ReferenceValue Theme { get; set; }
    }
}
