using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Common;

namespace Suncor.LessonsLearnedMP.Web.ViewData
{
    public class EmailTemplateViewData
    {
        public string Subject { get; set; }
        public RoleUser OwningUser { get; set; }
        public RoleUser OwningCoordinator { get; set; }
        public RoleUser OwningCoordinatorCreator { get; set; }
        public RoleUser OwningBpo { get; set; }
        public RoleUser Administrator { get; set; }
        public LessonViewModel Lesson { get; set; }
        public Enumerations.NotificationEmailType NotificationType { get; set; }
        public string MailTo { get; set; }
        public string OverrideMailTo { get; set; }
        public RoleUser AssignTo { get; set; }
        public bool Redirecting
        {
            get { return !string.IsNullOrWhiteSpace(OverrideMailTo); }
        }

        public EmailTemplateViewData(LessonViewModel lesson, Enumerations.NotificationEmailType notificationType, IApplicationContext appContext, string overrideMailTo = null)
        {
            Lesson = lesson;
            NotificationType = notificationType;
            OwningUser = appContext.AllUsers.Where(x => x.Sid == lesson.OwnerSid).FirstOrDefault() ?? new RoleUser();
            OwningBpo = appContext.DisciplineUsers.Where(x => x.Primary && x.DisciplineId == lesson.DisciplineId).FirstOrDefault() ?? new RoleUser();
            Administrator = appContext.Admins.Where(x => x.Primary).FirstOrDefault() ?? new RoleUser();
            AssignTo = appContext.AllUsers.Where(x => x.Name == lesson.AssignToUserId).FirstOrDefault();

            if (lesson.CoordinatorOwnerSid == Constants.TextDefaults.LLCListPrimaryAdminLabel)
            {
                OwningCoordinator = Administrator;
            }
            else
            {
                OwningCoordinator = appContext.Coordinators.Where(x => x.Sid == lesson.CoordinatorOwnerSid).FirstOrDefault() ?? new RoleUser();
            }

            OwningCoordinatorCreator = appContext.Coordinators.Where(x => x.Sid == lesson.OwnerSid).FirstOrDefault() ?? new RoleUser();

            switch (notificationType)
            {
                case Enumerations.NotificationEmailType.N1_NewToAdmin:
                    Subject = "ACTION REQUIRED: New Lesson Added to LLD";
                    MailTo = Administrator.Email;
                    break;
                case Enumerations.NotificationEmailType.N2_AdminToBPO_And_BPOToBPO:
                    Subject = "ACTION REQUIRED: You have lessons to validate";
                    MailTo = OwningBpo.Email;
                    break;
                case Enumerations.NotificationEmailType.N3_AdminToClarification:
                    Subject = "ACTION REQUIRED: You have lessons to clarify";
                    MailTo = OwningCoordinator.Email;
                    break;
                case Enumerations.NotificationEmailType.N4_ClarificationToAdmin:
                    Subject = "ACTION REQUIRED: Lesson Clarification Complete";
                    MailTo = Administrator.Email;
                    break;
                case Enumerations.NotificationEmailType.N5_BPOToClarification:
                    Subject = "ACTION REQUIRED: You have lessons to clarify";
                    MailTo = OwningCoordinator.Email;
                    break;
                case Enumerations.NotificationEmailType.N6_ClarificationToBPO:
                    Subject = "ACTION REQUIRED: Lesson Clarification Complete";
                    MailTo = OwningBpo.Email;
                    break;
                case Enumerations.NotificationEmailType.N7_BPOToCloseLLC:
                    Subject = "NOTIFICATION: Lesson validation is complete for #" + lesson.Id.ToString();
                    MailTo = OwningCoordinator.Email;
                    break;
                case Enumerations.NotificationEmailType.N7_BPOToCloseOwner:
                    Subject = "NOTIFICATION: Lesson validation is complete for #" + lesson.Id.ToString();
                    MailTo = OwningUser.Email;
                    break;
                case Enumerations.NotificationEmailType.N8_BPOUnvalidatedReminder:
                    Subject = "REMINDER: ACTION REQUIRED";
                    MailTo = OwningBpo.Email;
                    break;
                case Enumerations.NotificationEmailType.N9_LLCDraftReminder:
                    Subject = "REMINDER: ACTION REQUIRED";
                    MailTo = OwningCoordinatorCreator.Email;
                    break;
                case Enumerations.NotificationEmailType.N10_AssignToUser:
                    Subject = "ACTION REQUIRED: You have lessons to validate";
                    MailTo = AssignTo.Email;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("notificationType");
            }

            if (string.IsNullOrWhiteSpace(overrideMailTo) && string.IsNullOrWhiteSpace(MailTo))
            {
                OverrideMailTo = Administrator.Email;
            }
            else
            {
                OverrideMailTo = overrideMailTo;
            }
        }
    }
}