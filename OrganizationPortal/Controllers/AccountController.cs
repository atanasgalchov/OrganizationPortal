using OrganizationPortal.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using OrganizationPortal.Data;
using OrganizationPortal.Helpers;
using System.Linq;
using Wangkanai.Detection;
using System.Net.Mail;

namespace OrganizationPortal.Controllers
{
	[Authorize]
    public class AccountController : BaseController
	{
		private RoleManager<OrgRole> _roleManager;
		public AccountController(UserManager<OrgUser> userManager, SignInManager<OrgUser> signInManager, IConfiguration config, HostingEnvironment hostingEnvironment, IResources resources, IApppSettings appSettings, IDataProvider dataProvider, RoleManager<OrgRole> roleManager, IDetection detection, IAppMailer appMailer) :
			base(userManager, signInManager, config, hostingEnvironment, resources, appSettings, dataProvider, detection, appMailer)
		{
			_roleManager = roleManager;
		}

		[HttpGet]
		[AllowAnonymous]
		public ActionResult NoRightsLogin()
		{
			ViewBag.ViewMessage = "You do not have the right to reach this page.";
			return View("Login");
		}
		[HttpGet]
		[AllowAnonymous]
		public ActionResult Login()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> Login(LoginViewModel loginModel)
		{
			if (!ModelState.IsValid)
			{
				return View(loginModel);
			}

			OrgUser identityUser = DataProvider.GetUserByUserName(loginModel.UserName); 

			if (identityUser == null)
			{
				ViewBag.ViewMessage = "Wrong username !";
				return View(loginModel);
			}

			if (!identityUser.EmailConfirmed) 
			{
				ViewBag.ViewMessage = "Your account is not confirmed.You will receive an email when you already can log in!";
				return View(loginModel);
			}


			ClaimsIdentity identity =   new ClaimsIdentity(GetUserClaims(identityUser), CookieAuthenticationDefaults.AuthenticationScheme);
			ClaimsPrincipal principal = new ClaimsPrincipal(identity);
			
			await HttpContext.SignInAsync
				(
					CookieAuthenticationDefaults.AuthenticationScheme, 
					principal,
					new AuthenticationProperties
					{
						IsPersistent = false,
						AllowRefresh = false
					}
				);

			return RedirectToAction("Index", "Admin");
		}

		[HttpPost]
		public async Task<ActionResult> Logout()
		{
			//HttpContext.Session.Remove("OrgUser");
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Index", "Home");
		}
		[AllowAnonymous]
		[HttpGet]
		public ActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public async Task<JsonResult> RegisterUserJson(RegisterViewModel model)
		{
			if (!ModelState.IsValid)
				return  Json(new DataResult() { Type = DataResultTypes.Error, Message = "Not valid !" });
			
			OrgUser user = AutoMapper.Mapper.Map<RegisterViewModel, OrgUser>(model);
			user.Id = Guid.NewGuid().ToString();
			// NOTE: Add first user as master	
			var isFirstUser = DataProvider.GetUsers<OrgUser>().Count == 0;
			if (isFirstUser) 
				user.EmailConfirmed = true;
						
			IdentityResult result =  await _userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Errors \n");
				foreach (var error in result.Errors)				
					sb.AppendLine($"{error.Description} \n");
				
				// If we got this far, something failed, redisplay form
				return Json(new DataResult() { Type = DataResultTypes.Error, Message = sb.ToString() });
			}

			if (isFirstUser)
			{
				IdentityResult craeteRoleResult = await _roleManager.CreateAsync(new OrgRole { Name = "Master" });
				if (!craeteRoleResult.Succeeded)
					// If we got this far, something failed, redisplay form
					return Json(new DataResult() { Type = DataResultTypes.Error, Message = "Role does not created." });

				user = _dataProvider.GetUserByUserName(user.UserName);
				IdentityResult addRoleToUserresult = await _userManager.AddToRoleAsync(user, "Master");

				if (!addRoleToUserresult.Succeeded)
					return Json(new DataResult() { Type = DataResultTypes.Error, Message = "Role does not add to user." });
			}

			return Json(new DataResult() { Type = DataResultTypes.Success });
		}

		// POST: /Account/Register
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			if (!ModelState.IsValid)
			{
				ViewBag.ViewMessage = "Not valid !";
				return View(model);
			}

			OrgUser user = AutoMapper.Mapper.Map<RegisterViewModel, OrgUser>(model);
			user.Id = Guid.NewGuid().ToString();

			// NOTE: Confirm frist user
			bool isFirstUser = DataProvider.GetUsers<OrgUser>().Count == 0;
			if (isFirstUser) {
				user.EmailConfirmed = true;
			};

			if (DataProvider.GetUserByUserName(model.UserName) != null) 
			{
				ViewBag.ViewMessage = $"Username {model.UserName} already exist.";
				return View(model);
			}
				
			IdentityResult result = await _userManager.CreateAsync(user, model.Password);
			if (!result.Succeeded)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Errors \n");
				foreach (var error in result.Errors)
				{
					sb.AppendLine($"{error.Description} \n");
				}
				
				ViewBag.ViewMessage = sb.ToString();
				// If we got this far, something failed, redisplay form
				return View(model);
			}
			// NOTE: Made fiest user master
			if (isFirstUser)
			{
				IdentityResult craeteRoleResult = await _roleManager.CreateAsync(new OrgRole { Name = "Master" });
				if (!craeteRoleResult.Succeeded)
				{
					// If we got this far, something failed, redisplay form
					ViewBag.ViewMessage = "Role does not created.";
					return View(model);
				}


				user = _dataProvider.GetUserByUserName(user.UserName);
				IdentityResult addRoleToUserresult = await _userManager.AddToRoleAsync(user, "Master");

				if (!addRoleToUserresult.Succeeded) 
				{
					ViewBag.ViewMessage = "Role does not add to user.";
					return View(model);
				}
			}
			else
			{
				//MailAddressCollection toEmails = new MailAddressCollection();
				//List<OrgUser> masterUsers = _dataProvider.GetUsers<OrgUser>(true).Where(x => x.UserRoles.Any(r => r.Role.Name == "Master")).ToList();
				//foreach (var item in masterUsers)				
				//	toEmails.Add(item.Email);

				//MailMessage address = new MailMessage();
				//address.
				EmailMessage emailModel = new EmailMessage(_appSettings.GetAppSettingValue(OrganizationPortal.AppSettings.AppSettingsKeys.Email), user.Email);
				emailModel.Subject = String.Format(Resources.GetAppResourcesValue(OrganizationPortal.Resources.AppResourcesKeys.EmailRegistrationConfirmSubject), Resources.GetAppResourcesValue(OrganizationPortal.Resources.AppResourcesKeys.AppName));
				emailModel.Body = String
					.Format(
						Resources.GetAppResourcesValue(OrganizationPortal.Resources.AppResourcesKeys.EmailRegistrationConfirmContent),
						user.FirstName, user.LastName,
						Resources.GetAppResourcesValue(OrganizationPortal.Resources.AppResourcesKeys.AppName)
					);
				try
				{
					SendEmail(emailModel);
				}
				catch (Exception ex)
				{
					ViewBag.ViewMessage = ex.Message;
					return View(model);
				}
			}
			// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
			// Send an email with this link
			//var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			//var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
			//await _emailSender.SendEmailAsync(model.UserName, model.Email, "Confirm your account", $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
			////Line commented to prevent user logging in without confirming account. //await _signInManager.SignInAsync(user, isPersistent: false);
			//_logger.LogInformation(3, $"User {user.Id} created a new account");

			return RedirectToAction("Login", "Account");
		}

		private IEnumerable<Claim> GetUserClaims(OrgUser user)
		{
			List<Claim> claims = new List<Claim>();

			claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
			claims.Add(new Claim(ClaimTypes.Name, user.UserName));
			claims.Add(new Claim(ClaimTypes.Email, user.Email));
			claims.AddRange(this.GetUserRoleClaims(user));
			return claims;
		}
		private IEnumerable<Claim> GetUserRoleClaims(OrgUser user)
		{
			List<Claim> claims = new List<Claim>();

			claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
			//claims.Add(new Claim(ClaimTypes.Role, user.UserPermissionType));
			return claims;
		}
	}
}