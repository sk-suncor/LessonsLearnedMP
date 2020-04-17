using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Data
{
    public partial class RoleUser
    {
        public string Name
        {
            get
            {
                return Utility.FormatUserNameForDisplay(this.LastName, this.FirstName, this.Email, this.Enabled, true);
            }
        }
    }
}
