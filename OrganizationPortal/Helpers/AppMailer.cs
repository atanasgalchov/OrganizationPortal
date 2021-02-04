using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using OrganizationPortal.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OrganizationPortal
{
	public interface IAppMailer
	{
		void SendEmail(EmailMessage message, string viewName, BaseController controller);
		void SendEmail<T>(EmailMessage<T> message, string viewName, BaseController controller);
	}
	public class AppMailer : IAppMailer
	{
		private SmtpClient _smtpClient;
		private readonly IRazorViewEngine _razorViewEngine;
		private readonly ITempDataProvider _tempDataProvider;
		private readonly IServiceProvider _serviceProvider;

		public AppMailer(SmtpClient smtpClient, IRazorViewEngine razorViewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
		{
			_smtpClient = smtpClient;
			_razorViewEngine = razorViewEngine;
			_tempDataProvider = tempDataProvider;
			_serviceProvider = serviceProvider;
		}
		public async void SendEmail(EmailMessage message, string viewName, BaseController controller)
		{
			
			if (viewName != null) 
			{
				message.IsBodyHtml = true;
				message.Body = await RenderToStringAsync(viewName, message, controller);
			}
				
			_smtpClient.Send(message);
		}
		public async void SendEmail<T>(EmailMessage<T> message, string viewName, BaseController controller)
		{

			if (viewName != null) 
			{
				message.IsBodyHtml = true;
				message.Body = await RenderToStringAsync(viewName, message, controller);
			}
				
			_smtpClient.Send(message);
		}

		public async Task<string> RenderToStringAsync(string viewName, object model, BaseController controller)
		{
			var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
			var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

			using (var sw = new StringWriter())
			{
				viewName = String.Format("AppMailer/{0}", viewName);
				var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);
				
				if (viewResult.View == null)
				{
					throw new ArgumentNullException($"{viewName} does not match any available view");
				}

				var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
				{
					Model = model
				};

				var viewContext = new ViewContext(
					actionContext,
					viewResult.View,
					viewDictionary,
					new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
					sw,
					new HtmlHelperOptions()
				);

				viewContext.ViewBag.Model = model;
				viewContext.ViewBag.Controller = controller;
				viewContext.ViewBag.Action = viewName;

				await viewResult.View.RenderAsync(viewContext);
				return sw.ToString();
			}
		}
	}
	public class EmailMessage<T> : EmailMessage
	{
		public EmailMessage(string from, string to) : base(from, to)
		{

		}

		//public string Content { get; set; }
		public T Model { get; set; }
	} 
	public class EmailMessage : MailMessage
	{
		public EmailMessage(string from, string to) : base(from, to)
		{

		}
		public string To { get; set; }
		public string From { get; set; }
		//public string Content { get; set; }
	}
}
