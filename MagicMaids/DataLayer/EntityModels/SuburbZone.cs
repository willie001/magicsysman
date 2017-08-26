#region Using
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
#endregion 

namespace MagicMaids.EntityModels
{
	[Table("SuburbZones")]
	public class SuburbZone: BaseModel 
	{
		#region Property, Public
		[Required]
		[DataType(DataType.Text)]
		public string SuburbName
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public string PostCode
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public string Zone
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public string LinkedZones
		{
			get;
			set;
		}
		#endregion

		#region Properties, Foreign Key
		public Guid? FranchiseId
		{
			get;
			set;
		}
		#endregion
	}
}
