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
		private DateTime ServiceDate;
		private Int32 ServiceGapMinutes;
		private IList<String> ServiceZone;
		private JobTypeEnum JobType;

		const Int32 minJobSizeMins = 0;
		#endregion

		#region Constructor
		public AvailabilityFactory(CleanerMatchResultVM cleaner, DateTime serviceDate, Int32 serviceGapMins, JobTypeEnum serviceType, IList<String> serviceZone)
		{
			Cleaner = cleaner ?? throw new ArgumentException("No cleaner specified.", nameof(Cleaner));

			if (serviceGapMins <= 0)
			{
				throw new ArgumentException("Invalid service gap requested.", nameof(ServiceGapMinutes));
			}

			if (serviceZone == null || serviceZone.Count == 0)
			{
				throw new ArgumentException("Invalid service suburb/zone requested.", nameof(ServiceZone));
			}	

			ServiceDate = serviceDate;
			ServiceGapMinutes = serviceGapMins;
			ServiceZone = serviceZone;
			JobType = serviceType;
		}
		#endregion

		#region Properties
		public String ServiceWeekDay
		{
			get
			{
				if (ServiceDate.Year > 1950)
				{
					return ServiceDate.DayOfWeek.ToString();
				}

				// periodic cleans
				return "";
			}
		}

		public Int32 CleanerTeamSize
		{
			get
			{
				return Cleaner.TeamSize;
			}
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
		internal IList<JobBookingsVM> GetCleanerDaySchedule()
		{
			var dayList = new List<JobBookingsVM>();
			long dayStart = 0;
			long dayEnd = 0;

			var cleanerRoster = GetCleanerRoster();
			foreach (CleanerRosterVM item in cleanerRoster)
			{
				if (item.Weekday.Equals(ServiceWeekDay, StringComparison.InvariantCultureIgnoreCase))
				{
					Cleaner.SelectedRosterDay = ServiceWeekDay;
					Cleaner.TeamSize = item.TeamCount;
					dayStart = item.TimeOfDayFrom;
					dayEnd = item.TimeOfDayTo;
					break;
				}
			}

			if (dayStart == 0)
			{
				// not found for the specific day
				throw new NoTeamRosteredException(Cleaner.FirstName + ' ' + Cleaner.LastName, ServiceWeekDay);
			}

			long previousEndTime = 0;
			var existingSchedule = GetCleanerFutureBookings();
			foreach(JobBookingsVM item in existingSchedule)
			{
				if (previousEndTime == dayEnd)
				{
					break;
				}

				if (item.WeekDay == ServiceWeekDay)
				{
					if (item.StartTime > previousEndTime)
					{
						AddAvailableTimeSlot(dayList, (previousEndTime == 0 ? dayStart : previousEndTime), item.StartTime);
						previousEndTime = item.StartTime;
					}
					else
					{
						dayList.Add(item);
						previousEndTime = item.EndTime;
					}
				}
			}

			// adds the last gap of the day as available
			if (previousEndTime > 0 && previousEndTime < dayEnd)
			{
				AddAvailableTimeSlot(dayList, previousEndTime, dayEnd);
			}

			if (dayList.Count == 0)
			{
				// no jobs - all day is available
				AddAvailableTimeSlot(dayList, dayStart, dayEnd);
			}

			if (dayList.Count == 0)
			{
				throw new NoSuitableGapAvailable(ServiceWeekDay, minJobSizeMins + AdjustedGapMins);
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
		private IEnumerable<JobBookingsVM> GetCleanerFutureBookings()
		{
			List<JobBooking> _entityList = new List<JobBooking>();
			StringBuilder _sql = new StringBuilder($"Select * from JobBooking where PrimaryCleanerRefId = '{Cleaner.Id}'");
			_sql.Append($" and JobStatus in ('{BookingStatus.CONFIRMED}', '{BookingStatus.PENDING}')");
			if (!String.IsNullOrWhiteSpace(ServiceWeekDay))
			{
				_sql.Append($" and WeekDay = '{ServiceWeekDay}'");
			}
			if (ServiceDate.Year >= DateTime.Now.Year)
			{
				_sql.Append($" and DATE(JobDate) = DATE('{ServiceDate}') ");
			}
			_sql.Append(" order by JobDate, WeekDay, StartTime, EndTime");

			using (DBManager db = new DBManager())
			{
				_entityList = db.getConnection().Query<JobBooking>(_sql.ToString()).ToList();
			}

			foreach (JobBooking _item in _entityList)
			{
				var _vm = new JobBookingsVM();
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

			if (isFirstJob)
			{
				// #1.1
				return ServiceGapMinutes;
			}

			// #1.2
			var prevZoneList = Cleaner.PreviousJobLocation.GetZoneListBySuburb(false);
			if (prevZoneList.Intersect(ServiceZone).Any())
			{
				// #1.2.1
				return ServiceGapMinutes + SystemSettings.GapSameZoneMinutes;
			}

			// #1.2.2
			return ServiceGapMinutes + SystemSettings.GapOtherZoneMinutes;	
		}

		// adds a new available timeslot 
		private void AddAvailableTimeSlot(IList<JobBookingsVM> list, long startTime, long endTime)
		{
			var gapSize = endTime - startTime;

			if (gapSize < minJobSizeMins+AdjustedGapMins)
				return;

			list.Add(new JobBookingsVM()
			{
				StartTime = startTime,
				EndTime = endTime,
				CleanerId = Cleaner.Id,
				JobDate = ServiceDate,
				JobStatus = BookingStatus.AVAILABLE,
				JobSuburb = "",
				JobType = JobType,
				WeekDay = ServiceWeekDay,
				JobColourCode = CalculateSuburbColourCode("")
					
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
		#endregion
	}
}
