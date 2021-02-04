using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace OrganizationPortal
{
    public class OrgUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ProfilePictureUrl { get; set; }
        public List<UserRole> UserRoles { get; set; }
        [NotMapped]
        public bool IsAdministrator { get { return UserRoles.Any(x => x.Role.Name == "Administrator" || x.Role.Name == "Master"); } }
        [NotMapped]
        public bool IsMaster { get { return UserRoles.Any(x => x.Role.Name == "Master"); } }
    }
    public class OrgRole : IdentityRole
    {
        public List<UserRole> UserRoles { get; set; }
    }
    public class UserRole : IdentityUserRole<string>
    {
        public virtual OrgUser User { get; set; }
        public virtual OrgRole Role { get; set; }
    }

    public class AppSetting
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class AppResource
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public string DefaultValue { get; set; }
    }
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Required]
        public string Title { get; set; }
        public string Content { get; set; }
        public string MainPictureUrl { get; set; }
        public string EventProgramPictureUrl { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public DateTime? StartDate { get; set; }
        public decimal TicketPrice { get; set; }

        [ForeignKey("Location")]
        public int LocationId { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        [Required]
        [ForeignKey("Category")]
        public int? CategoryId { get; set; }

        // join	
        public Location Location { get; set; }
        public OrgUser User { get; set; }
        public Category Category { get; set; }
    }
    public class News
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string MainPictureUrl { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int VisitedCount { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        [Required]
        [ForeignKey("Category")]
        public int? CategoryId { get; set; }

        // join	
        public OrgUser User { get; set; }
        public Category Category { get; set; }
    }
    public class Notice
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime? ModifyDate { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        [Required]
        [ForeignKey("Category")]
        public int? CategoryId { get; set; }

        // join	
        public OrgUser User { get; set; }
        public Category Category { get; set; }
    }
    public class Location
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Lat { get; set; }
        public string Lang { get; set; }
    }
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PhoneNumber
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
    }
    public class Document
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadedOn { get; set; }
        [Required]
        [ForeignKey("Category")]
        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        [NotMapped]
        public string FileBase64String { get; set; }
        [NotMapped]
        public string FileExtension { get; set; }
        [NotMapped]
        public long FileSize { get; set; }
        [NotMapped]
        public List<SelectListItem> CategoriesItems { get; set; }
    }
    public class Photo
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadedOn { get; set; }       
        [ForeignKey("Album")]
        [Required]
        public int? AlbumId { get; set; }
        public Album Album { get; set; }
      
        [NotMapped]
        public string AlbumName { get; set; }
        [NotMapped]
        public string FileBase64String { get; set; }
        [NotMapped]
        public IFormFile PictureFile { get; set; }
        [NotMapped]
        public string FileExtension { get; set; }
        [NotMapped]
        public long FileSize { get; set; }
        [NotMapped]
        public List<SelectListItem> AlbumsItems { get; set; }

    }
    public class Album 
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateOn { get; set; }
        [ForeignKey("Category")]
        public int? CategoryId { get; set; }
        public Category Category { get; set; }
        public List<Photo> Photos { get; set; }
        [NotMapped]
        public List<SelectListItem> CategoriesItems { get; set; }
    }
    public class Hall
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string PictureUrl { get; set; }

        [NotMapped]
        public string PictureBase64 { get; set; }
        [NotMapped]
        public string PictureFileExtension { get; set; }
        [NotMapped]
        public IFormFile PictureFile { get; set; }
    }

    public class VisitorLog
    {
        [Key]
        public int Id { get; set; }
        public string Session { get; set; }
        public string Ip { get; set; }
        public string Browser { get; set; }
        public string Device { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public string City { get; set; }
    }
}
