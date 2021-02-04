using AutoMapper;
using AutoMapper.QueryableExtensions;
using OrganizationPortal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection;

namespace OrganizationPortal.Data
{

	public interface IDataProvider
	{
		List<T> GetUsers<T>(bool includeRoles = false);
		List<T> GetUsers<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null, bool includeRoles = true);
		T GetUser<T>(string id);
		OrgUser GetUserByUserName(string userName);
		OrgUser UpdateUser(OrgUser model);
		//OrgUser AddUserRole(string userId, List<UserRole> Roles);
		int DeleteUser(string id);
		
		List<OrgRole> GetRoles();
		OrgRole SaveRole(OrgRole model);
		int DeleteRole(string id);
		
		List<T> GetEvents<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null);
		T GetEvent<T>(int id);
		Event SaveEvent(Event model);
		int DeleteEvent(int id);
		
		List<T> GetNotices<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null);
		T GetNotice<T>(int id);
		Notice SaveNotice(Notice model);
		int DeleteNotice(int id);
		
		List<T> GetLocations<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null);
		List<T> GetLocations<T>();
		T GetLocation<T>(int id);
		Location SaveLocation(Location model);
		int DeleteLocation(int id);
		
		List<T> GetCategories<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null);
		List<T> GetCategories<T>();
		T GetCategory<T>(int id);
		Category SaveCategory(Category model);
		int DeleteCategory(int id);
		
		List<T> GetNews<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null);
		T GetNews<T>(int id);
		News SaveNews(News model);
		int DeleteNews(int id);
		
		List<T> GetAppResources<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null);
		List<AppResource> GetAppResources();
		T GetAppResource<T>(string key);
		AppResource SaveAppResource(AppResource model);

		List<T> GetAppSettings<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null);
		List<AppSetting> GetAppSettings();
		T GetAppSetting<T>(string key);
		AppSetting SaveAppSetting(AppSetting model);

		List<T> GetPhoneNumbers<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null);
		List<T> GetPhoneNumbers<T>();
		T GetPhoneNumber<T>(int id);
		PhoneNumber SavePhoneNumber(PhoneNumber model);
		int DeletePhoneNumber(int id);

		List<T> GetDocuments<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null);
		List<T> GetDocuments<T>();
		T GetDocument<T>(int id);
		Document SaveDocument(Document model);
		int DeleteDocument(int id);

		List<T> GetPhotos<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null);
		List<T> GetPhotos<T>();
		T GetPhoto<T>(int id);
		Photo SavePhoto(Photo model);
		int DeletePhoto(int id);

		List<T> GetAlbums<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null, bool includePhotos = false);
		List<T> GetAlbums<T>(bool includePhotos = false);
		T GetAlbum<T>(int id);
		Album SaveAlbum(Album model);
		int DeletAlbum(int id);

		List<T> GetHalls<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null);
		List<T> GetHalls<T>();
		T GetHall<T>(int id);
		Hall SaveHall(Hall model);
		int DeleteHall(int id);

		void AddVisitorLog(VisitorLog log);
		List<VisitorLog> GetVisitorLogs(out int totalRecords, out int totalFilteredRecords, DbParameters<VisitorLogFilters> parameters = null);

		List<VisitorsGroupedByDateModel> GetAgregatedVisitorsDates(DateTime startDate, DateTime endDate);
		void Dispose();
	}

	public class DataProvider : IDataProvider
	{
		private DbContext context;
		public DataProvider(DbContext context)
		{
			this.context = context;
		}

		// Methods

		// Users
		public List<T> GetUsers<T>(bool includeRoles = false)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;
			var parameters = new DbParameters<T>();
			return GetUsers<T>(out totalRecords, out totalFilteredRecords, parameters, includeRoles);
		}
		public List<T> GetUsers<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null, bool includeRoles = true)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<OrgUser> result = context.OrgUsers;

			if (includeRoles)
				result.Include(user => user.UserRoles).ThenInclude(role => role.User);

			totalRecords = result.Count();
			// filter
			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((string)filters.GetType().GetProperty("Id").GetValue(filters) != null)
					// filter
					result = result.Where(x => x.Id.Equals((string)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("UserName").GetValue(filters) != null && (string)filters.GetType().GetProperty("UserName").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.UserName.Contains((string)filters.GetType().GetProperty("UserName").GetValue(filters)));
			}

			totalFilteredRecords = result.Count();

			if (parameters.SortBy == null)
				result = result.OrderBy(x => x.UserName);

			return Mapper.Map<List<T>>(result
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public T GetUser<T>(string id)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Id").SetValue(filters, id);

			return GetUsers<T>(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }, true).FirstOrDefault();
		}
		public OrgUser GetUserByUserName(string userName)
		{
			// TODO USE APP Session for preventing get user on every request
			return context.OrgUsers
				.Include(user => user.UserRoles)
				.ThenInclude(role => role.Role)
				.FirstOrDefault(x => x.UserName == userName);
		}
		public OrgUser UpdateUser(OrgUser model)
		{
			// get db model
			OrgUser dbModel = this.context.OrgUsers.Find(model.Id);

			// set new values
			dbModel.UserName = model.UserName;
			dbModel.EmailConfirmed = model.EmailConfirmed;
			dbModel.Address = model.Address;
			dbModel.PhoneNumber = model.PhoneNumber;
			dbModel.ProfilePictureUrl = model.ProfilePictureUrl;
			dbModel.Email = model.Email;
			dbModel.FirstName = model.FirstName;
			dbModel.LastName = model.LastName;

			context.Entry(dbModel).State = EntityState.Modified;

			// save change
			this.context.SaveChanges();

			//if (model.UserRoles != null)
			//	AddUserRole(dbModel.Id, model.UserRoles);

			return GetUser<OrgUser>(model.Id);
		}

		public int DeleteUser(string id)
		{
			OrgUser curentUser = context.OrgUsers.Find(id);
			context.OrgUsers.Remove(curentUser);

			return context.SaveChanges();
		}

		// Roles
		public List<OrgRole> GetRoles()
		{
			List<OrgRole> roles = context.OrgRoles.Include(x => x.UserRoles).ThenInclude(x => x.User).ToList();

			return roles;
		}
		public OrgRole SaveRole(OrgRole model)
		{
			// get db model
			OrgRole dbModel = this.context.OrgRoles.FirstOrDefault(x => x.Id == model.Id);

			if (dbModel == null)
			{
				model.Id = Guid.NewGuid().ToString();
				this.context.OrgRoles.Add(model);
			}
			else
			{
				// set new values
				dbModel.Name = model.Name;

				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetRoles().FirstOrDefault(x => x.Id == model.Id);
		}
		public int DeleteRole(string id)
		{
			OrgRole currentRole = context.OrgRoles.Find(id);
			context.OrgRoles.Remove(currentRole);

			return context.SaveChanges();
		}

		// Events
		public List<T> GetEvents<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<Event> result = context.Events
				.Include(x => x.Location)
				.Include(x => x.Category)
				.Include(x => x.User);

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Name").GetValue(filters) != null && (string)filters.GetType().GetProperty("Name").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Name.Contains((string)filters.GetType().GetProperty("Name").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Title").GetValue(filters) != null && (string)filters.GetType().GetProperty("Title").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Title.Contains((string)filters.GetType().GetProperty("Title").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Content").GetValue(filters) != null && (string)filters.GetType().GetProperty("Content").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Content.Contains((string)filters.GetType().GetProperty("Content").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("UserId").GetValue(filters) != null && (string)filters.GetType().GetProperty("UserId").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.UserId.Equals((string)filters.GetType().GetProperty("UserId").GetValue(filters)));
				if ((DateTime?)filters.GetType().GetProperty("StartDate").GetValue(filters) != null)
				{
					if (parameters.FiltersOperators.ContainsKey("StartDate"))
					{
						if (parameters.FiltersOperators["StartDate"] == FilterOperators.GtEq)
							result = result.Where(x => x.StartDate >= (DateTime)filters.GetType().GetProperty("StartDate").GetValue(filters));
					}
					else
					{
						result = result.Where(x => x.StartDate == (DateTime)filters.GetType().GetProperty("StartDate").GetValue(filters));
					}
				}
				if ((int?)filters.GetType().GetProperty("CategoryId").GetValue(filters) != null && (int?)filters.GetType().GetProperty("CategoryId").GetValue(filters) != 0)
					result = result.Where(x => x.CategoryId == (int)filters.GetType().GetProperty("CategoryId").GetValue(filters));

			}

			totalFilteredRecords = result.Count();

			if (parameters.SortBy == null)
				result = parameters.SortOrder == "asc" ? result.OrderBy(x => x.PublishedDate) : result.OrderByDescending(x => x.PublishedDate);
			if (parameters.SortBy == "StartDate")
				result = parameters.SortOrder == "asc" ? result.OrderBy(x => x.StartDate) : result.OrderByDescending(x => x.StartDate);

			return Mapper.Map<List<T>>(result.ToList()
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public T GetEvent<T>(int id)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Id").SetValue(filters, id);

			return GetEvents(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }).FirstOrDefault();
		}
		public Event SaveEvent(Event model)
		{
			if (model.Id == 0)
			{
				model.PublishedDate = DateTime.Now;
				context.Events.Add(model);
			}
			else
			{
				// get db model
				Event dbModel = this.context.Events.Find(model.Id);

				// keep props
				model.PublishedDate = dbModel.PublishedDate;
				model.ModifyDate = DateTime.Now;

				// set new values
				dbModel.Name = model.Name;
				dbModel.LocationId = model.LocationId;
				dbModel.Title = model.Title != null && model.Title != String.Empty ? model.Title : model.Name;
				dbModel.Content = model.Content;
				dbModel.MainPictureUrl = model.MainPictureUrl;
				dbModel.EventProgramPictureUrl = model.EventProgramPictureUrl;
				dbModel.StartDate = model.StartDate;
				dbModel.TicketPrice = model.TicketPrice;
				dbModel.CategoryId = model.CategoryId;

				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetEvent<Event>(model.Id);
		}
		public int DeleteEvent(int id)
		{
			Event curentEvent = context.Events.FirstOrDefault(x => x.Id == id);
			//context.Contents.Remove(currentContent);
			context.Events.Remove(curentEvent);

			return context.SaveChanges();
		}

		// Notices
		public List<T> GetNotices<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<Notice> result = context.Notices
				.Include(x => x.Category)
				.Include(x => x.User);

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Name").GetValue(filters) != null && (string)filters.GetType().GetProperty("Name").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Name.Contains((string)filters.GetType().GetProperty("Name").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Title").GetValue(filters) != null && (string)filters.GetType().GetProperty("Title").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Title.Contains((string)filters.GetType().GetProperty("Title").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Content").GetValue(filters) != null && (string)filters.GetType().GetProperty("Content").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Content.Contains((string)filters.GetType().GetProperty("Content").GetValue(filters)));
				if ((int?)filters.GetType().GetProperty("CategoryId").GetValue(filters) != null)
					// filter
					result = result.Where(x => x.CategoryId.Equals((int)filters.GetType().GetProperty("CategoryId").GetValue(filters)));
			}

			totalFilteredRecords = result.Count();

			if (parameters.SortBy == null)
				result = parameters.SortOrder == "asc" ? result.OrderBy(x => x.PublishedDate) : result.OrderByDescending(x => x.PublishedDate);
			else
				result = parameters.SortOrder == "asc" ? result.OrderBy(x => parameters.SortBy) : result.OrderByDescending(x => parameters.SortBy);

			return Mapper.Map<List<T>>(result
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public T GetNotice<T>(int id)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Id").SetValue(filters, id);

			return GetNotices(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }).FirstOrDefault();
		}
		public Notice SaveNotice(Notice model)
		{
			if (model.Id == 0)
			{
				model.PublishedDate = DateTime.Now;
				context.Notices.Add(model);
			}
			else
			{
				// get db model
				Notice dbModel = this.context.Notices.Find(model.Id);

				// keep props
				model.PublishedDate = dbModel.PublishedDate;
				model.ModifyDate = DateTime.Now;

				// set new values
				dbModel.Name = model.Name;
				dbModel.Title = model.Title;
				dbModel.Content = model.Content;
				dbModel.CategoryId = model.CategoryId;

				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetNotice<Notice>(model.Id);
		}
		public int DeleteNotice(int id)
		{
			Notice curentEvent = context.Notices.FirstOrDefault(x => x.Id == id);
			//context.Contents.Remove(currentContent);
			context.Notices.Remove(curentEvent);

			return context.SaveChanges();
		}

		// Locaions
		public List<T> GetLocations<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<Location> result = context.Locations;

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Name").GetValue(filters) != null && (string)filters.GetType().GetProperty("Name").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Name.Contains((string)filters.GetType().GetProperty("Name").GetValue(filters)));
			}

			totalFilteredRecords = result.Count();

			return Mapper.Map<List<T>>(result
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public List<T> GetLocations<T>()
		{
			return context.Locations.ProjectTo<T>().ToList();
		}
		public T GetLocation<T>(int id)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Id").SetValue(filters, id);

			return GetLocations<T>(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }).FirstOrDefault();
		}
		public Location SaveLocation(Location model)
		{
			if (model.Id == 0)
			{
				context.Locations.Add(model);
			}
			else
			{
				// get db model
				Location dbModel = this.context.Locations.Find(model.Id);

				// set new values
				dbModel.Name = model.Name;
				dbModel.Type = model.Type;
				dbModel.Lang = model.Lang;
				dbModel.Lat = model.Lat;
				dbModel.Address = model.Address;

				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetLocation<Location>(model.Id);
		}
		public int DeleteLocation(int id)
		{
			Location entity = context.Locations.FirstOrDefault(x => x.Id == id);
			context.Locations.Remove(entity);

			return context.SaveChanges();
		}

		// Categories
		public List<T> GetCategories<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<Category> result = context.Categories;

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Name").GetValue(filters) != null && (string)filters.GetType().GetProperty("Name").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Name.Contains((string)filters.GetType().GetProperty("Name").GetValue(filters)));
			}

			totalFilteredRecords = result.Count();

			return Mapper.Map<List<T>>(result
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public List<T> GetCategories<T>()
		{
			return context.Categories.ProjectTo<T>().ToList();
		}
		public T GetCategory<T>(int id)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Id").SetValue(filters, id);

			return GetCategories<T>(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }).FirstOrDefault();
		}
		public Category SaveCategory(Category model)
		{
			if (model.Id == 0)
			{
				context.Categories.Add(model);
			}
			else
			{
				// get db model
				Category dbModel = this.context.Categories.Find(model.Id);

				// set new values
				dbModel.Name = model.Name;

				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetCategory<Category>(model.Id);
		}
		public int DeleteCategory(int id)
		{
			Category entity = context.Categories.FirstOrDefault(x => x.Id == id);
			context.Categories.Remove(entity);

			return context.SaveChanges();
		}

		// News
		public List<T> GetNews<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<News> result = context.News
				.Include(x => x.Category)
				.Include(x => x.User);

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Name").GetValue(filters) != null && (string)filters.GetType().GetProperty("Name").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Name.Contains((string)filters.GetType().GetProperty("Name").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Title").GetValue(filters) != null && (string)filters.GetType().GetProperty("Title").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Title.Contains((string)filters.GetType().GetProperty("Title").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Content").GetValue(filters) != null && (string)filters.GetType().GetProperty("Content").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Content.Contains((string)filters.GetType().GetProperty("Content").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("UserId").GetValue(filters) != null && (string)filters.GetType().GetProperty("UserId").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.UserId.Equals((string)filters.GetType().GetProperty("UserId").GetValue(filters)));
				if ((DateTime?)filters.GetType().GetProperty("PublishedDate").GetValue(filters) != null)
				{
					if (parameters.FiltersOperators.ContainsKey("PublishedDate"))
					{
						if (parameters.FiltersOperators["PublishedDate"] == FilterOperators.GtEq)
							result = result.Where(x => x.PublishedDate >= (DateTime)filters.GetType().GetProperty("PublishedDate").GetValue(filters));
						if (parameters.FiltersOperators["PublishedDate"] == FilterOperators.LtEq)
							result = result.Where(x => x.PublishedDate <= (DateTime)filters.GetType().GetProperty("PublishedDate").GetValue(filters));
						if (parameters.FiltersOperators["PublishedDate"] == FilterOperators.Gt)
							result = result.Where(x => x.PublishedDate > (DateTime)filters.GetType().GetProperty("PublishedDate").GetValue(filters));
						if (parameters.FiltersOperators["PublishedDate"] == FilterOperators.Lt)
							result = result.Where(x => x.PublishedDate < (DateTime)filters.GetType().GetProperty("PublishedDate").GetValue(filters));
					}
					else
					{
						result = result.Where(x => x.PublishedDate == (DateTime)filters.GetType().GetProperty("PublishedDate").GetValue(filters));
					}
				}
				if ((int?)filters.GetType().GetProperty("CategoryId").GetValue(filters) != null && (int?)filters.GetType().GetProperty("CategoryId").GetValue(filters) != 0)
					result = result.Where(x => x.CategoryId == (int)filters.GetType().GetProperty("CategoryId").GetValue(filters));

			}

			totalFilteredRecords = result.Count();

			if (parameters.SortBy == null)
				result = parameters.SortOrder == "asc" ? result.OrderBy(x => x.PublishedDate) : result.OrderByDescending(x => x.PublishedDate);

			return Mapper.Map<List<T>>(result
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public T GetNews<T>(int id)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Id").SetValue(filters, id);

			return GetNews(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }).FirstOrDefault();
		}
		public News SaveNews(News model)
		{
			if (model.Id == 0)
			{
				model.PublishedDate = DateTime.Now;
				context.News.Add(model);
			}
			else
			{
				// get db model
				News dbModel = this.context.News.Find(model.Id);

				// keep props
				model.PublishedDate = dbModel.PublishedDate;
				model.ModifyDate = DateTime.Now;

				// set new values
				dbModel.Name = model.Name;
				dbModel.Title = model.Title;
				dbModel.Content = model.Content;
				dbModel.MainPictureUrl = model.MainPictureUrl;
				dbModel.CategoryId = model.CategoryId;

				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetNews<News>(model.Id);
		}
		public int DeleteNews(int id)
		{
			News curentNews = context.News.FirstOrDefault(x => x.Id == id);
			context.News.Remove(curentNews);

			return context.SaveChanges();
		}

		// App Resources
		public List<T> GetAppResources<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<AppResource> result = context.AppResources;

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Key").GetValue(filters) != null && (string)filters.GetType().GetProperty("Key").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Key.Contains((string)filters.GetType().GetProperty("Key").GetValue(filters)));
			}

			totalFilteredRecords = result.Count();

			return Mapper.Map<List<T>>(result
				.ToList()
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public List<AppResource> GetAppResources()
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;
			return GetAppResources<AppResource>(out totalRecords, out totalFilteredRecords);
		}
		public T GetAppResource<T>(string key)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Key").SetValue(filters, key);

			return GetAppResources(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }).FirstOrDefault();
		}
		public AppResource SaveAppResource(AppResource model)
		{

			// get db model
			AppResource dbModel = this.context.AppResources.FirstOrDefault(x => x.Key == model.Key);

			if (dbModel == null)
			{
				this.context.AppResources.Add(model);
			}
			else
			{
				dbModel.Value = model.Value;
				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetAppResource<AppResource>(model.Key);
		}

		// App Settings
		public List<T> GetAppSettings<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<AppSetting> result = context.AppSettings;

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Key").GetValue(filters) != null && (string)filters.GetType().GetProperty("Key").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Key.Contains((string)filters.GetType().GetProperty("Key").GetValue(filters)));
			}

			totalFilteredRecords = result.Count();

			return Mapper.Map<List<T>>(result
				.ToList()
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public List<AppSetting> GetAppSettings()
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;
			return GetAppSettings<AppSetting>(out totalRecords, out totalFilteredRecords);
		}
		public T GetAppSetting<T>(string key)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Key").SetValue(filters, key);

			return GetAppSettings(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }).FirstOrDefault();
		}
		public AppSetting SaveAppSetting(AppSetting model)
		{
			// get db model
			AppSetting dbModel = this.context.AppSettings.FirstOrDefault(x => x.Key == model.Key);

			if (dbModel == null)
			{
				this.context.AppSettings.Add(model);
			}
			else
			{
				dbModel.Value = model.Value;
				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetAppSetting<AppSetting>(model.Key);
		}

		// Phone Numbers
		public List<T> GetPhoneNumbers<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<PhoneNumber> result = context.PhoneNumbers;

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Name").GetValue(filters) != null && (string)filters.GetType().GetProperty("Name").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Name.Contains((string)filters.GetType().GetProperty("Name").GetValue(filters)));
			}

			totalFilteredRecords = result.Count();

			return Mapper.Map<List<T>>(result
				.ToList()
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public List<T> GetPhoneNumbers<T>()
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;
			return GetPhoneNumbers<T>(out totalRecords, out totalFilteredRecords);
		}
		public T GetPhoneNumber<T>(int id)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Id").SetValue(filters, id);

			return GetPhoneNumbers(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }).FirstOrDefault();
		}
		public PhoneNumber SavePhoneNumber(PhoneNumber model)
		{
			if (model.Id == 0)
			{
				context.PhoneNumbers.Add(model);
			}
			else
			{
				// get db model
				PhoneNumber dbModel = this.context.PhoneNumbers.Find(model.Id);

				// set new values
				dbModel.Name = model.Name;
				dbModel.Description = model.Description;
				dbModel.Number = model.Number;

				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetPhoneNumber<PhoneNumber>(model.Id);
		}
		public int DeletePhoneNumber(int id)
		{
			PhoneNumber entity = context.PhoneNumbers.FirstOrDefault(x => x.Id == id);
			context.PhoneNumbers.Remove(entity);

			return context.SaveChanges();
		}

		// Documents
		public List<T> GetDocuments<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<Document> result = context.Documents.Include(x => x.Category);

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Name").GetValue(filters) != null && (string)filters.GetType().GetProperty("Name").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Name.Contains((string)filters.GetType().GetProperty("Name").GetValue(filters)));
				if ((int?)filters.GetType().GetProperty("CategoryId").GetValue(filters) != null && (int?)filters.GetType().GetProperty("CategoryId").GetValue(filters) > 0)
					// filter
					result = result.Where(x => x.CategoryId == (int?)filters.GetType().GetProperty("CategoryId").GetValue(filters));
			}

			if (parameters.SortBy == null)
				result = parameters.SortOrder == "asc" ? result.OrderBy(x => x.UploadedOn) : result.OrderByDescending(x => x.UploadedOn);

			totalFilteredRecords = result.Count();

			return Mapper.Map<List<T>>(result
				.ToList()
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public List<T> GetDocuments<T>()
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;
			return GetDocuments<T>(out totalRecords, out totalFilteredRecords);
		}
		public T GetDocument<T>(int id)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Id").SetValue(filters, id);

			return GetDocuments(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }).FirstOrDefault();
		}
		public Document SaveDocument(Document model)
		{
			if (model.Id == 0)
			{
				model.UploadedOn = DateTime.Now;
				context.Documents.Add(model);
			}
			else
			{
				// get db model
				Document dbModel = this.context.Documents.Find(model.Id);

				// set new values
				dbModel.UploadedOn = DateTime.Now;
				dbModel.Name = model.Name;
				dbModel.Description = model.Description;
				dbModel.FileUrl = model.FileUrl;
				dbModel.CategoryId = model.CategoryId;

				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetDocument<Document>(model.Id);
		}
		public int DeleteDocument(int id)
		{
			Document entity = context.Documents.FirstOrDefault(x => x.Id == id);
			context.Documents.Remove(entity);

			return context.SaveChanges();
		}

		// Photos
		public List<T> GetPhotos<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<Photo> result = context.Photos.Include(x => x.Album);

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Name").GetValue(filters) != null && (string)filters.GetType().GetProperty("Name").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Name.Contains((string)filters.GetType().GetProperty("Name").GetValue(filters)));
				if ((int?)filters.GetType().GetProperty("AlbumId").GetValue(filters) != null)
					// filter
					result = result.Where(x => x.AlbumId == (int)filters.GetType().GetProperty("AlbumId").GetValue(filters));
			}

			if (parameters.SortBy == null)
				result = parameters.SortOrder == "asc" ? result.OrderBy(x => x.UploadedOn) : result.OrderByDescending(x => x.UploadedOn);

			totalFilteredRecords = result.Count();

			return Mapper.Map<List<T>>(result
				.ToList()
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public List<T> GetPhotos<T>()
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;
			return GetPhotos<T>(out totalRecords, out totalFilteredRecords);
		}
		public T GetPhoto<T>(int id)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Id").SetValue(filters, id);

			return GetPhotos(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }).FirstOrDefault();
		}
		public Photo SavePhoto(Photo model)
		{
			if (model.Id == 0)
			{
				model.UploadedOn = DateTime.Now;
				context.Photos.Add(model);
			}
			else
			{
				// get db model
				Photo dbModel = this.context.Photos.Find(model.Id);

				// set new values
				dbModel.UploadedOn = DateTime.Now;
				dbModel.Name = model.Name != null && model.Name != String.Empty ? model.Name : model.FileUrl;
				dbModel.Description = model.Description;
				dbModel.FileUrl = model.FileUrl;
				dbModel.AlbumId = model.AlbumId;

				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetPhoto<Photo>(model.Id);
		}
		public int DeletePhoto(int id)
		{
			Photo entity = context.Photos.FirstOrDefault(x => x.Id == id);
			context.Photos.Remove(entity);

			return context.SaveChanges();
		}

		// Albums

		public List<T> GetAlbums<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null, bool includePhotos = false)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<Album> result = context.Albums.Include(x => x.Category);

			if (includePhotos == true)
				result = result.Include(x => x.Photos);

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Name").GetValue(filters) != null && (string)filters.GetType().GetProperty("Name").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Name.Contains((string)filters.GetType().GetProperty("Name").GetValue(filters)));
				if ((int?)filters.GetType().GetProperty("CategoryId").GetValue(filters) != null && (int?)filters.GetType().GetProperty("CategoryId").GetValue(filters) != 0)
					result = result.Where(x => x.CategoryId == (int)filters.GetType().GetProperty("CategoryId").GetValue(filters));
			}

			totalFilteredRecords = result.Count();

			if (parameters.SortBy == null)
				result = parameters.SortOrder == "asc" ? result.OrderBy(x => x.CreateOn) : result.OrderByDescending(x => x.CreateOn);

			return Mapper.Map<List<T>>(result
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public List<T> GetAlbums<T>(bool includePhotos = false)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			return GetAlbums<T>(out totalRecords, out totalFilteredRecords, new DbParameters<T>(), includePhotos);
		}
		public T GetAlbum<T>(int id)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Id").SetValue(filters, id);

			return GetAlbums<T>(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }, true).FirstOrDefault();
		}
		public Album SaveAlbum(Album model)
		{
			if (model.Id == 0)
			{
				model.CreateOn = DateTime.Now;
				context.Albums.Add(model);
			}
			else
			{
				// get db model
				Album dbModel = this.context.Albums.Find(model.Id);

				// set new values
				dbModel.Name = model.Name;
				dbModel.CategoryId = model.CategoryId;

				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetAlbum<Album>(model.Id);
		}
		public int DeletAlbum(int id)
		{
			Album entity = context.Albums.FirstOrDefault(x => x.Id == id);
			context.Albums.Remove(entity);

			return context.SaveChanges();
		}

		// Halls

		public List<T> GetHalls<T>(out int totalRecords, out int totalFilteredRecords, DbParameters<T> parameters = null)
		{
			parameters = parameters ?? new DbParameters<T>();

			IQueryable<Hall> result = context.Halls;

			totalRecords = result.Count();

			T filters = parameters.Filters;
			if (filters != null)
			{
				if ((int)filters.GetType().GetProperty("Id").GetValue(filters) != 0)
					// filter
					result = result.Where(x => x.Id.Equals((int)filters.GetType().GetProperty("Id").GetValue(filters)));
				if ((string)filters.GetType().GetProperty("Name").GetValue(filters) != null && (string)filters.GetType().GetProperty("Name").GetValue(filters) != "")
					// filter
					result = result.Where(x => x.Name.Contains((string)filters.GetType().GetProperty("Name").GetValue(filters)));
			}

			totalFilteredRecords = result.Count();

			return Mapper.Map<List<T>>(result
				.ToList()
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}
		public List<T> GetHalls<T>()
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;
			return GetHalls<T>(out totalRecords, out totalFilteredRecords);
		}
		public T GetHall<T>(int id)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			var filters = Activator.CreateInstance(typeof(T));
			filters.GetType().GetProperty("Id").SetValue(filters, id);

			return GetHalls(out totalRecords, out totalFilteredRecords, new DbParameters<T> { Filters = (T)filters }).FirstOrDefault();
		}
		public Hall SaveHall(Hall model)
		{
			if (model.Id == 0)
			{
				context.Halls.Add(model);
			}
			else
			{
				// get db model
				Hall dbModel = this.context.Halls.Find(model.Id);

				// set new values
				dbModel.Name = model.Name;
				dbModel.Description = model.Description;
				dbModel.Address = dbModel.Address;
				dbModel.Phone = model.Phone;
				dbModel.Email = model.Email;
				dbModel.PictureUrl = model.PictureUrl;

				context.Entry(dbModel).State = EntityState.Modified;
			}

			// save change
			this.context.SaveChanges();

			return GetHall<Hall>(model.Id);
		}
		public int DeleteHall(int id)
		{
			Hall entity = context.Halls.FirstOrDefault(x => x.Id == id);
			context.Halls.Remove(entity);

			return context.SaveChanges();
		}


		public void Dispose()
		{
			context.Dispose();
		}

		public void RollBack()
		{
			var changedEntries = context.ChangeTracker.Entries()
				.Where(x => x.State != EntityState.Unchanged).ToList();

			foreach (var entry in changedEntries)
			{
				switch (entry.State)
				{
					case EntityState.Modified:
						entry.CurrentValues.SetValues(entry.OriginalValues);
						entry.State = EntityState.Unchanged;
						break;
					case EntityState.Added:
						entry.State = EntityState.Detached;
						break;
					case EntityState.Deleted:
						entry.State = EntityState.Unchanged;
						break;
				}
			}
		}

		public void AddVisitorLog(VisitorLog model)
		{
			RollBack();
			context.VisitorLogs.Add(model);

			// save change
			this.context.SaveChanges();
		}

		public List<VisitorLog> GetVisitorLogs(out int totalRecords, out int totalFilteredRecords, DbParameters<VisitorLogFilters> parameters = null)
		{
			parameters = parameters ?? new DbParameters<VisitorLogFilters>();

			IQueryable<VisitorLog> result = context.VisitorLogs;

			totalRecords = result.Count();

			VisitorLogFilters filters = parameters.Filters;
			if (filters != null)
			{
				if (filters.Id != null)
					result = result.Where(x => x.Id.Equals(filters.Id));
				if (filters.Session != null)
					result = result.Where(x => x.Session.Equals(filters.Session));
				if (filters.Url != null)
					result = result.Where(x => x.Url.Contains(filters.Url));
				if (filters.Ip != null)
					result = result.Where(x => x.Ip.Equals(filters.Ip));
				if (filters.StartDate != null)
					result = result.Where(x => x.Date >= filters.StartDate);
				if (filters.EndDate != null)
					result = result.Where(x => x.Date <= filters.EndDate);
			}

			totalFilteredRecords = result.Count();

			if (parameters.SortBy == null)
				result = parameters.SortOrder == "asc" ? result.OrderBy(x => x.Date) : result.OrderByDescending(x => x.Date);

			return Mapper.Map<List<VisitorLog>>(result
				.ToList()
				.Skip(parameters.PageSize * (parameters.PageIndex == 1 || parameters.PageIndex == 0 ? 0 : parameters.PageIndex - 1))
				.Take(parameters.PageSize));
		}

		public List<VisitorsGroupedByDateModel> GetAgregatedVisitorsDates(DateTime startDate, DateTime endDate)
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;
			List<VisitorLog> visitors = GetVisitorLogs(
				out totalRecords,
				out totalFilteredRecords,
				new DbParameters<VisitorLogFilters>
				{
					PageSize = Int32.MaxValue,
					Filters = new VisitorLogFilters { StartDate = startDate, EndDate = endDate }
				});
			int days = endDate.Subtract(startDate).Days;
			string format = days > 1 ? days > 180 ? "MM/yyyy" : days > 1800 ? "yyyy" : "MM/dd/yyyy" : "MM/dd/yyyy hh:mm";
			List<VisitorsGroupedByDateModel> result = visitors
				.GroupBy(x => new
				{
					Date = x.Date.ToString(format)
				})
				.Select(x => new VisitorsGroupedByDateModel { Date = x.Key.Date, Count = x.Count(), GroupedFormat = format })
				.ToList();

			return result;
		}
	}
}