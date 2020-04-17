using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Common;

namespace Suncor.LessonsLearnedMP.Web.ViewData
{
    public class AdminViewData : IValidatableObject
    {
        private IDistributedCache _cache;
        public AdminViewData(IDistributedCache cache)
        {
            EditingReferenceValueList = new List<ReferenceValue>();
            ReferenceValueEnabled = new List<string>();
            ReferenceValueDisabled = new List<string>();
            DisciplineUsers = new List<string>();
            _cache = cache;
        }

        public int EditReferenceType { get; set; }
        public List<ReferenceValue> EditingReferenceValueList { get; set; }
        public List<string> ReferenceValueEnabled { get; set; }
        public List<string> ReferenceValueDisabled { get; set; }
        public string ListName { get; set; }
        public string AddReferenceValue { get; set; }
        public List<string> DisciplineUsers { get; set; }
        public string PrimaryDisciplineUser { get; set; }
        public int EditingDisciplineUsersReferenceValue { get; set; }
        public string PrimaryAdminUser { get; set; }
        public int EmailNotificationType { get; set; }
        public int NotificationDays { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            ApplicationContext appContext = new ApplicationContext(_cache);

            if (string.IsNullOrEmpty(PrimaryDisciplineUser))
            {
                yield return new ValidationResult("Primary Bpo is Required.", new[] { "PrimaryDisciplineUser" });
            }
            else if (appContext.Bpos.Where(x => x.Sid == PrimaryDisciplineUser && string.IsNullOrWhiteSpace(x.Email)).Any())
            {
                yield return new ValidationResult("The Primary BPO is invalid (No Email).", new[] { "PrimaryDisciplineUser" });
            }
            else if (!DisciplineUsers.Contains(PrimaryDisciplineUser))
            {
                yield return new ValidationResult("The Primary BPO must be an assigned user.", new[] { "PrimaryDisciplineUser" });
            }

            if (string.IsNullOrEmpty(PrimaryAdminUser))
            {
                yield return new ValidationResult("Primary Administrator is Required.", new[] { "PrimaryAdminUser" });
            }
            else if (appContext.Admins.Where(x => x.Sid == PrimaryAdminUser && string.IsNullOrWhiteSpace(x.Email)).Any())
            {
                yield return new ValidationResult("The Primary Admin User is invalid (No Email).", new[] { "PrimaryAdminUser" });
            }

            if (ReferenceValueEnabled.Count == 0)
            {
                yield return new ValidationResult("A minimum of one value must be enabled.", new[] { "ReferenceValueEnabled" });
            }
        }
    }
}