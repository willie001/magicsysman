#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels
{
	[Table("Cleaners")]
	public class Cleaner : BaseModel
	{
		#region Property, Public
		public Int32 CleanerCode
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public String Initials
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public String FirstName
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public String LastName
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public String Region
		{
			get;
			set;
		}

		public Int32? Rating
		{
			get;
			set;
		}

		public String GenderFlag
		{
			get;
			set;
		}

		[Required]
		public Boolean Ironing
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

		public Boolean CleanerOnLeave
		{
			get;
			set;
		}
		#endregion

		#region Properties, Foreign Key
		public String MasterFranchiseRefId
		{
			get;
			set;
		}

		public String PhysicalAddressRefId
		{
			get;
			set;
		}

		public string PrimaryZone
		{
			get;
			set;
		}

		public string SecondaryZone
		{
			get;
			set;
		}

		public string ApprovedZone
		{
			get;
			set;
		}

		[ForeignKey("PhysicalAddressRefId")]
		public Address PhysicalAddress
		{
			get;
			set;
		}

		#endregion
	}
}
