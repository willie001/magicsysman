#region Using
using MagicMaids.DataAccess;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
#endregion

namespace MagicMaids.Controllers
{
    public class CleanersController : BaseController
    {
		// todo - get dependency injection
		//http://www.dotnetcurry.com/aspnet-mvc/1155/aspnet-mvc-repository-pattern-perform-database-operations
		#region Fields
		//private IRepository<Cleaner> repository;
		#endregion

		#region Constructors
		public CleanersController(MagicMaidsContext dbContext) : base(dbContext)
        {
			
		}
		#endregion

		#region Properties, Public
		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult Cleaners()
		{
			bool _chkShowDisabled = false;

			//if (repository == null)
			//{
			//	repository = new CleanerRepository();
			//}
			//List<Cleaner> _cleaners = repository.GetAll(_chkShowDisabled).ToList<Cleaner>();

			//return View(_cleaners);
			return View();
		}

		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult CleanerDetails()
		{
			TempData["CleanerName"] = "Joe Bloggs";
			return View();
		}
		#endregion
    }
}
