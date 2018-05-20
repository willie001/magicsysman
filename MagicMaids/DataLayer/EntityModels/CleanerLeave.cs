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
			get
			{
				return _startDate;
			}
			set
			{
				var convertedValue = value;
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
				return _endDate;
			}
			set
			{
				var convertedValue = value;
				if (convertedValue != _endDate)
				{
					_endDate = convertedValue;
				}
			}
		}
		private DateTime _endDate;

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
		#endregion
	}
}
