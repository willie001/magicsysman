#region Using
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

using AutoMapper;
using Dapper;
using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
using MagicMaids.ViewModels;

#endregion

namespace MagicMaids.Controllers
{
	public class SearchController : BaseController
	{
		#region Constructor
		public SearchController() : base()
		{
		}
		#endregion

		#region Methods, Public
		public ActionResult Search()
		{
			return View();
		}
		#endregion

		#region Service Functions
		[HttpGet]
		public JsonResult GetSearchCriteria()
		{
			SearchVM searchCriteria = new SearchVM();
			searchCriteria = searchCriteria.RestoreSearchCookieCriteria("cleanerMatch");

			return new JsonNetResult() { Data = new { item = searchCriteria }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpDelete]
		public void ClearSearchCriteria()
		{
			var cookieName = "SearchCriteria_cleanerMatch";
			if (Response.Cookies[cookieName] != null)
			{
				HttpCookie cookie = Request.Cookies[cookieName];
				Response.Cookies.Remove(cookieName);
				cookie.Expires = DateTime.Now.AddDays(-10);  // or any other time in the past
				cookie.Value = null;
				Response.Cookies.Set(cookie);
				Request.Cookies.Set(cookie);
			}
		}

		[HttpPost]
		public ActionResult MatchCleaners(SearchVM searchCriteria)
		{
			if (searchCriteria == null)
			{
				ModelState.AddModelError(string.Empty, $"No search criteria specified.");
			}

			if (searchCriteria.ServiceLength > SystemSettings.WorkSessionMaxHours)
			{
				ModelState.AddModelError(string.Empty, $"Service duration can not exceed { SystemSettings.WorkSessionMaxHours} hours.");
			}

			if ((searchCriteria.OneOffJob || searchCriteria.VacateClean) && ((searchCriteria.ServiceDate-DateTime.Now.ToUTC()).TotalDays > SystemSettings.BookingsDaysAllowed))
			{
				ModelState.AddModelError(string.Empty, $"Services can't be booked more than {SystemSettings.BookingsDaysAllowed} days in advance.");
			}

			if (ModelState.IsValid)
			{
				try
				{
					searchCriteria.HasCriteria = true;
					searchCriteria.StoreSearchCookieCiteria("cleanerMatch");

					using (DBManager db = new DBManager())
					{
						StringBuilder sql = new StringBuilder(@"select * from Cleaners C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID where C.IsActive=1");

						if (searchCriteria.RequireIroning)
						{
							sql.Append($" and Ironing = {searchCriteria.RequireIroning}");
						}

						if (!String.IsNullOrWhiteSpace(searchCriteria.Suburb))
						{
							sql.Append($" and (Ph.Suburb like '%{searchCriteria.Suburb}%' or Ph.PostCode = '{searchCriteria.Suburb}')");
						}

						if (searchCriteria.FilterRating > 0)
						{
							sql.Append($" and C.Rating >= {searchCriteria.FilterRating}");
						}

						sql.Append(" order by LastName, FirstName");

						var _orderedResults = db.getConnection().Query<Cleaner, Address, Cleaner>(sql.ToString(), (cl, phys) => {
							cl.PhysicalAddress = phys;
							return cl;
						}).ToList();

						var _vmResults = Mapper.Map<List<Cleaner>, List<CleanerJobMatchVM>>(_orderedResults);

						foreach(CleanerJobMatchVM _item in _vmResults)
						{
							_item.PrimaryZoneList = new List<string>(new string[] { _item.PrimaryZone });
							if (!String.IsNullOrWhiteSpace(_item.SecondaryZone))
							{
								_item.SecondaryZoneList = _item.SecondaryZone.Split(new char[] { ',', ';' })
									.Distinct()
									.ToList();
							}
							else
							{
								_item.SecondaryZoneList = new List<string>();
							}
								
							if (!String.IsNullOrWhiteSpace(_item.ApprovedZone))
							{
								_item.ApprovedZoneList = _item.ApprovedZone.Split(new char[] { ',', ';' })
									.Distinct()
									.ToList();
							}
							else
							{
								_item.ApprovedZoneList = new List<string>();
							}
						}

						BookingFactory resultsProcessor = new BookingFactory(_vmResults, searchCriteria);
						return new JsonNetResult() { Data = new { SearchResults = resultsProcessor.GetProcessedResults() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, $"Error performing search ({ex.Message})");

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, "Error performing search", nameof(SearchController), ex, null);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SearchController), null);
			}

			return JsonFormResponse();
		}
		#endregion



	}
}
