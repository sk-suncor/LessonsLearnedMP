using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suncor.LessonsLearnedMP.Framework
{
    public class SortColumn
    {
        public Enumerations.LessonListSortColumn Column { get; set; }
        public Enumerations.SortDirection Direction { get; set; }
        public int SortOrder { get; set; }
    }
}
