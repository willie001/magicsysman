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

		public String DateKind
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

		public virtual ICollection<CleanerRosteredTeam> CleanerRosteredTeam
		{
			get;
			set;
		}
		#endregion
	}
}
