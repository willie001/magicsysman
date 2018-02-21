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
    public class PartialsController : BaseController
    {
        #region Constructor
        public PartialsController(): base()
		{
        }
        #endregion

        [OutputCache(CacheProfile = "CacheForDemo")]
        public ActionResult TopNavbar()
        {
            return PartialView();
        }

		[OutputCache(CacheProfile = "CacheForDemo")]
        public ActionResult Sidebar()
        {
            return PartialView();
        }

		[OutputCache(CacheProfile = "CacheForDemo")]
        public ActionResult Footer()
        {
            return PartialView();
        }
    }
}