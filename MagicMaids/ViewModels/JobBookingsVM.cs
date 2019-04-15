#region Using
using System;
using MagicMaids.EntityModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endregion

namespace MagicMaids.ViewModels
{
	public enum BookingStatus
	{
		AVAILABLE,
		NEW,
		PENDING,
		CONFIRMED,
		CANCELLED
	}

	public enum JobTypeEnum
	{
		Weekly,
		Fortnighly,
		OneOff,
		Vacate
	}

    public enum ZoneMatch
    {
        Primary,
        Secondary,
        Approved,
        None
    }

	public class JobBookingsVM : BaseViewModel
	{
			
		#region Constructor
		public JobBookingsVM()
		{
			JobStatus = BookingStatus.NEW;
		}

		#endregion

		#region Properties, Public
		public String Id
		{
			get;
			set;
		}

		public Boolean IsNewItem
		{
			get;
			set;
		}

		public String CleanerId
		{
			get;
			set;
		}

		public String ClientId
		{
			get;
			set;
		}

		public JobTypeEnum JobType
		{
			get;
			set;
		}

		public String JobTypeName
		{
			get
			{
				return JobType.ToString();
			}
		}

		public DateTime? JobDateUTC
		{
			get;
			set;
		}

        public DateTime JobDate
        {
            get;
            set;
        }

        public DateTime JobEndDateUTC
        {
            get;
            set;
        }
                
        public DateTime JobEndDate
        {
            get;
            set;           
        }

		public String JobDateFormatted
		{
            //get
            //{
            //    if (JobDateUTC.HasValue)
            //    {
            //        return JobDateUTC.Value.FormatUserDate();
            //    }
            //    return "";
            //}

            get
            {
                if (JobDate == DateTime.MinValue)
                {
                    return JobDate.FormatUserDate();
                }
                return JobDate.ToUTC().FormatUserDate();
            }
        }

        public String JobEndDateFormatted
        {
            get
            {
                if (JobEndDate == DateTime.MinValue)
                {
                    return JobEndDate.FormatUserDate();
                }
                return JobEndDate.ToUTC().FormatUserDate();
            }
        }

        public String JobDateFormattedForList
        {
            get
            {
                if (JobDateUTC.HasValue)
                {
                    return JobDateUTC.Value.FormatUserDate();
                }
                return "";
            }

            //get
            //{
            //    return JobDate.ToUTC().FormatUserDate();
            //}
        }

        public string WeekDay
		{
			get;
			set;
		}

		public Int32 TeamSize
		{
			get;
			set;
		}

		public string ManHours
		{
			get
			{

				return ((int)(EndTime - StartTime)).ToTimeDuration();
			}
		}


		public String CleanerTeam
		{
			get
			{
				if (String.IsNullOrWhiteSpace(_cleanerTeam))
				{
					return "";
				}

				return (_cleanerTeam.Contains(",")) ? _cleanerTeam.Replace(",", "<br/>") : _cleanerTeam;
			}
			set
			{
				_cleanerTeam = value;
			}
		}
		private String _cleanerTeam;

		public long StartTime
		{
			get => _startTime;
			set
			{
				_startTime = value;
				_startForControl = _startTime.ToTime();
			}
		}
		private long _startTime;

		// need to be get/set plain to allow control to set it
		public DateTime StartTimeForControl
		{
			get => _startForControl;
			set
			{
				_startForControl = value;
				_startTime = _startForControl.ToMinutes();
			}
		}
		private DateTime _startForControl;

		public long EndTime
		{
			get => _endTime;
			set
			{
				_endTime = value;
				_endForControl = _endTime.ToTime();
			}
		}
		private long _endTime;

		// need to be get/set plain to allow control to set it
		public DateTime EndTimeForControl
		{
			get => _endForControl;
			set
			{
				_endForControl = value;
				_endTime = _endForControl.ToMinutes();
			}
		}
		private DateTime _endForControl;

		public string StartTimeOfDay
		{
			get
			{
				return StartTime.ToTime().FormatTime();
			}
		}

		// Created this property for the dialog to display formatted time changed from parent form
		// for some reason the readonly property StartTimeOfDay does not update on the dialog
		public string StartTimeOfDayPopupDisplay
		{
			get;
			set;
		}
		public string EndTimeOfDayPopupDisplay
		{
			get;
			set;
		}

		public string EndTimeOfDay
		{
			get
			{
				return EndTime.ToTime().FormatTime();
			}
		}

        public string ControlHeight
        {
            get
            {
                long duration = EndTime - StartTime;

                if (duration > 60 && duration <= 120)
                { duration = duration + 10; }
                else if (duration > 120 && duration <= 180)
                { duration = duration + 20; }
                else if (duration > 180 && duration <= 240)
                { duration = duration + 30; }
                else if (duration > 240 && duration <= 300)
                { duration = duration + 40; }
                else if (duration > 300 && duration <= 360)
                { duration = duration + 50; }
                else if (duration > 360 && duration <= 420)
                { duration = duration + 60; }
                else if (duration > 420 && duration <= 480)
                { duration = duration + 70; }
                else if (duration > 480 && duration <= 540)
                { duration = duration + 80; }
                else if (duration > 540 && duration <= 600)
                { duration = duration + 90; }
                else if (duration > 600 && duration <= 660)
                { duration = duration + 100; }
                else if (duration > 660 && duration <= 720)
                { duration = duration + 110; }

                return (duration / 2).ToString() + "px";
            }
        }

		[JsonConverter(typeof(StringEnumConverter))]
		public BookingStatus JobStatus
		{
			get;
			set;
		}

		public String JobSuburb
		{
			get;
			set;
		}
                
        public String NextJobSuburb
        {
            get;
            set;
        }

        public String JobColourCode
		{
			get;
			set;
		}

        public String JobTypeClass
        {
            get
            {
                if (JobType == JobTypeEnum.Fortnighly || JobType == JobTypeEnum.Weekly)
                {
                    return "Repeat_Job";
                }
                else
                {
                    return "One_Off_Job";
                }
            }
        }

		public String JobDescription
		{
			get
			{
				if ((JobDate != null || JobDate != DateTime.MinValue) && (JobType == JobTypeEnum.OneOff || JobType == JobTypeEnum.Vacate))
				{
					return $"{WeekDay} on {JobDate.FormatUserDate()} ({StartTimeOfDay}-{EndTimeOfDay})";
				}
				else
				{
					return $"{WeekDay} ({StartTimeOfDay}-{EndTimeOfDay})";
				}
			}
		}

        public String JobWeekYearStyle
        {
            get
            {
                if (JobType == JobTypeEnum.Fortnighly)
                {
                    return DateTimeWrapper.WeekYearStyle(JobDateUTC);
                } else
                {
                    return null;
                }
            }
        }

        public String JobWeekYearStyleNextWeek
        {
            get
            {
                if (JobType == JobTypeEnum.Fortnighly)
                {
                    return DateTimeWrapper.WeekYearStyle(JobDateUTC.Value.AddDays(7));
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region Methods, Public
        public void PopulateVM(Guid? cleanerId, JobBooking entityModel)
		{
			if (entityModel == null)
				return;

			if (cleanerId.Equals(Guid.Empty))
				return;

			Id = (Helpers.IsValidGuid(entityModel.Id)) ? entityModel.Id : "";

			CleanerId = entityModel.PrimaryCleanerRefId;
			ClientId = entityModel.ClientRefId;
			StartTime = entityModel.StartTime;
			EndTime = entityModel.EndTime;
			JobType = entityModel.JobType;
			JobStatus = entityModel.JobStatus;
			WeekDay = entityModel.WeekDay;
			JobDateUTC = entityModel.JobDate;
			JobSuburb = entityModel.JobSuburb;
			TeamSize = entityModel.TeamSize;
            JobEndDateUTC = entityModel.JobEndDate;
		}
		#endregion
	}
}
