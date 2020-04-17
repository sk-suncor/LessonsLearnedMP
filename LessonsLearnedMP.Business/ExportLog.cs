using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Business
{
    public class ExportLog
    {
        public Enumerations.LogType LogType { get; set; }
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
        public bool Downloaded { get; set; }

        public string ContentType
        {
            get
            {
                return "application/vnd.ms-excel";
                //switch (LogType)
                //{
                //    case Enumerations.LogType.ActionLog:
                //        return "application/vnd.ms-excel";
                //        break;
                    //case Enumerations.LogType.CsvExport:
                    //    return "text/csv";
                    //    break;
                    //default:
                    //    return "";
                //}
            }
        }

    }
}
