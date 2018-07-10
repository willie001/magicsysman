using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using FluentValidation.Mvc;
using FluentValidation;
using AutoMapper;

using Newtonsoft.Json;
using Dapper;
using MySql.Data.MySqlClient;
using MagicMaids.EntityModels;
using MagicMaids.ViewModels;
using System.Web.Helpers;
using System.Security.Claims;

namespace MagicMaids
{
    public class Global : System.Web.HttpApplication
    {
		protected void Application_BeginRequest()
		{
		}

        protected void Application_Start()
        {
			MySqlConnection.ClearAllPools();

			SimpleCRUD.SetDialect(SimpleCRUD.Dialect.MySQL);

			// Removing all the view engines
			ViewEngines.Engines.Clear();

			//Add Razor Engine (which we are using)
			ViewEngines.Engines.Add(new CustomViewEngine());

			AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FluentValidationModelValidatorProvider.Configure();

			AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

			ValidatorOptions.CascadeMode = CascadeMode.StopOnFirstFailure;

			Mapper.Initialize(cfg =>  
			{  
			    cfg.CreateMap<Cleaner, CleanerDetailsVM>();  
				cfg.CreateMap<Address, UpdateAddressViewModel>();
				cfg.CreateMap<Client, ClientDetailsVM>(); 
				cfg.CreateMap<Cleaner, CleanerJobMatchVM>();  
			});  

			JsonConvert.DefaultSettings = () => new JsonSerializerSettings
			{
				DateParseHandling = DateParseHandling.None,
				DateTimeZoneHandling = DateTimeZoneHandling.Utc,

			};
        }

		protected void Application_End()
		{
			MySqlConnection.ClearAllPools();
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			Boolean disableGlobalErrorHandling = false;
			Boolean.TryParse(ConfigurationManager.AppSettings["DisableGlobalErrorHandling"], out disableGlobalErrorHandling);

			if (disableGlobalErrorHandling)
				return;
			
			Exception exception = Server.GetLastError();

			// Clear the response stream 
			var httpContext = ((HttpApplication)sender).Context;
			var currentController = String.Empty;
			var currentAction = String.Empty;
			var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

			if (currentRouteData != null)
			{
				if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
				{
					currentController = currentRouteData.Values["controller"].ToString();
				}

				if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
				{
					currentAction = currentRouteData.Values["action"].ToString();
				}
			}

			httpContext.ClearError();
			httpContext.Response.Clear();
			httpContext.Response.StatusCode = exception is HttpException ? ((HttpException)exception).GetHttpCode() : 500;
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
				//var logger = LogManager.GetCurrentClassLogger();
				LogHelper log = new LogHelper();
				if (!String.IsNullOrWhiteSpace(currentController) )
				{
				//	logger.Log(LogLevel.Error, exception, $"Exception from {currentController}.{currentAction}");
					log.Log(LogHelper.LogLevels.Error, $"Exception from {currentController}.{currentAction}", "", exception, null);
				}
				else
				{
					log.Log(LogHelper.LogLevels.Error, $"Unhandled Exception", "", exception, null);
				//	logger.Log(LogLevel.Error, exception, $"Unhandled Exception");
				}
			}

			// Manage to display a friendly view 
			InvokeErrorAction(httpContext, exception, currentController, currentAction);
		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
			// Simulate internet latency on local browsing
			if (Request.IsLocal)
				System.Threading.Thread.Sleep(50);

			var request = Request;
			var url = request.Url;
			var applicationPath = request.ApplicationPath;

			string fullurl = url.ToString();
			string baseUrl = url.Scheme + "://" + url.Authority + applicationPath.TrimEnd('/') + '/';

			string currentRelativePath = request.AppRelativeCurrentExecutionFilePath;

			if (request.HttpMethod == "GET")
			{
				if (currentRelativePath.EndsWith(".aspx"))
				{
					// get the folder path from relative path. Eg ~/page.aspx returns empty. ~/folder/page.aspx returns folder/                    
					var folderPath = currentRelativePath.Substring(2, currentRelativePath.LastIndexOf('/') - 1);

					Response.Filter = new StaticContentFilter(
							Response,
							relativePath =>
							{
								if (Context.Cache[relativePath] == null)
								{
									var physicalPath = Server.MapPath(relativePath);
									var version = "?v=" +
									new System.IO.FileInfo(physicalPath).LastWriteTime.ToString("yyyyMMddhhmmss");
									Context.Cache.Add(relativePath, version, null, DateTimeWrapper.NowUtc.AddMinutes(1), TimeSpan.Zero,
									System.Web.Caching.CacheItemPriority.Normal, null);
									return version;
								}
								else
								{
									return Context.Cache[relativePath] as string;
								}
							},
							"http://images.mydomain.com/",
							"http://scripts.mydomain.com/",
							"http://styles.mydomain.com/",
							baseUrl,
							applicationPath,
							folderPath);
				}
			}

		}

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
