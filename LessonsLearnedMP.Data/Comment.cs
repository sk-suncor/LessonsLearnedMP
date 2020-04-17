using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suncor.LessonsLearnedMP.Data
{
    [Serializable]
    [Table("Comment")]
    public partial class Comment
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public int CommentTypeId { get; set; }
        public string Content { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public bool? Enabled { get; set; }

        public virtual ReferenceValue CommentType { get; set; }
        public virtual Lesson Lesson { get; set; }
    }
}
