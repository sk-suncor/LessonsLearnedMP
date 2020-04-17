using System.ComponentModel;
using System.Runtime.Serialization;

namespace Suncor.LessonsLearnedMP.Framework
{
    public static class Enumerations
    {
        public enum AuthFailedBehaviour
        {
            EmptyString = 1,
            InnerHtml,
            AlternateLink
        }

        public enum JavaScript
        {
            Jquery = 1,
            JqueryUi,
            JqueryBgiframe,
            JqueryForm,
            JqueryLivequery,
            JqueryValidate,
            Json2,
            LessonsLearnedCommon,
            LessonsLearnedSearch,
            LessonsLearnedEdit,
            LessonsLearnedLessonIndex,
            LessonsLearnedAdmin,
            LessonsLearnedDashboard,
            JqueryDataTables,
            Date,
            JqueryContextMenu,
            Bootstrap,
            JquerySelectMenu,
            Pie,
            Excanvas,
            JqueryJqplot,
            JqueryJqplot_PieRenderer,
            JqueryJqplot_BarRenderer,
            JqueryJqplot_CategoryAxisRenderer,
            JqueryJqplot_PointLabels,
            JqueryJqplot_Cursor,
            JqueryJqplot_Highlighter,
            JqueryJqplot_DateAxisRenderer,
            JqueryMultiselect,
            JqueryTooltip
        }

        public enum Role
        {
            User = 242,
            Coordinator,
            BPO,
            Administrator
        }

        public enum ReadOnlyBehaviour
        {
            EmptyString = 1,
            InnerHtml,
            Disabled,
            CheckedIcon,
            AuthorizationLink
        }

        public enum Css
        {
            JqueryUi = 1,
            JqueryUiInverse,
            Site,
            LeftRightList,
            DataTables,
            JqueryContextMenu,
            Bootstrap,
            JquerySelectMenu,
            JqueryJqplot,
            JqueryMultiselect,
            JqueryTooltip
        }

        public enum SortDirection
        {
            Ascending = 1,
            Descending
        }

        public enum LessonListSortColumn
        {
            Number = 2,
            Status = 3,
            Title = 4,
            Discipline = 5,
            SubmitDate = 6,
            Contact = 7
        }

        public enum LessonCommentListSortColumn
        {
            None = 1
        }

        public enum LessonCommentListFilterColumn
        {
            None = 1
        }

        public enum LessonStatus
        {
            MIGRATION = 0,
            Draft = 1,
            New = 2,
            [Description("Admin Review")]
            AdminReview = 3,
            [Description("Waiting For Clarification")]
            Clarification = 4,
            [Description("Under BPO Review")]
            BPOReview = 5,
            Closed = 6
        }

        public enum ReferenceType
        {
            LessonStatus = 1,

            [Description("Projects")]
            Project,

            [Description("Phases")]
            Phase,

            [Description("Classifications")]
            Classification,

            [Description("Locations")]
            Location,

            [Description("Benefit Ranges")]
            ImpactBenefitRange,

            [Description("Cost Impacts")]
            CostImpact,

            [Description("Risk Rankings")]
            RiskRanking,

            [Description("Disciplines")]
            Discipline,

            [Description("Credibility Checklists")]
            CredibilityChecklist,

            [Description("Lesson Types (Valid)")]
            LessonTypeValid,

            [Description("Lesson Types (Invalid)")]
            LessonTypeInvalid,

            CommentType,
            System,

            [Description("Themes")]
            Theme,

            Role
        }

        public enum PageAction
        {
            Submit = 1,
            Edit,
            Search,
            Bulk,
            MyLessons
        }

        public enum SaveAction
        {
            None = 0,
            SaveChanges = 1,
            DraftToNew,
            NewToAdmin,
            AdminToClarification,
            AdminToBPO,
            BPOToBPO,
            BPOToClarification,
            BPOToClose,
            ClarificationToAdmin,
            ClarificationToBPO,
            Delete,
            UnDelete,
            ClosedToBPO,
            ActionLog,
            ActionLogAll,
            CsvLog,
            CsvLogAll,
            SaveDraft,
            SubmitAnother,
            AssignToUser
        }

        public enum CommentType
        {
            BPOValidation = 7,
            Clarification = 8,
            ClarificationRequired = 9,
            TransferToBPO = 10
        }

        public enum LogType
        {
            ActionLog = 1,
            GenericLog,
            AdminLog
        }

        public enum NotificationEmailType
        {
            N1_NewToAdmin = 1,
            N2_AdminToBPO_And_BPOToBPO,
            N3_AdminToClarification,
            N4_ClarificationToAdmin,
            N5_BPOToClarification,
            N6_ClarificationToBPO,
            N7_BPOToCloseLLC,
            N7_BPOToCloseOwner,
            N8_BPOUnvalidatedReminder,
            N9_LLCDraftReminder,
            N10_AssignToUser
        }

        public enum NavigationPage
        {

            Home = 1,
            Edit,
            Review,
            Validate,
            [Description("My Lessons")]
            MyLessons,
            Search,
            Dashboard,
            [Description("Admin")]
            Administration,
            Submit
        }

        public enum HelpTopic
        {
            LessonList = 1,
            DashboardOpenChart,
            DashboardClosedChart,
            EditProjectName,
            EditLLCoordinator,
            EditProjectPhase,
            EditProjectClassification,
            EditSessionDate,
            EditLocation,
            EditLessonTitle,
            EditDescription,
            EditCausalFactors,
            EditSuggestedAction,
            EditBenefit,
            EditBenefitRange,
            EditCostImpact,
            EditRiskRanking,
            EditDiscipline,
            EditCredibilityChecklist,
            EditContact,
            EditThemes,
            EditSupportingDocuments,
            EditEstimatedCompletion,
            EditResolution,
            EditNumber,
            EditStatus,
            EditSubmittedBy,
            EditSubmittedDate,
            EditLessonAge,
            EditSessionQuarter,
            EditBPOTransfers,
            EditTimesSentBPO,
            EditDateLastSentBPO,
            EditDateLastSentClarification,
            EditTimesSentClarification
        }
    }
}
