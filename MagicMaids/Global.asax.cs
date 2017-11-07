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
			});  
        }

		protected void Application_Error(object sender, EventArgs e)
		{
			Exception exception = Server.GetLastError();

			// Clear the response stream 
			var httpContext = ((HttpApplication)sender).Context;
			httpContext.Response.Clear();
			httpContext.ClearError();
			httpContext.Response.TrySkipIisCustomErrors = true;

			if (exception != null)
			{
				// bypass this error for now to avoid logging.
				if (exception.Source != null && (exception.Source.ToString() == "System.Web" && exception.Message == "This type of page is not served."))
					return;

				//if (exception.InnerException != null && exception.InnerException is System.Web.Compilation.CompilationExcetion)
				//{
				//	HttpCompileException ex = (HttpCompileException)exception.InnerException;

				//}
				var logger = LogManager.GetCurrentClassLogger();
				logger.Log(LogLevel.Error, exception, "Unhandled Exception");
			}

			// Manage to display a friendly view 
			InvokeErrorAction(httpContext, exception);
		}

		protected void InvokeErrorAction(HttpContext context, Exception ex)
		{
			int errorCode = 0;
			if (ex is HttpException)
			{
				errorCode = ((HttpException)ex).GetHttpCode();
			}
			var routeData = new RouteData();
			routeData.Values["controller"] = "Pages";
			routeData.Values["action"] = "Error";
			routeData.Values["errorCode"] = errorCode;
			routeData.Values["exception"] = ex;
			using (var controller = new MagicMaids.Controllers.PagesController())
			{
				((IController)controller).Execute(
					new RequestContext(new HttpContextWrapper(context), routeData));
			}
		}
    }
}
