#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels
{
	[Table("Methods")]
	public class ClientMethod : BaseModel
	{
		#region Property, Public
		[Required]
		[DataType(DataType.Text)]
		public String Details
		{
			get;
			set;
		}

		public String Validated
		{
			get;
			set;
		}
		#endregion 
	}
}