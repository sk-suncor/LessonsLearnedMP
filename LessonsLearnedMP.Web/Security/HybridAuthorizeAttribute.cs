using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Web.Security
{
    public class HybridAuthorizeAttribute : AuthorizeAttribute
    {
        public Enumerations.Role[] RequiredPrivileges { get; private set; }

        public HybridAuthorizeAttribute(params Enumerations.Role[] requiredPrivileges)
        {
            RequiredPrivileges = requiredPrivileges;
        }
        
        public bool UserIsAuthorized()
        {
            // If only User exists, allow access
            if (RequiredPrivileges.Contains(Enumerations.Role.User) && RequiredPrivileges.Count() == 1)
            {
                return true;
            }

            // Check if the user has rights
            return RequiredPrivileges.Where(p => p != Enumerations.Role.User).Any(Common.Utils.CurrentUserHasAccess);
        }/*

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            return UserIsAuthorized();
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Home/AuthenticationError");
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }*/
    }
}
