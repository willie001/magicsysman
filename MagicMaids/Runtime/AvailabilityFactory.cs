﻿using System;
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
		public DateTime ServiceDateUTC;	//UTC
		private Int32 ServiceGapMinutes;
		private IList<String> ServiceZone;
		private JobTypeEnum JobType;
		private DayOfWeek ServiceDay;


		const Int32 minJobSizeMins = 0;
		#endregion

		#region Constructor
		public AvailabilityFactory(CleanerMatchResultVM cleaner, Int32 serviceGapMins, JobTypeEnum serviceType, IList<String> serviceZone)
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

			ServiceDay = cleaner.SelectedRosterDay;
			if (cleaner.SelectedServiceDate.HasValue && cleaner.SelectedServiceDate != default(DateTime))
			{
				ServiceDateUTC = cleaner.SelectedServiceDate.Value.ToUTC();
			}
			else if (JobType == JobTypeEnum.Fortnighly || JobType == JobTypeEnum.Weekly)
			{
				// calculate next date
				ServiceDateUTC = DateTimeWrapper.FindNextDateForDay((DayOfWeek)ServiceDay);
			}
			JobType = serviceType;

			ServiceGapMinutes = serviceGapMins;
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
		internal IList<JobBookingsVM> GetCleanerDaySchedule()
		{
			var dayList = new List<JobBookingsVM>();
			long dayStart = 0;
			long dayEnd = 0;
			SuitableTimeSlots = 0;

			var cleanerRoster = GetCleanerRoster();
			foreach (CleanerRosterVM item in cleanerRoster)
			{
				if (item.Weekday.Equals(ServiceDay.ToString(), StringComparison.InvariantCultureIgnoreCase))
				{
					Cleaner.SelectedRosterDay = ServiceDay;
					Cleaner.TeamSize = item.TeamCount;
					dayStart = item.TimeOfDayFrom;
					dayEnd = item.TimeOfDayTo;
					break;
				}
			}

			if (dayStart == 0)
			{
				// not found for the specific day
				throw new NoTeamRosteredException(Cleaner.FirstName + ' ' + Cleaner.LastName, ServiceDay.ToString());
			}

			long previousEndTime = 0;
			var existingSchedule = GetCleanerFutureBookings();
			foreach(JobBookingsVM item in existingSchedule)
			{
				if (previousEndTime == dayEnd)
				{
					break;
				}

				if (item.WeekDay == ServiceDay.ToString())
				{
					if (item.StartTime > previousEndTime)
					{
						AddAvailableTimeSlot(dayList, (previousEndTime == 0 ? dayStart : previousEndTime), item.StartTime);
					}
						
					dayList.Add(item);
					previousEndTime = item.EndTime;
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
		private IEnumerable<JobBookingsVM> GetCleanerFutureBookings()
		{
			List<JobBooking> _entityList = new List<JobBooking>();
			StringBuilder _sql = new StringBuilder($"Select * from JobBooking where PrimaryCleanerRefId = '{Cleaner.Id}'");
			_sql.Append($" and JobStatus in ('{BookingStatus.CONFIRMED}', '{BookingStatus.PENDING}')");
			if (!String.IsNullOrWhiteSpace(ServiceDay.ToString()))
			{
				_sql.Append($" and WeekDay = '{ServiceDay.ToString()}'");
			}
			_sql.Append($" and JobDate > '{DateTime.UtcNow.AddDays(-1).FormatDatabaseDate()}'");
			_sql.Append(" order by JobDate, WeekDay, StartTime, EndTime");

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
			var virtualServiceGap = (CleanerTeamSize == 1) ? ServiceGapMinutes : (int)Math.Ceiling((decimal)ServiceGapMinutes / CleanerTeamSize);
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

		// adds a new available timeslot 
		private void AddAvailableTimeSlot(IList<JobBookingsVM> list, long startTime, long endTime)
		{
			var gapSize = endTime - startTime;

			if (gapSize < minJobSizeMins+AdjustedGapMins)
				return;

			SuitableTimeSlots++;
			list.Add(new JobBookingsVM()
			{
				StartTime = startTime,
				EndTime = endTime,
				CleanerId = Cleaner.Id,
				JobDateUTC = ServiceDateUTC,
				JobStatus = BookingStatus.AVAILABLE,
				JobSuburb = "",
				JobType = JobType,
				WeekDay = ServiceDay.ToString(),
				JobColourCode = CalculateSuburbColourCode(""),
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
		#endregion
	}
}
