using OrganizationPortal.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.IO;
using OrganizationPortal;
using Microsoft.AspNetCore.Identity;

public class DbContext : IdentityDbContext<OrgUser, OrgRole, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public DbContext()
    {
    }
    public DbContext(DbContextOptions<DbContext> options)
       : base(options)
    {
    }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (!optionsBuilder.IsConfigured)
		{
			IConfigurationRoot configuration = new ConfigurationBuilder()
			   .SetBasePath(Directory.GetCurrentDirectory())
			   .AddJsonFile("appsettings.json")
			   .Build();

			var connectionString = configuration.GetConnectionString("SQLDbConectionString");
			optionsBuilder.UseSqlServer(connectionString);
		}
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<UserRole>(userRole =>
		{
			userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

			userRole.HasOne(ur => ur.Role)
				.WithMany(r => r.UserRoles)
				.HasForeignKey(ur => ur.RoleId)
				.IsRequired();

			userRole.HasOne(ur => ur.User)
				.WithMany(r => r.UserRoles)
				.HasForeignKey(ur => ur.UserId)
				.IsRequired();
		});
	}

	public DbSet<OrgUser> OrgUsers { get; set; }
	public DbSet<OrgRole> OrgRoles { get; set; }
	public DbSet<AppSetting> AppSettings { get; set; }
    public DbSet<AppResource> AppResources { get; set; }
    public DbSet<Event> Events { get; set; }
	public DbSet<News> News { get; set; }
	public DbSet<Notice> Notices { get; set; }
	public DbSet<Location> Locations { get; set; }
	public DbSet<Category> Categories { get; set; }
	public DbSet<PhoneNumber> PhoneNumbers { get; set; }
	public DbSet<Document> Documents { get; set; }
	public DbSet<Photo> Photos { get; set; }
	public DbSet<Album> Albums { get; set; }
	public DbSet<Hall> Halls { get; set; }
	public DbSet<VisitorLog> VisitorLogs { get; set; }
}

