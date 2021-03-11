using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPortal.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using OrganizationPortal.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net.Mail;
using System.Net;
using OrganizationPortal.Controllers;

namespace OrganizationPortal
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		[Obsolete]
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<IISOptions>(options =>
			{
				options.AutomaticAuthentication = false;
			});

			services.AddDbContext<DbContext>
				(options => options.UseSqlServer(Configuration.GetConnectionString("SQLDbConectionString"))
			);

			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			services.AddDefaultIdentity<OrgUser>(config =>
			{
				config.SignIn.RequireConfirmedEmail = true;
				config.User.RequireUniqueEmail = true;
				config.Password.RequiredLength = 6;
				config.Password.RequireLowercase = false;
				config.Password.RequireDigit = false;
				config.Password.RequireNonAlphanumeric = false;
				config.Password.RequireUppercase = false;
			})
			.AddRoles<OrgRole>()
			.AddRoleManager<RoleManager<OrgRole>>()
			.AddDefaultUI()
			.AddDefaultTokenProviders()
			.AddEntityFrameworkStores<DbContext>();

			services.AddScoped<UserClaimsPrincipalFactory<OrgUser, OrgRole>>();

			//services.AddDefaultIdentity<OrgUser>()
			//	.AddRoles<OrgRole>()
			//	.AddEntityFrameworkStores<DbContext>();

			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
					.AddCookie(options =>
					{
						options.LoginPath = new PathString("/Account/Login");
					});

			services.AddSession();

			services.AddScoped<SmtpClient>((serviceProvider) =>
			{
				var config = serviceProvider.GetRequiredService<IConfiguration>();
				return new SmtpClient()
				{
					Host = config.GetValue<String>("Email:Smtp:Host"),
					Port = config.GetValue<int>("Email:Smtp:Port"),
					Credentials = new NetworkCredential(
							config.GetValue<String>("Email:Smtp:Username"),
							config.GetValue<String>("Email:Smtp:Password")
						),
					DeliveryMethod = SmtpDeliveryMethod.Network,
					EnableSsl = true
				};
			});

			services.AddScoped<IAppMailer, AppMailer>();

			services.AddScoped<IDataProvider, DataProvider>();

			services.AddScoped<IdentityUser, OrgUser>();
					
			services.AddScoped<IApppSettings, AppSettings>();
			
			services.AddScoped<IResources, Resources>();
			services.AddLocalization(o => o.ResourcesPath = "Resources");		
			services.Configure<RequestLocalizationOptions>(options =>
			{
				var supportedCultures = new[]
				{
					new CultureInfo("en-US"),
					new CultureInfo("bg-BG")
				};
				options.DefaultRequestCulture = new RequestCulture("bg-BG", "bg-BG");

				// You must explicitly state which cultures your application supports.
				// These are the cultures the app supports for formatting 
				// numbers, dates, etc.

				options.SupportedCultures = supportedCultures;

				// These are the cultures the app supports for UI strings, 
				// i.e. we have localized resources for.

				options.SupportedUICultures = supportedCultures;
			});

			services.AddSingleton<IAppCache, AppCache>();

			Mapper.Initialize(cfg => {
				cfg.CreateMap<OrgUser, OrgUserViewModel>().ReverseMap();
				cfg.CreateMap<OrgUser, OrgUser>();
				cfg.CreateMap<EventViewModel, Event>().ReverseMap();
				cfg.CreateMap<Notice, NoticeViewModel>().ReverseMap();
				cfg.CreateMap<Notice, Notice>();
				cfg.CreateMap<Location, LocationViewModel>().ReverseMap();
				cfg.CreateMap<Location, Location>();
				cfg.CreateMap<Category, Category>();
				cfg.CreateMap<Album, Album>();
				cfg.CreateMap<Photo, Photo>();
				cfg.CreateMap<PhoneNumber, PhoneNumber>();
				cfg.CreateMap<Document, Document>();
				cfg.CreateMap<Category, CategoryViewModel>().ReverseMap();
				cfg.CreateMap<News, NewsViewModel>().ReverseMap();
				cfg.CreateMap<LoginViewModel, OrgUser>().ReverseMap();
				cfg.CreateMap<RegisterViewModel, OrgUser>().ReverseMap();
			});

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddDetection();
			services.AddDetectionCore().AddBrowser();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			var cultureInfo = new CultureInfo("bg-BG");
			cultureInfo.NumberFormat.NumberGroupSeparator = ".";

			CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
				// TODO DELETE
				app.UseFileServer(new FileServerOptions
				{
					FileProvider = new PhysicalFileProvider("D:/Projects/OrganizationPortal/Develop/OrganizationPortal/OrganizationPortal/wwwroot/Content"),
					RequestPath = new PathString("/Content"),
					EnableDirectoryBrowsing = false,

				});
			}

			//Add request localization middleware
			app.UseRequestLocalization();
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();
			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});
			app.UseAuthentication();
			app.UseSession();
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
