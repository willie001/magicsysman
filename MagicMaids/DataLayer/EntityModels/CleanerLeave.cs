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
		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime StartDate
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime EndDate
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
