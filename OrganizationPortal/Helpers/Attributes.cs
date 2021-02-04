using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using OrganizationPortal.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationPortal.Helpers
{
	public class AuthAttribute : ActionFilterAttribute
	{
		private string[] _roles;

		public AuthAttribute(string[] Roles)
		{
			_roles = Roles;

		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			BaseController controller = (BaseController)context.Controller;
			if (!context.HttpContext.User.Identity.IsAuthenticated || !_roles.Any(x => controller.UserIsInRole(x))) 
			{
				string actionName = "Login";
				if (!_roles.Any(x => controller.UserIsInRole(x)))
					actionName = "NoRightsLogin";

				context.Result = new RedirectToRouteResult(
					new RouteValueDictionary
					{
						{ "controller", "Account" },
						{ "action", actionName }
					});
			}

		}
	}
}
