#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels
{
	public enum AddressTypeSetting
	{
		Postal,
		Physical
	}

	[Table("Addresses")]
	public class Address : BaseModel
	{
		
		#region Properties, Public
		public AddressTypeSetting AddressType
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public string AddressLine1
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string AddressLine2
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string AddressLine3
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public string Suburb
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public string State
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
		public string Country
		{
			get;
			set;
		}
		#endregion
	}
}

