using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Data
{
    

    public partial class Lesson
    {
        [NotMapped]
        public DateTime? LastSentForClarification
        {
            get
            {
                if (this.LessonHistories != null && this.LessonHistories.Count > 0)
                {
                    return this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.Clarification).Select(x => (DateTime?)x.CreateDate).DefaultIfEmpty(null).Max();
                }

                return null;
            }
        }
        [NotMapped]
        public DateTime? LastSentToBpo
        {
            get
            {
                if (this.LessonHistories != null && this.LessonHistories.Count > 0)
                {
                    return this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.BPOReview).Select(x => (DateTime?)x.CreateDate).DefaultIfEmpty(null).Max();
                }

                return null;
            }
        }
        [NotMapped]
        public DateTime? DateAdminToBpo
        {
            get
            {
                if (this.LessonHistories != null && this.LessonHistories.Count > 0)
                {
                    return this.LessonHistories.Where(x => x.PreviousStatusId == (int)Enumerations.LessonStatus.AdminReview && x.NewStatusId == (int)Enumerations.LessonStatus.BPOReview).Select(x => (DateTime?)x.CreateDate).DefaultIfEmpty(null).Max();
                }

                return null;
            }
        }
        [NotMapped]
        public DateTime? DateLastSentToBpo
        {
            get
            {
                if (this.LessonHistories != null && this.LessonHistories.Count > 0)
                {
                    return this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.BPOReview).Select(x => (DateTime?)x.CreateDate).DefaultIfEmpty(null).Max();
                }

                return null;
            }
        }

        [NotMapped]
        public DateTime? ClosedDate
        {
            get
            {
                if (this.LessonHistories != null && this.LessonHistories.Count > 0)
                {
                    return this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.Closed).Select(x => (DateTime?)x.CreateDate).DefaultIfEmpty(null).Max();
                }

                return null;
            }
        }
        [NotMapped]
        public int? ClosedQuarter
        {
            get
            {
                if (this.ClosedDate.HasValue)
                {
                    return (this.ClosedDate.Value.Month - 1) / 3 + 1;
                }

                return null;
            }
        }
        [NotMapped]
        public int? SessionQuarter
        {
            get
            {
                if (this.SessionDate.HasValue)
                {
                    return (this.SessionDate.Value.Month - 1) / 3 + 1;
                }

                return null;
            }
        }
        [NotMapped]
        /// <summary>
        /// Days between Submitted Date and Closed Date
        /// </summary>
        public int LessonAge

        {
            get
            {
                DateTime? closedDate = this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.Closed).Select(x => (DateTime?)x.CreateDate).DefaultIfEmpty(null).Max();

                if (closedDate.HasValue)
                {
                    return (closedDate.Value - (this.SubmittedDate.HasValue ? this.SubmittedDate.Value : this.CreateDate) ).Days;
                }
                else
                {
                    return (Utility.GetCurrentDateTimeAsMST() - (this.SubmittedDate.HasValue ? this.SubmittedDate.Value : this.CreateDate)).Days;
                }
            }
        }
        [NotMapped]

        //Number of times the lessons has moved to (or returned to) BPO Review
        public int TimesSentToBpo
        {
            get
            {
                if (this.LessonHistories != null && this.LessonHistories.Count > 0)
                {
                    return this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.BPOReview).Count();
                }

                return 0;
            }
        }
        [NotMapped]
        public int TimesSentForClarification
        {
            get
            {
                if (this.LessonHistories != null && this.LessonHistories.Count > 0)
                {
                    return this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.Clarification).Count();
                }

                return 0;
            }
        }
        [NotMapped]
        //Number of times the discipline of the lesson changes (After new status)
        public int BpoTransfers
        {
            get
            {
                if (this.LessonHistories != null && this.LessonHistories.Count > 0)
                {
                    return this.LessonHistories.Where(x =>
                        x.NewStatusId != (int)Enumerations.LessonStatus.New
                        && x.NewStatusId != (int)Enumerations.LessonStatus.Draft
                        && x.PreviousDisciplineId != x.NewDisciplineId).Count();
                }

                return 0;
            }
        }
        [NotMapped]
        public string SubmittedBy
        {
            get
            {
                string result = "";

                if (this.LessonHistories != null && this.LessonHistories.Count > 0)
                {
                    if (this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.New).Any())
                    {
                        result = this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.New).Select(x => x.CreateUser).FirstOrDefault();
                    }
                }

                return result;
            }
        }
        [NotMapped]
        public DateTime? SubmittedDate
        {
            get
            {
                DateTime? result = null;

                if (this.LessonHistories != null && this.LessonHistories.Count > 0)
                {
                    if (this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.New).Any())
                    {
                        result = this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.New).Select(x => x.CreateDate).FirstOrDefault();
                    }
                }

                return result;
            }
        }
        [NotMapped]
        public string ValidatedBy
        {
            get
            {
                string result = "";

                if (this.LessonHistories != null && this.LessonHistories.Count > 0)
                {
                    result = this.LessonHistories.Where(x => x.NewStatusId == (int)Enumerations.LessonStatus.Closed).OrderByDescending(x => x.CreateDate).Select(x => x.CreateUser).FirstOrDefault();
                }

                return result ?? "";
            }
        }
        [NotMapped]
        public string AssignTo
        { get; set; }
    }
}
