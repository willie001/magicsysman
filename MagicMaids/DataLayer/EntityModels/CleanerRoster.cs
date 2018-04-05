#region Using
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels
{
	[Table("CleanerRoster")]
	public class CleanerRoster : BaseModel
	{
		#region Properties, Public
		[Required]
		[DataType(DataType.Text)]
		public String Weekday
		{
			get;
			set;
		}

		[Required]
		public Int32 TeamCount
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Time)]
		public DateTime StartTime
		{
			get
			{
				return DateTimeWrapper.UTCtoLocal(_startTime);
			}
			set
			{
				var convertedValue = DateTimeWrapper.LocaltoUTC(value);
				if (convertedValue != _startTime)
				{
					_startTime = convertedValue;
				}
			}
		}
		private DateTime _startTime;

		[Required]
		[DataType(DataType.Time)]
		public DateTime EndTime
		{
			get
			{
				return DateTimeWrapper.UTCtoLocal(_endTime);
			}
			set
			{
				var convertedValue = DateTimeWrapper.LocaltoUTC(value);
				if (convertedValue != _endTime)
				{
					_endTime = convertedValue;
				}
			}
		}
		private DateTime _endTime;

		#endregion

		#region Properties, Foreign Key
		public Guid PrimaryCleanerRefId
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

		public virtual ICollection<CleanerRosteredTeam> CleanerRosteredTeam
		{
			get;
			set;
		}
		#endregion
	}
}
