#region Using
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using MagicMaids.DataAccess;
using MagicMaids.EntityModels ;
#endregion

namespace MagicMaids.Controllers
{
	public class ClientsController : BaseController
	{

		// todo - get dependency injection
		//http://www.dotnetcurry.com/aspnet-mvc/1155/aspnet-mvc-repository-pattern-perform-database-operations
		#region Fields
		//private IRepository<Client> repository;
		#endregion

		#region Constructor
		public ClientsController(MagicMaidsContext dbContext) : base(dbContext)
        {
		}
		#endregion 

		#region Method, Public
		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult Clients()
		{
			bool _chkShowDisabled = false;

			//if (repository ==  null)
			//{
			//	repository = new ClientRepository();
			//}
			//List<Client> _clients = repository.GetAll(_chkShowDisabled).ToList<Client>();

			//return View(_clients);
			return View();
		}

		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult ClientDetails()
		{
			TempData["ClientName"] = "Sue Skeleton";
			return View();
		}
		#endregion

		#region Service Functions
		//// GET clients
		//public JsonResult GetClients()
		//{
		//	bool _chkShowDisabled = false;
		//	bool _chkShowActive = true;

		//	if (repository == null)
		//		repository	= new ClientRepository();

		//	List<Client> _itemList = repository.GetAll(_chkShowActive, _chkShowDisabled).ToList<Client>();

		//	return Json(new { list = _itemList }
		//		, JsonRequestBehavior.AllowGet);
		//}
		#endregion
	}
}
