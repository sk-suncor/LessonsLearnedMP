using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Caching.Distributed;
using Suncor.LessonsLearnedMP.Business;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Common;
using Suncor.LessonsLearnedMP.Web.ViewData;

namespace Suncor.LessonsLearnedMP.Web.Controllers
{
    public abstract class ControllerBase : Controller
    {   
        private IUserSessionContext _userSessionContext;
        private IApplicationContext _applicationContext;
        private LessonsLearnedMPEntities _dbcontext;
        private IDistributedCache _cache;

        private static string _alertMessage = "";
        private static string _alertSprite = "";
        private static bool _showAlert = false;
        private static bool _alertDeferred = false;

        [CoverageExclude]
        protected ControllerBase(LessonsLearnedMPEntities context, IDistributedCache cache)
        {
            _dbcontext = context;
            _cache = cache;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _userSessionContext = new UserSessionContext(this.ControllerContext.HttpContext);
            _applicationContext = new ApplicationContext(_cache);
            if (_userSessionContext.CurrentUser == null || _userSessionContext.CurrentUser.RoleId<=0)
                InitializeUserSession(filterContext);

            this.LoadSiteViewData(filterContext);
            this.CacheLists();
            base.OnActionExecuting(filterContext);
        }

        private void InitializeUserSession(ActionExecutingContext filterContext)
        {
            Logger.Debug("Session_Start", "Begin");

            Logger.Debug("Session_Start", "var userSessionContext = new UserSessionContext(new HttpContextWrapper(HttpContext.Current));");
            
            var userSessionContext = new UserSessionContext(filterContext.HttpContext);

            Logger.Debug("Session_Start", "LessonBusiness businessManager = new LessonBusiness();");
            LessonBusiness businessManager = new LessonBusiness(_dbcontext);

            Logger.Debug("Session_Start", "userSessionContext.CurrentUser = businessManager.GetCurrentUser();");
            userSessionContext.CurrentUser = businessManager.GetCurrentUser();

            if (!string.IsNullOrWhiteSpace(Utility.SafeGetAppConfigSetting("Debug_UserPermission", "")))
            {
                Enumerations.Role debugPrivilege = (Enumerations.Role)Enum.Parse(typeof(Enumerations.Role), Utility.SafeGetAppConfigSetting("Debug_UserPermission", "User"));
                if (userSessionContext.CurrentUser == null)
                {
                    if (bool.Parse(Utility.SafeGetAppConfigSetting("Debug_PopulateFakeUsers", "false")))
                    {
                        userSessionContext.CurrentUser = new Data.RoleUser
                        {
                            LastName = "User",
                            FirstName = "Debug",
                            Sid = "S-1-5-21-861617734-1335137780-1834409622-8391"
                        };
                    }
                }

                if (userSessionContext.CurrentUser != null)
                {
                    userSessionContext.CurrentUser.RoleId = (int)debugPrivilege;
                }
            }
        }


        /// <summary>
        /// Add all site-wide view data here so they're available for every controller and view
        /// </summary>
        private void LoadSiteViewData(ActionExecutingContext filterContext)
        {
        }

        protected bool ErrorOccured
        {
            get
            {
                return TempData.Keys.Where(x => x.StartsWith("_FAULT")).Any();
            }
        }

        protected LessonsLearnedMPEntities DbContext { get => _dbcontext; }

        protected IDistributedCache Cache { get => _cache; }

        protected void AddError(string message, bool log = false, Logger.LogType logType = Logger.LogType.Debug)
        {
            // Don't add the same message more than once
            if (!TempData.Values.Where(x => x.ToString().Equals(message)).Any())
            {
                TempData["_FAULT__" + Guid.NewGuid().ToString()] = message;
            }

            if (log)
            {
                StringBuilder sb = new StringBuilder(256);
                var frames = new System.Diagnostics.StackTrace().GetFrames();
                for (int i = 1; i < frames.Length; i++) /* Ignore current StackTraceToString method...*/
                {
                    var currFrame = frames[i];
                    var method = currFrame.GetMethod();
                    sb.Append(string.Format("{0}:{1}\n", method.ReflectedType != null ? method.ReflectedType.Name : string.Empty, method.Name));
                }

                Logger.Log(logType, sb.ToString(), message);
            }
        }

        protected void ShowAlert(string message, string sprite, bool deferred = false)
        {
            _alertMessage = message;
            _alertSprite = sprite;
            _showAlert = true;
            _alertDeferred = deferred;
        }

        protected void SetSuccessfulSave()
        {
            ShowAlert("Information was successfully saved.", "sprite-accept");
        }

        protected void SetEmailsSent()
        {
            ShowAlert("Notifications have been sent.", "sprite-mail-send");
        }

        private void CacheLists()
        {
            LessonBusiness businessManager = new LessonBusiness(DbContext);

            if (_userSessionContext.SessionExpired)
            {
                _userSessionContext.SessionExpired = false;
                _userSessionContext.CurrentUser = businessManager.GetCurrentUser();
            }

            if ((DateTime.Now - _applicationContext.LastRefresh).TotalMinutes > int.Parse(Utility.SafeGetAppConfigSetting("CacheMinutes", "1440")))
            {
                _applicationContext.Clear();

                _applicationContext.AllReferenceValues = businessManager.GetAllReferenceValues();
                _applicationContext.ReferenceTypes = businessManager.GetReferenceTypes();
                _applicationContext.AllUsers = businessManager.GetAllUsers();
                _applicationContext.LastRefresh = DateTime.Now;
            }
        }
        /*
        protected override ViewResult View(IView view, object model)
        {
            if (_userSessionContext.CurrentUser == null || !_userSessionContext.CurrentUser.Enabled)
            {
                ModelState.AddModelError("", "You could not be logged in, or have been disabled.  Please contact Support.");
                return base.View("Error");
            }

            if (ErrorOccured)
            {
                foreach (var key in TempData.Keys.Where(x => x.StartsWith("_FAULT")))
                {
                    ModelState.AddModelError(key, TempData[key].ToString());
                }
            }

            return base.View(view, model);
        }*/
        
        
        public override ViewResult View(string viewName, object model)
        {
            // Set alert message
            ViewBag.ShowAlert = _showAlert;
            ViewBag.AlertDeferred = _alertDeferred;
            _showAlert = false;
            ViewBag.AlertMessage = _alertMessage;
            ViewBag.AlertSprite = _alertSprite;

            if (viewName != "Error" && (_userSessionContext.CurrentUser == null || !_userSessionContext.CurrentUser.Enabled))
            {
                ModelState.AddModelError("", "You could not be logged in, or have been disabled.  Please contact Support.");
                return base.View("Error");
            }

            if (ErrorOccured)
            {
                foreach (var key in TempData.Keys.Where(x => x.StartsWith("_FAULT")))
                {
                    ModelState.AddModelError(key, TempData[key].ToString());
                }
            }

            return base.View(viewName, model);
        }
        
    }
}
