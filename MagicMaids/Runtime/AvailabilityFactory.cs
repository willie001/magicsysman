using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Dapper;
using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
using MagicMaids.ViewModels;

namespace MagicMaids
{
    public class AvailabilityFactory
    {
        #region Fields
        private CleanerMatchResultVM Cleaner;
        public DateTime ServiceDateUTC; //UTC
        public DateTime ServiceDateUTCNextWeek;
        private Int32 ServiceLenghtMinutes;
        private IList<String> ServiceZone;
        private JobTypeEnum JobType;
        private DayOfWeek ServiceDay;
        private long RosterDayEndTime;


        const Int32 minJobSizeMins = 0;
        #endregion

        #region Constructor
        public AvailabilityFactory(CleanerMatchResultVM cleaner, Int32 serviceLenghtMins, JobTypeEnum serviceType, IList<String> serviceZone)
        {
            ValidateConstructorArguments(cleaner, serviceLenghtMins, serviceZone);

            if (cleaner.SelectedServiceDate.HasValue && cleaner.SelectedServiceDate != default(DateTime))
            {
                ServiceDateUTC = cleaner.SelectedServiceDate.Value.ToUTC();
            }
            else if (JobType == JobTypeEnum.Fortnighly || JobType == JobTypeEnum.Weekly)
            {
                // calculate next date
                ServiceDateUTC = DateTimeWrapper.FindNextDateForDay((DayOfWeek)ServiceDay);
            }

            ServiceDateUTCNextWeek = ServiceDateUTC.AddDays(7);

            ServiceDay = cleaner.SelectedRosterDay;
            JobType = serviceType;
            ServiceLenghtMinutes = serviceLenghtMins;
            ServiceZone = serviceZone;

        }
        #endregion

        #region Properties

        public Int32 CleanerTeamSize
        {
            get
            {
                return Cleaner.TeamSize;
            }
        }

        // number of timeslots available and matching gap searched for
        public Int32 SuitableTimeSlots
        {
            get;
            set;
        }

        /// <summary>
        /// The Adjusted availability gap - calculated from the search's service gap requirement
        /// + the cleaner's travel time allowance for the zone.
        /// </summary>
        /// <value>The adjusted gap.</value>
        private Int32 AdjustedGapMins
        {
            get
            {
                if (!_gapAdjusted)
                {
                    _adjustedGap = AdjustGap();
                    _gapAdjusted = true;
                }
                return _adjustedGap;
            }
        }
        private Int32 _adjustedGap = 0;
        private Boolean _gapAdjusted = false;

        #endregion

        #region Methods, Public
        /// <summary>
        /// Gets the list of bookings combined with available time slots for the day
        /// </summary>
        /// <returns>The cleaner availability.</returns>
        internal IList<JobBookingsVM> GetCleanerDaySchedule(Boolean NextWeek = false)
        {
            
            long rosterDayStart = 0;
            long rosterDayEnd = 0;
            SuitableTimeSlots = 0;

            var cleanerRosterList = GetCleanerRoster(); //Cleaner roster is the times that the cleaner team are available
            foreach (CleanerRosterVM cleanerRosterItem in cleanerRosterList)
            {
                if (cleanerRosterItem.Weekday.Equals(ServiceDay.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    Cleaner.SelectedRosterDay = ServiceDay;
                    Cleaner.TeamSize = cleanerRosterItem.TeamCount;
                    rosterDayStart = cleanerRosterItem.TimeOfDayFrom;
                    rosterDayEnd = cleanerRosterItem.TimeOfDayTo;
                    RosterDayEndTime = cleanerRosterItem.TimeOfDayTo;
                    break;
                }
            }

            if (rosterDayStart == 0)
            {
                // Cleaner team is not available on this day
                throw new NoTeamRosteredException(Cleaner.FirstName + ' ' + Cleaner.LastName, ServiceDay.ToString());
            }
            
            var existingScheduleListAll = GetCleanerFutureBookings(NextWeek);
            var dayList = BuildDaylist(existingScheduleListAll, rosterDayStart, rosterDayEnd, NextWeek);

            return dayList;
        }

        private List<JobBookingsVM> BuildDaylist(IEnumerable<JobBookingsVM> jobList, long rosterDayStart, long rosterDayEnd, Boolean NextWeek)
        {
            var dayList = new List<JobBookingsVM>();
            var existingScheduleListFiltered = new List<JobBookingsVM>();
            long previousEndTime = 0;
            DateTime serviceDate;

            if (NextWeek)
            {
                serviceDate = Cleaner.SelectedServiceDateNextWeek ?? DateTime.Now;
            }
            else
            {
                serviceDate = Cleaner.SelectedServiceDate ?? DateTime.Now;
                
            }

            serviceDate = serviceDate.ToUser();

            foreach (JobBookingsVM existingScheduleItem in jobList)
            {
                if (existingScheduleItem.JobType == JobTypeEnum.Fortnighly && existingScheduleItem.JobWeekYearStyle == DateTimeWrapper.WeekYearStyle(serviceDate)) //Cleaner.StyleWeekday
                {
                    existingScheduleListFiltered.Add(existingScheduleItem);
                }
                else if (existingScheduleItem.JobType != JobTypeEnum.Fortnighly)
                {
                    existingScheduleListFiltered.Add(existingScheduleItem);
                }
            }

            foreach (JobBookingsVM existingScheduleItem in existingScheduleListFiltered)
            {

                if (previousEndTime == rosterDayEnd)
                {
                    break;
                }

                if (existingScheduleItem.WeekDay == ServiceDay.ToString())
                {
                    AddAvailableTimeSlot(dayList, (previousEndTime == 0 ? rosterDayStart : previousEndTime), existingScheduleItem.StartTime, serviceDate, existingScheduleListFiltered);
                    dayList.Add(existingScheduleItem);
                    previousEndTime = existingScheduleItem.EndTime;
                }
            }

            // adds the last gap of the day as available
            if (previousEndTime > 0 && previousEndTime < rosterDayEnd)
            {
                AddAvailableTimeSlot(dayList, previousEndTime, rosterDayEnd, serviceDate, existingScheduleListFiltered);
            }

            if (dayList.Count == 0)
            {
                // no jobs - all day is available
                AddAvailableTimeSlot(dayList, rosterDayStart, rosterDayEnd, serviceDate, existingScheduleListFiltered);
            }

            if (dayList.Count == 0)
            {
                throw new NoSuitableGapAvailable(ServiceDay.ToString(), minJobSizeMins + AdjustedGapMins);
            }

            return dayList;
        }

        /// <summary>
        /// Gets a list of cleaner's leave dates.
        /// </summary>
        /// <returns>The cleaner leave dates.</returns>
        /// <param name="cleanerId">Cleaner identifier.</param>
        /// <param name="activeOnly">If set to <c>true</c> active only.</param>
        internal static IEnumerable<CleanerLeaveVM> GetCleanerLeaveDates(Guid cleanerId, Boolean activeOnly)
        {
            List<CleanerLeave> _entityList = new List<CleanerLeave>();
            StringBuilder _sql = new StringBuilder($"Select * from CleanerLeave where PrimaryCleanerRefId = '{cleanerId.ToString()}'");
            if (activeOnly)
            {
                _sql.Append($" and StartDate >= '{DateTimeWrapper.NowUtc.FormatDatabaseDate()}'");
            }
            _sql.Append(" order by StartDate desc, EndDate desc");

            using (DBManager db = new DBManager())
            {
                _entityList = db.getConnection().Query<CleanerLeave>(_sql.ToString()).ToList();
            }

            foreach (CleanerLeave _item in _entityList)
            {
                var _vm = new CleanerLeaveVM();
                _vm.PopulateVM(cleanerId, _item);
                yield return _vm;
            }
        }
        #endregion

        #region Methods, Static

        /// <summary>
        /// Gets the cleaner avalability roster.
        /// </summary>
        /// <returns>The cleaner roster.</returns>
        internal List<CleanerRosterVM> GetCleanerRoster()
        {
            List<CleanerRoster> _entityList = new List<CleanerRoster>();
            StringBuilder _sql = new StringBuilder($"Select ID RosterID, PrimaryCleanerRefId, WeekDay, StartTime as TimeOfDayFrom, EndTime as TimeOfDayTo, TeamCount from CleanerRoster where PrimaryCleanerRefId = '{Cleaner.Id}'");

            using (DBManager db = new DBManager())
            {
                _entityList = db.getConnection().Query<CleanerRoster>(_sql.ToString()).ToList();
            }

            var _vmResults = Mapper.Map<List<CleanerRoster>, List<CleanerRosterVM>>(_entityList);
            return _vmResults;
        }
        #endregion

        #region Methods, Private
        /// <summary>
        /// Gets the cleaner future job bookings.
        /// </summary>
        /// <returns>The cleaner future bookings.</returns>
        private IEnumerable<JobBookingsVM> GetCleanerFutureBookings(Boolean NextWeek = false)
        {
            DateTime bookingDate;

            //if (NextWeek)
            //{ bookingDate = DateTime.Now.AddDays(7); }
            //else
            //{ bookingDate = DateTime.Now; }

            if (NextWeek)
            { bookingDate = ServiceDateUTC.ToUser().AddDays(7).Date; }
            else
            { bookingDate = ServiceDateUTC.ToUser().Date; }

            List<JobBooking> _entityList = new List<JobBooking>();
            StringBuilder _sql = new StringBuilder($"Select * from JobBooking where PrimaryCleanerRefId = '{Cleaner.Id}'");
            _sql.Append($" and JobStatus in ('{BookingStatus.CONFIRMED}', '{BookingStatus.PENDING}')");
            if (!String.IsNullOrWhiteSpace(ServiceDay.ToString()))
            {
                _sql.Append($" and WeekDay = '{ServiceDay.ToString()}'");
            }
            _sql.Append($" and (JobDate = '{bookingDate.FormatDatabaseDate()}' or JobType in ('Fortnighly', 'Weekly'))");
            _sql.Append(" order by StartTime, EndTime, JobDate, WeekDay");

            using (DBManager db = new DBManager())
            {
                _entityList = db.getConnection().Query<JobBooking>(_sql.ToString()).ToList();
            }

            foreach (JobBooking _item in _entityList)
            {
                var _vm = new JobBookingsVM();
                _vm.IsNewItem = true;
                _vm.PopulateVM(new Guid(Cleaner.Id), _item);
                _vm.JobColourCode = CalculateSuburbColourCode(_item.JobSuburb);
                yield return _vm;
            }
        }

        /// <summary>
        /// Adjust the service gap to include the cleaner's travel time before and after the job based on other bookings and zones
        /// </summary>
        /// <remarks>
        /// Business rules:
        /// 0. Service Gap devided by # of team members is gap we use here. Assuming 2 cleaners cleans twice as fast as 1, etc.
        ///    Rounding the number to upper INT becomes the gap.
        ///    I'll assume travel time is not factored as seperate - both cleaners in this example travel together
        /// 1. 		Is this the cleaner's first job of the day?
        /// 1.1 	YES: Don't apply any travel gap in the beginning
        /// 1.2 	NO: then is cleaner's previous job zone in search job zone?
        /// 1.2.1 		YES: then apply SAME zone gap start
        /// 1.2.2		NO: apply OTHER zone gap start
        /// </remarks>
        /// <returns>The gap.</returns>
        private Int32 AdjustGap()
        {
            var isFirstJob = Cleaner.IsFirstJob;
            var virtualServiceGap = (CleanerTeamSize == 1) ? ServiceLenghtMinutes : (int)Math.Ceiling((decimal)ServiceLenghtMinutes / CleanerTeamSize);
            if (isFirstJob)
            {
                // #1.1
                return virtualServiceGap;
            }

            // #1.2
            var prevZoneList = Cleaner.PreviousJobLocation.GetZoneListBySuburb(false);
            if (prevZoneList.Intersect(ServiceZone).Any())
            {
                // #1.2.1
                return virtualServiceGap + SystemSettings.GapSameZoneMinutes;
            }

            // #1.2.2
            return virtualServiceGap + SystemSettings.GapOtherZoneMinutes;
        }

        private long CalculateTravelGap(String comparisonSuburb)
        {
            long travelGap = 0;                 

            //1. if searchZone = comparisonZone then return GapSameZoneMinutes
            //2. if searchNeighbouringZones.intersect comparisonZone then return GapSecondaryZoneMinutes
            //3. else return GapOtherZoneMinutes

            SuburbZone searchSuburbDetails = Cleaner.SearchSuburb.GetSuburbDetails();
            SuburbZone comparisonSuburbDetails = comparisonSuburb.GetSuburbDetails();

            if (searchSuburbDetails.Zone == comparisonSuburbDetails.Zone)
            {
                return travelGap + SystemSettings.GapSameZoneMinutes;
            }

            if (searchSuburbDetails.LinkedZones.Contains(comparisonSuburbDetails.Zone))
            {
                return travelGap + SystemSettings.GapSecondaryZoneMinutes;
            }           
           
            return travelGap + SystemSettings.GapOtherZoneMinutes;
        }

        // adds a new available timeslot 
        private void AddAvailableTimeSlot(IList<JobBookingsVM> list, long startTime, long endTime, DateTime serviceDate, List<JobBookingsVM> existingJobs)
        {
            var gapSize = endTime - startTime;

            if (gapSize < minJobSizeMins + AdjustedGapMins)
                return;

            String prevSuburb = "";

            if (list.Count > 0)
            {
                prevSuburb = list[list.Count - 1].JobSuburb;
            }

            long travelGap = 0;
            
            if (prevSuburb != "") { travelGap = CalculateTravelGap(prevSuburb); };
            


            if (endTime != RosterDayEndTime)
            {
                if (existingJobs.Count > 0)
                {
                    JobBookingsVM nextJob = new JobBookingsVM();

                    //Find next job by comparing the end time with the job start time
                    foreach (JobBookingsVM job in existingJobs)
                    {
                        if (job.WeekDay == ServiceDay.ToString())
                        {
                            if (job.StartTime == endTime)
                            {
                                nextJob = job;
                                break;
                            }
                        }
                    }
                    
                    //Calculate travel gap according to nextJob suburb                                        
                    endTime = endTime - CalculateTravelGap(nextJob.JobSuburb);
                }
            }

            startTime = startTime + travelGap;
            if (startTime == endTime) return;

            SuitableTimeSlots++;

            list.Add(new JobBookingsVM()
            {
                StartTime = startTime,
                EndTime = endTime,
                CleanerId = Cleaner.Id,
                JobDateUTC = ServiceDateUTC,
                JobDate = serviceDate,
                JobStatus = BookingStatus.AVAILABLE,
                JobSuburb = Cleaner.SearchSuburb,                 
                JobType = JobType,
                WeekDay = ServiceDay.ToString(),
                JobColourCode = "",
                IsNewItem = true
            });
        }

        private string CalculateSuburbColourCode(String suburb)
        {
            if (String.IsNullOrWhiteSpace(suburb))
            {
                return "";
            }

            var suburbZone = suburb.GetZoneListBySuburb(false);

            if (Cleaner.PrimaryZoneList.Intersect(suburbZone).Any())
            {
                return NamedColours.PrimaryJobColor;
            }

            if (Cleaner.SecondaryZoneList.Intersect(suburbZone).Any())
            {
                return NamedColours.SecondaryJobColor;
            }

            return NamedColours.ApprovedJobColor;

        }

        private void ValidateConstructorArguments(CleanerMatchResultVM cleaner, Int32 serviceLenghtMins, IList<String> serviceZone)
        {
            Cleaner = cleaner ?? throw new ArgumentException("No cleaner specified.", nameof(Cleaner));

            if (serviceLenghtMins <= 0)
            {
                throw new ArgumentException("Invalid service gap requested.", nameof(ServiceLenghtMinutes));
            }

            if (serviceZone == null || serviceZone.Count == 0)
            {
                throw new ArgumentException("Invalid service suburb/zone requested.", nameof(ServiceZone));
            }
        }
        #endregion
    }
}
