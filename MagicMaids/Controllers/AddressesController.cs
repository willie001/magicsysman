#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;

using MagicMaids.DataAccess;
using MagicMaids.EntityModels;

using NLog;
#endregion

namespace MagicMaids.Controllers
{
	public class AddressesController : BaseController
	{
		#region Constructor
		public AddressesController(MagicMaidsContext dbContext): base(dbContext)
		{
		}
		#endregion

		#region Methods, Public
		public ActionResult Addresses()
		{
			return View();
		}
		#endregion

		#region Service Functions
		[HttpGet]
		public JsonResult GetAddressTypesJson()
		{
			var enumVals = new List<object>();

			foreach (var item in Enum.GetValues(typeof(AddressTypeSetting)))
			{

				enumVals.Add(new
				{
					id = (int)item,
					name = item.ToString()
				});
			}

			return Json(new { item = enumVals }, JsonRequestBehavior.AllowGet);
		}
		#endregion
	}
}
