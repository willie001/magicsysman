#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using MagicMaids.DataAccess;
using MagicMaids.ViewModels;

using NLog;

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
			Log = LogManager.GetLogger(GetType().FullName);
			//MagicMaidsInitialiser.CheckConnection();
		}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        #endregion

        #region Properties, Protected
        protected Logger Log { get; private set; }
        #endregion

        #region Methods, Protected
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

            //Response.StatusCode = 400;
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
