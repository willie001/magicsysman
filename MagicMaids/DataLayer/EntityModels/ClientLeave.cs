#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels
{
	[Table("ClientLeave")]
	public class ClientLeave : BaseModel 
	{
		#region Properties, Public
		[Required]
		[DataType(DataType.Date)]
		public DateTime StartDate
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Date)]
		public DateTime EndDate
		{
			get;
			set;
		}

		#endregion

		#region Properties, Foreign Key
		public Guid ClientRefId
		{
			get;
			set;
		}

		[ForeignKey("ClientRefId")]
		public Cleaner Client
		{
			get;
			set;
		}
		#endregion
	}
}
