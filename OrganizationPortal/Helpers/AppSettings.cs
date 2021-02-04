using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrganizationPortal.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationPortal
{
	public interface IApppSettings
	{
		bool IsExistSetting(string key);
		List<AppSetting> GetAppSettings();
		AppSetting GetAppSetting(string key);
		AppSetting GetAppSetting(AppSettings.AppSettingsKeys key);
		string GetAppSettingValue(string key);
		string GetAppSettingValue(AppSettings.AppSettingsKeys key);
		void Invalidate();
	}

	public class AppSettings : IApppSettings
	{
		private IAppCache _cache;
		private IDataProvider _dataProvider;

		public AppSettings(IAppCache cache, IDataProvider dataProvider)
		{
			_cache = cache;
			_dataProvider = dataProvider;
		}

		public bool IsExistSetting(string key)
		{
			return Enum.GetValues(typeof(AppSettingsKeys)).OfType<AppSettingsKeys>().Any(x => x.ToString() == key);
		}
		public List<AppSetting> GetAppSettings()
		{
			// get edited app settings from db
			List<AppSetting> editedAppSettings = (List<AppSetting>)_cache.GetValue(AppCache.AppCacheKeys.EditedAppSettings.ToString());
			if (editedAppSettings == null)
			{
				editedAppSettings = _dataProvider.GetAppSettings();
				_cache.SetValue(AppCache.AppCacheKeys.EditedAppSettings.ToString(), editedAppSettings);
			}

			// get default app settings
			List<AppSetting> appSettings = (List<AppSetting>)_cache.GetValue(AppCache.AppCacheKeys.AppSettings.ToString());
			if (appSettings == null) 
			{				
				// read app settings
				string appSettingsString = File.ReadAllText("orgappsettings.json");
				if (appSettingsString != null && appSettingsString != String.Empty) 
				{
					appSettings = new List<AppSetting>();
					var appSettingsObj = JsonConvert.DeserializeObject(appSettingsString);
					foreach (var prop in JsonConvert.DeserializeObject(appSettingsString).GetType().GetProperties())
					{
						if (prop.Name == nameof(JToken.First) && prop.PropertyType.Name == nameof(JToken))
						{
							var token = (JToken)prop.GetValue(appSettingsObj);
							while (token != null)
							{
								if (token is JProperty castProp)
									appSettings.Add(new AppSetting { Key = castProp.Name, Value = castProp.Value.ToString() });

								token = token.Next;
							}
						}
					}
				}
				_cache.SetValue(AppCache.AppCacheKeys.AppSettings.ToString(), appSettings);
			}

			foreach (var appSetting in appSettings)
			{
				AppSetting editedAppSetting = editedAppSettings.FirstOrDefault(x => x.Key == appSetting.Key);
				if (editedAppSetting != null)
					appSetting.Value = editedAppSetting.Value;
			}

			return appSettings;
		}
		public AppSetting GetAppSetting(string key)
		{
			return GetAppSettings().FirstOrDefault(x => x.Key == key);
		}
		public AppSetting GetAppSetting(AppSettingsKeys key)
		{
			return GetAppSetting(key.ToString());
		}
		public string GetAppSettingValue(string key)
		{
			return GetAppSetting(key).Value;
		}
		public string GetAppSettingValue(AppSettingsKeys key)
		{
			return GetAppSetting(key.ToString()).Value;
		}
		public void Invalidate()
		{
			_cache.Remove(AppCache.AppCacheKeys.EditedAppSettings.ToString());

			List<AppSetting> editedAppSettings = null;
			editedAppSettings = _dataProvider.GetAppSettings();
			_cache.SetValue(AppCache.AppCacheKeys.EditedAppSettings.ToString(), editedAppSettings);
		}

		public enum AppSettingsKeys
		{
			StyleSkinFileUrl,
			GMapsAPIKey,
			GMapsAPIZoom,
			GMapsAPIMapType,
			GMapsAPIEnableMousewheel,
			Email
		}
	}
}
