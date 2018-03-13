#region Using
using System;
using FluentValidation.Attributes;
using MagicMaids.Validators;
#endregion

namespace MagicMaids.ViewModels
{
	[Validator(typeof(SearchCleanerMatch))]
	public class SearchVM
	{
		#region Properties, Public
		public string Suburb
		{
			get;
			set;
		}

		public string ServiceType
		{
			get;
			set;
		}

		public Boolean WeeklyJob
		{
			get;
			set;
		}

		public Boolean FortnightlyJob
		{
			get;
			set;
		}

		public Boolean OneOffJob
		{
			get;
			set;
		}

		public Boolean VacateClean
		{
			get;
			set;
		}


		public String ServiceDay
		{
			get;
			set;
		}

		public Int32 ServiceDayValue
		{
			get;
			set;
		}

		public DateTime ServiceDate
		{
			get;
			set;
		}

		public Decimal ServiceLength
		{
			get;
			set;
		}
		#endregion

	}
}
