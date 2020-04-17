using Microsoft.AspNetCore.Http;
using Suncor.LessonsLearnedMP.Business;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Web.Helpers;
using Suncor.LessonsLearnedMP.Web.ViewData;
using System.Collections.Generic;

namespace Suncor.LessonsLearnedMP.Web.Common
{
    public class UserSessionContext : IUserSessionContext
    {
        private readonly HttpContext _context;

        public UserSessionContext(HttpContext context)
        {
            _context = context;
        }

        public bool SessionExpired
        {
            get
            {
                if (_context != null && _context.Session != null)
                {
                    try
                    {
                        return (bool)_context.Session.Get<bool>("SessionExpired");
                    }
                    catch
                    {
                        //Just assume expired and return true
                    }
                }

                return true;
            }
            set
            {
                if (_context != null && _context.Session != null)
                {
                    _context.Session.Set<bool>("SessionExpired",value);
                }
            }
        }

        public RoleUser CurrentUser
        {
            get
            {
                if (_context != null && _context.Session != null)
                {
                    return (RoleUser)_context.Session.Get<RoleUser>("CurrentUser");
                }

                return new RoleUser();
            }
            set
            {
                if (_context != null && _context.Session != null)
                {
                    _context.Session.Set<RoleUser>("CurrentUser",value);
                }
            }
        }

        /// <summary>
        /// This is the definitive dataset in the lesson list grid.
        /// </summary>
        public List<Lesson> LastSearchResults
        {
            get
            {
                if (_context != null && _context.Session != null)
                {
                    return (List<Lesson>)_context.Session.Get<List<Lesson>>("LastSearchResults");
                }

                return new List<Lesson>();
            }
            set
            {
                if (_context != null && _context.Session != null)
                {
                    _context.Session.Set<List<Lesson>>("LastSearchResults",value);
                }
            }
        }

        /// <summary>
        /// This is the last user initiated search action taken
        /// </summary>
        public SearchViewModel LastSearch
        {
            get
            {
                SearchViewModel result = null;
                if (_context != null && _context.Session != null)
                {
                    result = (SearchViewModel)_context.Session.Get<SearchViewModel>("LastSearch");
                }

                return result ?? new SearchViewModel { Blank = true };
            }
            set
            {
                if (_context != null && _context.Session != null)
                {
                    _context.Session.Set<SearchViewModel>("LastSearch", value);
                }
            }
        }

        /// <summary>
        /// This is the definitive last search criteria used to populate the lesson list
        /// It differs from <see cref="LastSearch"/> in that the system could have initiated the search (i.e. MyLessons)
        /// </summary>
        public SearchViewModel LastSystemSearch
        {
            get
            {
                SearchViewModel result = null;
                if (_context != null && _context.Session != null)
                {
                    result = (SearchViewModel)_context.Session.Get<SearchViewModel>("LastSystemSearch");
                }

                return result ?? new SearchViewModel { Blank = true };
            }
            set
            {
                if (_context != null && _context.Session != null)
                {
                    _context.Session.Set<SearchViewModel>("LastSystemSearch",value);
                }
            }
        }

        public ExportLog ExportLog
        {
            get
            {
                if (_context != null && _context.Session != null)
                {
                    return (ExportLog)_context.Session.Get<ExportLog>("ExportLog");
                }

                return null;
            }
            set
            {
                if (_context != null && _context.Session != null)
                {
                    _context.Session.Set<ExportLog>("ExportLog",value);
                }
            }
        }

        public LessonViewModel DraftDefaults
        {
            get
            {
                LessonViewModel result = null;
                if (_context != null && _context.Session != null)
                {
                    result = (LessonViewModel)_context.Session.Get<LessonViewModel>("DraftDefaults");
                }

                return result ?? new LessonViewModel(_context) { OwnerSid = this.CurrentUser.Sid, CreatedUser = Utils.FormatUserNameForDisplay(this.CurrentUser, false) };
            }
            set
            {
                //Format the defaults before saving them.
                //Only the Project Phase section is defaulted, so clear all other values
                var result = new LessonViewModel(_context);
                result.PhaseId = value.PhaseId;
                result.LocationId = value.LocationId;
                result.SessionDate = value.SessionDate;
                result.ProjectId = value.ProjectId;
                result.ClassificationId = value.ClassificationId;
                result.Coordinator = value.Coordinator;
                result.CoordinatorOwnerSid = value.CoordinatorOwnerSid;
                result.OwnerSid = value.OwnerSid;
                result.CreatedUser = value.CreatedUser;

                if (_context != null && _context.Session != null)
                {
                    _context.Session.Set<LessonViewModel>("DraftDefaults", result);
                }
            }
        }
    }
}
