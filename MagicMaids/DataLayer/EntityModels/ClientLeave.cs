#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;
#endregion

namespace MagicMaids.EntityModels
{
	[Table("ClientLeave")]
	public class ClientLeave : BaseModel 
	{
		#region Properties, Public
		[Required]
		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime StartDate
		{
			get
			{
				return _startDate.ToUser();
			}
			set
			{
				var convertedValue = value.ToUTC();
				if (convertedValue != _startDate)
				{
					_startDate = convertedValue;
				}
			}
		}
		private DateTime _startDate;

		[Required]
		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime EndDate
		{
			get
			{
				return _endDate.ToUser();
			}
			set
			{
				var convertedValue = value.ToUTC();
				if (convertedValue != _endDate)
				{
					_endDate = convertedValue;
				}
			}
		}
		private DateTime _endDate;

		#endregion

		#region Properties, Foreign Key
		public String ClientRefId
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
