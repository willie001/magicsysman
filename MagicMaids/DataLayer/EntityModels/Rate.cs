#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels
{
	[Flags]
	public enum RateApplicationsSettings
	{
		None = 0,
		Residential = 1,
		Commercial = 2,
		FirstVisit = 4,
		NormalVisit = 8,
		OneOff = 16,
		Vacancy = 32,
		OneHour = 64	
	}

	[Table("Rates")]
	public class Rate : BaseModel
	{
		#region Properties, Public
		[Required]
		[DataType(DataType.Text)]
		public string RateCode
		{
			get;
			set;
		}

		[DataType(DataType.DateTime)]
		public DateTime? ActivationDate
		{
			get;
			set;
		}

		[DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
		[Required]
		[DataType(DataType.Currency)]
		public decimal RateAmount
		{
			get;
			set;
		}

		[Required]
		public RateApplicationsSettings RateApplications
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
