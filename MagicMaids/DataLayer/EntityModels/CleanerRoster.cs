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
			get;
			set;
		}

		[Required]
		[DataType(DataType.Time)]
		public DateTime EndTime
		{
			get;
			set;
		}

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
