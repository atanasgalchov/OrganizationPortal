using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using OrganizationPortal.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace OrganizationPortal
{
	public interface IResources 
	{
		bool IsExistResources(string key);
		List<AppResource> GetAppResources();
		AppResource GetAppResource(string key);
		AppResource GetAppResource(Resources.AppResourcesKeys key);
		string GetAppResourcesValue(string key);
		string GetAppResourcesValue(Resources.AppResourcesKeys key);
		void Invalidate();
	}

	public class Resources : IResources
	{
		private readonly IStringLocalizer<Resources> _localizer;
		private IAppCache _cache;
		private IDataProvider _dataProvider;

		public Resources(IStringLocalizer<Resources> localizer, IAppCache cache, IDataProvider dataProvider) 
		{
			_localizer = localizer;
			_cache = cache;
			_dataProvider = dataProvider;
		}
	
		public bool IsExistResources(string key)
		{
			return !_localizer[key].ResourceNotFound;
		}
		public List<AppResource> GetAppResources()
		{
			string culture = CultureInfo.CurrentCulture.Name;

			List<AppResource> editedAppResources = (List<AppResource>)_cache.GetValue(AppCache.AppCacheKeys.EditedAppResources.ToString());
			if (editedAppResources == null) 
			{
				editedAppResources = _dataProvider.GetAppResources();
				_cache.SetValue(AppCache.AppCacheKeys.EditedAppResources.ToString(), editedAppResources);
			}

			List<AppResource> appResources = Enum.GetValues(typeof(AppResourcesKeys)).OfType<AppResourcesKeys>()
				.Where(x => IsExistResources(x.ToString()))
				.Select(x => new AppResource() { Key = x.ToString(), Value = GetString(x.ToString()), DefaultValue = GetString(x.ToString()) })
				.ToList();

			foreach (var appResource in appResources)
			{
				AppResource editedAppResource = editedAppResources.FirstOrDefault(x => x.Key == appResource.Key);
				if (editedAppResource != null)
					appResource.Value = editedAppResource.Value;
			}

			return appResources;
		}
		public AppResource GetAppResource(string key)
		{
			return GetAppResources().FirstOrDefault(x => x.Key == key);
		}

		public AppResource GetAppResource(AppResourcesKeys key)
		{
			return GetAppResource(key.ToString());
		}
		public string GetAppResourcesValue(string key)
		{
			return GetAppResource(key).Value;
		}
		public string GetAppResourcesValue(AppResourcesKeys key)
		{
			return GetAppResourcesValue(key.ToString());
		}
		public void Invalidate() 
		{
			_cache.Remove(AppCache.AppCacheKeys.EditedAppResources.ToString());

			List<AppResource> editedAppResources = null;
			editedAppResources = _dataProvider.GetAppResources();
			_cache.SetValue(AppCache.AppCacheKeys.EditedAppResources.ToString(), editedAppResources);
		}

		private string GetString(string name) => _localizer[name];

		public enum AppResourcesKeys 
		{
			AppName,
			AppPrivacyPolicyText,
			AppPrivacyPolicyUserConfirmationText,
			AboutText,
			HistoryText,
			Address,
			City,
			Cordinates, 
			Country,
			Description, 
			PhoneNumber,
			HowComeTo, 
			TimeZome,
			ZIPCode,
			HeaderText,
			Population,
			Municipal,
			County,
			Area,
			FacebookPageLink,
			EmailRegistrationConfirmSubject,
			EmailRegistrationConfirmContent,
			EmailRegistrationConfirmEmailContent,
			SearchResultErrorMessage,
			// Admin
			AdminWelcomeText
		}	
	}
}
