#region Using
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Claims;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Dapper;
using LazyCache;
using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
using MagicMaids.ViewModels;

#endregion

namespace MagicMaids.Controllers
{
	public class BaseController : Controller
	{
		#region Fields
		#endregion

		#region Constructor
		public BaseController()
		{
		}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        #endregion

        #region Properties, Protected
		#endregion

		#region Methods, Protected

		protected IEnumerable<T> GetList<T>()
		{
			return GetList<T>(new TimeSpan(0, 5, 0));
		}

		protected IEnumerable<T> GetList<T>(TimeSpan time)
		{
			IEnumerable<T> results = new List<T>();
			String cacheName = (typeof(T)).ToString();

			if (String.IsNullOrWhiteSpace(cacheName))
			{
				return results;
			}

			IAppCache cache = new CachingService();
			return cache.GetOrAdd(cacheName, () => GetListFromDb<T>(), time);
		}

		private IEnumerable<T> GetListFromDb<T>()
		{
			using (DBManager db = new DBManager())
			{
				return db.getConnection().GetList<T>();
			}
		}

		//Verander die dat dit die cache check. As die item nie daar is nie expire cache en laai item direk
		//protected T GetItem<T>(object primaryKey)
		//{
		//	var result = default(T);
		//	if (primaryKey is null)
		//	{
		//		return result;
		//	}

		//	var cachedResults = GetList<T>();
		//	if (cachedResults == null)
		//	{
		//		return result;
		//	}

		//	result = cachedResults.FirstOrDefault<T>((System.Func<T, bool>)primaryKey);

		//	return result;
		//}

		/// <summary>
		/// Read the timezone offset value from cookie and store in session.
		/// </summary>
		/// <param name="filterContext"></param>
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (HttpContext.Request.Cookies.AllKeys.Contains("timezoneoffset"))
			{
				Session["timezoneoffset"] = HttpContext.Request.Cookies["timezoneoffset"].Value;
			}
			if (HttpContext.Request.Cookies.AllKeys.Contains("timezonename"))
			{
				var timeZoneName = HttpContext.Request.Cookies["timezonename"].Value;
				if (!String.IsNullOrWhiteSpace(timeZoneName))
				{
					Session["timezonename"] = timeZoneName;
				}
			}
			base.OnActionExecuting(filterContext);
		}

        //http://benjii.me/2014/10/handling-validation-errors-with-angularjs-and-asp-net-mvc/
        //http://stephenwalther.com/archive/2015/01/18/asp-net-5-and-angularjs-part-5-form-validation
        protected ActionResult JsonFormResponse(JsonRequestBehavior jsonRequestBehaviour = JsonRequestBehavior.DenyGet)
        {
            if (ModelState.IsValid)
            {
                return new HttpStatusCodeResult(200);
            }

            var errorList = new List<JsonFormValidationError>();
            foreach (var key in ModelState.Keys)
            {
                ModelState modelState = null;
                if (ModelState.TryGetValue(key, out modelState))
                {
                    foreach (var error in modelState.Errors)
                    {
                        errorList.Add(new JsonFormValidationError()
                        {
                            Key = key,
                            Message = error.ErrorMessage
                        });
                    }
                }
            }

            var response = new InfoViewModel()
            {
                MsgType = InfoMsgTypes.Validation,
                Message = "",
                Errors = errorList
            };

			return new JsonNetResult() { Data = response, JsonRequestBehavior  = jsonRequestBehaviour };
        }

		protected ActionResult JsonSuccessResponse(string message, Object dataItem= null)
		{
			JsonRequestBehavior jsonRequestBehaviour = JsonRequestBehavior.DenyGet;
			if (ModelState.IsValid)
			{
				var response = new InfoViewModel()
				{
					MsgType = InfoMsgTypes.Success,
					Message = message
				};

				if (dataItem != null)
				{
					response.DataItem = dataItem;
				}

				Response.StatusCode = 200;
				return new JsonNetResult() { Data = response, JsonRequestBehavior = jsonRequestBehaviour };
			}
			else
				return JsonFormResponse();
		}

		protected T UpdateAuditTracking<T>(T dataInstance)
		{
			if (dataInstance == null)
			{
				return dataInstance;
			}

			Type t = dataInstance.GetType();
			IDataModel _instance = null;
			var _dateTimeStamp = DateTimeWrapper.NowUtc;
			try
			{
				_instance = (IDataModel)dataInstance;
				if (_instance != null)
				{
					if (_instance.CreatedAt.Year < 1950)
					{
						_instance.CreatedAt = _dateTimeStamp;
					}

					_instance.UpdatedAt = _dateTimeStamp;
					_instance.RowVersion = _dateTimeStamp;
					_instance.UpdatedBy = ClaimsPrincipal.Current?.UserName();

					dataInstance = (T)_instance;
				}
			}
			catch
			{
				
			}

			//Type t = dataInstance.GetType();
			//if (t.IsGenericType)
			//{
			//	if (t.GetGenericTypeDefinition() == typeof(IDataModel)) //check the object is our type
			//	{
			//		//Get the property value
			//		//return t.GetProperty("UpdatedAt").SetValue = DateTime.Now;
			//	}
			//}

			//dataInstance.
			//				change.Entity.UpdatedAt = DateTime.Now;
			//				change.Entity.RowVersion = DateTime.Now;
			//				change.Entity.UpdatedBy = currentUser;

			//				if (change.State == EntityState.Added)
			//				{
			//					change.Entity.CreatedAt = DateTime.Now;
			//				}

			return dataInstance;
		}

		//protected override void OnException(ExceptionContext filterContext)
		//{
		//	filterContext.ExceptionHandled = true;

		//	//Log the error!!
		//	var logger = LogManager.GetCurrentClassLogger();
		//	logger.Log(LogLevel.Error, filterContext.Exception, "Unhandled Exception");

		//	//Redirect or return a view, but not both.
		//	filterContext.Result = RedirectToAction("Error", "Pages", new { errorCode = Response.StatusCode, ex = filterContext.Exception } );
		//	// OR 
		//	//filterContext.Result = new ViewResult
		//	//{
		//	//	ViewName = "~/Views/ErrorHandler/Index.cshtml"
		//	//};
		//}
        #endregion

    }
}
