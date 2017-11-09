#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MagicMaids.DataAccess;
using MagicMaids.EntityModels;

using NLog;
#endregion

namespace MagicMaids.Controllers
{
    public class PagesController : Controller
    {
		#region Methods, Public
		public ActionResult NotFound( string path)
		{
			Response.StatusCode = 404;

			ViewBag.Path = path;

			return View("Error404");
		}

		public ActionResult Internal(string path)
		{
			Response.StatusCode = 500;

			ViewBag.Path = path;

			return View("Error500");
		}

		public ActionResult Error(int? errorCode, string path)
		{
			Response.StatusCode = errorCode ?? 0;

			String _view = string.Empty;

			if (ViewData.Model != null && ViewData.Model is System.Web.Mvc.HandleErrorInfo)
			{
				var container = ((HandleErrorInfo)ViewData.Model);
				var ex = container.Exception;
				if (ex != null)
				{
					ViewBag.Message = ex.Message;
				}

			}
			ViewBag.Code = errorCode;
			ViewBag.Path = path;

			switch(errorCode)
			{
				case 404:
					_view = "Error404";
					break;
				case 500:
					_view = "Error500";
					break;
				default:
					break;
			}
			return View(_view);
		}

		#endregion
    }
}
