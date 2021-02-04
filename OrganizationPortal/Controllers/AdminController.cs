using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AutoMapper;
using OrganizationPortal.Data;
using OrganizationPortal.Helpers;
using OrganizationPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Wangkanai.Detection;
using System.Transactions;

namespace OrganizationPortal.Controllers
{
	[Authorize]
    public class AdminController : BaseController
    {
		protected RoleManager<OrgRole> _roleManager;

		public AdminController(UserManager<OrgUser> userManager, SignInManager<OrgUser> signInManager, IConfiguration config, IHostingEnvironment hostingEnvironment, IResources resources, IApppSettings appSettings, IDataProvider dataProvider, RoleManager<OrgRole> roleManager, IDetection detection, IAppMailer appMailer) : 
			base(userManager, signInManager, config, hostingEnvironment, resources, appSettings, dataProvider, detection, appMailer)
		{
			_roleManager = roleManager;
		}

		[Auth(new string[] { "Administrator",  "Master" })]
		public IActionResult Index()
        {
			int totalNews = 0;
			int totalFilteredNews = 0;
			List<NewsViewModel> recentNews = DataProvider
				.GetNews<NewsViewModel>(out totalNews, out totalFilteredNews, new DbParameters<NewsViewModel> 
				{ 
					SortOrder = "desc",
					PageSize = 10 
				});

			int totalEvents = 0;
			int totalFilteredEvents = 0;
			List<EventViewModel> recentEvents = DataProvider
				.GetEvents<EventViewModel>(out totalEvents, out totalFilteredEvents, new DbParameters<EventViewModel>
				{
					SortOrder = "desc",
					PageSize = 10,
				});

			IndexViewModel model = new IndexViewModel 
			{
				TotalUsers = DataProvider.GetUsers<OrgUser>().Count(),

				TotalNews = totalNews,
				RecentNews = recentNews,

				TotalEvents = totalEvents,
				RecentEvents = recentEvents,

				
			};

			return View(model);
        }

		// Users
		//[Authorize(Roles = "Administrator")]

		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult MyProfile() 
		{
			OrgUserViewModel model = DataProvider.GetUser<OrgUserViewModel>(Account.Id);
			if (model.ProfilePictureUrl != null)
				model.ProfilePictureUrl = System.IO.Path.Combine(GetImagesFolderPath("Users"), model.ProfilePictureUrl);

			model.UserRolesIds = DataProvider.GetRoles()?.Where(x => x.UserRoles != null && x.UserRoles.Any(y => y.UserId.ToString() == Account.Id))?.Select(x => x.Id).ToList();
			
			int totalCount = 0;
			int totalFilteredCout = 0;
			model.Events = DataProvider.GetEvents<EventViewModel>(out totalCount, out totalFilteredCout, new DbParameters<EventViewModel>() { Filters = new EventViewModel { UserId = model.Id }, SortOrder = "desc", PageSize = 10 });
			model.Events.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Events"), x.MainPictureUrl) : null);
			model.EventsActivityIndex = totalCount != 0 ? ((decimal)totalFilteredCout / (decimal)totalCount) * 100 : 0;

			model.News = DataProvider.GetNews<NewsViewModel>(out totalCount, out totalFilteredCout, new DbParameters<NewsViewModel>() { Filters = new NewsViewModel { UserId = model.Id }, SortOrder = "desc", PageSize = 10 });
			model.News.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("News"), x.MainPictureUrl) : null);
			model.NewsActivityIndex = totalCount != 0 ? ((decimal)totalFilteredCout / (decimal)totalCount) * 100 : 0;

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult ViewProfile(string userName)
		{
			OrgUser entity = Mapper.Map<OrgUser, OrgUser>(DataProvider.GetUserByUserName(userName));

			if (entity == null)
				throw new ArgumentNullException();
			
			OrgUserViewModel model = new OrgUserViewModel();
			
			Mapper.Map<OrgUser, OrgUserViewModel>(entity, model);
			
			if (model.ProfilePictureUrl != null)
				model.ProfilePictureUrl = System.IO.Path.Combine(GetImagesFolderPath("Users"), model.ProfilePictureUrl);

			int totalCount = 0;
			int totalFilteredCout = 0;
			model.Events = DataProvider.GetEvents<EventViewModel>(out totalCount, out totalFilteredCout, new DbParameters<EventViewModel>() { Filters = new EventViewModel { UserId = model.Id }, SortOrder = "desc", PageSize = 10 });
			model.Events.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Events"), x.MainPictureUrl) : null);
			model.EventsActivityIndex = totalCount != 0 ? ((decimal)totalFilteredCout / (decimal)totalCount) * 100 : 0;

			model.News = DataProvider.GetNews<NewsViewModel>(out totalCount, out totalFilteredCout, new DbParameters<NewsViewModel>() { Filters = new NewsViewModel { UserId = model.Id }, SortOrder = "desc", PageSize = 10 });
			model.News.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("News"), x.MainPictureUrl) : null);
			model.NewsActivityIndex = totalCount != 0 ? ((decimal)totalFilteredCout / (decimal)totalCount) * 100 : 0;

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult Users()
		{
			return View();
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult UsersJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<OrgUserViewModel> parameters = new DbParameters<OrgUserViewModel>
			{
				PageSize = request.length,
								PageIndex = request.start / request.length + 1,
				Filters = new OrgUserViewModel
				{
					UserName = request.search,
				}
			};
			List<OrgUserViewModel> model = DataProvider.GetUsers<OrgUserViewModel>(out totalRecords, out totalFilteredRecords, parameters, false);
			model.ForEach(x => x.ProfilePictureUrl = x.ProfilePictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Users"), x.ProfilePictureUrl) : null);

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Master" })]
		public ActionResult EditUser(string id)
		{
			OrgUserViewModel model = DataProvider.GetUser<OrgUserViewModel>(id);
			if (model.ProfilePictureUrl != null)
				model.ProfilePictureUrl = System.IO.Path.Combine(GetImagesFolderPath("Users"), model.ProfilePictureUrl);

			model.UserRolesIds = DataProvider.GetRoles()?.Where(x => x.UserRoles != null && x.UserRoles.Any(y => y.UserId.ToString() == model.Id))?.Select(x => x.Id).ToList();

			int totalCount = 0;
			int totalFilteredCout = 0;
			model.Events = DataProvider.GetEvents<EventViewModel>(out totalCount, out totalFilteredCout, new DbParameters<EventViewModel>() { Filters = new EventViewModel { UserId = model.Id } });
			model.Events.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Events"), x.MainPictureUrl) : null);
			model.EventsActivityIndex = totalCount != 0 ? ((decimal)totalFilteredCout / (decimal)totalCount) * 100 : 0;

			model.News = DataProvider.GetNews<NewsViewModel>(out totalCount, out totalFilteredCout, new DbParameters<NewsViewModel>() { Filters = new NewsViewModel { UserId = model.Id } });
			model.News.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("News"), x.MainPictureUrl) : null);
			model.NewsActivityIndex = totalCount != 0 ? ((decimal)totalFilteredCout / (decimal)totalCount) * 100 : 0;

			return View(model);
		}
		[HttpPost]
		public async Task<JsonResult> EditUser([FromBody] OrgUserViewModel model)
		{
			if(!Account.IsMaster && model.Id != Account.Id)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = "User does not have permission !" });

			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			model.ProfilePictureUrl = model.ProfilePictureUrl == string.Empty ? null : Path.GetFileName(model.ProfilePictureUrl);
			OrgUser entity = Mapper.Map<OrgUser>(model);

			OrgUser existingEntity = Mapper.Map<OrgUser, OrgUser>(DataProvider.GetUserByUserName(model.UserName));
			if (existingEntity != null && !existingEntity.EmailConfirmed && model.EmailConfirmed) 
			{
				EmailMessage emailModel = new EmailMessage(_appSettings.GetAppSettingValue(OrganizationPortal.AppSettings.AppSettingsKeys.Email), existingEntity.Email);
				emailModel.Subject = String.Format(Resources.GetAppResourcesValue(OrganizationPortal.Resources.AppResourcesKeys.EmailRegistrationConfirmSubject), Resources.GetAppResourcesValue(OrganizationPortal.Resources.AppResourcesKeys.AppName));
				emailModel.Body = String
					.Format(
						String.Format(Resources.GetAppResourcesValue(OrganizationPortal.Resources.AppResourcesKeys.EmailRegistrationConfirmEmailContent), Request.Scheme + "://" + Request.Host + "/Admin", Resources.GetAppResourcesValue(OrganizationPortal.Resources.AppResourcesKeys.AppName)),
						existingEntity.FirstName, existingEntity.LastName,
						Resources.GetAppResourcesValue(OrganizationPortal.Resources.AppResourcesKeys.AppName)
					);
				try
				{
					SendEmail(emailModel);
				}
				catch (Exception ex)
				{
					return Json(new DataResult { Type = DataResultTypes.Warning, Message = ex.Message });
				}
			}

			entity = DataProvider.UpdateUser(entity);

			if (model.ProfilePictureBase64 != null)
			{
				entity.ProfilePictureUrl = string.Format("{0}.{1}", entity.Id.ToString(), model.ProfilePictureFileExtension);

				// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
				string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Users");
				AppHelper.SaveFile(model.ProfilePictureBase64, filePath, entity.ProfilePictureUrl);
			}
			if (Account.IsMaster) 
			{
				entity = DataProvider.GetUserByUserName(entity.UserName);
				IEnumerable<string> userRolesToAdd = model.UserRolesIds?.Select(x => DataProvider.GetRoles().FirstOrDefault(role => role.Id == x)?.Name);
				if (userRolesToAdd != null)
				{
					IdentityResult result = null;
					if (entity.UserRoles != null)
						result = await _userManager.RemoveFromRolesAsync(entity, entity.UserRoles.Select(x => x.Role.Name));

					if (result != null && !result.Succeeded)
						return Json(new DataResult { Type = DataResultTypes.Warning, Message = result.Errors.Select(x => x.Description).Join("") });

					result = await _userManager.AddToRolesAsync(entity, userRolesToAdd);

					if (!result.Succeeded)
						return Json(new DataResult { Type = DataResultTypes.Warning, Message = result.Errors.Select(x => x.Description).Join("") });
				}
			}

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("Users", "Admin") });
		}
		[Auth(new string[] { "Master" })]
		[HttpPost]
		public ActionResult DeleteUser(string id)
		{
			OrgUser entity = this.DataProvider.GetUser<OrgUser>(id);
			if (entity == null)
				return RedirectToAction("Users");

			if (entity.ProfilePictureUrl != null) 
			{
				// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
				string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Users");

				AppHelper.DeleteFile(filePath, entity.ProfilePictureUrl);
			}
				
			DataProvider.DeleteUser(entity.Id);

			return RedirectToAction("Users");
		}
		[HttpPost]
		public ActionResult SaveUserImage(string id, IFormFile image)
		{
			if(!Account.IsMaster && id != Account.Id)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = "User does not have permission !" });

			if (image == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = "Image file does not exist !" });

			OrgUser entity = this.DataProvider.GetUser<OrgUser>(id);
			
			if (entity == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = $"User with #{id} does not exist !" });

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Users");

			if (entity.ProfilePictureUrl != null)
				entity.ProfilePictureUrl = Path.GetFileName(entity.ProfilePictureUrl);

			if (entity.ProfilePictureUrl == null || entity.ProfilePictureUrl == "")
				entity.ProfilePictureUrl = entity.Id + ".png";

			DataResult result = AppHelper.SaveFile(image, filePath, entity.ProfilePictureUrl);
			if(result.IsSuccess)
				DataProvider.UpdateUser(entity);

			return Json(new DataResult() { Data = System.IO.Path.Combine(GetImagesFolderPath("Users"), entity.ProfilePictureUrl), Type = result.Type, Message = result.Message });
		}
		public ActionResult DeleteUserImage(string id)
		{
			if (!Account.IsMaster && id != Account.Id)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = "User does not have permission !" });

			OrgUser entity = this.DataProvider.GetUser<OrgUser>(id);
			if (entity == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = $"User with #{id} does not exist !" });

			if (entity.ProfilePictureUrl == null)
				return Json(new DataResult() { Type = DataResultTypes.Success });

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Users");

			AppHelper.DeleteFile(filePath, entity.ProfilePictureUrl);
			entity.ProfilePictureUrl = null;

			DataProvider.UpdateUser(entity);

			return Json(new DataResult() { Type = DataResultTypes.Success });
		}

		// Roles
		[Auth(new string[] { "Master" })]
		public ActionResult Roles()
		{
			return View();
		}
		[Auth(new string[] { "Master" })]
		public JsonResult RolesJson(DataTableRequestModel request)
		{
			List<OrgRole> model = DataProvider.GetRoles();
			model?.ForEach(x => x.UserRoles = null);
			return Json(new DataTableDataSourceModel { data = model, recordsTotal = model.Count, recordsFiltered = model.Count });
		}
		[Auth(new string[] { "Master" })]
		public ActionResult EditRole(string id)
		{
			OrgRole model = id != null ?
				model = DataProvider.GetRoles().FirstOrDefault(x => x.Id == id) :
				new OrgRole();

			return View(model);
		}
		[Auth(new string[] { "Master" })]
		[HttpPost]
		public JsonResult EditRole([FromBody] OrgRole model)
		{
			if (model.Name == null || model.Name == String.Empty)
				ModelState.AddModelError("Name", "The Role Name cannot be null");

			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			OrgRole existRole = DataProvider.GetRoles().FirstOrDefault(x => x.Id == model.Id);
			if(existRole?.Name == model.Name)
				return Json(new DataResult { Type = DataResultTypes.Warning, Message = $"Role {model.Name} already exist." });

			DataProvider.SaveRole(model);

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("Roles", "Admin") });
		}
		[Auth(new string[] { "Master" })]
		[HttpPost]
		public async Task<JsonResult> AddRole([FromBody] OrgRole model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			OrgRole existRole = DataProvider.GetRoles().FirstOrDefault(x => x.Name == model.Name);
			if (existRole?.Name == model.Name)
				return Json(new DataResult { Type = DataResultTypes.Warning, Message = $"Role {model.Name} already exist." });

			IdentityResult roleResult = await _roleManager.CreateAsync(model);

			if (!roleResult.Succeeded)
				return Json(new DataResult { Type = DataResultTypes.Warning, Message = $"Can't create {model.Name} Role." });

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("Roles", "Admin") });
		}
		[Auth(new string[] { "Master" })]
		[HttpPost]
		public ActionResult DeleteRole(string id)
		{
			OrgRole entity = this.DataProvider.GetRoles().FirstOrDefault(x => x.Id == id);
			if (entity == null)
				return RedirectToAction("Roles");

			DataProvider.DeleteRole(id);

			return RedirectToAction("Roles");
		}

		// Events
		[Auth(new string[] { "Administrator", "Master" })]
		public IActionResult Events()
        {
            return View();
        }
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult EventsJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<EventViewModel> parameters = new DbParameters<EventViewModel>
			{
				PageSize = request.length,
				PageIndex = request.start / request.length + 1,
				Filters = new EventViewModel
				{
					Name = request.search,					
				}
			};
			List<EventViewModel> model = DataProvider.GetEvents(out totalRecords, out totalFilteredRecords, parameters);
			model.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Events"), x.MainPictureUrl) : null);

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult EditEvent(int? id)
		{
			EventViewModel model;
			if (id != null)
			{
				model = Mapper.Map<EventViewModel>(DataProvider.GetEvent<Event>((int)id));
				if (model.MainPictureUrl != null)
					model.MainPictureUrl = System.IO.Path.Combine(GetImagesFolderPath("Events"), model.MainPictureUrl);
			}
			else
			{
				model = new EventViewModel()
				{
					UserId = this.Account.Id
				};
			}

			model.LocationItems = DataProvider
				.GetLocations<Location>()
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = model.LocationId == x.Id })
				.Prepend(new SelectListItem { Text = "-- Select Location --", Value = "", Selected = model.LocationId == 0 })
				.ToList();

			model.CategoriesItems = DataProvider
				.GetCategories<Category>()
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = model.CategoryId == x.Id })
				.Prepend(new SelectListItem { Text = "-- Select Category --", Value = "", Selected = model.CategoryId == 0 })
				.ToList();

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public JsonResult EditEvent([FromBody] EventViewModel model)
		{
			if (model.StartDate < DateTime.Now)
				ModelState.AddModelError("StartDate", "Start Date cannot be less than date now");

			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}


			model.MainPictureUrl = model.MainPictureUrl == string.Empty ? null : Path.GetFileName(model.MainPictureUrl);

			Event entity = Mapper.Map<Event>(model);
			entity = DataProvider.SaveEvent(entity);

			if (model.MainPictureBase64 != null)
			{
				entity.MainPictureUrl = string.Format("{0}.{1}", entity.Id.ToString(), model.MainPictureFileExtension);

				// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
				string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Events");
				AppHelper.SaveFile(model.MainPictureBase64, filePath, entity.MainPictureUrl);
			
				entity = DataProvider.SaveEvent(entity);
			}

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("Events", "Admin") });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult DeleteEvent(int id)
		{
			Event entity = this.DataProvider.GetEvent<Event>(id);
			if (entity == null)
				return RedirectToAction("Events");

			if (entity.MainPictureUrl != null) 
			{
				// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
				string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Events");

				AppHelper.DeleteFile(filePath, entity.MainPictureUrl);
			}
				
			DataProvider.DeleteEvent(id);

			return RedirectToAction("Events");
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult SaveEventImage(int id, IFormFile image)
		{
			if (image == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = "Image file does not exist !" });

			Event entity = this.DataProvider.GetEvent<Event>(id);
			if(entity == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = $"Event with #{id} does not exist !" });

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Events");

			if (entity.MainPictureUrl != null)
				AppHelper.DeleteFile(filePath, entity.MainPictureUrl);

			if (entity.MainPictureUrl == null || entity.MainPictureUrl == "")
				entity.MainPictureUrl = entity.Id + ".png";

			DataResult result =  AppHelper.SaveFile(image, filePath, entity.MainPictureUrl);
			if(result.IsSuccess)
				DataProvider.SaveEvent(entity);

			return Json(new DataResult() { Data = System.IO.Path.Combine(GetImagesFolderPath("Events"), entity.MainPictureUrl), Type = result.Type, Message = result.Message });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult DeleteEventImage(int id)
		{
			Event entity = this.DataProvider.GetEvent<Event>(id);
			if (entity == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = $"Event with #{id} does not exist !" });

			if (entity.MainPictureUrl == null)
				return Json(new DataResult() { Type = DataResultTypes.Success });

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Events");

			AppHelper.DeleteFile(filePath, entity.MainPictureUrl);
			entity.MainPictureUrl = null;
			
			DataProvider.SaveEvent(entity);

			return Json(new DataResult() { Type = DataResultTypes.Success });
		}

		// Notices
		[Auth(new string[] { "Administrator", "Master" })]
		public IActionResult Notices()
		{
			return View();
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult NoticesJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<Notice> parameters = new DbParameters<Notice>
			{
				PageSize = request.length,
								PageIndex = request.start / request.length + 1,
				Filters = new Notice
				{
					Name = request.search,
				}
			};
			List<Notice> model = DataProvider.GetNotices(out totalRecords, out totalFilteredRecords, parameters);

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult EditNotice(int? id)
		{
			NoticeViewModel model;
			if (id != null)
				model = Mapper.Map<NoticeViewModel>(DataProvider.GetNotice<Notice>((int)id));			
			else		
				model = new NoticeViewModel(){ UserId = this.Account.Id };
			
			model.CategoriesItems = DataProvider
				.GetCategories<Category>()
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = model.CategoryId == x.Id })
				.Prepend(new SelectListItem { Text = "-- Select Category --", Value = "", Selected = model.CategoryId == 0 })
				.ToList();

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public JsonResult EditNotice([FromBody] NoticeViewModel model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			Notice entity = Mapper.Map<Notice>(model);
			entity = DataProvider.SaveNotice(entity);

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("Notices", "Admin") });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult DeleteNotice(int id)
		{
			Notice entity = this.DataProvider.GetNotice<Notice>(id);
			if (entity == null)
				return RedirectToAction("Notices");

			DataProvider.DeleteNotice(id);

			return RedirectToAction("Notices");
		}

		// Locations
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult Locations() 
		{
			return View();
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult LocationsJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<LocationViewModel> parameters = new DbParameters<LocationViewModel>
			{
				PageSize = request.length,
								PageIndex = request.start / request.length + 1,
				Filters = new LocationViewModel
				{
					Name = request.search,
				}
			};
			List<LocationViewModel> model = DataProvider.GetLocations(out totalRecords, out totalFilteredRecords, parameters);

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult EditLocation(int? id)
		{
			LocationViewModel model = id != null ? 
				model = Mapper.Map<LocationViewModel>(DataProvider.GetLocation<Location>((int)id)) : 
				new LocationViewModel();

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public JsonResult EditLocation([FromBody] LocationViewModel model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			Location entity = Mapper.Map<Location>(model);
			entity = DataProvider.SaveLocation(entity);

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("Locations", "Admin") });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult DeleteLocation(int id)
		{
			Location entity = this.DataProvider.GetLocation<Location>(id);
			if (entity == null)
				return RedirectToAction("Locations");

			DataProvider.DeleteLocation(id);

			return RedirectToAction("Locations");
		}

		// Categories
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult Categories()
		{
			return View();
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult CategoriesJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<Category> parameters = new DbParameters<Category>
			{
				PageSize = request.length,
								PageIndex = request.start / request.length + 1,
				Filters = new Category
				{
					Name = request.search,
				}
			};
			List<Category> model = DataProvider.GetCategories(out totalRecords, out totalFilteredRecords, parameters);

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult EditCategory(int? id)
		{
			Category model = id != null ?
				model = Mapper.Map<Category>(DataProvider.GetCategory<Category>((int)id)) :
				new Category();

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public JsonResult EditCategory([FromBody] Category model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			Category entity = Mapper.Map<Category>(model);
			entity = DataProvider.SaveCategory(entity);

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("Categories", "Admin") });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult DeleteCategory(int id)
		{
			Category entity = this.DataProvider.GetCategory<Category>(id);
			if (entity == null)
				return RedirectToAction("Categories");

			DataProvider.DeleteCategory(id);

			return RedirectToAction("Categories");
		}

		// News
		[Auth(new string[] { "Administrator", "Master" })]
		public IActionResult News()
		{
			return View();
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult NewsJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<NewsViewModel> parameters = new DbParameters<NewsViewModel>
			{
				PageSize = request.length,
				PageIndex = request.start / request.length + 1,
				Filters = new NewsViewModel
				{
					Title = request.search,
				}
			};
			List<NewsViewModel> model = DataProvider.GetNews(out totalRecords, out totalFilteredRecords, parameters);
			model.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("News"), x.MainPictureUrl) : null);

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult EditNews(int? id)
		{
			NewsViewModel model;
			if (id != null)
			{
				model = Mapper.Map<NewsViewModel>(DataProvider.GetNews<News>((int)id));
				if (model.MainPictureUrl != null)
					model.MainPictureUrl = System.IO.Path.Combine(GetImagesFolderPath("News"), model.MainPictureUrl);
			}
			else
			{
				model = new NewsViewModel()
				{
					UserId = this.Account.Id
				};
			}

			model.CategoriesItems = DataProvider
				.GetCategories<Category>()
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = model.CategoryId == x.Id })
				.Prepend(new SelectListItem { Text = "-- Select Category --", Value = "", Selected = model.CategoryId == 0 })
				.ToList();

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public JsonResult EditNews([FromBody] NewsViewModel model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			model.MainPictureUrl = model.MainPictureUrl == string.Empty ? null : Path.GetFileName(model.MainPictureUrl);

			News entity = Mapper.Map<News>(model);
			entity = DataProvider.SaveNews(entity);

			if (model.MainPictureBase64 != null)
			{
				entity.MainPictureUrl = string.Format("{0}.{1}", entity.Id.ToString(), model.MainPictureFileExtension);

				// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
				string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("News");
				AppHelper.SaveFile(model.MainPictureBase64, filePath, entity.MainPictureUrl);

				entity = DataProvider.SaveNews(entity);
			}

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("News", "Admin") });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult DeleteNews(int id)
		{
			News entity = this.DataProvider.GetNews<News>(id);
			if (entity == null)
				return RedirectToAction("News");

			if (entity.MainPictureUrl != null) 
			{
				// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
				string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("News");

				AppHelper.DeleteFile(filePath, entity.MainPictureUrl);
			}

			DataProvider.DeleteNews(id);

			return RedirectToAction("News");
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult SaveNewsImage(int id, IFormFile image)
		{
			if (image == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = "Image file does not exist !" });

			News entity = this.DataProvider.GetNews<News>(id);
			if (entity == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = $"News with #{id} does not exist !" });

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("News");

			if (entity.MainPictureUrl != null)
				AppHelper.DeleteFile(filePath, entity.MainPictureUrl);

			if (entity.MainPictureUrl == null || entity.MainPictureUrl == "")
				entity.MainPictureUrl = entity.Id + ".png";

			DataResult result = AppHelper.SaveFile(image, filePath, entity.MainPictureUrl);
			if(result.IsSuccess)
				DataProvider.SaveNews(entity);

			return Json(new DataResult() { Data = System.IO.Path.Combine(GetImagesFolderPath("News"), entity.MainPictureUrl), Type = result.Type, Message = result.Message });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult DeleteNewsImage(int id)
		{
			News entity = this.DataProvider.GetNews<News>(id);
			if (entity == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = $"News with #{id} does not exist !" });

			if (entity.MainPictureUrl == null)
				return Json(new DataResult() { Type = DataResultTypes.Success });

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("News");

			AppHelper.DeleteFile(filePath, entity.MainPictureUrl);
			entity.MainPictureUrl = null;

			DataProvider.SaveNews(entity);

			return Json(new DataResult() { Type = DataResultTypes.Success });
		}

		// App Resources
		[Auth(new string[] { "Master" })]
		public IActionResult AppResources()
		{
			return View();
		}
		[Auth(new string[] { "Master" })]
		public JsonResult AppResourcesJson(DataTableRequestModel request)
		{
			
			List<AppResource> resources = _resources.GetAppResources();
			int totalRecords = resources.Count;

			string searchText = Request.Query["search[value]"].ToString();
			if (searchText != null)
				resources = resources.Where(x => x.Key.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

			int totalFilteredRecords = resources.Count;
			
			int pageSize = request.length;
			int pageIndex = request.start / request.length + 1;
			var result = new DataTableDataSourceModel
			{
				data = resources.Skip(pageSize * (pageIndex == 1 || pageIndex == 0 ? 0 : pageIndex - 1)).Take(pageSize), 
				recordsTotal = totalRecords,
				recordsFiltered = totalFilteredRecords 
			};

			return Json(result);
		}
		[Auth(new string[] { "Master" })]
		public ActionResult EditAppResource(string key)
		{
			AppResource model = _resources.GetAppResource(key);

			return View(model);
		}
		[Auth(new string[] { "Master" })]
		[HttpPost]
		public JsonResult EditAppResource([FromBody] AppResource model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}
			
			AppResource entity = DataProvider.SaveAppResource(model);
			
			_resources.Invalidate();
			
			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("AppResources", "Admin") });
		}

		// App Settings
		[Auth(new string[] { "Master" })]
		public IActionResult AppSettings()
		{
			return View();
		}
		[Auth(new string[] { "Master" })]
		public JsonResult AppSettingsJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<AppSetting> parameters = new DbParameters<AppSetting>
			{
				PageSize = request.length,
				PageIndex = request.start / request.length + 1,
				Filters = new AppSetting
				{
					Key = request.search,
				}
			};

			List<AppSetting> model = _appSettings.GetAppSettings();

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Master" })]
		public ActionResult EditAppSetting(string key)
		{
			AppSetting model = _appSettings.GetAppSetting(key);

			return View(model);
		}
		[Auth(new string[] { "Master" })]
		[HttpPost]
		public JsonResult EditAppSetting([FromBody] AppSetting model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			AppSetting entity = DataProvider.SaveAppSetting(model);

			_appSettings.Invalidate();

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("AppSettings", "Admin") });
		}

		// Phone Numbers
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult PhoneNumbers()
		{
			return View();
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult PhoneNumbersJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<PhoneNumber> parameters = new DbParameters<PhoneNumber>
			{
				PageSize = request.length,
				PageIndex = request.start / request.length + 1,
				Filters = new PhoneNumber
				{
					Name = request.search,
				}
			};
			List<PhoneNumber> model = DataProvider.GetPhoneNumbers(out totalRecords, out totalFilteredRecords, parameters);

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult EditPhoneNumber(int? id)
		{
			PhoneNumber model = id != null ?
				model = DataProvider.GetPhoneNumber<PhoneNumber>((int)id) :
				new PhoneNumber();

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public JsonResult EditPhoneNumber([FromBody] PhoneNumber model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			DataProvider.SavePhoneNumber(model);

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("PhoneNumbers", "Admin") });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult DeletePhoneNumber(int id)
		{
			PhoneNumber entity = this.DataProvider.GetPhoneNumber<PhoneNumber>(id);
			if (entity == null)
				return RedirectToAction("PhoneNumbers");

			DataProvider.DeletePhoneNumber(id);

			return RedirectToAction("PhoneNumbers");
		}

		// Documents
		[Auth(new string[] { "Administrator", "Master" })]
		public IActionResult Documents()
		{
			return View();
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult DocumentsJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<Document> parameters = new DbParameters<Document>
			{
				PageSize = request.length,
				PageIndex = request.start / request.length + 1,
				Filters = new Document
				{
					Name = request.search,
				}
			};
			List<Document> model = DataProvider.GetDocuments(out totalRecords, out totalFilteredRecords, parameters);
			model.ForEach(x => x.FileUrl = x.FileUrl != null ? System.IO.Path.Combine(GetDataFolderPath("Documents"), x.FileUrl) : null);

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult EditDocument(int? id)
		{
			Document model;
			if (id != null)
			{
				model = DataProvider.GetDocument<Document>((int)id);
				if (model.FileUrl != null)
					model.FileUrl = System.IO.Path.Combine(GetDataFolderPath("Documents"), model.FileUrl);
			}
			else
			{
				model = new Document()
				{
					UploadedBy = this.Account.Id
				};
			}

			model.CategoriesItems = DataProvider
				.GetCategories<Category>()
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = model.CategoryId == x.Id })
				.Prepend(new SelectListItem { Text = "-- Select Category --", Value = "", Selected = model.CategoryId == null })
				.ToList();

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public JsonResult EditDocument([FromBody] Document model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}
			
			model.FileUrl = model.FileUrl == string.Empty ? null : Path.GetFileName(model.FileUrl);

			Document entity = Mapper.Map<Document>(model);
			entity = DataProvider.SaveDocument(entity);

			if (model.FileBase64String != null)
			{
				entity.FileUrl = string.Format("{0}.{1}", entity.Id.ToString(), model.FileExtension);
				
				// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
				string filePath = _hostingEnvironment.WebRootPath + GetDataFolderPath("Documents");
						
				DataResult result = AppHelper.SaveFile(model.FileBase64String, filePath, entity.FileUrl);
				if (result.IsSuccess)
					entity = DataProvider.SaveDocument(entity);
				else
					return Json(result);		
			}

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("Documents", "Admin") });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult DeleteDocument(int id)
		{
			Document entity = this.DataProvider.GetDocument<Document>(id);
			if (entity == null)
				return RedirectToAction("Documents");

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetDataFolderPath("Documents");
			if (entity.FileUrl != null)
				AppHelper.DeleteFile(filePath, entity.FileUrl);

			DataProvider.DeleteDocument(id);

			return RedirectToAction("Documents");
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult SaveDocumentFile(int id, IFormFile file, string fileName)
		{
			if (file == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = "The file does not exist !" });

			Document entity = this.DataProvider.GetDocument<Document>(id);
			if (entity == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = $"Document with #{id} does not exist !" });
			
			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetDataFolderPath("Documents");

			if (entity.FileUrl != null)
				AppHelper.DeleteFile(filePath, entity.FileUrl);

			entity.FileUrl = string.Format("{0}{1}", entity.Id.ToString(), Path.GetExtension(fileName));

			DataResult result = AppHelper.SaveFile(file, filePath, entity.FileUrl);
			if (result.IsSuccess)
				DataProvider.SaveDocument(entity);
			else 
			{
				DataProvider.DeleteDocument(entity.Id);
				return Json(result);
			}
			return Json(new DataResult() { Data = System.IO.Path.Combine(GetImagesFolderPath("Documents"), entity.FileUrl), Type = result.Type, Message = result.Message });
		}

		// Photos
		[Auth(new string[] { "Administrator", "Master" })]
		public IActionResult Photos()
		{
			return View();
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult PhotosJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<Photo> parameters = new DbParameters<Photo>
			{
				PageSize = request.length,
				PageIndex = request.start / request.length + 1,
				Filters = new Photo
				{
					Name = request.search,
				}
			};
			List<Photo> model = DataProvider.GetPhotos(out totalRecords, out totalFilteredRecords, parameters);
			foreach (var item in model)
			{
				item.FileUrl = item.FileUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Photos"), item.FileUrl) : null;
				item.AlbumName = item.Album != null ? item.Album.Name : null;
				// need set null for bug with data tables js
				item.Album = null;
			}

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult EditPhoto(int? id)
		{
			Photo model;
			if (id != null)
			{
				model = Mapper.Map<Photo>(DataProvider.GetPhoto<Photo>((int)id));
				if (model.FileUrl != null)
					model.FileUrl = System.IO.Path.Combine(GetImagesFolderPath("Photos"), model.FileUrl);
			}
			else
			{
				model = new Photo()
				{
					UploadedBy = this.Account.Id
				};
			}

			model.AlbumsItems = DataProvider.GetAlbums<Album>()?
				.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = model.AlbumId == x.Id })
				.Prepend(new SelectListItem { Text = "-- Select Album --", Value = "", Selected = model.AlbumId == null})
				.ToList();

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public JsonResult EditPhoto([FromBody] Photo model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			model.FileUrl = model.FileUrl == string.Empty ? null : Path.GetFileName(model.FileUrl);

			Photo entity = Mapper.Map<Photo>(model);
			entity = DataProvider.SavePhoto(entity);

			if (model.FileBase64String != null)
			{
				entity.FileUrl = string.Format("{0}.{1}", entity.Id.ToString(), model.FileExtension);

				// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
				string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Photos");

				AppHelper.SaveFile(model.FileBase64String, filePath, entity.FileUrl);

				entity = DataProvider.SavePhoto(entity);
			}

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("Photos", "Admin") });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult DeletePhoto(int id)
		{
			Photo entity = this.DataProvider.GetPhoto<Photo>(id);
			if (entity == null)
				return RedirectToAction("Photos");

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Photos");
			if (entity.FileUrl != null)
				AppHelper.DeleteFile(filePath, entity.FileUrl);

			DataProvider.DeletePhoto(id);

			return RedirectToAction("Photos");
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult SavePhotoFile(int id, IFormFile file, string fileName)
		{
			if (file == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = "The file does not exist !" });

			Photo entity = this.DataProvider.GetPhoto<Photo>(id);
			if (entity == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = $"Photo with #{id} does not exist !" });

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Photos");

			if (entity.FileUrl != null)
				AppHelper.DeleteFile(filePath, entity.FileUrl);

			entity.FileUrl = string.Format("{0}{1}", entity.Id.ToString(), Path.GetExtension(fileName));

			DataResult result = AppHelper.SaveFile(file, filePath, entity.FileUrl);
			if(result.IsSuccess)
				DataProvider.SavePhoto(entity);

			return Json(new DataResult() { Data = System.IO.Path.Combine(GetImagesFolderPath("Photos"), entity.FileUrl), Type = result.Type, Message = result.Message });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult DeletePhotoFile(int id)
		{
			Photo entity = this.DataProvider.GetPhoto<Photo>(id);
			if (entity == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = $"Photo with #{id} does not exist !" });

			if (entity.FileUrl == null)
				return Json(new DataResult() { Type = DataResultTypes.Success });

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Photos");

			AppHelper.DeleteFile(filePath, entity.FileUrl);
			entity.FileUrl = null;

			DataProvider.SavePhoto(entity);

			return Json(new DataResult() { Type = DataResultTypes.Success });
		}
		// Albums
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult Albums()
		{
			return View();
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult AlbumsJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<Album> parameters = new DbParameters<Album>
			{
				PageSize = request.length,
				PageIndex = request.start / request.length + 1,
				Filters = new Album
				{
					Name = request.search,
				}
			};
			List<Album> model = DataProvider.GetAlbums(out totalRecords, out totalFilteredRecords, parameters);

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult EditAlbum(int? id)
		{
			Album model = id != null ? DataProvider.GetAlbum<Album>((int)id) : new Album();
			model.CategoriesItems = DataProvider
				.GetCategories<Category>()
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = model.CategoryId == x.Id })
				.Prepend(new SelectListItem { Text = "-- Select Category --", Value = "", Selected = model.CategoryId == null })
				.ToList();

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public JsonResult EditAlbum([FromBody] Album model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			Album entity = Mapper.Map<Album>(model);
			entity = DataProvider.SaveAlbum(entity);

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("Albums", "Admin") });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult DeleteAlbum(int id)
		{
			Album entity = this.DataProvider.GetAlbum<Album>(id);
			if (entity == null)
				return RedirectToAction("Albums");

			DataProvider.DeletAlbum(id);

			return RedirectToAction("Albums");
		}

		// Halls
		[Auth(new string[] { "Administrator", "Master" })]
		public IActionResult Halls()
		{
			return View();
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult HallsJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<Hall> parameters = new DbParameters<Hall>
			{
				PageSize = request.length,
				PageIndex = request.start / request.length + 1,
				Filters = new Hall
				{
					Name = request.search,
				}
			};
			List<Hall> model = DataProvider.GetHalls(out totalRecords, out totalFilteredRecords, parameters);
			model.ForEach(x => x.PictureUrl = x.PictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Halls"), x.PictureUrl) : null);

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult EditHall(int? id)
		{
			Hall model = new Hall();
			if (id != null)
			{
				model = DataProvider.GetHall<Hall>((int)id);
				if (model.PictureUrl != null)
					model.PictureUrl = System.IO.Path.Combine(GetImagesFolderPath("Halls"), model.PictureUrl);
			}

			return View(model);
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public JsonResult EditHall([FromBody] Hall model)
		{
			if (!ModelState.IsValid)
			{
				string errorMessage = string
					.Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

				return Json(new DataResult { Type = DataResultTypes.Warning, Message = errorMessage });
			}

			model.PictureUrl = model.PictureUrl == string.Empty ? null : Path.GetFileName(model.PictureUrl);

			Hall entity = model;
			entity = DataProvider.SaveHall(entity);

			if (model.PictureBase64 != null)
			{
				entity.PictureUrl = string.Format("{0}.{1}", entity.Id.ToString(), model.PictureFileExtension);

				// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
				string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Halls");
				AppHelper.SaveFile(model.PictureBase64, filePath, entity.PictureUrl);

				entity = DataProvider.SaveHall(entity);
			}

			return Json(new DataResult { Type = DataResultTypes.Success, RedirectUrl = Url.Action("Halls", "Admin") });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult DeleteHall(int id)
		{
			Hall entity = this.DataProvider.GetHall<Hall>(id);
			if (entity == null)
				return RedirectToAction("Halls");

			if (entity.PictureUrl != null) 
			{
				// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
				string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Halls");

				AppHelper.DeleteFile(filePath, entity.PictureUrl);
			}

			DataProvider.DeleteHall(id);

			return RedirectToAction("Halls");
		}
		[Auth(new string[] { "Administrator", "Master" })]
		[HttpPost]
		public ActionResult SaveHallImage(int id, IFormFile image)
		{
			if (image == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = "Image file does not exist !" });

			Hall entity = this.DataProvider.GetHall<Hall>(id);
			if (entity == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = $"Hall with #{id} does not exist !" });

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Halls");

			if (entity.PictureUrl != null)
				AppHelper.DeleteFile(filePath, entity.PictureUrl);

			if (entity.PictureUrl == null || entity.PictureUrl == "")
				entity.PictureUrl = entity.Id + ".png";

			DataResult result = AppHelper.SaveFile(image, filePath, entity.PictureUrl);
			if(result.IsSuccess)
				DataProvider.SaveHall(entity);

			return Json(new DataResult() { Data = System.IO.Path.Combine(GetImagesFolderPath("Halls"), entity.PictureUrl), Type = result.Type, Message = result.Message });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public ActionResult DeleteHallImage(int id)
		{
			Hall entity = this.DataProvider.GetHall<Hall>(id);
			if (entity == null)
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = $"Hall with #{id} does not exist !" });

			if (entity.PictureUrl == null)
				return Json(new DataResult() { Type = DataResultTypes.Success });

			// NOTE: Append strings instead use Path.Combine cuz  do not combine corectly
			string filePath = _hostingEnvironment.WebRootPath + GetImagesFolderPath("Halls");

			AppHelper.DeleteFile(filePath, entity.PictureUrl);
			entity.PictureUrl = null;

			DataProvider.SaveHall(entity);

			return Json(new DataResult() { Type = DataResultTypes.Success });
		}


		// Visitors
		[Auth(new string[] { "Administrator", "Master" })]
		public IActionResult Visitors()
		{
			return View();
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult VisitorsJson(DataTableRequestModel request)
		{
			request.search = Request.Query["search[value]"].ToString();

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			DbParameters<VisitorLogFilters> parameters = new DbParameters<VisitorLogFilters>
			{
				PageSize = request.length,
								PageIndex = request.start / request.length + 1,
				Filters = new VisitorLogFilters
				{
					Ip = request.search != String.Empty ? request.search : null,
				}
			};

			List<VisitorLog> model = DataProvider.GetVisitorLogs(out totalRecords, out totalFilteredRecords, parameters);

			return Json(new DataTableDataSourceModel { data = model, recordsTotal = totalRecords, recordsFiltered = totalFilteredRecords });
		}
		[Auth(new string[] { "Administrator", "Master" })]
		public JsonResult VisitorsStatsJson(DateTime startDate, DateTime endDate)
		{

			List<VisitorsGroupedByDateModel> result = DataProvider.GetAgregatedVisitorsDates(startDate, endDate);
			
			int totalRecords = 0;
			int totalFilteredRecords = 0;
			string mostDeviceVisitors = DataProvider.GetVisitorLogs(
				out totalRecords,
				out totalFilteredRecords,
				new DbParameters<VisitorLogFilters>
				{
					PageSize = Int32.MaxValue,
					Filters = new VisitorLogFilters { StartDate = startDate, EndDate = endDate }
				})
				?.GroupBy(x => x.Device )
				.Select(x => new { Device = x.Key, Count = x.Count() })
				.OrderByDescending(x => x.Count)
				.FirstOrDefault()
				?.Device;

			string mostCityVisitors = DataProvider.GetVisitorLogs(
			out totalRecords,
			out totalFilteredRecords,
			new DbParameters<VisitorLogFilters>
			{
				PageSize = Int32.MaxValue,
				Filters = new VisitorLogFilters { StartDate = startDate, EndDate = endDate }
			})
			?.Where(x => x.City != null)
			?.GroupBy(x => x.City)
			.Select(x => new { City = x.Key, Count = x.Count() })
			.OrderByDescending(x => x.Count)
			.FirstOrDefault()
			?.City;

			return Json(new DataResult { Type = DataResultTypes.Success, Data  = new { Visitors = result, Total = result?.Sum(x => x.Count), MostVisitorsByDevice = mostDeviceVisitors, MostVisitorsByCity = mostCityVisitors } });
		}

		// ovveride actions

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (this.Account == null)
			{
				foreach (var cookie in Request.Cookies.Keys)
					Response.Cookies.Delete(cookie);
			}

			base.OnActionExecuting(context);
		}
		public override void OnActionExecuted(ActionExecutedContext context)
		{
			ViewBag.Controller = this;
			ViewBag.Account = this.Account;
			
			base.OnActionExecuted(context);
		}
	}
}