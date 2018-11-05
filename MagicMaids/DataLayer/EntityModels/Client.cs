#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels                  
{
	[Table("Clients")]
	public class Client: BaseModel
	{
		#region Property, Public
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

		public string ClientType
		{
			get;
			set;
		}

		public string KeyLocation
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
		public Address PhysicalAddress
		{
			get;
			set;
		}

		#endregion
	}
}
