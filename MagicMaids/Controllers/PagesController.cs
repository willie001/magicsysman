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
		public ActionResult Error404()
		{
			return View();
		}

		public ActionResult Error500()
		{
			return View();
		}

		public ActionResult Error(int? errorCode, Exception ex)
		{
			Response.StatusCode = errorCode ?? 0;

			String _view = string.Empty;

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
