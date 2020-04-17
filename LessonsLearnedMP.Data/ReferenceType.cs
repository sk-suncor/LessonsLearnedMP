using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suncor.LessonsLearnedMP.Data
{
    [Serializable]
    [Table("ReferenceType")]
    public partial class ReferenceType
    {
        public ReferenceType()
        {
            ReferenceValue = new HashSet<ReferenceValue>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool System { get; set; }

        public virtual ICollection<ReferenceValue> ReferenceValue { get; set; }
    }
}
