using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using LazyCache;
using MagicMaids.DataAccess;
using MagicMaids.ViewModels;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MagicMaidsTesting.BookingFactory_Tests")]
namespace MagicMaids
{
	public class BookingFactory
	{
		#region Fields
		private IList<CleanerMatchResultVM> resultsList;
		private SearchVM criteria;

		#endregion

		#region Constructor
		public BookingFactory(List<CleanerMatchResultVM> searchResults, SearchVM searchCriteria)
		{
			resultsList = searchResults;
			criteria = searchCriteria;

			if (criteria != null)
			{
				SearchZoneList = criteria.Suburb.GetZoneListBySuburb(false);
			}
		}
		#endregion

		#region Properties, Public
		public bool ValidSearchZone
		{
			get
			{
				return (SearchZoneList.Count > 0);
			}
		}
		#endregion 

		#region Properties, Private
		private List<String> SearchZoneList
		{
			get;
			set;
		}
		#endregion

		#region Methods, Public
		public IEnumerable<CleanerMatchResultVM> GetProcessedResults()
		{
			if (resultsList == null)
			{
				resultsList = new List<CleanerMatchResultVM>();
			}

			foreach (CleanerMatchResultVM item in resultsList)
			{
				yield return PopulateCleanerAvailability(item);
			}
		}

		#endregion

		#region Methods, Private
		private JobTypeEnum JobType
		{
			get
			{
				if (criteria.OneOffJob)
					return JobTypeEnum.OneOff;

				if (criteria.VacateClean)
					return JobTypeEnum.Vacate;

				if (criteria.FortnightlyJob)
					return JobTypeEnum.Fortnighly;

				return JobTypeEnum.Weekly;
			}
		}

		private CleanerMatchResultVM PopulateCleanerAvailability(CleanerMatchResultVM item)
		{
			if (!criteria.FilterZonesNone && !ApplyZoneFilter(item))
			{
				return null;
			}

			item.DisplayHomeBase = String.IsNullOrWhiteSpace(item.PhysicalAddress.Suburb) ? "no booking" : item.PhysicalAddress.Suburb;

			// Style Formatting
			FormatStyleForHome(item);
			FormatStyleForWeekday(item);

			// All data loaded - calculate cleaner's current availability
			AvailabilityFactory factory = new AvailabilityFactory(item, criteria.ServiceDate, criteria.ServiceLengthMins, JobType, SearchZoneList);
			try
			{
				item.ScheduledJobs = factory.GetCleanerDaySchedule();
				item.TeamSize = factory.CleanerTeamSize;
				item.SelectedRosterDay = factory.ServiceWeekDay;

				if (factory.SuitableTimeSlots == 0)
				{
					return null;
				}

			}
			catch(NoTeamRosteredException nex)
			{
				LogHelper log = new LogHelper();
				log.Log(LogHelper.LogLevels.Warning, $"Cleaner not rostered - but should not be checked", nameof(PopulateCleanerAvailability), nex, item, null);

				return null;
			}
			catch (NoSuitableGapAvailable nex2)
			{
				item.CustomErrorMessage = nex2.Message;
			}
			catch
			{
				throw;
			}

			factory = null;
			return item;
		}

		/// <summary>
		///  Creates formatted output for the Locality field in the results for home, previous or following job
		/// based on suburb selected in search resuts
		/// </summary>
		/// <returns>The location.</returns>
		/// <param name="item">Item.</param>
		//private CleanerMatchResultVM ParseJobLocation(CleanerMatchResultVM item)
		//{
		//item.StylePreviousJobLocation = FormatStyleForJobZone(true, item);
		//item.StyleNextJobLocation = FormatStyleForJobZone(false, item);
		//item.PreviousJobLocation = item.IsFirstJob ? "" : item.PreviousJobLocation;
		//item.NextJobLocation = String.IsNullOrWhiteSpace(item.NextJobLocation) ? "no booking" : item.NextJobLocation;
		//	return item;
		//}

		/// <summary>
		/// Checks if the search criteria has zone filters and if it applies to the current 
		/// cleaner's zones
		/// </summary>
		/// <returns><c>true</c>, if zone filter was applied, <c>false</c> otherwise.</returns>
		private Boolean ApplyZoneFilter(CleanerMatchResultVM item)
		{
			if (item == null)
			{
				return false;
			}

			var matchPrimary = criteria.FilterZonesPrimary;
			var matchSecondary = criteria.FilterZonesSecondary;
			var matchApproved = criteria.FilterZonesApproved;

			// nothing set - don't worry about zone filter.
			if (!(matchPrimary || matchSecondary || matchApproved))
			{
				return true;
			}

			matchPrimary = matchPrimary && item.PrimaryZoneList.Intersect(SearchZoneList).Any();
			matchSecondary = matchSecondary && item.SecondaryZoneList.Intersect(SearchZoneList).Any();
			matchApproved = matchApproved && item.ApprovedZoneList.Intersect(SearchZoneList).Any();

			if ((matchPrimary || matchSecondary || matchApproved))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Sets formatting style for the cleaner's home suburb
		/// </summary>
		/// <param name="cleaner">Cleaner.</param>
		private void FormatStyleForHome(CleanerMatchResultVM cleaner)
		{
			cleaner.StyleHomeBase = "";
			if (cleaner == null)
			{
				return;
			}

			var isFirstJob = cleaner.IsFirstJob;

			if (isFirstJob)
			{
				cleaner.StyleHomeBase = NamedColours.FirstJobColor;
			}

			var cleanerBaseZoneList = cleaner.PhysicalAddress.Suburb.GetZoneListBySuburb(false);
			if (applyColor(cleanerBaseZoneList, SearchZoneList))
			{
				cleaner.StyleHomeBase = NamedColours.PrimaryJobColor;
			}

			if (applyColor(cleanerBaseZoneList, cleaner.SecondaryZoneList))
			{
				cleaner.StyleHomeBase = NamedColours.SecondaryJobColor;
			}

			if (applyColor(cleanerBaseZoneList, cleaner.ApprovedZoneList))
			{
				cleaner.StyleHomeBase = NamedColours.ApprovedJobColor;
			}

			return;

		}

		/// <summary>
		/// Sets formatting style for day of the week based on even / odd wweeks
		/// </summary>
		/// <param name="cleaner">Cleaner.</param>
		private void FormatStyleForWeekday(CleanerMatchResultVM cleaner)
		{
			cleaner.StyleWeekday = "";
			if (cleaner == null)
			{
				return;
			}

			if (criteria.OneOffJob || criteria.VacateClean)
			{
				cleaner.StyleWeekday = criteria.ServiceDate.WeekYearStyle();
			}

			// todo fortnightly / weekly match


			return;

		}

		/// <summary>
		/// Formats the display style for the Job Suburb based on zone 
		/// for previous and next job.
		/// </summary>
		/// <returns>The style for job zone.</returns>
		/// <param name="forPrevJob">If set to <c>true</c> for previous job.</param>
		/// <param name="cleaner">Cleaner.</param>
		private String FormatStyleForJobZone(bool forPrevJob, CleanerMatchResultVM cleaner)
		{
			String searchJobLocation = (criteria == null ? "" : criteria.Suburb);
			if (String.IsNullOrWhiteSpace(searchJobLocation))
			{
				return "";
			}

			if (cleaner == null)
			{
				return "";
			}

			var prevZoneList = cleaner.PreviousJobLocation.GetZoneListBySuburb(false);
			var nextZoneList = cleaner.NextJobLocation.GetZoneListBySuburb(false);

			// if search suburb is in cleaner primary zone and prev/next job is also in primary zone
			if (applyColor(cleaner.PrimaryZoneList, SearchZoneList))
			{
				if (forPrevJob && applyColor(cleaner.PrimaryZoneList, prevZoneList))
				{
					return NamedColours.PrimaryJobColor;
				}
				else if (!forPrevJob && applyColor(cleaner.PrimaryZoneList, nextZoneList))
				{
					return NamedColours.PrimaryJobColor;
				}
			}

			// if search suburb is in cleaner secondary zone and prev/next job is also in secondary zone
			if (applyColor(cleaner.SecondaryZoneList, SearchZoneList))
			{
				if (forPrevJob && applyColor(cleaner.SecondaryZoneList, prevZoneList))
				{
					return NamedColours.SecondaryJobColor;
				}
				else if (!forPrevJob && applyColor(cleaner.SecondaryZoneList, nextZoneList))
				{
					return NamedColours.SecondaryJobColor;
				}
			}

			// if search suburb is in cleaner approved zone and prev/next job is also in approved zone
			if (applyColor(cleaner.ApprovedZoneList, SearchZoneList))
			{
				if (forPrevJob && applyColor(cleaner.ApprovedZoneList, prevZoneList))
				{
					return NamedColours.ApprovedJobColor;
				}
				else if (!forPrevJob && applyColor(cleaner.ApprovedZoneList, nextZoneList))
				{
					return NamedColours.ApprovedJobColor;
				}
			}

			return "";
		}



		/// <summary>
		/// Applies color if zonelist contains zones in the compare list
		/// </summary>
		/// <returns><c>true</c>, if color was applied, <c>false</c> otherwise.</returns>
		/// <param name="cleanerZoneList">Cleaner zone list.</param>
		/// <param name="compareList">Compare list.</param>
		private Boolean applyColor(List<String> cleanerZoneList, List<String> compareList)
		{
			if (cleanerZoneList.Intersect(compareList).Any())
			{
				return true;
			}

			return false;
		}

		#endregion
	}
}
