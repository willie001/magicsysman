#region Using
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using LazyCache;
using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
#endregion

namespace MagicMaids.Controllers
{
	public class AddressesController : BaseController
	{
		#region Constructor
		public AddressesController() : base()
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
		public JsonNetResult GetAddressTypesJson()
		{
			var enumVals = new List<object>();

			IAppCache cache = new CachingService();
			enumVals = cache.GetOrAdd("address_types", () => GetAddressTypes(), new TimeSpan(8, 0, 0));

			return new JsonNetResult() { Data = new { item = enumVals }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		#endregion

		#region Methods,Private
		private List<object> GetAddressTypes()
		{
			var enumVals = new List<object>();

			foreach (var item in Enum.GetValues(typeof(AddressTypeSetting)))
			{

				enumVals.Add(new
				{
					id = (int) item,
					name = item.ToString()
				});
			}

			return enumVals;
		}
		#endregion 
	}
}
