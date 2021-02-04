using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrganizationPortal
{
	public class IndexViewModel
	{
		public int TotalUsers { get; set; }
		public int TotalEvents { get; set; }
		public List<NewsViewModel> RecentNews { get; set; }
		public int TotalNews { get; set; }
		public List<EventViewModel> RecentEvents { get; set; }

		public Dictionary<string, int> VisitorsCountByDate { get; set; }
	}

	public class SearchResultViewModel 
	{
		[MinLength(3, ErrorMessage = "Моля въведете най-малко 3 символа !")]
		[Required(ErrorMessage = "Моля въведете най-малко 3 символа !")]
		public string SearchText { get; set; }

		public List<NewsViewModel> News { get; set; }
		public List<EventViewModel> Events { get; set; }
		public List<NoticeViewModel> Notices { get; set; }
	}
	public class OrgUserViewModel
	{
		public string Id { get; set; }
		public string UserName { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public virtual bool PhoneNumberConfirmed { get; set; }
		public virtual string PhoneNumber { get; set; }
		public virtual bool EmailConfirmed { get; set; }
		public virtual string Email { get; set; }
		public string ProfilePictureUrl { get; set; }
		public List<string> UserRolesIds { get; set; }

		[AutoMapper.IgnoreMap]
		public IFormFile MainPicture { get; set; }
		[AutoMapper.IgnoreMap]
		public string ProfilePictureBase64 { get; set; }
		[AutoMapper.IgnoreMap]
		public string ProfilePictureFileExtension { get; set; }
		[AutoMapper.IgnoreMap]
		public List<SelectListItem> RolesItems { get; set; }
		[AutoMapper.IgnoreMap]
		public decimal NewsActivityIndex { get; set; }
		[AutoMapper.IgnoreMap]
		public decimal EventsActivityIndex { get; set; }
		[AutoMapper.IgnoreMap]
		public List<NewsViewModel> News { get; set; }
		[AutoMapper.IgnoreMap]
		public List<EventViewModel> Events { get; set; }

	}
	public class EventViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string MainPictureUrl { get; set; }
		public string EventProgramPictureUrl { get; set; }
		public DateTime? PublishedDate { get; set; }
		public DateTime? ModifyDate { get; set; }
		[Required]
		public DateTime? StartDate { get; set; }
		[Required(ErrorMessage = "Location is required !")]
		public int? LocationId { get; set; }
		public LocationViewModel Location { get; set; }
		public decimal TicketPrice { get; set; }
		[Required]
		public string Title { get; set; }
		public string Content { get; set; }
		[Required]
		public int CategoryId { get; set; }
		public Category Category { get; set; }
		public string UserId { get; set; }
		public OrgUserViewModel User { get; set; }

		[AutoMapper.IgnoreMap]
		public IFormFile MainPicture { get; set; }
		public string MainPictureBase64 { get; set; }
		public string MainPictureFileExtension { get; set; }
		[AutoMapper.IgnoreMap]
		public List<SelectListItem> LocationItems { get; set; }
		[AutoMapper.IgnoreMap]
		public List<SelectListItem> CategoriesItems { get; set; }
	}
	public class NoticeViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public DateTime? PublishedDate { get; set; }
		public DateTime? ModifyDate { get; set; }
		public string UserId { get; set; }
		public OrgUserViewModel User { get; set; }
		public int? CategoryId { get; set; }
		public Category Category { get; set; }

		[AutoMapper.IgnoreMap]
		public List<SelectListItem> CategoriesItems { get; set; }
	}
	public class NewsViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string MainPictureUrl { get; set; }
		public DateTime? PublishedDate { get; set; }
		public DateTime? ModifydDate { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		[Required]
		public int CategoryId { get; set; }
		public Category Category { get; set; }
		public string UserId { get; set; }
		public OrgUserViewModel User { get; set; }
		public int VisitedCount { get; set; }

		[AutoMapper.IgnoreMap]
		public IFormFile MainPicture { get; set; }
		public string MainPictureBase64 { get; set; }
		public string MainPictureFileExtension { get; set; }
		[AutoMapper.IgnoreMap]
		public List<SelectListItem> CategoriesItems { get; set; }
	}
	public class LocationViewModel
	{
		public int Id { get; set; }
		public string Type { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
		public string Lat { get; set; }
		public string Lang { get; set; }
	}
	public class LoginViewModel
	{
		[Required]
		public string UserName { get; set; }
		[Required]
		public string Password { get; set; }
	}

	public class RegisterViewModel
	{
		[Required]
		[MinLength(5)]
		public string UserName { get; set; }

		[Required]
		[Display(Name = "First Name")]
		[StringLength(100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string FirstName { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		[StringLength(100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string LastName { get; set; }

		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[Display(Name = "Password")]
		//[RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{6,}$", ErrorMessage = "Passwords must be at least 6 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The passwords do not match.")]
		public string ConfirmPassword { get; set; }

		public string PhoneNumber { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public int? RoleId { get; set; }
	}
	public class VisitorLogFilters
	{
		public string Id { get; set; }
		public string Ip { get; set; }
		public string Url { get; set; }
		public string Session { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}
	public class VisitorsGroupedByDateModel 
	{
		public string GroupedFormat{ get; set; }
		public string Date { get; set; }
		public int Count { get; set; }
	}
	public class NewsPageViewModel
	{
		public int PageInex { get; set; }
		public int PageCount { get; set; }
		public int? CategoryId { get; set; }
		public List<NewsViewModel> News { get; set; }
	}
	public class EventsPageViewModel
	{
		public int PageInex { get; set; }
		public int PageCount { get; set; }
		public int? CategoryId { get; set; }
		public List<EventViewModel> Events { get; set; }
	}
	public class DocumentsPageViewModel
	{
		public int PageInex { get; set; }
		public int PageCount { get; set; }
		public int? CategoryId { get; set; }
		public List<Document> Documents { get; set; }
	}

	public class GalleryPageViewModel
	{
		public int PageInex { get; set; }
		public int PageCount { get; set; }
		public int? CategoryId { get; set; }
		public List<Album> Albums { get; set; }
	}
	public class AlbumPageViewModel
	{
		public int PageInex { get; set; }
		public int PageCount { get; set; }
		public Album Album { get; set; }
		public int AlbumId { get { return Album.Id; } }
		public List<Photo> Photos { get; set; }
	}
	public class NoticePageViewModel
	{
		public int PageInex { get; set; }
		public int PageCount { get; set; }
		public int? CategoryId { get; set; }
		public List<Notice> Notices { get; set; }
	}
	public class NewsDetailsPageviewModel
	{
		public NewsViewModel MainNews { get; set; }
		public NewsViewModel PrevNews { get; set; }
		public NewsViewModel NextNews { get; set; }
	}
	public class CategoryViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		[AutoMapper.IgnoreMap]
		public int ItemsCount { get; set; }
	}

	public class ContactViewModel
	{
		public string UserName { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string ProfilePictureUrl { get; set; }
		
		// Inputs
		public string SenderPhone { get; set; }
		[Required(ErrorMessage = "Полето 'Име' е задължително !")]
		public string SenderFirstName { get; set; }
		[Required(ErrorMessage = "Полето 'Поща' е задължително !")]
		[RegularExpression("^\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*$", ErrorMessage = "Невалиден имелил адрес !")]
		public string SenderEmail { get; set; }
		public string Subject { get; set; }
		[Required(ErrorMessage = "Полето 'Съобщение' е задължително !")]
		public string Message { get; set; }
	
	}

	public class EmailViewModel
	{
		public string[] To { get; set; }
		public string From { get; set; }
		public string SenderPhone { get; set; }
		public string SenderFirstName { get; set; }
		public string SenderLastName { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }
	}
}
