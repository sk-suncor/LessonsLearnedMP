using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suncor.LessonsLearnedMP.Data
{
    [Serializable]
    public class EmailInfo
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string MailTo { get; set; }
    }
}
