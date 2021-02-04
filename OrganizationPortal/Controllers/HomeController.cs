using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrganizationPortal.Models;
using OrganizationPortal.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting.Internal;
using Wangkanai.Detection;
using System;
using System.Linq;
using System.IO;
using System.Net.Mail;
using AutoMapper;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace OrganizationPortal.Controllers
{
    public class HomeController : BaseController
    {
		public HomeController(UserManager<OrgUser> userManager, SignInManager<OrgUser> signInManager, IConfiguration config, HostingEnvironment hostingEnvironment, IResources resources, IApppSettings appSettings, IDataProvider dataProvider, IDetection detection, IAppMailer appMailer) :
			base(userManager, signInManager, config, hostingEnvironment, resources, appSettings, dataProvider, detection, appMailer)
		{
		}

        // Index
        public IActionResult Index()
        {
            int totalRecords = 0;
            int totalFilteredRecords = 0;

            List<NewsViewModel> result = DataProvider
                .GetNews<NewsViewModel>
                (
                    out totalRecords,
                    out totalFilteredRecords,
                    new DbParameters<NewsViewModel>
                    {
                        SortOrder = "desc"                  
                    }
                )
                .Take(6)
                .ToList();

            result.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("News"), x.MainPictureUrl) : null);

            return View(result);
        }

        public ActionResult Search(string searchText) 
        {
            if (searchText == null || searchText.Length < 3) 
            {
                return View(new SearchResultViewModel { SearchText = searchText });
            }

            int totalRecors = 0;
            int totalFilteredRecords = 0;

            List<NewsViewModel> newsResult = DataProvider.GetNews(out totalRecors, out totalFilteredRecords, new DbParameters<NewsViewModel> { Filters = new NewsViewModel { Title = searchText } });
            newsResult.AddRange(
                DataProvider
                    .GetNews(out totalRecors, out totalFilteredRecords, new DbParameters<NewsViewModel> { Filters = new NewsViewModel { Content = searchText } }).Where(x => !newsResult.Any(n => n.Id == x.Id))
                );

            List<EventViewModel> eventsResult = DataProvider.GetEvents(out totalRecors, out totalFilteredRecords, new DbParameters<EventViewModel> { Filters = new EventViewModel { Title = searchText } });
            eventsResult.AddRange(
                DataProvider
                    .GetEvents(out totalRecors, out totalFilteredRecords, new DbParameters<EventViewModel> { Filters = new EventViewModel { Content = searchText } }).Where(x => !eventsResult.Any(n => n.Id == x.Id))
                );

            List<NoticeViewModel> noticesResult = DataProvider.GetNotices(out totalRecors, out totalFilteredRecords, new DbParameters<NoticeViewModel> { Filters = new NoticeViewModel { Content = searchText } });
            noticesResult.AddRange(
                DataProvider
                    .GetNotices(out totalRecors, out totalFilteredRecords, new DbParameters<NoticeViewModel> { Filters = new NoticeViewModel { Content = searchText } }).Where(x => !noticesResult.Any(n => n.Id == x.Id))
                );

            SearchResultViewModel result = new SearchResultViewModel
            {
                SearchText = searchText,
                News = newsResult,
                Events = eventsResult,
                Notices = noticesResult
            };

            return View(result);
        }

        // News
        public ActionResult News(int pageIndex = 1, int? category = null)
        {
            int pageSize = 3;

            int totalRecords = 0;
            int totalFilteredRecords = 0;

            List<NewsViewModel> result = DataProvider
                .GetNews<NewsViewModel>
                (
                    out totalRecords,
                    out totalFilteredRecords,
                    new DbParameters<NewsViewModel>
                    {
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        SortOrder = "desc",                  
                        Filters = category != null ? new NewsViewModel { CategoryId = (int)category } : null,
                    }
                )
                .ToList();

            result.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("News"), x.MainPictureUrl) : null);

            NewsPageViewModel model = new NewsPageViewModel 
            { 
                News = result, 
                PageCount = (totalFilteredRecords > pageSize ? (int)Math.Ceiling((decimal)totalFilteredRecords / pageSize) : 1),
                PageInex = pageIndex,
                CategoryId = category
            };

            return View(model);
        }
        public ActionResult NewsDetails(int id)
        {
            NewsViewModel mainNews = DataProvider.GetNews<NewsViewModel>((int)id);
            if (mainNews == null)
                return RedirectToAction("Error");

            mainNews.MainPictureUrl = mainNews.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("News"), mainNews.MainPictureUrl) : null;

            int totalRecords = 0;
            int totalFilteredRecords = 0;
            NewsViewModel prevNews = DataProvider.GetNews(out totalRecords, out totalFilteredRecords, new DbParameters<NewsViewModel> 
            {
                PageSize = 1,
                Filters = new NewsViewModel { PublishedDate = mainNews.PublishedDate },
                FiltersOperators = new Dictionary<string, FilterOperators> { { nameof(NewsViewModel.PublishedDate), FilterOperators.Gt } }
            }).FirstOrDefault();

            if(prevNews != null)
                prevNews.MainPictureUrl = prevNews.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("News"), prevNews.MainPictureUrl) : null;

            NewsViewModel nextNews = DataProvider.GetNews(out totalRecords, out totalFilteredRecords, new DbParameters<NewsViewModel>
            {
                PageSize = 1,
                Filters = new NewsViewModel { PublishedDate = mainNews.PublishedDate },
                SortOrder = "desc",
                FiltersOperators = new Dictionary<string, FilterOperators> { { nameof(NewsViewModel.PublishedDate), FilterOperators.Lt } }
            }).FirstOrDefault();

            if (nextNews != null)
                nextNews.MainPictureUrl = nextNews.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("News"), nextNews.MainPictureUrl) : null;

            NewsDetailsPageviewModel model = new NewsDetailsPageviewModel { MainNews = mainNews, NextNews = nextNews, PrevNews = prevNews };

            return View(model);
        }

        // Gallery
        public ActionResult Gallery(int pageIndex = 1, int? category = null) 
        {
            int pageSize = 9;

            int totalRecords = 0;
            int totalFilteredRecords = 0;

            List<Album> albums = DataProvider
                .GetAlbums<Album>
                (
                    out totalRecords, 
                    out totalFilteredRecords,
                    new DbParameters<Album> { PageSize  = pageSize, PageIndex = pageIndex, Filters = category != null ? new Album { CategoryId = (int)category } : null }, 
                    true
                )
                .Where(x => x.Photos != null && x.Photos.Count > 0)
                .ToList();

            foreach (var album in albums)
            {
                album.Photos = album.Photos.OrderBy(p => p.UploadedOn).Take(1).ToList();
                album.Photos.ForEach(photo => photo.FileUrl = photo.FileUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Photos"), photo.FileUrl) : null);
            }

            GalleryPageViewModel model = new GalleryPageViewModel
            {
                Albums = albums,
                PageCount = (totalFilteredRecords > pageSize ? (int)Math.Ceiling((decimal)totalFilteredRecords / pageSize) : 1),
                PageInex = pageIndex,
                CategoryId = category
            };

            return View(model);
        }
        public ActionResult Album(int pageIndex = 1, int? albumid = null)
        {
            int pageSize = 9;

            int totalRecords = 0;
            int totalFilteredRecords = 0;

            if (albumid == null)
                return RedirectToAction("Gallery", "Home");

             Album album = DataProvider.GetAlbum<Album>((int)albumid);

            album.Photos = DataProvider
                .GetPhotos(out totalRecords, out totalFilteredRecords, new DbParameters<Photo> { PageSize = pageSize, PageIndex = pageIndex, Filters = new Photo { AlbumId = album.Id } });

            foreach (var photo in album.Photos)            
                photo.FileUrl = photo.FileUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Photos"), photo.FileUrl) : null;
            
            AlbumPageViewModel model = new AlbumPageViewModel
            {
                Photos = album.Photos,
                PageCount = (totalFilteredRecords > pageSize ? (int)Math.Ceiling((decimal)totalFilteredRecords / pageSize) : 1),
                PageInex = pageIndex,
                Album = album
            };

            return View(model);
        }

        // Notices
        public ActionResult Notices(int pageIndex = 1, int? category = null)
        {
            int pageSize = 4;

            int totalRecords = 0;
            int totalFilteredRecords = 0;

            List<Notice> result = DataProvider
                .GetNotices<Notice>
                (
                    out totalRecords,
                    out totalFilteredRecords,
                    new DbParameters<Notice>
                    {
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        SortOrder = "desc",
                        Filters = category != null ? new Notice { CategoryId = (int)category } : null,
                    }
                )
                .ToList();

            NoticePageViewModel model = new NoticePageViewModel
            {
                Notices = result,
                PageCount = (totalFilteredRecords > pageSize ? (int)Math.Ceiling((decimal)totalFilteredRecords / pageSize) : 1),
                PageInex = pageIndex,
                CategoryId = category
            };

            return View(model);
        }
        public ActionResult NoticeDetails(int id)
        {
            Notice notice = DataProvider.GetNotice<Notice>((int)id);
            if (notice == null)
                return RedirectToAction("Error");
    
            return View(notice);
        }

        // Events
        public ActionResult Events(int pageIndex = 1, int? category = null)
        {
            int pageSize = 3;

            int totalRecords = 0;
            int totalFilteredRecords = 0;

            List<EventViewModel> result = DataProvider
                .GetEvents<EventViewModel>
                (
                    out totalRecords,
                    out totalFilteredRecords,
                    new DbParameters<EventViewModel>
                    {
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        Filters = new EventViewModel { StartDate = DateTime.Now, CategoryId = category != null ? (int)category : 0 },
                        FiltersOperators = new Dictionary<string, FilterOperators> { { nameof(EventViewModel.StartDate), FilterOperators.GtEq } },
                        SortBy = nameof(EventViewModel.StartDate)
                    }
                )
                .ToList();

            result.ForEach(x => x.MainPictureUrl = x.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Events"), x.MainPictureUrl) : null);

            EventsPageViewModel model = new EventsPageViewModel
            {
                Events = result,
                PageCount = (totalFilteredRecords > pageSize ? (int)Math.Ceiling((decimal)totalFilteredRecords / pageSize) : 1),
                PageInex = pageIndex,
                CategoryId = category
            };

            return View(model);
        }
        public ActionResult EventDetails(int id)
        {
            EventViewModel eventItem = DataProvider.GetEvent<EventViewModel>((int)id);
            if (eventItem == null)
                return RedirectToAction("Error");

            eventItem.MainPictureUrl = eventItem.MainPictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Events"), eventItem.MainPictureUrl) : null;

            return View(eventItem);
        }

        // Documents
        public ActionResult Documents(int pageIndex = 1, int? category = null) 
        {
            int pageSize = 10;

            int totalRecords = 0;
            int totalFilteredRecords = 0;

            List<Document> result = DataProvider
                .GetDocuments<Document>
                (
                    out totalRecords,
                    out totalFilteredRecords,
                    new DbParameters<Document>
                    {
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        Filters = new Document { CategoryId = category != null ? (int)category : 0 }
                    }
                )
                .ToList();

            foreach (var document in result)
            {
                document.FileExtension = document.FileUrl.Substring(document.FileUrl.LastIndexOf('.'), (document.FileUrl.Length - document.FileUrl.LastIndexOf('.')));
                document.FileUrl = document.FileUrl != null ? System.IO.Path.Combine(GetDataFolderPath("Documents"), document.FileUrl) : null;
                document.FileSize = System.IO.File.Exists(_hostingEnvironment.WebRootPath + document.FileUrl) ? new FileInfo(_hostingEnvironment.WebRootPath + document.FileUrl).Length : 0;
            }

            DocumentsPageViewModel model = new DocumentsPageViewModel
            {
                Documents = result.Where(x => x.FileSize > 0).ToList(),
                PageCount = (totalFilteredRecords > pageSize ? (int)Math.Ceiling((decimal)totalFilteredRecords / pageSize) : 1),
                PageInex = pageIndex,
                CategoryId = category
            };

            return View(model);
        }

        // Phone Numbers
        public ActionResult PhoneNumbers() 
        {
            List<PhoneNumber> result = DataProvider.GetPhoneNumbers<PhoneNumber>();

            return View(result);
        }

        // Contacts
        [HttpGet]
        public ActionResult Contact()
        {
            ContactViewModel model = new ContactViewModel();
            OrgUser mayor = Mapper.Map<OrgUser, OrgUser>(DataProvider.GetUserByUserName("Mayor"));
            if (mayor != null)
            {
                model.FirstName = mayor.FirstName;
                model.LastName = mayor.LastName;
                model.MiddleName = mayor.MiddleName;
                model.ProfilePictureUrl = mayor.ProfilePictureUrl != null ? System.IO.Path.Combine(GetImagesFolderPath("Users"), mayor.ProfilePictureUrl) : null;
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid) 
            {
                ViewBag.ErrorMessage = string
                       .Join("</br>", ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => $"{E.ErrorMessage}").ToArray());

                return View(model);
            }              
            
            OrgUser mayor = Mapper.Map<OrgUser, OrgUser>(DataProvider.GetUserByUserName("Mayor"));

            EmailMessage<ContactViewModel> message = new EmailMessage<ContactViewModel>(model.SenderEmail, mayor.Email)
            {
                Subject = String.Format("{0}{1}",Resources.GetAppResourcesValue(OrganizationPortal.Resources.AppResourcesKeys.AppName) , model.Subject != null ? " | " + model.Subject : "" ),
                Model = model
            };

            try
            {
               SendEmail<ContactViewModel>(message, "SendEmailToMayor");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View(model);
            }
            
            ViewBag.SuccessMessage = "Имейлът беше изпратен успешно !";
            
            ModelState.Clear();
            return View(new ContactViewModel());
        }

        // About
        public ActionResult About()
        {
            return View();
        }

        // Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SetLanguage(string culture)
        {

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            if (culture == "bg")
            {
                CultureInfo cultureInfo = new CultureInfo("bg-BG");
                cultureInfo.NumberFormat.CurrencySymbol = "лв";
                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            }
            else if (culture == "en") {
                CultureInfo cultureInfo = new CultureInfo("en-US");
                cultureInfo.NumberFormat.CurrencySymbol = "€";
                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            }

            return Json(new DataResult() { Type = DataResultTypes.Success });
        }
    }
}
