#region Using
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels
{
	[Table("CleanerTeam")]
	public class CleanerTeam : BaseModel
	{
		#region Properties, Public
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
		public string MobileNumber
		{
			get;
			set;
		}

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

		public virtual ICollection<CleanerRosteredTeam> CleanerRosteredTeam
		{
			get;
			set;
		}
		#endregion 
	}
}
