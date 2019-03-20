using System;
using System.Collections.Generic;
using System.Linq;
using MagicMaids.ViewModels;
using System.Runtime.CompilerServices;
using MagicMaids.EntityModels;

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

            foreach (CleanerMatchResultVM CleanerMatchResult in resultsList)
            {
                yield return PopulateCleanerAvailability(CleanerMatchResult);
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

        private CleanerMatchResultVM PopulateCleanerAvailability(CleanerMatchResultVM CleanerMatchResult)
        {
            if (!criteria.FilterZonesNone && !ApplyZoneFilter(CleanerMatchResult))
            {
                return null;
            }

            CleanerMatchResult.DisplayHomeBase = String.IsNullOrWhiteSpace(CleanerMatchResult.PhysicalAddress.Suburb) ? "no booking" : CleanerMatchResult.PhysicalAddress.Suburb;

            // Style Formatting
            // StyleHomebase should always be the primary zone colour           
            CleanerMatchResult.StyleHomeBase = NamedColours.PrimaryJobColor;
            CleanerMatchResult.StyleZoneClass = CalculateSuburbColourCode(CleanerMatchResult);
            CleanerMatchResult.ZoneClass = SetZoneClass(CleanerMatchResult);
            FormatStyleForWeekday(CleanerMatchResult);

            // All data loaded - calculate cleaner's current availability
            AvailabilityFactory Availability = new AvailabilityFactory(CleanerMatchResult, criteria.ServiceLengthMins, JobType, SearchZoneList);

            try
            {
                CleanerMatchResult.SearchSuburb = criteria.Suburb; 
                CleanerMatchResult.ScheduledJobs = Availability.GetCleanerDaySchedule();
                CleanerMatchResult.ScheduledJobsNextWeek = Availability.GetCleanerDaySchedule(true);
                CleanerMatchResult.TeamSize = Availability.CleanerTeamSize;
                                

                if (Availability.SuitableTimeSlots == 0)
                {
                    return null;
                }
                
                var leaveDates = AvailabilityFactory.GetCleanerLeaveDates(new Guid(CleanerMatchResult.Id), true).FirstOrDefault<CleanerLeaveVM>();
                if (leaveDates != null)
                {
                    CleanerMatchResult.CleanerOnLeave = ((DayOfWeek)criteria.ServiceDayValue).IsDayInRange(leaveDates.StartDate, leaveDates.EndDate);
                    CleanerMatchResult.LeaveDates = DateTimeWrapper.FormatDateRange(leaveDates.StartDate, leaveDates.EndDate);
                }
                
            }
            catch (NoTeamRosteredException nex)
            {
                LogHelper log = new LogHelper();
                log.Log(LogHelper.LogLevels.Warning, $"Cleaner not rostered - but should not be checked", nameof(PopulateCleanerAvailability), nex, CleanerMatchResult, null);

                return null;
            }
            catch (NoSuitableGapAvailable nex2)
            {
                CleanerMatchResult.CustomErrorMessage = nex2.Message;
            }
            catch
            {
                throw;
            }

            Availability = null;
            return CleanerMatchResult;
        }        

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

        private ZoneMatch GetZoneMatch(CleanerMatchResultVM item)
        {
            if (item == null)
            {
                return ZoneMatch.None;
            }

            var matchPrimary = criteria.FilterZonesPrimary;
            var matchSecondary = criteria.FilterZonesSecondary;
            var matchApproved = criteria.FilterZonesApproved;

            matchPrimary = matchPrimary && item.PrimaryZoneList.Intersect(SearchZoneList).Any();
            matchSecondary = matchSecondary && item.SecondaryZoneList.Intersect(SearchZoneList).Any();
            matchApproved = matchApproved && item.ApprovedZoneList.Intersect(SearchZoneList).Any();

            if (matchPrimary)
            {
                return ZoneMatch.Primary;
            }
            else if (matchSecondary)
            {
                return ZoneMatch.Secondary;
            }
            else if (matchApproved)
            {
                return ZoneMatch.Approved;
            }

            return ZoneMatch.None;
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
            if (ApplyColor(cleanerBaseZoneList, SearchZoneList))
            {
                cleaner.StyleHomeBase = NamedColours.PrimaryJobColor;
            }

            if (ApplyColor(cleanerBaseZoneList, cleaner.SecondaryZoneList))
            {
                cleaner.StyleHomeBase = NamedColours.SecondaryJobColor;
            }

            if (ApplyColor(cleanerBaseZoneList, cleaner.ApprovedZoneList))
            {
                cleaner.StyleHomeBase = NamedColours.ApprovedJobColor;
            }

            return;

        }

        private string CalculateSuburbColourCode(CleanerMatchResultVM cleaner)
        {
            SuburbZone searchSuburbDetails = criteria.Suburb.GetSuburbDetails();

            if (searchSuburbDetails.Zone == cleaner.PrimaryZone)
            {
                return NamedColours.PrimaryJobColor;
            }

            if (cleaner.SecondaryZoneList.Contains(searchSuburbDetails.Zone))
            {
                return NamedColours.SecondaryJobColor;
            }

            return NamedColours.ApprovedJobColor;

        }

        private string SetZoneClass(CleanerMatchResultVM cleaner)
        {
            SuburbZone searchSuburbDetails = criteria.Suburb.GetSuburbDetails();

            if (searchSuburbDetails.Zone == cleaner.PrimaryZone)
            {
                return "Primary";
            }

            if (cleaner.SecondaryZoneList.Contains(searchSuburbDetails.Zone))
            {
                return "Secondary";
            }

            if (cleaner.ApprovedZoneList.Contains(searchSuburbDetails.Zone))
            {
                return "Approved";
            }

            return "Other";
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
                cleaner.StyleWeekday = criteria.ServiceDateUser.Date.WeekYearStyle();
                cleaner.StyleWeekdayNextWeek = criteria.ServiceDateUser.Date.WeekYearStyle(true);
            }
            else
            {
                cleaner.StyleWeekday = DateTimeWrapper.FindNextDateForDay((DayOfWeek)criteria.ServiceDayValue).Date.WeekYearStyle();
                cleaner.StyleWeekdayNextWeek = DateTimeWrapper.FindNextDateForDay((DayOfWeek)criteria.ServiceDayValue).Date.WeekYearStyle(true);
            }

            return;

        } 

        /// <summary>
        /// Applies color if zonelist contains zones in the compare list
        /// </summary>
        /// <returns><c>true</c>, if color was applied, <c>false</c> otherwise.</returns>
        /// <param name="cleanerZoneList">Cleaner zone list.</param>
        /// <param name="compareList">Compare list.</param>
        private Boolean ApplyColor(List<String> cleanerZoneList, List<String> compareList)
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
