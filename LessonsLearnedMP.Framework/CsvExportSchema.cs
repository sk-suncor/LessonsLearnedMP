using System;
using System.Collections.Generic;

namespace Suncor.LessonsLearnedMP.Framework
{
    public class CsvExportSchema
    {
        public List<CsvExportSchemaColumn> Columns { get; set; }

        public Guid FileId { get; set; }

        public bool IsDefined
        {
            get
            {
                return Columns != null && Columns.Count > 0;
            }
        }

        //public static CsvExportSchema GetLogSchema(Enumerations.LogType logType)
        //{
        //    CsvExportSchema schema = new CsvExportSchema();

        //    switch (logType)
        //    {
        //        case Enumerations.LogType.ActionLog:
        //            schema = new CsvExportSchema
        //            {
        //                Columns = new List<CsvExportSchemaColumn>
        //                {
        //                    new CsvExportSchemaColumn { ColumnHeader = "Record No", Property = "Id" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Project Name", Property = "ProjectName" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Session Date", Property = "SessionDate" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Location", Property = "Location" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Learned Coordinator", Property = "Coordinator" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Phase", Property = "Phase" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Title", Property = "Title" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Description", Property = "Description" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Causal Factors", Property = "CausalFactors" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Benefits", Property = "Benefit" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Cost Impact", Property = "CostImpact" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Suggested Action", Property = "SuggestedAction" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Responsible Discipline", Property = "Discipline" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Contact", Property = "Contact" },

        //                    new CsvExportSchemaColumn { ColumnHeader = "", Property = "" },

        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Type", Property = "LessonType" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Status", Property = "Status" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Risk Matrix Rating", Property = "RiskRanking" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "BPO Comments", Property = "ValidationComment" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Date Closed", Property = "DateClosed" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Supporting Documents", Property = "SupportingDocuments" },

        //                    new CsvExportSchemaColumn { ColumnHeader = "", Property = "" },

        //                    new CsvExportSchemaColumn { ColumnHeader = "Action Owner", Property = "" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Action Taken", Property = "" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Benefits Realized / Knowledge Gained", Property = "" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Estimated Dollar Value Realized", Property = "" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Action Complete Y/N", Property = "" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Action Completed Date", Property = "" }
        //                }
        //            };
        //            break;
        //        case Enumerations.LogType.CsvExport:
        //            schema = new CsvExportSchema
        //            {
        //                Columns = new List<CsvExportSchemaColumn>
        //                {
        //                    new CsvExportSchemaColumn { ColumnHeader = "Record No", Property = "Id" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Project Name", Property = "ProjectName" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Session Date", Property = "SessionDate" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Location", Property = "Location" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Learned Coordinator", Property = "Coordinator" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Phase", Property = "Phase" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Title", Property = "Title" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Description", Property = "Description" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Causal Factors", Property = "CausalFactors" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Benefits", Property = "Benefit" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Cost Impact", Property = "CostImpact" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Suggested Action", Property = "SuggestedAction" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Responsible Discipline", Property = "Discipline" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Contact", Property = "Contact" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Type", Property = "LessonType" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Lesson Status", Property = "Status" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Risk Matrix Rating", Property = "RiskRanking" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "BPO Comments", Property = "ValidationComment" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Date Closed", Property = "DateClosed" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Supporting Documents", Property = "SupportingDocuments" },

        //                    new CsvExportSchemaColumn { ColumnHeader = "Resolution", Property = "Resolution" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Estimated Completion", Property = "EstimatedCompletion" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Validated User", Property = "ValidatedUser" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Classification", Property = "Classification" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Benefit Range", Property = "ImpactBenefit" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Credibility Checklist", Property = "CredibilityChecklist" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Enabled?", Property = "Enabled" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Created By", Property = "CreatedUser" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Created Date", Property = "CreatedDate" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Last Updated By", Property = "UpdateUser" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Last Updated", Property = "UpdateDate" },
        //                    new CsvExportSchemaColumn { ColumnHeader = "Migration Record?", Property = "MigrationRecord" }
        //                }
        //            };
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException("logType");
        //    }

        //    return schema;
        //}
    }

    public class CsvExportSchemaColumn
    {
        public string ColumnHeader { get; set; }
        public string Property { get; set; }
    }
}


