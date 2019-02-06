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
using LazyCache;
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

		[HttpGet]
		public JsonResult GetSearchSuburbs()
		{
			IAppCache cache = new CachingService();
			List<String> suburbs = (List<string>)cache.GetOrAdd("SuburbNames", () => GetSuburbNames(), new TimeSpan(1, 0, 0));
			return new JsonNetResult() { Data = new { item = suburbs }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		public ActionResult MatchCleaners(SearchVM searchCriteria, String CleanerId = null)
		{
			Boolean searchByCriteria = SetSearchByCriteria(CleanerId);
						
			if (searchByCriteria)
			{
                SetServiceDate(ref searchCriteria);
                ValidateSearchCriteria(searchCriteria);
			}

			if (ModelState.IsValid)
			{
				try
				{
					searchCriteria.HasCriteria = true;
					//searchCriteria.StoreSearchCookieCiteria("cleanerMatch");

                    List<CleanerMatchResultVM> CleanerMatchResultList = GetCleanerMatchResults(searchCriteria, searchByCriteria, CleanerId);

                    SetCleanerMatchProperties(ref CleanerMatchResultList, searchCriteria);     

					if (searchByCriteria)
					{
						BookingFactory resultsProcessor = new BookingFactory(CleanerMatchResultList, searchCriteria);
						if (!resultsProcessor.ValidSearchZone)
						{
							ModelState.AddModelError(string.Empty, $"The suburb '{searchCriteria.Suburb.ToUpper()}' does not have any zones defined yet.");
						}
						else
						{
							var results = resultsProcessor.GetProcessedResults().ToList<CleanerMatchResultVM>().Where(x => x != null);
							return new JsonNetResult() { Data = new { SearchResults = results }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
						}
					}
					else
					{
						var results = CleanerMatchResultList[0];
						return new JsonNetResult() { Data = new { SearchResults = results }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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

		#region Methods, Private
		private IList<String> GetSuburbNames()
		{
			List<String> suburbs = new List<string>();
			using (DBManager db = new DBManager())
			{
				suburbs = db.getConnection().Query<String>($"select distinct SuburbName from SuburbZones order by SuburbName").ToList();
			}
			return suburbs;

		}

        private Boolean SetSearchByCriteria(String  CleanerId)
        {
            if (!String.IsNullOrWhiteSpace(CleanerId))
            {               
                ModelState.Clear();
                return false;
            }

            return true;
        }

        private void SetServiceDate(ref SearchVM searchCriteria)
        {
            if (searchCriteria.WeeklyJob || searchCriteria.FortnightlyJob)
            {
                searchCriteria.ServiceDate = DateTime.MinValue;
            }
        }

        private void ValidateSearchCriteria(SearchVM searchCriteria)
        {
            if (searchCriteria == null)
            {
                ModelState.AddModelError(string.Empty, $"No search criteria specified.");
            }

            if (searchCriteria.ServiceLengthMins > SystemSettings.WorkSessionMaxHours * 60)
            {
                ModelState.AddModelError(string.Empty, $"Service duration can not exceed { SystemSettings.WorkSessionMaxHours} hours.");
            }

            if ((searchCriteria.OneOffJob || searchCriteria.VacateClean) && ((searchCriteria.ServiceDate - DateTime.Now.ToUTC()).TotalDays > SystemSettings.BookingsDaysAllowed))
            {
                ModelState.AddModelError(string.Empty, $"Services can't be booked more than {SystemSettings.BookingsDaysAllowed} days in advance.");
            }
        }

        private StringBuilder BuildCleanerMatchSQL(SearchVM searchCriteria, Boolean searchByCriteria, String CleanerId)
        {
            StringBuilder sql = new StringBuilder(@"select * from Cleaners C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID where C.IsActive=1");

            if (searchByCriteria)
            {
                if (searchCriteria.RequireIroning)
                {
                    sql.Append($" and Ironing = {searchCriteria.RequireIroning}");
                }

                if (searchCriteria.FilterRating > 0)
                {
                    sql.Append($" and C.Rating >= {searchCriteria.FilterRating}");
                }

                if (searchCriteria.OneOffJob || searchCriteria.VacateClean)
                {
                    // not on leave
                    sql.Append($" and C.ID not in (select distinct PrimaryCleanerRefId from CleanerLeave where '{searchCriteria.ServiceDate.ToUTC().FormatDatabaseDate()}' between DATE(StartDate) and DATE(EndDate))");
                }
                
                // and rostered for weekday ????
                sql.Append($" and C.ID in (select distinct PrimaryCleanerRefId from CleanerRoster where Upper(WeekDay) = Upper('{searchCriteria.ServiceDay}'))");
            }
            else
            {
                sql.Append($" and C.ID = '{CleanerId}'");
            }

            sql.Append(" order by LastName, FirstName");

            return sql;
        }

        private List<CleanerMatchResultVM> GetCleanerMatchResults(SearchVM searchCriteria, Boolean searchByCriteria, String CleanerId)
        {
            StringBuilder sql = BuildCleanerMatchSQL(searchCriteria, searchByCriteria, CleanerId);

            List<CleanerMatchResultVM> _vmResults;
            using (DBManager db = new DBManager())
            {
                var _orderedResults = db.getConnection().Query<Cleaner, Address, Cleaner>(sql.ToString(), (cl, phys) =>
                {
                    cl.PhysicalAddress = phys;
                    return cl;
                }).ToList();

                _vmResults = Mapper.Map<List<Cleaner>, List<CleanerMatchResultVM>>(_orderedResults);
            }

            return _vmResults;
        }

        private void SetCleanerMatchProperties(ref List<CleanerMatchResultVM> CleanerMatchResultList, SearchVM searchCriteria)
        {
            foreach (CleanerMatchResultVM CleanerMatchResult in CleanerMatchResultList)
            {               

                CleanerMatchResult.PrimaryZoneList = new List<string>(new string[] { CleanerMatchResult.PrimaryZone });
                CleanerMatchResult.SelectedRosterDay = searchCriteria.ServiceDay.ToDayOfWeek();
                CleanerMatchResult.SelectedServiceDate = (searchCriteria.WeeklyJob || searchCriteria.FortnightlyJob) ? DateTimeWrapper.FindNextDateForDay(CleanerMatchResult.SelectedRosterDay) : searchCriteria.ServiceDate.ToUser();

                if (!String.IsNullOrWhiteSpace(CleanerMatchResult.SecondaryZone))
                {
                    CleanerMatchResult.SecondaryZoneList = CleanerMatchResult.SecondaryZone.Split(new char[] { ',', ';' })
                        .Distinct()
                        .ToList();
                }
                else
                {
                    CleanerMatchResult.SecondaryZoneList = new List<string>();
                }

                if (!String.IsNullOrWhiteSpace(CleanerMatchResult.ApprovedZone))
                {
                    CleanerMatchResult.ApprovedZoneList = CleanerMatchResult.ApprovedZone.Split(new char[] { ',', ';' })
                        .Distinct()
                        .ToList();
                }
                else
                {
                    CleanerMatchResult.ApprovedZoneList = new List<string>();
                }
            }
        }

        #endregion

    }
}
