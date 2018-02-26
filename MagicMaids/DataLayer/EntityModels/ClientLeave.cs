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
			get
			{
				return _startDate.ToLocalTime();
			}
			set
			{
				_startDate = value.ToUniversalTime();
			}
		}
		private DateTime _startDate;


		[Required]
		[DataType(DataType.Date)]
		public DateTime EndDate
		{
			get
			{
				return _endDate.ToLocalTime();
			}
			set
			{
				_endDate = value.ToUniversalTime();
			}
		}
		private DateTime _endDate;

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
