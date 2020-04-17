using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suncor.LessonsLearnedMP.Data
{
    [Serializable]
    [Table("RoleUser")]
    public partial class RoleUser
    {
        public string Sid { get; set; }
        public int? DisciplineId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public bool Primary { get; set; }
        public bool Enabled { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string DistinguishedName { get; set; }
        public bool? AutoUpdate { get; set; }

        public virtual ReferenceValue Discipline { get; set; }
        public virtual ReferenceValue Role { get; set; }
    }
}
