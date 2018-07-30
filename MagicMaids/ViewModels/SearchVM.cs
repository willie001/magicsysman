#region Using
using System;
using System.Collections.Generic;
using FluentValidation.Attributes;
using MagicMaids.Validators;
using Newtonsoft.Json;
#endregion

namespace MagicMaids.ViewModels
{
	[Validator(typeof(SearchCleanerMatch))]
	public class SearchVM : BaseViewModel
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

		public Boolean RequireIroning
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

		public Boolean FilterZonesPrimary
		{
			get;
			set;
		}

		public Boolean FilterZonesSecondary
		{
			get;
			set;
		}

		public Boolean FilterZonesApproved
		{
			get;
			set;
		}

		public Boolean FilterZonesOther
		{
			get;
			set;
		}

		public Int32 FilterRating
		{
			get;
			set;
		}

		public Boolean HasCriteria
		{
			get;
			set;
		}
		#endregion

		#region Methods, Public
		public override string ToString()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Formatting = Formatting.Indented,
				NullValueHandling = NullValueHandling.Ignore
			};

			return JsonConvert.SerializeObject(this, settings);
		}
		#endregion 

	}
}
