#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MagicMaids.ViewModels;
#endregion

namespace MagicMaids.EntityModels
{
	[Table("JobBooking")]
	public class JobBooking : BaseModel
	{
		#region Properties, Public
		public JobTypeEnum JobType
		{
			get;
			set;
		}

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

		public DateTime? JobDate
		{
			get;
			set;
		}

		public String WeekDay
		{
			get;
			set;
		}

		[Required]
		public long StartTime
		{
			get
			{
				return _startTime;
			}
			set
			{
				var convertedValue = value;
				if (convertedValue != _startTime)
				{
					_startTime = convertedValue;
				}
			}
		}
		private long _startTime;

		[Required]
		public long EndTime
		{
			get
			{
				return _endTime;
			}
			set
			{
				var convertedValue = value;
				{
					_endTime = convertedValue;
				}
			}
		}
		private long _endTime;
		#endregion

		#region Properties, Foreign Key
		public String PrimaryCleanerRefId
		{
			get;
			set;
		}

		[ForeignKey("PrimaryCleanerRefId")]
		public Cleaner PrimaryCleaner
		{
			get;
			set;
		}

		public String ClientRefId
		{
			get;
			set;
		}

		[ForeignKey("ClientRefId")]
		public Client Client
		{
			get;
			set;
		}
		#endregion
	}
}
