using Microsoft.Extensions.Caching.Distributed;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suncor.LessonsLearnedMP.Web.Common
{
    public class ApplicationContext : IApplicationContext
    {
        private readonly IDistributedCache _cache;
        public void Clear()
        {
            this.AllUsers = null;
            this.AllReferenceValues = null;
        }

        public ApplicationContext(IDistributedCache cache)
        {
            _cache = cache;
        }

        public List<ReferenceValue> AllReferenceValues
        {
            get
            {
                
                if (_cache != null)
                {
                    return _cache.TryGet<List<ReferenceValue>>("AllReferenceValues");
                }
                
                return new List<ReferenceValue>();
            }
            set
            {
                
                if (_cache != null)
                {
                    _cache.Set<List<ReferenceValue>>("AllReferenceValues",value);
                }
            }
        }

        public List<ReferenceValue> LessonStatuses
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.LessonStatus || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> Projects
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.Project || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> Phases
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.Phase || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> Classifications
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.Classification || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> Locations
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.Location || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> ImpactBenefitRanges
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.ImpactBenefitRange || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> CostImpacts
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.CostImpact || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> RiskRankings
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.RiskRanking || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> Disciplines
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.Discipline || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> CredibilityChecklists
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.CredibilityChecklist || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> LessonTypesValid
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.LessonTypeValid || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> LessonTypesInvalid
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.LessonTypeInvalid || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> CommentTypes
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.CommentType || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> SystemValues
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceValue> Themes
        {
            get
            {
                return this.AllReferenceValues.Where(x => x.ReferenceTypeId == (int)Enumerations.ReferenceType.Theme || x.ReferenceTypeId == (int)Enumerations.ReferenceType.System).OrderBy(x => x.SortOrder).ToList();
            }
        }

        public List<ReferenceType> ReferenceTypes
        {
            get
            {
                
                if (_cache != null)
                {
                    return _cache.Get<List<ReferenceType>>("ReferenceTypes");
                }
                
                return new List<ReferenceType>();
            }
            set
            {
                
                if (_cache!=null)
                {
                    _cache.Set("ReferenceTypes", value);
                }
            }
        }

        public List<RoleUser> Coordinators
        {
            get
            {
                return this.AllUsers.Where(x => x.RoleId == (int)Enumerations.Role.Coordinator).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
            }
        }

        public List<RoleUser> Bpos
        {
            get
            {
                return this.AllUsers.Where(x => x.RoleId == (int)Enumerations.Role.BPO).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
            }
        }

        public List<RoleUser> Admins
        {
            get
            {
                return this.AllUsers.Where(x => x.RoleId == (int)Enumerations.Role.Administrator).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
            }
        }

        public List<RoleUser> DisciplineUsers
        {
            get
            {
                return this.AllUsers.Where(x => x.DisciplineId != null).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
            }
        }

        public List<RoleUser> AllUsers
        {
            get
            {
                
                if (_cache != null )
                {
                    return _cache.Get<List<RoleUser>>("AllUsers");
                }

                return new List<RoleUser>();
            }
            set
            {
                
                if (_cache != null)
                {
                    _cache.Set("AllUsers",value);
                }
            }
        }

        public DateTime LastRefresh
        {
            get
            {
                
                if (_cache!=null)
                {
                    var result = _cache.Get< DateTime>("LastRefresh");
                    return result == null ? DateTime.MinValue : result;
                }

                return DateTime.MinValue;
            }
            set
            {
                
                if (_cache != null)
                {
                    _cache.Set("LastRefresh", value);
                }
            }
        }
    }
}
