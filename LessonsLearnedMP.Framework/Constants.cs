using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suncor.LessonsLearnedMP.Framework
{
    public static class Constants
    {

        public static class DatabaseStringLengths
        {
            public const int Title = 100;
            public const int ShortText = 50;
            public const int LongText = 255;
            public const int ExtraLongText = 4096;
        }

        public static class TextDefaults
        {
            public const string DefaultLessonTitle = "New Draft Lesson";
            public const string LLCListPrimaryAdminLabel = "Lessons Learned Administrator";
        }

        public static class UiDefaults
        {
            public const int LongFormViewHeight = 500;
            public const int LessonListHeight = 200;
        }
    }
}
