using OrganizationPortal.Data;
using OrganizationPortal.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using OrganizationPortal.Helpers;
using System;
using System.Net;
using System.Linq;
using Wangkanai.Detection;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;
using AutoMapper;
using Microsoft.AspNetCore.Localization;

namespace OrganizationPortal.Controllers
{
	public class BaseController : Controller
	{
		protected IDataProvider _dataProvider;
		protected UserManager<OrgUser> _userManager;
		protected SignInManager<OrgUser> _signInManager;

		protected IConfiguration _configuration;
		protected IHostingEnvironment _hostingEnvironment;
		private readonly IDetection _detection;
		protected IResources _resources;
		protected IApppSettings _appSettings;
		protected IAppMailer _appMailer;

		public BaseController(UserManager<OrgUser> userManager, SignInManager<OrgUser> signInManager, IConfiguration config, IHostingEnvironment hostingEnvironment, IResources resource, IApppSettings appSettings, IDataProvider dataProvider, IDetection detection, IAppMailer appMailer)
		{
			_dataProvider = dataProvider;
			_userManager = userManager;
			_signInManager = signInManager;
			_configuration = config;
			_hostingEnvironment = hostingEnvironment;
			_resources = resource;
			_appSettings = appSettings;
			_detection = detection;
			_appMailer = appMailer;
		}
		public BaseController(UserManager<OrgUser> userManager, SignInManager<OrgUser> signInManager, IConfiguration config, IHostingEnvironment hostingEnvironment, IResources resource, IApppSettings appSettings, IDataProvider dataProvider, IDetection detection)
		{
			_dataProvider = dataProvider;
			_userManager = userManager;
			_signInManager = signInManager;
			_configuration = config;
			_hostingEnvironment = hostingEnvironment;
			_resources = resource;
			_appSettings = appSettings;
			_detection = detection;
		}
		protected IDataProvider DataProvider { get { return _dataProvider; } }
		public IResources Resources { get { return _resources; } }
		public IApppSettings AppSettings { get { return _appSettings; } }
		public IHostingEnvironment HostingEnvironment { get { return _hostingEnvironment; } }
		private OrgUser GetAccount()
		{
			if (_signInManager.Context.User.Identity.IsAuthenticated)
			{
				OrgUser model = Mapper.Map<OrgUser, OrgUser>(DataProvider.GetUserByUserName(_signInManager.Context.User.Identity.Name));
				if (model == null)
					return null;

				model.ProfilePictureUrl = model.ProfilePictureUrl  != null ? System.IO.Path.Combine(GetImagesFolderPath("Users"), model.ProfilePictureUrl) : null;

				return model;
			}
			return null;
		}

		public bool UserIsInRole(string role) 
		{
			return this.Account?.UserRoles?.Find(x => x.Role?.Name == role) != null;
		}

		public List<Document> GetLayoutDocuments() 
		{
			List<Document>  documents = DataProvider.GetDocuments<Document>().Take(4).ToList();
			foreach (var document in documents)
			{
				document.FileExtension = document.FileUrl.Substring(document.FileUrl.LastIndexOf('.'), (document.FileUrl.Length - document.FileUrl.LastIndexOf('.')));
				document.FileUrl = document.FileUrl != null ? System.IO.Path.Combine(GetDataFolderPath("Documents"), document.FileUrl) : null;
				document.FileSize = System.IO.File.Exists(_hostingEnvironment.WebRootPath + document.FileUrl) ? new FileInfo(_hostingEnvironment.WebRootPath + document.FileUrl).Length : 0;
			}

			return documents.Where(x => x.FileSize > 0).ToList();
		}
		public List<Photo> GetLayoutPhotos()
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			List<Photo> result = DataProvider.GetPhotos<Photo>(out totalRecords, out totalFilteredRecords, new DbParameters<Photo> { SortOrder = "desc" }).Take(1).ToList();
			foreach (var photo in result)
			{
				photo.FileExtension = photo.FileUrl.Substring(photo.FileUrl.LastIndexOf('.'), (photo.FileUrl.Length - photo.FileUrl.LastIndexOf('.')));
				photo.FileUrl = photo.FileUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Photos"), photo.FileUrl) : null;
				photo.FileSize = System.IO.File.Exists(_hostingEnvironment.WebRootPath + photo.FileUrl) ? new FileInfo(_hostingEnvironment.WebRootPath + photo.FileUrl).Length : 0;
			}

			return result;
		}
		public List<Notice> GetLayoutNotice()
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			List<Notice> notices = DataProvider.GetNotices<Notice>(out totalRecords, out totalFilteredRecords).OrderByDescending(x => x.ModifyDate).Take(4).ToList();

			return notices;
		}
		public List<EventViewModel> GetLayoutEvents()
		{
			int totalRecords = 0;
			int totalFilteredRecords = 0;

			List<EventViewModel> events = DataProvider
				.GetEvents<EventViewModel>
				(
					out totalRecords, 
					out totalFilteredRecords, 
					new DbParameters<EventViewModel> 
					{  
						SortBy = nameof(EventViewModel.StartDate),
						Filters = new EventViewModel  { StartDate = DateTime.Now },
						FiltersOperators = new Dictionary<string, FilterOperators> { { nameof(EventViewModel.StartDate), FilterOperators.GtEq } }
					}
				)
				.Take(4)
				.ToList();

			events.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Events"), x.MainPictureUrl) : null);

			return events;
		}
		public List<PhoneNumber> GetLayoutPhoneNumbers()
		{
			List<PhoneNumber> result = DataProvider
				.GetPhoneNumbers<PhoneNumber>()
				.Take(4)
				.ToList();

			return result;
		}
		public List<CategoryViewModel> GetLayoutCategories(string action)
		{
			List<CategoryViewModel> categories = DataProvider
				.GetCategories<CategoryViewModel>()
				.ToList();

			int totalRecords = 0;
			int totlaFilteredRecords = 0;
			if (action == "News")
				foreach (var category in categories)
					category.ItemsCount = DataProvider.GetNews(out totalRecords, out totlaFilteredRecords, new DbParameters<News> { Filters = new News { CategoryId = (int)category.Id } }).Count();
			if (action == "Events")
				foreach (var category in categories)
					category.ItemsCount = DataProvider.GetEvents(out totalRecords, out totlaFilteredRecords, new DbParameters<Event> { Filters = new Event { CategoryId = (int)category.Id, StartDate = DateTime.Now }, FiltersOperators = new Dictionary<string, FilterOperators> { { nameof(EventViewModel.StartDate), FilterOperators.GtEq } } }).Count();
			if (action == "Notices" || action == "Notice")
				foreach (var category in categories)
					category.ItemsCount = DataProvider.GetNotices(out totalRecords, out totlaFilteredRecords, new DbParameters<Notice> { Filters = new Notice { CategoryId = (int)category.Id } }).Count();
			if(action == "Gallery" || action == "Album")
				foreach (var category in categories)
					category.ItemsCount = DataProvider.GetAlbums(out totalRecords, out totlaFilteredRecords, new DbParameters<Album> { Filters = new Album { CategoryId = (int)category.Id } }).Count();
			if (action == "Documents")
				foreach (var category in categories)
					category.ItemsCount = DataProvider.GetDocuments(out totalRecords, out totlaFilteredRecords, new DbParameters<Document> { Filters = new Document { CategoryId = (int)category.Id } }).Count();

			categories = categories.Where(x => x.ItemsCount > 0).ToList();
			
			return categories;
		}

		public List<Album> GetLayoutAlbums()
		{

			int totalRecords = 0;
			int totalFilteredRecords = 0;
			List<Album> albums = DataProvider
				.GetAlbums<Album>
				(
					out totalRecords,
					out totalFilteredRecords,					
					new DbParameters<Album> { PageSize = 4 },
					true
				)
				.Where(x => x.Photos != null && x.Photos.Count > 0)
				.ToList();

			foreach (var album in albums)
			{
				album.Photos = album.Photos.OrderBy(p => p.UploadedOn).Take(1).ToList();
				album.Photos.ForEach(photo => photo.FileUrl = photo.FileUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Photos"), photo.FileUrl) : null);
			}

			return albums;
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public bool IsSignedIn { get { return Account != null; } }
		protected OrgUser Account { get { return GetAccount(); } }
		protected string GetImagesFolderPath(string module)
		{
			string webRootPath = _hostingEnvironment.WebRootPath;

			string resourcePath = _configuration.GetValue<string>("AppSettings:ResourcesFolderPath");
			string imagesPath = _configuration.GetValue<string>("AppSettings:ImagesFolderPath");

			string fullPath = System.IO.Path.Combine(new string[] { webRootPath, resourcePath, imagesPath, module });

			return fullPath;
		}
		protected string GetDataFolderPath(string module)
		{
			string webRootPath = _hostingEnvironment.WebRootPath;

			string resourcePath = _configuration.GetValue<string>("AppSettings:ResourcesFolderPath");
			string dataPath = _configuration.GetValue<string>("AppSettings:DataFolderPath");

			string fullPath = System.IO.Path.Combine(new string[] { webRootPath, resourcePath, dataPath, module });

			return fullPath;
		}

		protected void SendEmail(EmailMessage message, string view)
		{

			_appMailer.SendEmail(message, view, this);
		}
		protected void SendEmail(EmailMessage message)
		{

			_appMailer.SendEmail(message, "CommonEmailView", this);
		}

		protected void SendEmail<T>(EmailMessage<T> message, string view)
		{

			_appMailer.SendEmail<T>(message, view, this);
		}
		protected void SendEmail<T>(EmailMessage<T> message)
		{

			_appMailer.SendEmail<T>(message, "CommonEmailView", this);
		}
		public override void OnActionExecuted(ActionExecutedContext context)
		{
			ViewBag.Controller = this;
			ViewBag.Action = ((ControllerActionDescriptor)context.ActionDescriptor).ActionName;
			if (Request.Headers["X-Requested-With"] != "XMLHttpRequest") 
			{

				VisitorLog log = new VisitorLog
				{
					Ip = context.HttpContext.Connection.RemoteIpAddress.ToString(),
					Date = DateTime.Now,
					Url = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}",
					Session = context.HttpContext.Session.Id,
					Browser = $"{_detection.Browser.Type.ToString()} {_detection.Browser.Version}",
					Device = _detection.Device.Type.ToString(),
					City = AppHelper.GetUserLocationDetailsyByIp(context.HttpContext.Connection.RemoteIpAddress.ToString())?.City
				};

				DataProvider.AddVisitorLog(log);
			}

			base.OnActionExecuted(context);
		}
	}
}