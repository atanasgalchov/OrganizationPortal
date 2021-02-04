using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationPortal
{
	public interface IAppCache
	{
		object GetValue(string key);
		void SetValue(string key, object value);
		void Remove(string key);
	}
	public class AppCache : IAppCache
	{
		private IMemoryCache _cache;
		public AppCache(IMemoryCache cache)
		{
			_cache = cache;
		}

		public object GetValue(string value)
		{
			return _cache.Get(value);
		}

		public void SetValue(string key, object value)
		{
			_cache.Set(key, value);
		}

		public void Remove(string key) 
		{
			_cache.Remove(key);
		}

		public enum AppCacheKeys 
		{
			EditedAppResources,
			AppSettings,
			EditedAppSettings
		}
	}
}
