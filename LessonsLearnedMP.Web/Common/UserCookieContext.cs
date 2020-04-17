using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Web.Common
{
    public class UserCookieContext : IUserCookieContext
    {
        private readonly HttpContext _context;

        public UserCookieContext(HttpContext context)
        {
            _context = context;
        }

        public int SearchistPageSize
        {
            get
            {
                int result = 0;

                if (_context.Request.Cookies.Keys.Contains("SearchistPageSize"))
                {
                    var cookie = _context.Request.Cookies["SearchistPageSize"];

                    if (cookie != null)
                    {
                        int.TryParse(cookie, out result);
                    }
                }

                if (result == 0)
                {
                    result = Utils.PagingSize;
                }

                return result;
            }
            set
            {
                _context.Response.Cookies.Append("SearchistPageSize", value.ToString());
            }
        }

        public int LongFormViewHeight
        {
            get
            {
                int result = 0;

                if (_context.Request.Cookies.Keys.Contains("LongFormViewHeight"))
                {
                    var cookie = _context.Request.Cookies["LongFormViewHeight"];

                    if (cookie != null)
                    {
                        int.TryParse(cookie, out result);
                    }
                }

                if (result == 0)
                {
                    result = Constants.UiDefaults.LongFormViewHeight;
                }

                return result;
            }
            set
            {
                _context.Response.Cookies.Append("LongFormViewHeight", value.ToString());
            }
        }

        public int LessonListHeight
        {
            get
            {
                int result = 0;

                if (_context.Request.Cookies.Keys.Contains("LessonListHeight"))
                {
                    var cookie = _context.Request.Cookies["LessonListHeight"];

                    if (cookie != null)
                    {
                        int.TryParse(cookie, out result);
                    }
                }

                if (result == 0)
                {
                    result = Constants.UiDefaults.LessonListHeight;
                }

                return result;
            }
            set
            {
                _context.Response.Cookies.Append("LessonListHeight", value.ToString());
            }
        }
    }
}