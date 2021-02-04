using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OrganizationPortal.Data;
using Wangkanai.Detection;

namespace OrganizationPortal.Controllers
{
    public class AppMailerController : BaseController
    {
        public AppMailerController(UserManager<OrgUser> userManager, SignInManager<OrgUser> signInManager, IConfiguration config, IHostingEnvironment hostingEnvironment, IResources resource, IApppSettings appSettings, IDataProvider dataProvider, IDetection detection) 
            : base(userManager, signInManager, config, hostingEnvironment, resource, appSettings, dataProvider, detection)
        {
        }

        public ActionResult SendEmailToMayor(EmailViewModel model)
        {
            return View(model);
        }
        public ActionResult SendConfirmRegistrationToUserEmail(EmailMessage model)
        {
            return View(model);
        }
    }
}