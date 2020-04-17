using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Suncor.LessonsLearnedMP.Data;

namespace Suncor.LessonsLearnedMP.Web.Common
{
    public interface IApplicationContext
    {
        List<ReferenceValue> AllReferenceValues { get; set; }
        List<ReferenceValue> LessonStatuses { get; }
        List<ReferenceValue> Projects { get; }
        List<ReferenceValue> Phases { get; }
        List<ReferenceValue> Classifications { get; }
        List<ReferenceValue> Locations { get; }
        List<ReferenceValue> ImpactBenefitRanges { get; }
        List<ReferenceValue> CostImpacts { get; }
        List<ReferenceValue> RiskRankings { get; }
        List<ReferenceValue> Disciplines { get; }
        List<ReferenceValue> CredibilityChecklists { get; }
        List<ReferenceValue> LessonTypesValid { get; }
        List<ReferenceValue> LessonTypesInvalid { get; }
        List<ReferenceValue> CommentTypes { get; }
        List<ReferenceValue> SystemValues { get; }
        List<ReferenceValue> Themes { get; }
        List<ReferenceType> ReferenceTypes { get; set; }
        List<RoleUser> Coordinators { get; }
        List<RoleUser> Bpos { get; }
        List<RoleUser> Admins { get; }
        List<RoleUser> DisciplineUsers { get; }
        List<RoleUser> AllUsers { get; set; }
        DateTime LastRefresh { get; set; }
        void Clear();
    }
}