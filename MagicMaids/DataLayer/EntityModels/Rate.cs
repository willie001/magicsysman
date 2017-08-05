#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
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

	public class Rate : BaseModel
	{
		#region Properties, Public
		public string RateCode
		{
			get;
			set;
		}

		public string ApplicationDescription
		{
			get;
			set;
		}

		[DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
		public decimal RateAmount
		{
			get;
			set;
		}

		public RateApplicationsSettings RateApplications
		{
			get;
			set;
		}

		private string _rateApplicationFormat;
		public string RateApplicationHtml
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(_rateApplicationFormat))
				{
					return _rateApplicationFormat;
				}

				System.Text.StringBuilder _output = new System.Text.StringBuilder();

				if (RateApplications.Equals(RateApplicationsSettings.None))
				{
					_rateApplicationFormat = string.Empty;
					return "<div class=\"label bg-danger-light\">Not set</div>";
				}
				else
				{
					if (RateApplications.HasFlag(RateApplicationsSettings.Residential))
					{
						if (_output.Length > 0) _output.Append("&nbsp;");
						_output.Append("<div class=\"label bg-success\">Residential</div>");	
					}

					if (RateApplications.HasFlag(RateApplicationsSettings.Commercial))
					{
						if (_output.Length > 0) _output.Append("&nbsp;");
						_output.Append("<div class=\"label bg-primary\">Commercial</div>");
					}

					if (RateApplications.HasFlag(RateApplicationsSettings.FirstVisit))
					{
						if (_output.Length > 0) _output.Append("&nbsp;");
						_output.Append("<div class=\"label bg-inverse-light\">Initial</div>");
					}

					if (RateApplications.HasFlag(RateApplicationsSettings.NormalVisit))
					{
						if (_output.Length > 0) _output.Append("&nbsp;");
						_output.Append("<div class=\"label bg-warning\">Standard</div>");
					}

					if (RateApplications.HasFlag(RateApplicationsSettings.OneHour))
					{
						if (_output.Length > 0) _output.Append("&nbsp;");
						_output.Append("<div class=\"label bg-yellow\">One Hour</div>");
					}

					if (RateApplications.HasFlag(RateApplicationsSettings.OneOff))
					{
						if (_output.Length > 0) _output.Append("&nbsp;");
						_output.Append("<div class=\"label bg-purple\">One-Off</div>");
					}

					if (RateApplications.HasFlag(RateApplicationsSettings.Vacancy))
					{
						if (_output.Length > 0) _output.Append("&nbsp;");
						_output.Append("<div class=\"label bg-pink\">Vacancy</div>");
					}

					if (_output.Length == 0) _output.Append("<div class=\"label bg-danger-dark\">Invalid</div>");
				}
	
			    _rateApplicationFormat = _output.ToString();
				return _rateApplicationFormat;
			}
		}
		#endregion
	}
}
