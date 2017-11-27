#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels
{
	[Table("CleanerLeave")]
	public class CleanerLeave : BaseModel
	{
		#region Properties, Public
		[Required]
		[DataType(DataType.Date)]
		public DateTime LeaveStart
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Date)]
		public String LeaveEnd
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
		#endregion
	}
}
