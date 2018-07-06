#region Using
using MagicMaids.DataAccess;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
#endregion 

namespace MagicMaids.Controllers
{
    public class AppController : BaseController
    {

        #region Constructor
		public AppController() : base()
		{
		}
        #endregion

        // GET: Home
        public ActionResult Index()
        {
			return View();
        }


    }
}