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
    public class DashboardController : BaseController
    {
        #region Constructor
        public DashboardController(MagicMaidsContext dbContext): base(dbContext)
		{
        }
        #endregion

        // GET: Dashboard
        [OutputCache(CacheProfile = "CacheForDemo")]
        public ActionResult DashboardV1()
        {
            return View();
        }

		public ActionResult ServerVars()
		{
			return View();
		}
    }
}