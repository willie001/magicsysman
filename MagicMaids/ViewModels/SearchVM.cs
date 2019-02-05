#region Using
using System;
using FluentValidation.Attributes;
using MagicMaids.Validators;
using Newtonsoft.Json;
#endregion

namespace MagicMaids.ViewModels
{
	[Validator(typeof(SearchCleanerMatch))]
	public class SearchVM : BaseViewModel
	{
		#region Constructor
		public SearchVM()
		{
			ServiceLengthMins = 120;
		}
		#endregion 

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
			get
			{
				if (OneOffJob || VacateClean)
				{
					return ServiceDate.DayOfWeek.ToString();
				}

				return _serviceDay;
			}
			set
			{
				_serviceDay = value;
			}
		}
		private String _serviceDay;

		public Int32 ServiceDayValue
		{
			get;
			set;
		}

		public DateTime ServiceDate
		{
			get
            {
                return _serviceDate.ToUser();
            }
			set
            {
                _serviceDate = value;
            }
		}

        private DateTime _serviceDate;

		public String ServiceDateFormatted
		{
			get
			{
				if (ServiceDate.Year > 1900)
				{
					return ServiceDate.FormatUserDate();
				}

				return "";
			}
		}

		public Int32 ServiceLengthMins
		{
			get 
			{
				return Convert.ToInt32(ServiceLengthForControl.ToMinutes());
			}
			set
			{
				_serviceLength = value;
				ServiceLengthForControl = _serviceLength.ToTime();
			}
		}
		private Int32 _serviceLength;

		public String ServiceLengthFormatted
		{
			get
			{
				return ServiceLengthMins.ToTimeDuration();
			}
		}

		// need to be get/set plain to allow control to set it
		public DateTime ServiceLengthForControl
		{
			get;
			set;
		}

		// Sets to true to remove zone filtering (all cleaners)
		public Boolean FilterZonesNone
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

		public String RepeatCustomer
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
