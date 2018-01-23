using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NLog;
using NLog.Common;
using FluentValidation.Mvc;
using System.Globalization;
using System.Threading;
using FluentValidation;
using AutoMapper;
using MagicMaids.EntityModels;
using MagicMaids.ViewModels;
using MagicMaids.Controllers;

namespace MagicMaids
{
    public class Global : System.Web.HttpApplication
    {
		protected void Application_BeginRequest()
		{
		}

        protected void Application_Start()
        {
			// Removing all the view engines
			ViewEngines.Engines.Clear();

			//Add Razor Engine (which we are using)
			ViewEngines.Engines.Add(new CustomViewEngine());

			AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FluentValidationModelValidatorProvider.Configure();

			ValidatorOptions.CascadeMode = CascadeMode.StopOnFirstFailure;

			Mapper.Initialize(cfg =>  
			{  
			    cfg.CreateMap<Cleaner, CleanerDetailsVM>();  
				cfg.CreateMap<Address, UpdateAddressViewModel>();
				cfg.CreateMap<Client, ClientDetailsVM>();  
			});  

        }

		//protected void Application_Error(object sender, EventArgs e)
		//{
		//	Exception exception = Server.GetLastError();

		//	// Clear the response stream 
		//	var httpContext = ((HttpApplication)sender).Context;
		//	var currentController = String.Empty;
		//	var currentAction = String.Empty;
		//	var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

		//	if (currentRouteData != null)
		//	{
		//		if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
		//		{
		//			currentController = currentRouteData.Values["controller"].ToString();
		//		}

		//		if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
		//		{
		//			currentAction = currentRouteData.Values["action"].ToString();
		//		}
		//	}

		//	httpContext.ClearError();
		//	httpContext.Response.Clear();
		//	httpContext.Response.StatusCode = exception is HttpException ? ((HttpException)exception).GetHttpCode() : 500;
		//	httpContext.Response.TrySkipIisCustomErrors = true;

		//	if (exception != null)
		//	{
		//		// bypass this error for now to avoid logging.
		//		if (exception.Source != null && (exception.Source.ToString() == "System.Web" && exception.Message == "This type of page is not served."))
		//			return;

		//		//if (exception.InnerException != null && exception.InnerException is System.Web.Compilation.CompilationExcetion)
		//		//{
		//		//	HttpCompileException ex = (HttpCompileException)exception.InnerException;

		//		//}
		//		var logger = LogManager.GetCurrentClassLogger();
		//		if (!String.IsNullOrWhiteSpace(currentController) )
		//		{
		//			logger.Log(LogLevel.Error, exception, $"Exception from {currentController}.{currentAction}");
		//		}
		//		else
		//		{
		//			logger.Log(LogLevel.Error, exception, $"Unhandled Exception");
		//		}

		//	}


		//	// Manage to display a friendly view 
		//	InvokeErrorAction(httpContext, exception, currentController, currentAction);
		//}

		protected void InvokeErrorAction(HttpContext context, Exception ex, String currentController, String currentAction)
		{
			int errorCode = 0;
			var routeData = new RouteData();
			var action = "Error";


			if (ex is HttpException)
			{
				errorCode = ((HttpException)ex).GetHttpCode();
				switch (errorCode)
				{
					case 404:
						action = "Error404";
						break;
					case 500:
						action = "Error500";
						break;
				}
			}

			routeData.Values["controller"] = "Pages";
			routeData.Values["action"] = action;
			routeData.Values["errorCode"] = errorCode;
			routeData.Values["path"] = context.Request.Url.ToString() ?? "";
			using (var controller = new MagicMaids.Controllers.PagesController())
			{
				controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
    
				((IController)controller).Execute(
					new RequestContext(new HttpContextWrapper(context), routeData));
			}
		}
    }
}
