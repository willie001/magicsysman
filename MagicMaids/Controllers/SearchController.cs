#region Using
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using AutoMapper;
using Dapper;
using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
using MagicMaids.ViewModels;

using NLog;
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
		[HttpPost]
		public ActionResult MatchCleaners(SearchVM searchCriteria)
		{
			if (searchCriteria == null)
			{
				ModelState.AddModelError(string.Empty, $"No search criteria specified.");
			}

			if (ModelState.IsValid)
			{
				try
				{
					using (IDbConnection db = MagicMaidsInitialiser.getConnection())
					{
						StringBuilder sql = new StringBuilder(@"select * from Cleaners C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID where 1=1");

						if (searchCriteria.RequireIroning)
						{
							sql.Append($" and Ironing = {searchCriteria.RequireIroning}");
						}

						if (!String.IsNullOrWhiteSpace(searchCriteria.Suburb))
						{
							sql.Append($" and (Ph.Suburb like '%{searchCriteria.Suburb}%' or Ph.PostCode = '{searchCriteria.Suburb}')");
						}

						sql.Append(" order by LastName, FirstName");

						var _orderedResults = db.Query<Cleaner, Address, Cleaner>(sql.ToString(), (cl, phys) => {
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

							_item.SearchMatchScore = CalculateSearchMatchScore(searchCriteria, _item);

						}

						return new JsonNetResult() { Data = new { SearchResults = _vmResults }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, $"Error performing search ({ex.Message})");

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, "Error performing search", nameof(SearchController), ex, null);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SearchController), null);
			}

			return JsonFormResponse();
		}
		#endregion


		#region Methods, Private
		private Int32 CalculateSearchMatchScore(SearchVM searchCriteria, CleanerJobMatchVM item)
		{
			if (item == null || searchCriteria == null)
			{
				return 0;
			}

			string searchArea = searchCriteria.Suburb.Trim().ToLower();
			string cleanerSuburb = item.PhysicalAddress.Suburb.Trim().ToLower();
			string cleanerPostCode = item.PhysicalAddress.PostCode.Trim();

			var zoneList = SettingsController.GetZoneListBySuburb(searchArea);

			if (zoneList.Intersect(item.PrimaryZoneList).Count() > 0)
			{
				return 100;		// primary zone matching
			}

			if (zoneList.Intersect(item.SecondaryZoneList).Count() > 0)
			{
				return 75;     // secondary zone matching
			}

			if (zoneList.Intersect(item.ApprovedZoneList).Count() > 0)
			{
				return 50;     // secondary zone matching
			}


			return 0;

		}


		#endregion
	}
}
