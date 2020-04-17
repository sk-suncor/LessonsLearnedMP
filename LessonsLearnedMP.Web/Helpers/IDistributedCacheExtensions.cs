using Microsoft.Extensions.Caching.Distributed;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Suncor.LessonsLearnedMP.Web.Helpers
{
	public static class IDistributedCacheExtensions
    {
		public static T TryGet<T>(this IDistributedCache cache, string key)
		{
			try
			{
				byte[] value = cache.Get(key);
				return DeserializeToObject<T>(value);
			}
			catch { }
			
			return default;
		}

		private static T DeserializeToObject<T>(byte[] value)
		{
			if (value == null)
				return default;

			using (MemoryStream stream = new MemoryStream(value))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				object result = formatter.Deserialize(stream);
				return (T)Convert.ChangeType(result, typeof(T));
			}
		}

		public static T Get<T>(this IDistributedCache cache, string key)
		{
			byte[] value = cache.Get(key);

			return DeserializeToObject<T>(value);
		}

		public static void Set<T>(this IDistributedCache cache, string key, T value)
		{
			if (value == null)
			{
				cache.Remove(key);
			}
			else
			{
				BinaryFormatter formatter = new BinaryFormatter();
				using (MemoryStream stream = new MemoryStream())
				{
					formatter.Serialize(stream, value);
					stream.Flush();
					byte[] data = stream.ToArray();
					cache.Set(key, data, new DistributedCacheEntryOptions
					{
						  SlidingExpiration = TimeSpan.MaxValue
					});
				}
			}
		}

    }
	
}
