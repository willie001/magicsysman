#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels
{
	[Table("Franchises")]
	public class Franchise : BaseModel
	{
		#region Properties, Public
		[Required]
		[DataType(DataType.Text)]
		public string MasterFranchiseCode
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public string Name
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public string TradingName
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.EmailAddress)]
		public string EmailAddress
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string BusinessPhoneNumber
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string MobileNumber
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string OtherNumber
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public string CodeOfConductURL
		{
			get;
			set;
		}

		public decimal? ManagementFeePercentage
		{
			get;
			set;
		}

		public decimal? RoyaltyFeePercentage
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public string MetroRegion
		{
			get;
			set;
		}


		#endregion

		#region Properties, Foreign Key
		public String PhysicalAddressRefId
		{
			get;
			set;
		}

		[ForeignKey("PhysicalAddressRefId")]
		public  Address PhysicalAddress
		{
			get;
			set;
		}

		public String PostalAddressRefId
		{
			get;
			set;
		}

		[ForeignKey("PostalAddressRefId")]
		public  Address PostalAddress
		{
			get;
			set;
		}
		#endregion
	}
}
