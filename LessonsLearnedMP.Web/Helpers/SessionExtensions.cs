using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Suncor.LessonsLearnedMP.Business;
using Suncor.LessonsLearnedMP.Data;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Suncor.LessonsLearnedMP.Web.Helpers
{
	public static class SessionExtensions
	{
		public static T Get<T>(this ISession session, string key)
		{
			byte[] value = null;
			if (session.TryGetValue(key, out value))
			{
                object result = null;
                using (MemoryStream stream = new MemoryStream(value))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    if (stream.Length != 0)
                    { 
                      result = formatter.Deserialize(stream);
                    }
					return (T)Convert.ChangeType(result, typeof(T));
				}
			}
			return default;
		}

		public static void Set<T>(this ISession session, string key, T value)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			using (MemoryStream stream = new MemoryStream())
			{
                if(value!=null)
                { 
				    formatter.Serialize(stream, value);
                }
				stream.Flush();
				byte[] data = stream.ToArray();
				session.Set(key, data);
			}
		}

        public static void StartSession(this ISession session, HttpContext context, LessonsLearnedMPEntities db)
        {
            
            var userSessionContext = new UserSessionContext(context);
            if (userSessionContext.CurrentUser == null)
            {
                LessonBusiness businessManager = new LessonBusiness(db);

                userSessionContext.CurrentUser = businessManager.GetCurrentUser();
            }

            if (!string.IsNullOrWhiteSpace(Utility.SafeGetAppConfigSetting("Debug_UserPermission", "")))
            {
                Enumerations.Role debugPrivilege = (Enumerations.Role)Enum.Parse(typeof(Enumerations.Role), Utility.SafeGetAppConfigSetting("Debug_UserPermission", "User"));
                if (userSessionContext.CurrentUser == null)
                {
                    if (bool.Parse(Utility.SafeGetAppConfigSetting("Debug_PopulateFakeUsers", "false")))
                    {
                        userSessionContext.CurrentUser = new RoleUser
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
        
    }
	
}
